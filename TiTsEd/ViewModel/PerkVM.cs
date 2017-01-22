using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class PerkGroupVM : BindableBase {
        readonly GameVM _game;

        public PerkGroupVM(GameVM game, string name, PerkVM[] perks) {
            _game = game;
            Name = name;
            Perks = new UpdatableCollection<PerkVM>(perks.Where(x => x.Match(_game.PerkSearchText)));
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
            return _character.PerksArray;
        }

        public override AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        protected override void NotifyGameVM() {
            _character.OnPerkChanged(Name);
        }

        protected override void OnIsOwnedChanged() {
            _character.OnPerkAddedOrRemoved(Name, IsOwned);
        }
    }
}
