using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using TiTsEd.Model;


namespace TiTsEd.ViewModel {
    public sealed class ItemContainerVM {
        readonly ObservableCollection<ItemSlotVM> _slots = new ObservableCollection<ItemSlotVM>();
        readonly CharacterVM _character;

        public ItemContainerVM(CharacterVM character, string name, ItemCategories categories) {
            Name = name;
            _character = character;
            Categories = categories;
        }

        public string Name {
            get;
            private set;
        }

        public ItemCategories Categories {
            get;
            private set;
        }

        public ObservableCollection<ItemSlotVM> Slots {
            get { return _slots; }
        }

        public void Add(AmfObject obj) {
            _slots.Add(new ItemSlotVM(_character, obj, Categories));
        }

        public void Clear() {
            _slots.Clear();
        }

        public override string ToString() {
            return Name;
        }
    }

    public sealed class ItemSlotVM : ObjectVM {
        private readonly CharacterVM _character;
        private XmlItem _xml;

        public ItemSlotVM(CharacterVM character, AmfObject obj, ItemCategories categories)
            : base(obj) {
            Categories = categories;
            _character = character;

            //find the xml definition for this slot type
            var id = GetString("classInstance");
            _xml = XmlItem.Empty;
            foreach (XmlItem item in XmlData.Current.Items) {
                if (item.ID == id) {
                    _xml = item;
                    break;
                }
            }

            //create our groups
            var groups = new List<ItemGroupVM>();
            var enumNames = Enum.GetNames(typeof(ItemCategories));
            Array.Sort<String>(enumNames);

            //check enum support
            foreach (string ename in enumNames) {
                var etype = Enum.Parse(typeof(ItemCategories), ename);
                int eint = (int)etype;
                if (((int)categories & eint) == eint) {
                    //create the group for this supported type
                    ItemGroupVM vm = new ItemGroupVM(ename, this);
                    if (vm.Items.Count > 0) {
                        groups.Add(vm);
                    }
                }
            }

            AllGroups = new UpdatableCollection<ItemGroupVM>(groups);
        }

        public ItemCategories Categories {
            get;
            private set;
        }

        public UpdatableCollection<ItemGroupVM> AllGroups {
            get;
            private set;
        }

        public int MaxQuantity {
            get {
                return _xml.Stack;
            }
        }

        public int Quantity {
            get { return GetInt("quantity"); }
            set {
                SetValue("quantity", value);

                // Fix type
                if (value == 0) TypeID = "NOTHING!";

                // Property change
                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string TypeID {
            get {
                return _xml.ID;
            }
            set {
                var oldTypeId = _xml.ID;
                _xml = XmlItem.Empty;
                foreach (XmlItem item in XmlData.Current.Items) {
                    if (item.ID == value) {
                        _xml = item;
                        break;
                    }
                }

                //update data
                SetValue("classInstance", _xml.ID);
                SetValue("shortName", _xml.Name);
                SetValue("version", 1);

                if (Quantity > MaxQuantity) {
                    Quantity = MaxQuantity;
                }
                if (_xml != XmlItem.Empty && Quantity < 1) {
                    Quantity = 1;
                }

                //update all items for is selected
                foreach (var group in AllGroups) {
                    foreach (var item in group.Items) {
                        item.NotifyIsSelectedChanged();
                    }
                }

                OnPropertyChanged("TypeDescription");
                OnPropertyChanged("QuantityDescription");
                OnPropertyChanged("MaxQuantity");

                //check if we have to reflow the inventory
                if (oldTypeId == XmlItem.Empty.ID
                || _xml.ID == XmlItem.Empty.ID) {
                    _character.CleanupInventory();
                    _character.UpdateInventory();
                }
            }
        }

        public string TypeDescription {
            get {
                if (_xml == XmlItem.Empty) {
                    return _xml.Name;
                }
                return _xml.LongName;
            }
        }

        public string QuantityDescription {
            get {
                return GetInt("quantity").ToString();
            }
        }
    }

    /// <summary>
    /// View VM for an item category
    /// </summary>
    public sealed class ItemGroupVM {
        public ItemGroupVM(string name, ItemSlotVM slot) {
            Name = name;

            var items = new List<ItemVM>();
            foreach (XmlItem xml in XmlData.Current.Items) {
                //xmlitem.Type
                if (xml.Type == name) {
                    items.Add(new ItemVM(slot, xml));
                }
            }

            //sort items
            items.Sort((a, b) => {
                return a.Name.CompareTo(b.Name);
            });

            Items = new UpdatableCollection<ItemVM>(items);
        }

        public string Name {
            get;
            private set;
        }

        public UpdatableCollection<ItemVM> Items {
            get;
            private set;
        }

        public override string ToString() {
            return Name;
        }

    }

    /// <summary>
    /// View VM for a XmlItem
    /// </summary>
    public sealed class ItemVM : BindableBase {
        readonly ItemSlotVM _slot;
        readonly XmlItem _xml;

        public ItemVM(ItemSlotVM slot, XmlItem item) {
            _slot = slot;
            _xml = item;
        }

        public string ID {
            get { return _xml.ID; }
        }

        public string Name {
            get {
                if (ToolTip == null) {
                    return _xml.LongName;
                }
                return _xml.LongName + "\u202F*";
            }
        }

        public string ToolTip {
            get { return null; }
        }

        public bool IsSelected {
            get { return _slot.TypeID == _xml.ID; }
            set {
                if (!value) return;
                _slot.TypeID = _xml.ID;
            }
        }

        public void NotifyIsSelectedChanged() {
            OnPropertyChanged("IsSelected");
        }


        public override string ToString() {
            return Name;
        }
    }
}
