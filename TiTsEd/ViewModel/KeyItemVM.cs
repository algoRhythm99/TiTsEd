using System.ComponentModel;
using System.Linq;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class KeyItemGroupVM : BindableBase {
        private readonly CreatureVM Creature;

        public KeyItemGroupVM(CreatureVM creature, string name, KeyItemVM[] keyItems)
        {
            Creature = creature;
            Name = name;
            KeyItems = new UpdatableCollection<KeyItemVM>(keyItems.Where(x => x.Match(Creature.Game.SearchText)));
        }

        public new string Name {
            get;
            private set;
        }

        public UpdatableCollection<KeyItemVM> KeyItems {
            get;
            private set;
        }

        public Visibility Visibility {
            get { return KeyItems.Count != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void Update() {
            KeyItems.Update();
            OnPropertyChanged("Visibility");
        }
    }

    public sealed class KeyItemVM : StorageClassVM {

        public KeyItemVM(CreatureVM creature, AmfObject keyItems, XmlStorageClass xml)
            : base(creature, keyItems, xml)
        {
        }

        protected override void NotifyGameVM() {
            Creature.OnKeyItemChanged(Name);
        }

        public override AmfObject GetItems() {
            return Creature.KeyItemsArray;
        }

        public override AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        public override bool IsOwned {
            get { return GetObject() != null; }
            set {
                var items = GetItems();
                var pair = items.FirstOrDefault(x => IsObject(x.ValueAsObject));
                if ((pair != null) == value) return;

                if (value) {
                    var obj = new AmfObject(AmfTypes.Object);
                    InitializeObject(obj);
                    obj["value1"] = _xml.Value1;
                    obj["value2"] = _xml.Value2;
                    obj["value3"] = _xml.Value3;
                    obj["value4"] = _xml.Value4;
                    obj["tooltip"] = expandVars(_xml.Tooltip ?? _xml.Description);
                    items.Push(obj);
                } else {
                    items.Pop((int)pair.Key);
                }

                items.SortDensePart((x, y) => {
                    AmfObject xObj = (AmfObject)x;
                    AmfObject yObj = (AmfObject)y;
                    string xName = xObj.GetString("storageName");
                    string yName = yObj.GetString("storageName");
                    return xName.CompareTo(yName);
                });

                OnPropertyChanged("Value1");
                OnPropertyChanged("Value2");
                OnPropertyChanged("Value3");
                OnPropertyChanged("Value4");
                OnPropertyChanged("Comment");
                OnSavePropertyChanged();
                OnIsOwnedChanged();
            }
        }

        protected override void OnIsOwnedChanged() {
            Creature.OnKeyItemAddedOrRemoved(Name, IsOwned);
        }

    }
}
