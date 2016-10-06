using System;
using System.Collections;
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
        private ItemCategories _categories;
        private XmlItem _xml;

        public ItemSlotVM(CharacterVM character, AmfObject obj, ItemCategories categories)
            : base(obj) {
            Categories = categories;
            _character = character;
            _categories = categories;

            //find the xml definition for this slot type
            var className = GetString("classInstance");
            var variant = GetString("variant");
            setXml(getXmlItemForSlot(className, variant));

            UpdateItemGroups();
        }

        public void setXml(XmlItem xmlItem) {
            _xml = xmlItem;
        }

        public static XmlItem getXmlItemForSlot(string className, string variant) {
            XmlItem foundItem = XmlItem.Empty;
            foreach (XmlItem item in XmlData.Current.Items) {
                bool isItem = (item.ID == className);
                if (null != item.Variant) {
                    if (null != variant) {
                        isItem = isItem && (variant == item.Variant);
                    }
                }
                if (isItem) {
                    foundItem = item;
                    break;
                }
            }
            return foundItem;
        }

        public void UpdateFromXmlItem(XmlItem xmlItem) {
            var oldTypeId = _xml.ID;
            setXml(xmlItem);
            Name = xmlItem.Name;
            TypeID = xmlItem.ID;
            if (xmlItem != XmlItem.Empty) {
                SetValue("version", 1);
                if (null != xmlItem.LongName && xmlItem.LongName.Length > 0) {
                    LongName = xmlItem.LongName;
                }
                if (null != xmlItem.Tooltip && xmlItem.Tooltip.Length > 0) {
                    Tooltip = xmlItem.Tooltip;
                }
                if (null != xmlItem.Variant && xmlItem.Variant.Length > 0) {
                    Variant = xmlItem.Variant;
                }
                Quantity = xmlItem.Stack;
            }

            //update all items for is selected
            foreach (var group in AllGroups) {
                foreach (var item in group.Items) {
                    item.NotifyIsSelectedChanged();
                }
            }

            OnPropertyChanged("DisplayName");
            OnPropertyChanged("QuantityDescription");
            OnPropertyChanged("MaxQuantity");

            //check if we have to reflow the inventory
            if (oldTypeId == XmlItem.Empty.ID
            || xmlItem.ID == XmlItem.Empty.ID) {
                _character.CleanupInventory();
                _character.UpdateInventory();
            }
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
                // Fix type
                SetValue("quantity", value);
                if (value == 0) {
                    UpdateFromXmlItem(XmlItem.Empty);
                }
                // Property change
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("QuantityDescription");
            }
        }

        public string TypeID {
            get {
                return GetString("classInstance");
            }
            set {
                SetValue("classInstance", value);
            }
        }

        public string QuantityDescription {
            get {
                return GetInt("quantity").ToString();
            }
        }

        public new string Name {
            get {
                return GetString("shortName");
            }
            set {
                SetValue("shortName", value);
            }
        }

        public string LongName { get; set; }

        public string Tooltip { get; set; }

        public string Variant { get; set; }

        public string DisplayName {
            get {
                var xmlItem = getXmlItemForSlot(TypeID, Variant);
                return XmlItem.GetDisplayName(xmlItem, TypeID);
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

        public ItemVM(ItemSlotVM slot, XmlItem item) {
            _slot = slot;
            _xml = item;
        }

        public string ID {
            get { return _xml.ID; }
        }

        public new string Name {
            get { return _xml.Name; }
        }

        public string LongName {
            get { return _xml.LongName; }
        }

        public string DisplayName {
            get { return _xml.DisplayName; }
        }

        public string ToolTip {
            get { return _xml.Tooltip; }
        }

        public string Variant {
            get { return _xml.Variant; }
        }

        public bool IsSelected {
            get {
                bool _isSelected = (_slot.TypeID == ID);
                _isSelected = _isSelected && (_slot.Name == Name);
                if (null != Variant) {
                    _isSelected = _isSelected && (_slot.Variant == Variant);
                }
                return _isSelected;
            }
            set {
                if (!value) return;
                _slot.UpdateFromXmlItem(_xml);
            }
        }

        public void NotifyIsSelectedChanged() {
            OnPropertyChanged("IsSelected");
        }


        public override string ToString() {
            return Name;
        }

        int IComparable.CompareTo(object obj) {
            ItemVM bObj = (ItemVM)obj;
            if (this != bObj) {
                string a = DisplayName;
                string b = bObj.DisplayName;
                int result = a.CompareTo(b);
                if (0 == result && null != Variant && null != bObj.Variant) {
                    result = Variant.CompareTo(bObj.Variant);
                }
                return result;
            }
            return 0;
        }

    }
}
