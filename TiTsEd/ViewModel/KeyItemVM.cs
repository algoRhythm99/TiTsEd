using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class KeyItemVM : NamedVector4VM {
        public KeyItemVM(GameVM game, AmfObject keyItems, XmlNamedVector4 xml)
            : base(game, keyItems, xml) {
        }

        protected override void InitializeObject(AmfObject obj) {
            StorageClassVM.initialize(obj);
            obj["storageName"] = _xml.Name;
        }

        protected override bool IsObject(AmfObject obj) {
            var storageName = obj.GetString("storageName");
            StorageClassVM.fixup(obj);
            return storageName == _xml.Name;
        }

        protected override void NotifyGameVM() {
            _game.OnKeyItemChanged(_xml.Name);
        }

        public override bool IsOwned {
            get { return GetObject() != null; }
            set {
                var pair = _items.FirstOrDefault(x => IsObject(x.ValueAsObject));
                if ((pair != null) == value) return;

                if (value) {
                    var obj = new AmfObject(AmfTypes.Object);
                    InitializeObject(obj);
                    obj["tooltip"] = _xml.Description;
                    obj["value1"] = _xml.Value1;
                    obj["value2"] = _xml.Value2;
                    obj["value3"] = _xml.Value3;
                    obj["value4"] = _xml.Value4;
                    _items.Push(obj);
                } else {
                    _items.Pop((int)pair.Key);
                }

                _items.SortDensePart((x, y) => {
                    AmfObject xObj = (AmfObject)x;
                    AmfObject yObj = (AmfObject)y;
                    string xName = xObj.GetString("storageName");
                    string yName = yObj.GetString("storageName");
                    return xName.CompareTo(yName);
                });
                OnSavePropertyChanged();
                OnIsOwnedChanged();
            }
        }

        protected override void OnIsOwnedChanged() {
            _game.OnKeyItemAddedOrRemoved(_xml.Name, IsOwned);
        }

    }
}
