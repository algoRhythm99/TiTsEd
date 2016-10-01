using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ItemCategories _categories;
        private XmlItem _xml;

        public ItemSlotVM(CharacterVM character, AmfObject obj, ItemCategories categories)
            : base(obj) {
            Categories = categories;
            _character = character;
            _categories = categories;

            //find the xml definition for this slot type
            var id = GetString("classInstance");
            _xml = XmlItem.Empty;
            foreach (XmlItem item in XmlData.Current.Items) {
                if (item.ID == id) {
                    _xml = item;
                    break;
                }
            }

            UpdateItemGroups();
        }

        public void UpdateItemGroups() {
            //create our groups
            var groups = new List<ItemGroupVM>();
            var enumNames = Enum.GetNames(typeof(ItemCategories));
            Array.Sort<String>(enumNames);

            //check enum support
            foreach (string ename in enumNames) {
                var etype = Enum.Parse(typeof(ItemCategories), ename);
                int eint = (int)etype;
                if (((int)_categories & eint) == eint) {
                    //create the group for this supported type
                    ItemGroupVM vm = new ItemGroupVM(ename, this);
                    if (vm.Items.Count > 0) {
                        groups.Add(vm);
                    }
                }
            }

            AllGroups = new UpdatableCollection<ItemGroupVM>(groups);
            OnPropertyChanged("AllGroups");
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
        public const int MIN_ITEM_TEXT_SEARCH_LENGTH = 2; //should always be at least 1
        private string _Name;
        public ItemGroupVM(string name, ItemSlotVM slot) {
            Name = name;

            var items = new List<ItemVM>();
            var searchText = "";
            if (VM.Instance.Game != null) {
                searchText = VM.Instance.Game.ItemSearchText;
            }
            foreach (XmlItem xml in XmlData.Current.Items) {
                if (searchText.Length >= MIN_ITEM_TEXT_SEARCH_LENGTH
                    && !xml.Name.ToLower().Contains(searchText)
                    && !xml.ID.ToLower().Contains(searchText)) {
                    continue;
                }
                if (xml.Type == name) {
                    items.Add(new ItemVM(slot, xml));
                }
            }

            //sort items
            if (items.Count > 1) {
                items.Sort();
            }

            Items = new UpdatableCollection<ItemVM>(items);
        }

        public string Name {
            get { return _Name; }
            private set { _Name = value; }
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
    public sealed class ItemVM : BindableBase, IComparable {
        readonly ItemSlotVM _slot;
        readonly XmlItem _xml;
        private string _Name;

        public ItemVM(ItemSlotVM slot, XmlItem item) {
            _slot = slot;
            _xml = item;
            SetName();
        }

        public string ID {
            get { return _xml.ID; }
        }

        public new string Name {
            get {
                SetName();
                return _Name;
            }
            private set {
                _Name = value;
            }
        }

        private void SetName() {
            if (null == _Name) {
                if (null == ToolTip) {
                    _Name = _xml.LongName;
                } else {
                    _Name = _xml.LongName + "\u202F*";
                }
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

        int IComparable.CompareTo(object obj) {
            ItemVM bObj = (ItemVM) obj;
            if (this != bObj) {
                string a = Name;
                string b = bObj.Name;
                return a.CompareTo(b);
            }
            return 0;
        }

    }
}
