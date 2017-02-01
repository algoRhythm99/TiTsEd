using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class StatusGroupVM : BindableBase {
        private readonly CharacterVM Character;

        public StatusGroupVM(CharacterVM character, string name, StatusEffectVM[] statusEffects) {
            Character = character;
            Name = name;
            StatusEffects = new UpdatableCollection<StatusEffectVM>(statusEffects.Where(x => x.Match(Character.Game.RawDataSearchText)));
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
        public StatusEffectVM(CharacterVM character, AmfObject statuses, XmlStorageClass xml)
            : base(character, statuses, xml) {
        }

        public override Visibility MinutesLeftVisibility {
            get { return Visibility.Visible; }
        }

        public override Visibility IsHiddenVisibility {
            get { return Visibility.Visible; }
        }

        public override AmfObject GetItems() {
            return Character.StatusEffectsArray;
        }

        public override AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        protected override void InitializeObject(AmfObject obj) {
            base.InitializeObject(obj);
            obj["hidden"] = _xml.IsHidden ?? false;
        }

        protected override void NotifyGameVM() {
            Character.OnStatusChanged(Name);
        }

        protected override void OnIsOwnedChanged() {
            Character.OnStatusAddedOrRemoved(Name, IsOwned);
        }
    }
}
