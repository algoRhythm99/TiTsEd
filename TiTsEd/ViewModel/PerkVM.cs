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
        public PerkVM(GameVM game, AmfObject perksArray, XmlStorageClass xml)
            : base(game, perksArray, xml) {
        }

        public override AmfObject GetItems() {
            return _game.Character.PerksArray;
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

        protected override void NotifyGameVM() {
            _game.OnPerkChanged(Name);
        }

        protected override void OnIsOwnedChanged() {
            _game.OnPerkAddedOrRemoved(Name, IsOwned);
        }
    }
}
