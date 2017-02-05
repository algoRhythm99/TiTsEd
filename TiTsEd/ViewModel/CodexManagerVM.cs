using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public struct CodexProperties
    {
        public const string UNLOCKEDCODEXENTRIES = "unlockedCodexEntries";
        public const string VIEWEDCODEXENTRIES = "viewedCodexEntries";
    }


    public class CodexEntryVM : BindableBase
    {
        readonly string _name;

        public GameVM Game { get; private set; }

        public CodexEntryVM(GameVM game, XmlCodexEntry data)
        {
            _name = data.Name;
            Game = game;
            GameVMProperties = new HashSet<string>();
        }


        public new string Name
        {
            get { return _name; }
        }


        public bool Match(string str)
        {
            if (String.IsNullOrEmpty(str)) return true;

            int index = (Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0) return true;

            return false;
        }

        protected override void OnSavePropertyChanged(string propertyName = null)
        {
            if (!String.IsNullOrEmpty(propertyName))
            {
                base.OnSavePropertyChanged(propertyName);
            }
            Game.OnCodexChanged(Name);
        }

        public HashSet<string> GameVMProperties
        {
            get;
            private set;
        }

        public bool IsViewed
        {
            get
            {
                return Game.CodexViewedEntriesObj.ContainsByValue(Name);
            }
            set
            {
                bool isViewed = IsViewed;
                if (value)
                {
                    if (!isViewed)
                    {
                        Game.CodexViewedEntriesObj.Push(Name);
                        Game.OnCodexChanged(Name);
                    }
                }
                else if (IsViewed)
                {
                    Game.CodexViewedEntriesObj.RemoveByValue(Name);
                    Game.OnCodexChanged(Name);
                }
                OnPropertyChanged("IsViewed");
                OnPropertyChanged("IsUnknown");
                OnPropertyChanged("DisplayFontWeight");
            }
        }

        public bool IsUnlocked
        {
            get
            {
                return Game.CodexUnlockedEntriesObj.ContainsByValue(Name);
            }
            set
            {
                bool isUnlocked = IsUnlocked;
                if (value)
                {
                    if (!isUnlocked)
                    {
                        Game.CodexUnlockedEntriesObj.Push(Name);
                        Game.OnCodexChanged(Name);
                    }
                }
                else if (isUnlocked)
                {
                    Game.CodexUnlockedEntriesObj.RemoveByValue(Name);
                    Game.OnCodexChanged(Name);
                }
                OnPropertyChanged("IsUnlocked");
                OnPropertyChanged("IsUnknown");
                OnPropertyChanged("DisplayFontWeight");
            }
        }

        public bool IsUnknown
        {
            get
            {
                return (!IsViewed && !IsUnlocked);
            }
            set
            {
                bool isUnlocked = IsUnlocked;
                if (value)
                {
                    IsViewed = false;
                    IsUnlocked = false;
                }
                else if (!isUnlocked)
                {
                    IsUnlocked = true;
                }
                OnPropertyChanged("IsUnknown");
                OnPropertyChanged("IsViewed");
                OnPropertyChanged("IsUnlocked");
                OnPropertyChanged("DisplayFontWeight");
            }
        }

        public FontWeight DisplayFontWeight
        {
            get
            {
                if (IsUnlocked && !IsViewed) return FontWeights.SemiBold;
                if (IsViewed) return FontWeights.Bold;
                return FontWeights.Normal;
            }
        }
    }

}
