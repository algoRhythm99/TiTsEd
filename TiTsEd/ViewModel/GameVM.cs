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
                setCharacter("PC");
        }

        //for later
        private void copyCharacter(string src, string dst) {
            var chars = GetObj("characters");
            var srcChar = chars.GetObj(src);
            var dstChar = chars.GetObj(dst);

            foreach(var tag in XmlData.Current.General.CopyTags) {
                if(!srcChar.Contains(tag) || dstChar.Contains(tag)) {
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
