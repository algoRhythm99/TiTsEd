using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public sealed partial class GameVM : ObjectVM
    {
        private GeneralObjectVM _flags;
        private string _characterName = "PC";
        private string[] _characters;
        public readonly List<PerkVM> AllPerks = new List<PerkVM>();
        public readonly List<KeyItemVM> AllKeyItems = new List<KeyItemVM>();
        public readonly List<StatusEffectVM> AllStatusEffects = new List<StatusEffectVM>();
        public readonly SortedDictionary<string,FlagVM> AllFlags = new SortedDictionary<string,FlagVM>();
        public readonly SortedDictionary<string, CodexEntryVM> AllCodexEntries = new SortedDictionary<string, CodexEntryVM>();
        public readonly BindingList<AmfObjectVM> _RawStateVM = new BindingList<AmfObjectVM>();

        public GameVM(AmfFile file, GameVM previousVM)
            : base(file)
        {
            SetCharacterOptions();
            CharacterSelection = "PC";

            var shittyShips = GetObj("shittyShips") ?? new AmfObject(AmfTypes.Array);
            Ships = new ShipArrayVM(this, shittyShips);

            var flagsObject = FlagsObject;
            _flags = new GeneralObjectVM(flagsObject);

            // Flags
            var flagNames = AllFlags.Keys.ToList();
            foreach (var flagName in flagNames)
            {
                XmlEnum data = new XmlEnum();
                data.Name = flagName;
                AllFlags[flagName] = new FlagVM(this, ref flagsObject, data);
            }

            foreach (var xmlFlag in XmlData.Current.Flags)
            {
                if (!AllFlags.ContainsKey(xmlFlag.Name))
                {
                    AllFlags[xmlFlag.Name] = new FlagVM(this, ref flagsObject, xmlFlag);
                }
            }

            foreach (var flag in flagsObject)
            {
                string flagName = flag.ToString();
                if (!AllFlags.ContainsKey(flagName))
                {
                    XmlEnum data = new XmlEnum();
                    data.Name = flagName;
                    AllFlags[flagName] = new FlagVM(this, ref flagsObject, data);
                }
            }

            if (null != previousVM)
            {
                foreach (var flag in previousVM.AllFlags)
                {
                    string flagName = flag.Key.ToString();
                    if (!AllFlags.ContainsKey(flagName))
                    {
                        XmlEnum data = new XmlEnum();
                        data.Name = flagName;
                        AllFlags[flagName] = new FlagVM(this, ref flagsObject, data);
                    }
                }
            }

            Flags = new UpdatableCollection<FlagVM>(AllFlags.Values.ToList().Where(x => x.Match(SearchText)));
            OnPropertyChanged("Flags");

            // Codex
            var codexEntries = AllCodexEntries.Keys.ToList();
            foreach (var codexName in codexEntries)
            {
                XmlCodexEntry data = new XmlCodexEntry();
                data.Name = codexName;
                AllCodexEntries[codexName] = new CodexEntryVM(this, data);
            }

            foreach (var xmlCodex in XmlData.Current.CodexEntries)
            {
                if (!AllCodexEntries.ContainsKey(xmlCodex.Name))
                {
                    AllCodexEntries[xmlCodex.Name] = new CodexEntryVM(this, xmlCodex);
                }
            }

            foreach (var codexEntry in CodexUnlockedEntriesObj)
            {
                var codexName = codexEntry.Value?.ToString();
                if (!String.IsNullOrEmpty(codexName))
                {
                    if (!AllCodexEntries.ContainsKey(codexName))
                    {
                        XmlCodexEntry data = new XmlCodexEntry(codexName);
                        AllCodexEntries[codexName] = new CodexEntryVM(this, data);
                    }
                }
                else
                {
                    Logger.Debug(CodexEntryVM.CodexNameFromEntry(codexEntry, "unlockedCodexEntries"));
                }
            }

            foreach (var codexEntry in CodexViewedEntriesObj)
            {
                var codexName = codexEntry.Value?.ToString();
                if (!String.IsNullOrEmpty(codexName))
                {
                    if (!AllCodexEntries.ContainsKey(codexName))
                    {
                        XmlCodexEntry data = new XmlCodexEntry(codexName);
                        AllCodexEntries[codexName] = new CodexEntryVM(this, data);
                    }
                }
                else
                {
                    Logger.Debug(CodexEntryVM.CodexNameFromEntry(codexEntry, "viewedCodexEntries"));
                }
            }

            if (null != previousVM)
            {
                foreach (var codexEntry in previousVM.AllCodexEntries)
                {
                    var codexName = codexEntry.Value?.ToString();
                    if (!String.IsNullOrEmpty(codexName))
                    {
                        if (!AllCodexEntries.ContainsKey(codexName))
                        {
                            XmlCodexEntry data = new XmlCodexEntry(codexName);
                            AllCodexEntries[codexName] = new CodexEntryVM(this, data);
                        }
                    }
                    else
                    {
                        Logger.Debug(String.Format("codexEntry[{0}].Value is null??", codexEntry.Key));
                    }
                }
            }

            CodexEntries = new UpdatableCollection<CodexEntryVM>(AllCodexEntries.Values.ToList().Where(x => x.Match(SearchText)));
            OnPropertyChanged("CodexEntries");
        }

        public ShipArrayVM Ships { get; private set; }

        public static void ImportUnknownStorageClassEntries(AmfObject items, IEnumerable<XmlStorageClass> xmlItems, IList<XmlStorageClass> targetXmlList = null, string nameProperty = "storageName", Func<AmfObject, string> descriptionGetter = null)
        {
            if (targetXmlList == null) targetXmlList = (IList<XmlStorageClass>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in items)
            {
                var itemObject = pair.ValueAsObject;
                var name = itemObject.GetString(nameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlStorageClass { Name = name };
                if (descriptionGetter != null)
                {
                    xml.Description = descriptionGetter(itemObject);
                }
                else
                {
                    xml.Description = itemObject.GetString("tooltip");
                }
                targetXmlList.Add(xml);
            }
        }

        public static void ImportUnknownItems(List<ItemContainerVM> containers, List<String> types)
        {
            var unknownItemGroup = XmlData.Current.ItemGroups.Last();

            foreach (var slot in containers.SelectMany(x => x.Slots))
            {
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
                if (slot.HasRandomProperties)
                {
                    var obj = slot.GetAmfObject();
                    foreach (var prop in obj)
                    {
                        string key = prop.Key.ToString();
                        string val = prop.Value.ToString();
                        string propType = "string";
                        switch (key)
                        {
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

        public void CopyCharacterToPC()
        {
            CopyCharacter(CharacterSelection, "PC");
        }

        //for later
        private void CopyCharacter(string src, string dst)
        {
            var chars = GetObj("characters");
            var srcChar = chars.GetObj(src);
            var dstChar = chars.GetObj(dst);

            foreach (var tag in XmlData.Current.General.CopyTags)
            {
                if (!srcChar.Contains(tag) || !dstChar.Contains(tag))
                {
                    continue;
                }
                var value = srcChar[tag];
                if (value != null && value.GetType() == typeof(AmfObject))
                {
                    dstChar[tag] = (srcChar[tag] as AmfObject).clone();
                }
                else
                {
                    dstChar[tag] = value;
                }
            }
        }

        private void setCharacter(string name)
        {
            Character = GetCharacter(name);
            _characterName = name;
            OnPropertyChanged("CharacterSelection");
            Character.UpdateAll( _characterName );
            OnPropertyChanged("IsPC");
            OnPropertyChanged("Character");
        }

        public CharacterVM GetCharacter(string name)
        {
            var tmpChar = GetObj("characters");
            tmpChar = tmpChar.GetObj(name);
            var tcChar = new CharacterVM(this, tmpChar, name);
            return tcChar;
        }

        public CharacterVM Character { get; private set; }

        public string[] CharacterOptions
        {
            get
            {
                SetCharacterOptions();
                return _characters;
            }
            private set
            {
                _characters = value;
            }
        }

        private void SetCharacterOptions()
        {
            if (null == _characters)
            {
                var tmpChar = GetObj("characters");
                List<String> characters = new List<string>();
                foreach (AmfPair pair in tmpChar)
                {
                    characters.Add(pair.Key.ToString());
                }
                if (characters.Count > 1)
                {
                    characters.Sort();
                }
                CharacterOptions = characters.ToArray();
            }
        }

        public string CharacterSelection
        {
            get
            {
                return _characterName;
            }
            set
            {
                setCharacter(value);
                //update everything!
                OnPropertyChanged(null);
            }
        }


        public string Email
        {
            get
            {
                if (EmailEnabled)
                {
                    return _flags.GetString("PC_EMAIL_ADDRESS");
                }
                return "N/A";
            }
            set
            {
                if (EmailEnabled)
                {
                    var old = Email;
                    //ToAddressCache
                    //ContentCache

                    _flags.SetValue("PC_EMAIL_ADDRESS", value);

                    //Update messages in the mail system!
                    foreach (AmfPair pair in GetObj("mailSystem"))
                    {
                        ObjectVM vm = new GeneralObjectVM(pair.ValueAsObject);
                        if (vm.HasValue("ToAddressCache"))
                        {
                            var str = vm.GetString("ToAddressCache");
                            vm.SetValue("ToAddressCache", str.Replace(old, value));
                        }
                        if (vm.HasValue("ContentCache"))
                        {
                            var str = vm.GetString("ContentCache");
                            vm.SetValue("ContentCache", str.Replace(old, value));
                        }
                    }
                }
            }
        }

        public bool EmailEnabled
        {
            get
            {
                return IsPC && _flags.HasValue("PC_EMAIL_ADDRESS");
            }
        }

        public new string Name
        {
            get { return ( null != Character ) ? Character.Name : null; }
            set
            {
                var oldName = Name + " Steele";
                Character.Name = value;

                if (IsPC)
                {
                    var newName = value + " Steele";
                    //Update messages in the mail system!
                    foreach (AmfPair pair in GetObj("mailSystem"))
                    {
                        ObjectVM vm = new GeneralObjectVM(pair.ValueAsObject);
                        if (vm.HasValue("ToCache"))
                        {
                            var str = vm.GetString("ToCache");
                            vm.SetValue("ToCache", str.Replace(oldName, newName));
                        }
                        if (vm.HasValue("ContentCache"))
                        {
                            var str = vm.GetString("ContentCache");
                            vm.SetValue("ContentCache", str.Replace(oldName, newName));
                        }
                    }
                }
            }
        }

        public string Notes
        {
            get { return GetString("saveNotes"); }
            set { SetValue("saveNotes", String.IsNullOrWhiteSpace(value) ? "No notes available." : value); }
        }

        public int Days
        {
            get { return GetInt("daysPassed"); }
            set { SetValue("daysPassed", value); }
        }

        public int Hours
        {
            get { return GetInt("currentHours"); }
            set { SetValue("currentHours", value); }
        }

        public int Minutes
        {
            get { return GetInt("currentMinutes"); }
            set { SetValue("currentMinutes", value); }
        }

        public bool IsPC
        {
            get
            {
                return "PC".Equals(CharacterSelection);
            }
        }

        public bool IsNotPC
        {
            get
            {
                return !"PC".Equals(CharacterSelection);
            }
        }


        public int PCUpbringing
        {
            get
            {
                return _flags.GetInt("PC_UPBRINGING");
            }
            set
            {
                _flags.SetValue("PC_UPBRINGING", value);
                OnFlagChanged("PC_UPBRINGING");
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
                OnPropertyChanged("Flags");
                CodexEntries.Update();
                OnPropertyChanged("CodexEntries");
            }
        }

        public AmfObject FlagsObject
        {
            get { return GetObj("flags"); }
        }

        public UpdatableCollection<FlagVM> Flags { get; private set; }

        /// <summary>
        /// Returns the flag with the specified name (even if not set in the save) AND registers a dependency between the caller property and this flag.
        /// That way, anytime the flag value is changed, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public FlagVM GetFlag(string name, string propertyName = null)
        {
            FlagVM flag = null;
            if (AllFlags.ContainsKey(name))
            {
                flag = AllFlags[name];
                flag.GameVMProperties.Add(propertyName);
            }
            return flag;
        }

        public void OnFlagChanged(string name)
        {
            if (AllFlags.ContainsKey(name))
            {
                foreach (var prop in AllFlags[name].GameVMProperties)
                {
                    OnPropertyChanged(prop);
                }
            }
            OnPropertyChanged("Flags");
            OnSavePropertyChanged("RawStateVM");
        }

        public void RemoveFlag(FlagVM flag)
        {
            if (null != flag)
            {
                var flagName = flag.Name;
                // remove flag
                if (null != FlagsObject && !String.IsNullOrEmpty(flagName))
                {
                    if (null != FlagsObject[flagName])
                    {
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

        public AmfObject CodexUnlockedEntriesObj
        {
            get
            {
                return GetObj(CodexProperties.UNLOCKEDCODEXENTRIES) ?? new AmfObject(AmfTypes.Array);
            }
        }

        public AmfObject CodexViewedEntriesObj
        {
            get
            {
                return GetObj(CodexProperties.VIEWEDCODEXENTRIES) ?? new AmfObject(AmfTypes.Array);
            }
        }

        public UpdatableCollection<CodexEntryVM> CodexEntries
        {
            get;
            private set;
        }

        public void OnCodexChanged(string name)
        {
            if (AllCodexEntries.ContainsKey(name))
            {
                foreach (var prop in AllCodexEntries[name].GameVMProperties)
                {
                    OnPropertyChanged(prop);
                }
            }
            OnPropertyChanged("CodexEntries");
            OnPropertyChanged("CodexViewedEntriesObj");
            OnPropertyChanged("CodexUnlockedEntriesObj");
            OnSavePropertyChanged("RawStateVM");
        }

        public AmfFile SaveFile
        {
            get
            {
                return (AmfFile)GetAmfObject();
            }
        }

        public BindingList<AmfObjectVM> RawStateVM
        {
            get
            {
                _RawStateVM.Clear();
                _RawStateVM.Add(new AmfObjectVM(GetAmfObject(), SaveFile.Name, SaveFile.FilePath));
                return _RawStateVM;
            }
        }


        public void AllCodexUnknown()
        {
            foreach (var codexPair in AllCodexEntries)
            {
                codexPair.Value.IsUnlocked = false;
                codexPair.Value.IsViewed = false;
            }
        }


        public void AllCodexUnlocked()
        {
            foreach (var codexPair in AllCodexEntries)
            {
                codexPair.Value.IsUnlocked = true;
            }
        }


        public void AllCodexViewed()
        {
            foreach (var codexPair in AllCodexEntries)
            {
                codexPair.Value.IsViewed = true;
            }
        }


#region CommandHandlers

        private RelayCommand<FlagVM> _deleteFlagCommand;
        public RelayCommand<FlagVM> DeleteFlagCommand
        {
            get { return _deleteFlagCommand ?? (_deleteFlagCommand = new RelayCommand<FlagVM>(d => RemoveFlag(d))); }
        }
    }
#endregion
}
