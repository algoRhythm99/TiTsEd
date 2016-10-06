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
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed partial class GameVM : ObjectVM {
        private GeneralObjectVM _flags;
        private string _character;
        private bool _IsPC = true;
        private string[] _characters;
        readonly List<PerkVM> _allPerks = new List<PerkVM>();
        readonly KeyItemVM[] _allKeyitems;

        public GameVM(AmfFile file, GameVM previousVM)
            : base(file) {
            SetCharacterOptions();
            setCharacter("PC");

            _flags = new GeneralObjectVM(GetObj("flags"));

            // Perks
            var charPerks = Character.GetObj("perks");
            if (null == charPerks) {
                charPerks = new Model.AmfObject(AmfTypes.Array);
            }
            var xmlPerks = XmlData.Current.PerkGroups.SelectMany(x => x.Perks).ToArray();
            var unknownPerkGroup = XmlData.Current.PerkGroups.Last();
            ImportMissingNamedVectors(charPerks, xmlPerks, "storageName", x => x.GetString("tooltip"), unknownPerkGroup.Perks);

            PerkGroups = new List<PerkGroupVM>();
            foreach (var xmlGroup in XmlData.Current.PerkGroups) {
                var perksVM = xmlGroup.Perks.OrderBy(x => x.Name).Select(x => new PerkVM(this, charPerks, x)).ToArray();
                _allPerks.AddRange(perksVM);

                var groupVM = new PerkGroupVM(this, xmlGroup.Name, perksVM);
                PerkGroups.Add(groupVM);
            }

            // KeyItems
            _keyItemSearchText = previousVM._keyItemSearchText;
            var keyItems = Character.GetObj("keyItems");
            if (null == keyItems) {
                keyItems = new Model.AmfObject(AmfTypes.Array);
            }
            var xmlKeys = XmlData.Current.KeyItems;
            ImportMissingNamedVectors(keyItems, xmlKeys, "storageName", x => x.GetString("tooltip"));
            _allKeyitems = XmlData.Current.KeyItems.OrderBy(x => x.Name).Select(x => new KeyItemVM(this, keyItems, x)).ToArray();
            Character.KeyItems = new UpdatableCollection<KeyItemVM>(_allKeyitems.Where(x => x.Match(_keyItemSearchText)));
        }

        static void ImportMissingNamedVectors(AmfObject items, IEnumerable<XmlNamedVector4> xmlItems, string nameProperty, Func<AmfObject, String> descriptionGetter = null, IList<XmlNamedVector4> targetXmlList = null) {
            if (targetXmlList == null) targetXmlList = (IList<XmlNamedVector4>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in items) {
                var name = pair.ValueAsObject.GetString(nameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlNamedVector4 { Name = name };
                if (descriptionGetter != null) xml.Description = descriptionGetter(pair.ValueAsObject);
                targetXmlList.Add(xml);
            }
        }

        public void copyCharacterToPC() {
            copyCharacter(_character, "PC");
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
            _character = name;
            if (name == "PC") {
                IsPC = true;
                if (null == Character.KeyItems && (null != _allKeyitems)) {
                    Character.KeyItems = new UpdatableCollection<KeyItemVM>(_allKeyitems.Where(x => x.Match(_keyItemSearchText)));
                }
            } else {
                IsPC = false;
            }
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
                return _character;
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
            get {
                return _IsPC;
            }
            private set {
                _IsPC = value;
            }
        }

        public bool IsNotPC {
            get {
                return !IsPC;
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

        public List<PerkGroupVM> PerkGroups { get; private set; }

        string _perkSearchText = "";
        public string PerkSearchText {
            get { return _perkSearchText; }
            set {
                if (_perkSearchText == value) {
                    return;
                }
                _perkSearchText = value;
                foreach (var group in PerkGroups) group.Update();
            }
        }

        /// <summary>
        /// Returns the perk with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this perk.
        /// That way, anytime the perk is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public PerkVM GetPerk(string name, [CallerMemberName] string propertyName = null) {
            var perk = _allPerks.First(x => x.Name == name);
            perk.GameVMProperties.Add(propertyName);
            return perk;
        }

        // Whenever a PerkVM, FlagVM, or StatusVM is modified, it notifies GameVM with those functions so that it updates its dependent properties. 
        // See also GetPerk, GetFlag, and GetStatus.
        public void OnPerkChanged(string name) {
            foreach (var prop in _allPerks.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnPerkAddedOrRemoved(string name, bool isOwned) {
            // Grants/removes the player the appropriate bonuses when a perk is added or removed.
            // We do not add stats however since the user can already change them easily.
            switch (name) {
                default:
                    break;
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
                Character.KeyItems.Update();
            }
        }
        /// <summary>
        /// Returns the key item with the specified name (even if not owned by the player) AND registers a dependency between the caller property and this key item.
        /// That way, anytime the key item is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public KeyItemVM GetKeyItem(string name, [CallerMemberName] string propertyName = null) {
            var keyItem = _allKeyitems.First(x => x.Name == name);
            keyItem.GameVMProperties.Add(propertyName);
            return keyItem;
        }

        public void OnKeyItemChanged(string name) {
            foreach (var prop in _allKeyitems.First(x => x.Name == name).GameVMProperties) OnPropertyChanged(prop);
        }

        public void OnKeyItemAddedOrRemoved(string name, bool isOwned) {
            switch (name) {
                default:
                    break;
            }
        }
    }
}
