using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed partial class GameVM : ObjectVM {
        private GeneralObjectVM _flags;
        private string _characterName;
        private bool _IsPC = true;
        private string[] _characters;
        public readonly List<PerkVM> AllPerks = new List<PerkVM>();
        public readonly List<KeyItemVM> AllKeyItems = new List<KeyItemVM>();
        public readonly List<StatusEffectVM> AllStatusEffects = new List<StatusEffectVM>();
        readonly SortedDictionary<string,FlagVM> _allFlags = new SortedDictionary<string,FlagVM>();

        public GameVM(AmfFile file, GameVM previousVM)
            : base(file) {
            SetCharacterOptions();
            setCharacter("PC");
            SaveFile = new AmfObjectVM(file);

            var flagsObject = FlagsObject;
            _flags = new GeneralObjectVM(flagsObject);
            if (null != previousVM) {
                _searchText = previousVM._searchText;
            }

            // Flags
            foreach (var xmlFlag in XmlData.Current.Flags) {
                if (!_allFlags.ContainsKey(xmlFlag.Name)) {
                    _allFlags[xmlFlag.Name] = new FlagVM(this, ref flagsObject, xmlFlag);
                }
            }
            foreach (var flag in flagsObject) {
                string flagName = flag.ToString();
                if (!_allFlags.ContainsKey(flagName)) {
                    XmlEnum data = new XmlEnum();
                    data.Name = flagName;
                    _allFlags[flagName] = new FlagVM(this, ref flagsObject, data);
                }
            }
            Flags = new UpdatableCollection<FlagVM>(_allFlags.Values.ToList().Where(x => x.Match(SearchText)));
        }

        public static void ImportUnknownStorageClassEntries(AmfObject items, IEnumerable<XmlStorageClass> xmlItems, IList<XmlStorageClass> targetXmlList = null, string nameProperty = "storageName", Func<AmfObject, string> descriptionGetter = null) {
            if (targetXmlList == null) targetXmlList = (IList<XmlStorageClass>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in items) {
                var itemObject = pair.ValueAsObject;
                var name = itemObject.GetString(nameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlStorageClass { Name = name };
                if (descriptionGetter != null) {
                    xml.Description = descriptionGetter(itemObject);
                } else {
                    xml.Description = itemObject.GetString("tooltip");
                }
                targetXmlList.Add(xml);
            }
        }

        public static void ImportUnknownItems(List<ItemContainerVM> containers, List<String> types) {
            var unknownItemGroup = XmlData.Current.ItemGroups.Last();

            foreach (var slot in containers.SelectMany(x => x.Slots)) {
                // Add this item to the DB if it does not exist
                var type = slot.TypeID;
                if (String.IsNullOrEmpty(type)) continue;
                if ("classes.Items.Miscellaneous::EmptySlot" == type) continue;
                if (XmlData.Current.ItemGroups.SelectMany(x => x.Items).Any(x => x.ID == type)) continue;

                var shortName = slot.GetString("shortName");
                var longName = slot.GetString("longName");
                var stackSize = slot.GetInt("stackSize", 1);
                var tooltip = slot.GetString("tooltip");
                var version = slot.GetInt("version", 1);

                var fields = new List<XmlObjectField>();
                if (slot.HasRandomProperties) {
                    var obj = slot.GetAmfObject();
                    foreach (var prop in obj) {
                        string key = prop.Key.ToString();
                        string val = prop.Value.ToString();
                        string propType = "string";
                        switch (key) {
                            case "shortName":
                            case "version":
                            case "classInstance":
                            case "longName":
                            case "tooltip":
                            case "quantity":
                            case "stackSize":
                                break;
                            default:
                                bool boolResult = false;
                                if (Boolean.TryParse(val, out boolResult) || prop.Value is bool) propType = "bool";
                                int intResult = 0;
                                if (Int32.TryParse(val, out intResult) || (prop.Value is uint) || (prop.Value is int)) propType = "int";
                                fields.Add(new XmlObjectField { Name = key, Value = val, Type = propType });
                                break;
                        }
                    }
                }
                var xml = new XmlItem { ID = type, Name = shortName, LongName = longName, Tooltip = tooltip, Version = version, Stack = stackSize, Fields = fields };
                unknownItemGroup.Items.Add(xml);
                slot.UpdateItemGroups();
            }
        }

        public void copyCharacterToPC() {
            copyCharacter(_characterName, "PC");
        }

        //for later
        private void copyCharacter(string src, string dst) {
            var chars = GetObj("characters");
            var srcChar = chars.GetObj(src);
            var dstChar = chars.GetObj(dst);

            foreach (var tag in XmlData.Current.General.CopyTags) {
                if (!srcChar.Contains(tag) || !dstChar.Contains(tag)) {
                    continue;
                }
                var value = srcChar[tag];
                if (value != null && value.GetType() == typeof(AmfObject)) {
                    dstChar[tag] = (srcChar[tag] as AmfObject).clone();
                } else {
                    dstChar[tag] = value;
                }
            }
        }

        private void setCharacter(string name) {
            Character = GetCharacter(name);
            _characterName = name;
            if (name == "PC") {
                IsPC = true;
            } else {
                IsPC = false;
            }

            Character.UpdateAll( _characterName );
        }

        public CharacterVM GetCharacter(string name)
        {
            var tmpChar = GetObj("characters");
            tmpChar = tmpChar.GetObj(name);
            var tcChar = new CharacterVM(this, tmpChar);
            return tcChar;
        }

        public CharacterVM Character { get; private set; }

        public string[] CharacterOptions {
            get {
                SetCharacterOptions();
                return _characters;
            }
            private set {
                _characters = value;
            }
        }

        private void SetCharacterOptions() {
            if (null == _characters) {
                var tmpChar = GetObj("characters");
                List<String> characters = new List<string>();
                foreach (AmfPair pair in tmpChar) {
                    characters.Add(pair.Key.ToString());
                }
                if (characters.Count > 1) {
                    characters.Sort();
                }
                CharacterOptions = characters.ToArray();
            }
        }

        public string CharacterSelection {
            get {
                return _characterName;
            }
            set {
                setCharacter(value);
                //update everything!
                OnPropertyChanged(null);
            }
        }


        public string Email {
            get {
                if (EmailEnabled) {
                    return _flags.GetString("PC_EMAIL_ADDRESS");
                }
                return "N/A";
            }
            set {
                if (EmailEnabled) {
                    var old = Email;
                    //ToAddressCache
                    //ContentCache

                    _flags.SetValue("PC_EMAIL_ADDRESS", value);

                    //Update messages in the mail system!
                    foreach (AmfPair pair in GetObj("mailSystem")) {
                        ObjectVM vm = new GeneralObjectVM(pair.ValueAsObject);
                        if (vm.HasValue("ToAddressCache")) {
                            var str = vm.GetString("ToAddressCache");
                            vm.SetValue("ToAddressCache", str.Replace(old, value));
                        }
                        if (vm.HasValue("ContentCache")) {
                            var str = vm.GetString("ContentCache");
                            vm.SetValue("ContentCache", str.Replace(old, value));
                        }
                    }
                }
            }
        }

        public bool EmailEnabled {
            get {
                return IsPC && _flags.HasValue("PC_EMAIL_ADDRESS");
            }
        }

        public new string Name {
            get { return Character.Name; }
            set {
                var oldName = Name + " Steele";
                Character.Name = value;

                if (IsPC) {
                    var newName = value + " Steele";
                    //Update messages in the mail system!
                    foreach (AmfPair pair in GetObj("mailSystem")) {
                        ObjectVM vm = new GeneralObjectVM(pair.ValueAsObject);
                        if (vm.HasValue("ToCache")) {
                            var str = vm.GetString("ToCache");
                            vm.SetValue("ToCache", str.Replace(oldName, newName));
                        }
                        if (vm.HasValue("ContentCache")) {
                            var str = vm.GetString("ContentCache");
                            vm.SetValue("ContentCache", str.Replace(oldName, newName));
                        }
                    }
                }
            }
        }

        public string Notes {
            get { return GetString("saveNotes"); }
            set { SetValue("saveNotes", String.IsNullOrWhiteSpace(value) ? "No notes available." : value); }
        }

        public int Days {
            get { return GetInt("daysPassed"); }
            set { SetValue("daysPassed", value); }
        }

        public int Hours {
            get { return GetInt("currentHours"); }
            set { SetValue("currentHours", value); }
        }

        public int Minutes {
            get { return GetInt("currentMinutes"); }
            set { SetValue("currentMinutes", value); }
        }

        public bool IsPC {
            get { return _IsPC; }
            private set { _IsPC = value; }
        }

        public bool IsNotPC {
            get { return !IsPC; }
        }

        public int PCUpbringing {
            get {
                if (IsPC && _flags.HasValue("PC_UPBRINGING") ) {
                    return _flags.GetInt("PC_UPBRINGING");
                }
                return 0;
            }
            set {
                _flags.SetValue("PC_UPBRINGING", value);
                OnFlagChanged("PC_UPBRINGING");
            }
        }

        public int AssTease {
            get {
                if (IsPC && _flags.HasValue("TIMES_BUTT_TEASED")) {
                    return _flags.GetInt("TIMES_BUTT_TEASED");
                }
                return 0;
            }
            set {
                _flags.SetValue("TIMES_BUTT_TEASED", value);
                OnFlagChanged("TIMES_BUTT_TEASED");
            }
        }

        public int ChestTease {
            get {
                if (IsPC && _flags.HasValue("TIMES_CHEST_TEASED")) {
                    return _flags.GetInt("TIMES_CHEST_TEASED");
                }
                return 0;
            }
            set {
                _flags.SetValue("TIMES_CHEST_TEASED", value);
                OnFlagChanged("TIMES_CHEST_TEASED");
            }
        }

        public int CrotchTease {
            get {
                if (IsPC && _flags.HasValue("TIMES_CROTCH_TEASED")) {
                    return _flags.GetInt("TIMES_CROTCH_TEASED");
                }
                return 0;
            }
            set {
                _flags.SetValue("TIMES_CROTCH_TEASED", value);
                OnFlagChanged("TIMES_CROTCH_TEASED");
            }
        }

        public int HipsTease {
            get {
                if (IsPC && _flags.HasValue("TIMES_HIPS_TEASED")) {
                    return _flags.GetInt("TIMES_HIPS_TEASED");
                }
                return 0;
            }
            set {
                _flags.SetValue("TIMES_HIPS_TEASED", value);
                OnFlagChanged("TIMES_HIPS_TEASED");
            }
        }

        public int OralTease
        {
            get
            {
                if (IsPC && _flags.HasValue("TIMES_ORAL_TEASED"))
                {
                    return _flags.GetInt("TIMES_ORAL_TEASED");
                }
                return 0;
            }
            set
            {
                _flags.SetValue("TIMES_ORAL_TEASED", value);
                OnFlagChanged("TIMES_ORAL_TEASED");
            }
        }

        string _searchText = "";
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                string search = _searchText ?? "";
                if (search.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
                _searchText = value;
                //a less than optimal way of handling this
                Character.UpdateItemList();
                Character.UpdatePerksVisibility();
                Character.UpdateKeyItemsVisibility();
                Character.UpdateStatusEffectsVisibility();
                Flags.Update();
            }
        }

        public AmfObject FlagsObject {
            get { return GetObj("flags"); }
        }

        public UpdatableCollection<FlagVM> Flags { get; private set; }

        /// <summary>
        /// Returns the flag with the specified name (even if not set in the save) AND registers a dependency between the caller property and this flag.
        /// That way, anytime the flag value is changed, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public FlagVM GetFlag(string name, string propertyName = null) {
            FlagVM flag = null;
            if (_allFlags.ContainsKey(name)) {
                flag = _allFlags[name];
                flag.GameVMProperties.Add(propertyName);
            }
            return flag;
        }

        public void OnFlagChanged(string name) {
            if (_allFlags.ContainsKey(name)) {
                foreach (var prop in _allFlags[name].GameVMProperties) OnPropertyChanged(prop);
            }
        }

        public void RemoveFlag(FlagVM flag) {
            if (null != flag) {
                var flagName = flag.Name;
                // remove flag
                if (null != FlagsObject && !String.IsNullOrEmpty(flagName)) {
                    if (null != FlagsObject[flagName]) {
                        flag.Value = null;
                        FlagsObject[flagName] = null;
                        OnFlagChanged(flagName);
                        var search = SearchText;
                        SearchText = @"\u200B"; // setting to Unicode zero-width space
                        SearchText = search;
                    }
                }
            }
        }

        public AmfObjectVM SaveFile
        {
            get;
            set;
        }

        private RelayCommand<FlagVM> _deleteFlagCommand;
        public RelayCommand<FlagVM> DeleteFlagCommand {
            get { return _deleteFlagCommand ?? (_deleteFlagCommand = new RelayCommand<FlagVM>(d => RemoveFlag(d))); }
        }
    }
}
