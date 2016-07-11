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

        public GameVM(AmfFile file, GameVM previousVM)
            : base(file) {
            setCharacter("PC");

            _flags = new GeneralObjectVM(GetObj("flags"));
        }

        public void copyCharacterToPC() {
            copyCharacter(_character, "PC");
        }

        //for later
        private void copyCharacter(string src, string dst) {
            var chars = GetObj("characters");
            var srcChar = chars.GetObj(src);
            var dstChar = chars.GetObj(dst);

            foreach(var tag in XmlData.Current.General.CopyTags) {
                if(!srcChar.Contains(tag) || !dstChar.Contains(tag)) {
                    continue;
                }
                var value = srcChar[tag];
                if(value != null && value.GetType() == typeof(AmfObject)) {
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
            if(name == "PC") {
                IsPC = true;
            } else {
                IsPC = false;
            }
        }

        public CharacterVM Character { get; private set; }

        public string[] CharacterOptions {
            get {
                var tmpChar = GetObj("characters");
                List<String> characters = new List<string>();
                foreach (AmfPair pair in tmpChar) {
                    characters.Add(pair.Key.ToString());
                }
                characters.Sort();
                return characters.ToArray();
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

        public string Name {
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

        public bool IsPC { get; private set; }

        public bool IsNotPC {
            get {
                return !IsPC;
            }
        }
    }
}
