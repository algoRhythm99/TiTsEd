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
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed partial class GameVM : ObjectVM {
        public GameVM(AmfFile file, GameVM previousVM)
            : base(file) {
                //copy("ANNO","PC");
                setCharacter("PC");
        }

        private void copy(string src, string dst) {
            var chars = GetObj("characters");
            var srcChar = chars.GetObj(src);
            var dstChar = chars.GetObj(dst);
            foreach(var pair in srcChar) {
                var sKey = pair.Key.ToString();
                if(sKey == "classInstance") {
                    continue;
                }
                if(pair.Value != null && dstChar.Contains(pair.Key)) {
                    if(pair.Value is string || pair.Value is String) {
                        continue;
                    }
                    if(pair.Value.GetType() == typeof(AmfObject)) {
                        dstChar[pair.Key] = pair.ValueAsObject.clone();
                    } else {
                        dstChar[pair.Key] = pair.Value;
                    }
                    //dstChar[pair.Key] = pair.Value;
                }
            }
            //dstChar["uniqueName"] = "PC";
        }

        public void setCharacter(string name) {
            var tmpChar = GetObj("characters");
            tmpChar = tmpChar.GetObj(name);
            Character = new CharacterVM(this, tmpChar);
            if(name == "PC") {
                IsPC = true;
            } else {
                IsPC = false;
            }
        }

        public CharacterVM Character { get; private set; }

        public string Name {
            get { return Character.Name; }
            set { Character.Name = value; }
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

        public bool IsPC { get; set; }

    }
}
