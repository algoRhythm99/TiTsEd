using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class PerkGroupVM : BindableBase {
        private readonly CharacterVM Character;

        public PerkGroupVM(CharacterVM character, string name, PerkVM[] perks) {
            Character = character;
            Name = name;
            Perks = new UpdatableCollection<PerkVM>(perks.Where(x => x.Match(Character.Game.PerkSearchText)));
        }

        public new string Name {
            get;
            private set;
        }

        public UpdatableCollection<PerkVM> Perks {
            get;
            private set;
        }

        public Visibility Visibility {
            get { return Perks.Count != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void Update() {
            Perks.Update();
            OnPropertyChanged("Visibility");
        }
    }

    public sealed class PerkVM : StorageClassVM {
        public PerkVM(CharacterVM character, AmfObject perksArray, XmlStorageClass xml)
            : base(character, perksArray, xml) {
        }

        public override AmfObject GetItems() {
            return Character.PerksArray;
        }

        public override AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        protected override void NotifyGameVM() {
            Character.OnPerkChanged(Name);
        }

        protected override void OnIsOwnedChanged() {
            Character.OnPerkAddedOrRemoved(Name, IsOwned);
        }
    }
}
