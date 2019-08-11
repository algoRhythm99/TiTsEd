using System;
using System.ComponentModel;
using System.Linq;

using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class StatusGroupVM : BindableBase {
        private readonly CreatureVM Creature;

        public StatusGroupVM(CreatureVM creature, string name, StatusEffectVM[] statusEffects) {
            Creature = creature;
            Name = name;
            StatusEffects = new UpdatableCollection<StatusEffectVM>(statusEffects.Where(x => x.Match(Creature.Game.SearchText)));
        }

        public new string Name {
            get;
            private set;
        }

        public UpdatableCollection<StatusEffectVM> StatusEffects {
            get;
            private set;
        }

        public Visibility Visibility {
            get { return StatusEffects.Count != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void Update() {
            StatusEffects.Update();
            OnPropertyChanged("Visibility");
        }
    }

    public sealed class StatusEffectVM : StorageClassVM {
        public StatusEffectVM(CreatureVM creature, AmfObject statuses, XmlStorageClass xml)
            : base(creature, statuses, xml)
        {
        }

        public override Visibility MinutesLeftVisibility {
            get { return Visibility.Visible; }
        }

        public override Visibility IsHiddenVisibility {
            get { return Visibility.Visible; }
        }

        public override Visibility IconNameVisibility {
            get { return Visibility.Visible; }
        }

        public override Visibility IconShadeVisibility {
            get { return Visibility.Visible; }
        }

        public override Visibility IsCombatOnlyVisibility {
            get { return Visibility.Visible; }
        }

        public override AmfObject GetItems() {
            return Creature.StatusEffectsArray;
        }

        public override AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        protected override void InitializeObject(AmfObject obj) {
            base.InitializeObject(obj);
            obj["hidden"] = _xml.IsHidden ?? false;
        }

        protected override void NotifyGameVM() {
            Creature.OnStatusChanged(Name);
        }

        protected override void OnIsOwnedChanged() {
            Creature.OnStatusAddedOrRemoved(Name, IsOwned);
        }
    }
}
