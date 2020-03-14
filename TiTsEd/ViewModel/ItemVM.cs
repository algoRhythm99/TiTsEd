using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using TiTsEd.Common;
using TiTsEd.Model;


namespace TiTsEd.ViewModel {

    /// <summary>
    /// Defines a set of items for a character. This would be like the entire inventory. 
    /// </summary>
    public sealed class ItemContainerVM {
        readonly ObservableCollection<ItemSlotVM> _slots = new ObservableCollection<ItemSlotVM>();
        readonly CharacterVM _character;

        public ItemContainerVM(CharacterVM character, string name, List<String> itemTypes) {
            Name = name;
            _character = character;
            Types = itemTypes;
        }

        public string Name {
            get;
            private set;
        }

        public List<String> Types {
            get;
            private set;
        }

        public ObservableCollection<ItemSlotVM> Slots {
            get { return _slots; }
        }

        public void Add(AmfObject obj) {
            _slots.Add(new ItemSlotVM(_character, obj, Types));
        }

        public void Clear() {
            _slots.Clear();
        }

        public override string ToString() {
            return Name;
        }
    }

    /// <summary>
    /// This defines an individual item slot, say one entry in the inventory. This is the most important of the models.
    /// </summary>
    public sealed class ItemSlotVM : ObjectVM {
        private readonly CharacterVM _character;
        
        public ItemSlotVM(CharacterVM character, AmfObject obj, List<String> types)
            : base(obj) {
            Types = types;
            _character = character;

            Xml = getXmlItemForSlot();

            UpdateItemGroups();
        }

        public XmlItem Xml { get; private set; }

        private XmlItem getXmlItemForSlot() {
            var className = GetString("classInstance");
            //we will need to make this smarter eventually using the hasRandomProperties data
            //we should match all the data defined in the item fields against the actual item properties

            AmfObject tmp = _obj;

            XmlItem bestItem = XmlItem.Empty;
            int bestFieldMatch = -1;
            foreach (XmlItemGroup type in XmlData.Current.ItemGroups) {
                foreach (XmlItem item in type.Items) {
                    //class name must match, otherwise skip it
                    if (item.ID == className) {
                        //if there are any fields, try and match them
                        int fieldMatch = 0;
                        if (null != item.Fields) {
                        foreach (XmlObjectField field in item.Fields) {
                            //field.Name
                            if (HasValue(field.Name)) {
                                //do it by type
                                switch (field.Type.ToLower()) {
                                    case "bool":
                                        if (field.Value.StartsWith("t", true, null) == GetBool(field.Name)) {
                                            fieldMatch++;
                                        }
                                        break;
                                    case "int":
                                        if (int.Parse(field.Value) == GetInt(field.Name)) {
                                            fieldMatch++;
                                        }
                                        break;
                                    case "string":
                                        if (field.Value == GetString(field.Name)) {
                                            fieldMatch++;
                                        }
                                        break;
                                    default:
                                        //no match since we have no idea what type it is
                                        break;
                                }
                            }
                        }
                        }
                        if (fieldMatch > bestFieldMatch) {
                            bestFieldMatch = fieldMatch;
                            bestItem = item;
                        }
                    }
                }
            }
            return bestItem;
        }

        public void UpdateFromXmlItem(XmlItem xmlItem) {
            var oldTypeId = Xml.ID;
            Xml = xmlItem;
            Name = xmlItem.Name;
            TypeID = xmlItem.ID;
            if (xmlItem != XmlItem.Empty) {
                SetValue("version", 1);
                /* update the name as well */
                Name = xmlItem.Name;
                if (null != xmlItem.LongName && xmlItem.LongName.Length > 0) {
                    LongName = xmlItem.LongName;
                }
                if (null != xmlItem.Tooltip && xmlItem.Tooltip.Length > 0) {
                    Tooltip = xmlItem.Tooltip;
                }
                /* Update all extra fields with fields from the xml. */
                if (xmlItem.GetFieldValueAsBool("hasRandomProperties")) {
                    foreach (XmlObjectField field in xmlItem.Fields) {
                        switch (field.Type.ToLower()) {
                            case "bool":
                                SetValue(field.Name, field.Value.StartsWith("t", true, null));
                                break;
                            case "int":
                                SetValue(field.Name, int.Parse(field.Value));
                                break;
                            case "string":
                                SetValue(field.Name, field.Value);
                                break;
                            default:
                                //no match since we have no idea what type it is
                                break;
                        }
                    }
                }


                /* SetValue("quantity", value); */
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
            //var enumNames = Enum.GetNames(typeof(ItemCategories));
            var typeNames = Types;
            typeNames.Sort();

            //check enum support
            foreach (string typeName in typeNames) {
                ItemGroupVM vm = new ItemGroupVM(typeName, this);
                if (vm.Items.Count > 0) {
                    groups.Add(vm);
                }
            }

            AllGroups = new UpdatableCollection<ItemGroupVM>(groups);
            OnPropertyChanged("AllGroups");
        }

        public List<String> Types { get; private set; }

        public UpdatableCollection<ItemGroupVM> AllGroups { get; private set; }

        public int MaxQuantity {
            get {
                int i = GetInt("stackSize", 0);
                return (0 == i) ? Xml.Stack : i;
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

        public int Version {
            get { return GetInt("version"); }
            set { SetValue("version", value); }
        }

        public string TypeID {
            get {
                return GetString("classInstance");
            }
            set {
                SetValue("classInstance", value);
                OnPropertyChanged("DisplayName");
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
                OnPropertyChanged("DisplayName");
            }
        }

        public string LongName {
            get {
                return GetString("longName") ?? Xml.LongName;
            }
            set {
                SetValue("longName", value);
                OnPropertyChanged("DisplayName");
            }
        }

        public string Tooltip {
            get {
                return GetString("tooltip") ?? Xml.Tooltip;
            }
            set {
                SetValue("tooltip", value);
            }
        }

        public bool HasRandomProperties {
            get {
                return GetBool("hasRandomProperties");
            }
            set {
                SetValue("hasRandomProperties", value);
            }
        }

        public string DisplayName {
            get {
                return XmlItem.GetDisplayName(Xml, TypeID);
            }
        }
    }

    /// <summary>
    /// This defines a set of items.
    /// </summary>
    public sealed class ItemGroupVM
    {
        public const int MIN_ITEM_TEXT_SEARCH_LENGTH = 2; //should always be at least 1
        private string _Name;
        public ItemGroupVM(string name, ItemSlotVM slot) {
            Name = name;

            var items = new List<ItemVM>();
            var searchText = "";
            if (VM.Instance.Game != null) {
                searchText = VM.Instance.Game.SearchText;
            }

            //we made this easier now
            foreach (XmlItemGroup type in XmlData.Current.ItemGroups) {
                if (type.Name == name) {
                    //only add items from this item set into the group
                    foreach (XmlItem xml in type.Items) {
                        //skip items that do not match the search string
                        if (searchText.Length >= MIN_ITEM_TEXT_SEARCH_LENGTH
                            && !xml.Name.ContainsInsensitive(searchText)
                            && !xml.ID.ContainsInsensitive(searchText)) {
                            continue;
                        }
                        //add the item to the item group
                        items.Add(new ItemVM(slot, xml));
                    }
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

        public ItemVM(ItemSlotVM slot, XmlItem item) {
            _slot = slot;
            Xml = item;
        }

        public string ID {
            get { return Xml.ID; }
        }

        public new string Name {
            get { return Xml.Name; }
        }

        public string LongName {
            get { return Xml.LongName; }
        }

        public string DisplayName {
            get { return Xml.DisplayName; }
        }

        public string ToolTip {
            get { return Xml.Tooltip; }
        }

        public XmlItem Xml { get; private set; }

        public bool IsSelected {
            get {
                //probably not the best idea ever, but it works
                return _slot.Xml == Xml;
            }
            set {
                if (!value) return;
                _slot.UpdateFromXmlItem(Xml);
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
                /*
                if (0 == result && null != Variant && null != bObj.Variant) {
                    result = Variant.CompareTo(bObj.Variant);
                }*/
                return result;
            }
            return 0;
        }

        public string GetFieldValue(string fieldName) {
            return Xml.GetFieldValue(fieldName);
        }

        public int GetFieldValueAsInt(string fieldName) {
            return Xml.GetFieldValueAsInt(fieldName);
        }

    }
}
