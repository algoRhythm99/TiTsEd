using System;
using System.ComponentModel;
using System.Linq;

using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class PerkGroupVM : BindableBase {
        private readonly CreatureVM Creature;

        public PerkGroupVM(CreatureVM creature, string name, PerkVM[] perks) {
            Creature = creature;
            Name = name;
            Perks = new UpdatableCollection<PerkVM>(perks.Where(x => x.Match(Creature.Game.SearchText)));
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
        public PerkVM(CreatureVM creature, AmfObject perksArray, XmlStorageClass xml)
            : base(creature, perksArray, xml)
        {
        }

        public override AmfObject GetItems() {
            return Creature.PerksArray;
        }

        public override AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        protected override void NotifyGameVM() {
            Creature.OnPerkChanged(Name);
        }

        protected override void OnIsOwnedChanged() {
            Creature.OnPerkAddedOrRemoved(Name, IsOwned);
        }
    }
}
