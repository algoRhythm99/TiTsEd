using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

            var flagsObject = FlagsObject;
            _flags = new GeneralObjectVM(flagsObject);
            if (null != previousVM) {
                _perkSearchText = previousVM._perkSearchText;
                _keyItemSearchText = previousVM._keyItemSearchText;
                _rawDataSearchText = previousVM._rawDataSearchText;
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
            Flags = new UpdatableCollection<FlagVM>(_allFlags.Values.ToList().Where(x => x.Match(RawDataSearchText)));
        }

        public static void ImportMissingNamedVectors(AmfObject items, IEnumerable<XmlStorageClass> xmlItems, string nameProperty, Func<AmfObject, String> descriptionGetter = null, IList<XmlStorageClass> targetXmlList = null) {
            if (targetXmlList == null) targetXmlList = (IList<XmlStorageClass>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in items) {
                var itemObject = pair.ValueAsObject;
                var name = itemObject.GetString(nameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlStorageClass { Name = name };
                if (descriptionGetter != null) xml.Description = descriptionGetter(itemObject);
                targetXmlList.Add(xml);
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
            var tmpChar = GetObj("characters");
            tmpChar = tmpChar.GetObj(name);
            Character = new CharacterVM(this, tmpChar);
            _characterName = name;
            if (name == "PC") {
                IsPC = true;
            } else {
                IsPC = false;
            }

            Character.UpdateKeyItems();

            Character.UpdatePerks();

            Character.UpdateStatusEffects();
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

        string _itemSearchText = "";
        public string ItemSearchText {
            get { return _itemSearchText; }
            set {
                if (_itemSearchText == value) {
                    return;
                }
                _itemSearchText = value;
                //a less than optimal way of handling this
                Character.UpdateItemList();
            }
        }

        string _perkSearchText = "";
        public string PerkSearchText {
            get { return _perkSearchText; }
            set {
                if (_perkSearchText == value) {
                    return;
                }
                _perkSearchText = value;
                foreach (var group in Character.PerkGroups) group.Update();
            }
        }

        string _keyItemSearchText = "";
        public string KeyItemSearchText {
            get { return _keyItemSearchText; }
            set {
                if (_keyItemSearchText == value) {
                    return;
                }
                _keyItemSearchText = value;
                foreach (var group in Character.KeyItemGroups) group.Update();
            }
        }

        string _rawDataSearchText = "";
        public string RawDataSearchText
        {
            get { return _rawDataSearchText; }
            set {
                if (_rawDataSearchText == value) {
                    return;
                }
                _rawDataSearchText = value;
                foreach (var group in Character.StatusEffectGroups) group.Update();
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
        public FlagVM GetFlag(string name, [CallerMemberName] string propertyName = null) {
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
                        var search = RawDataSearchText;
                        RawDataSearchText = @"\u200B"; // setting to Unicode zero-width space
                        RawDataSearchText = search;
                    }
                }
            }
        }

        private RelayCommand<FlagVM> _deleteFlagCommand;
        public RelayCommand<FlagVM> DeleteFlagCommand {
            get { return _deleteFlagCommand ?? (_deleteFlagCommand = new RelayCommand<FlagVM>(d => RemoveFlag(d))); }
        }
    }
}
