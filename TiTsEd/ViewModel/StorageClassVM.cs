using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public abstract class StorageClassVM : BindableBase {
        protected readonly CharacterVM _character;
        protected readonly AmfObject _items;
        protected readonly XmlStorageClass _xml;
        protected readonly HashSet<string> _gameProperties = new HashSet<string>();

        protected StorageClassVM(CharacterVM character, AmfObject items, XmlStorageClass xml) {
            _xml = xml;
            _character = character;
            _items = items;
        }

        public HashSet<string> GameVMProperties {
            get { return _gameProperties; }
        }

        public virtual AmfObject GetItems() {
            return _items;
        }

        public virtual AmfObject GetObject() {
            return GetItems().Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        public virtual bool IsOwned {
            get { return GetObject() != null; }
            set {
                var items = GetItems();
                var pair = items.FirstOrDefault(x => IsObject(x.ValueAsObject));
                if ((pair != null) == value) return;

                if (value) {
                    var obj = new AmfObject(AmfTypes.Object);
                    InitializeObject(obj);
                    items.Push(obj);
                } else {
                    items.Pop((int)pair.Key);
                }

                items.SortDensePart((x, y) =>
                {
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
                OnPropertyChanged("Description");
                OnPropertyChanged("IconName");
                OnPropertyChanged("IconShade");
                OnPropertyChanged("IsHidden");
                OnPropertyChanged("IsCombatOnly");
                OnPropertyChanged("MinutesLeft");
                OnPropertyChanged("Comment");
                OnPropertyChanged("Subtitle");
                OnPropertyChanged("Tooltip");
                OnSavePropertyChanged();
                OnIsOwnedChanged();
            }
        }

        protected virtual void OnIsOwnedChanged() {
        }

        public string Tooltip {
            get {
                return expandVars(_xml.Tooltip);
            }
        }

        public string Comment {
            get {
                return expandVars(_xml.Comment);
            }
        }

        public Visibility CommentVisibility {
            get { return String.IsNullOrEmpty(Comment) ? Visibility.Collapsed : (Subtitle != Comment) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string Subtitle {
            get {
                return !String.IsNullOrEmpty(Description) ? Description : Comment;
            }
        }

        public Visibility SubtitleVisibility {
            get { return String.IsNullOrEmpty(Subtitle) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public new string Name {
            get { return _xml.Name; }
            set { SetValue("storageName", value); }
        }

        public string Description {
            get {
                var tooltip = String.IsNullOrEmpty(GetString("tooltip")) ? null : GetString("tooltip");
                var source = tooltip ?? _xml.Description;
                return expandVars(source);
            }
            set { SetValue("tooltip", value); }
        }

        public string IconName {
            get { return GetString("iconName"); }
            set { SetValue("iconName", value); }
        }

        public virtual Visibility IconNameVisibility {
            get { return Visibility.Collapsed; }
        }

        public int IconShade {
            get { return GetInt("iconShade"); }
            set { SetDoubleOrIntValue("iconShade", value); }
        }

        public virtual Visibility IconShadeVisibility {
            get { return Visibility.Collapsed; }
        }

        public bool IsHidden {
            get { return GetBool("hidden"); }
            set { SetValue("hidden", value); }
        }

        public virtual Visibility IsHiddenVisibility {
            get { return Visibility.Collapsed; }
        }

        public bool IsCombatOnly {
            get { return GetBool("combatOnly"); }
            set { SetValue("combatOnly", value); }
        }

        public virtual Visibility IsCombatOnlyVisibility {
            get { return Visibility.Collapsed; }
        }

        public int MinutesLeft {
            get { return GetInt("minutesLeft"); }
            set { SetDoubleOrIntValue("minutesLeft", value); }
        }

        public virtual Visibility MinutesLeftVisibility {
            get { return Visibility.Collapsed; }
        }

        /// <summary>
        /// Gets the first value, or 0 if the thing is not owned.
        /// </summary>
        public double Value1 {
            get { return GetDouble("value1"); }
            set { SetDoubleOrIntValue("value1", value); }
        }

        /// <summary>
        /// Gets the second value, or 0 if the thing is not owned.
        /// </summary>
        public double Value2 {
            get { return GetDouble("value2"); }
            set { SetDoubleOrIntValue("value2", value); }
        }

        /// <summary>
        /// Gets the third value, or 0 if the thing is not owned.
        /// </summary>
        public double Value3 {
            get { return GetDouble("value3"); }
            set { SetDoubleOrIntValue("value3", value); }
        }

        /// <summary>
        /// Gets the fourth value, or 0 if the thing is not owned.
        /// </summary>
        public double Value4 {
            get { return GetDouble("value4"); }
            set { SetDoubleOrIntValue("value4", value); }
        }

        public string Type1 {
            get {
                if (String.IsNullOrEmpty(_xml.Type1)) return "Int";
                return _xml.Type1;
            }
        }

        public string Type2 {
            get {
                if (String.IsNullOrEmpty(_xml.Type2)) return "Int";
                return _xml.Type2;
            }
        }

        public string Type3 {
            get {
                if (String.IsNullOrEmpty(_xml.Type3)) return "Int";
                return _xml.Type3;
            }
        }

        public string Type4 {
            get {
                if (String.IsNullOrEmpty(_xml.Type4)) return "Int";
                return _xml.Type4;
            }
        }

        public string Label1 {
            get {
                if (String.IsNullOrEmpty(_xml.Label1)) return "Value 1";
                return _xml.Label1;
            }
        }

        public string Label2 {
            get {
                if (String.IsNullOrEmpty(_xml.Label2)) return "Value 2";
                return _xml.Label2;
            }
        }

        public string Label3 {
            get {
                if (String.IsNullOrEmpty(_xml.Label3)) return "Value 3";
                return _xml.Label3;
            }
        }

        public string Label4 {
            get {
                if (String.IsNullOrEmpty(_xml.Label4)) return "Value 4";
                return _xml.Label4;
            }
        }

        public string GetString(string name) {
            var obj = GetObject();
            if (obj == null) return null;
            return obj.GetString(name);
        }

        public bool GetBool(string name) {
            var obj = GetObject();
            if (obj == null) return false;
            return obj.GetBool(name);
        }

        public int GetInt(string name) {
            var obj = GetObject();
            if (obj == null) return 0;
            return obj.GetInt(name);
        }

        public double GetDouble(string name) {
            var obj = GetObject();
            if (obj == null) return 0;
            var value = obj.GetDouble(name);
            if (Double.IsNaN(value)) return 0.0;
            return value;
        }

        void SetDoubleOrIntValue(string key, double value, [CallerMemberName] string propertyName = null) {
            if (value == (int)value) SetValue(key, (int)value, propertyName);
            else SetValue(key, (double)value, propertyName);
        }

        public bool SetValue(object key, object value, [CallerMemberName] string propertyName = null) {
            var obj = GetObject();
            if (obj == null) return false;
            bool success = SetValue(obj, key, value, propertyName);
            if (success) {
                if ("tooltip" != Convert.ToString(key)) {
                    var source = _xml.Description;
                    if (null != source) {
                        Description = expandVars(source);
                    }
                }
                OnPropertyChanged("Description");
                OnPropertyChanged("Subtitle");
                OnPropertyChanged("Comment");
            }
            return success;
        }

        virtual public bool Match(string str) {
            if (str == null) return true;

            int index = (Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Description ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Comment ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }

        protected override void OnSavePropertyChanged([CallerMemberName] string propertyName = null) {
            base.OnSavePropertyChanged(propertyName);
            NotifyGameVM();
        }

        protected virtual void InitializeObject(AmfObject obj) {
            if (null == obj) {
                obj = new AmfObject(AmfTypes.Object);
            }
            obj["storageName"] = Name;
            obj["tooltip"] = expandVars(_xml.Description);
            obj["iconName"] = _xml.IconName ?? "";
            obj["iconShade"] = _xml.IconShade ?? 0xFFFFFF;
            obj["minutesLeft"] = _xml.MinutesLeft;
            obj["value1"] = _xml.Value1;
            obj["value2"] = _xml.Value2;
            obj["value3"] = _xml.Value3;
            obj["value4"] = _xml.Value4;
            obj["hidden"] = _xml.IsHidden ?? true;
            obj["combatOnly"] = _xml.IsCombatOnly ?? false;
            obj["classInstance"] = "classes::StorageClass";
        }

        protected virtual bool IsObject(AmfObject obj) {
            var storageName = (null != obj) ? obj.GetString("storageName") : "";
            return storageName == Name;
        }

        protected abstract void NotifyGameVM();

        public override string ToString() {
            return Name;
        }

        public string expandVars(string source) {
            if (null == source) return "";
            var value1 = _xml.Value1;
            var value2 = _xml.Value2;
            var value3 = _xml.Value3;
            var value4 = _xml.Value4;
            var obj = GetObject();
            if (null != obj) {
                value1 = Value1;
                value2 = Value2;
                value3 = Value3;
                value4 = Value4;
            }
            var replacements = new Dictionary<string, object> {
                { "{Value1}", value1 }
              , { "{Value2}", value2 }
              , { "{Value3}", value3 }
              , { "{Value4}", value4 }
            };
            return Extensions.ReplaceUsingDictionary(source, replacements);
        }

    }
}
