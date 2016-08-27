using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public abstract class NamedVector4VM : BindableBase, IComparable
    {
        protected readonly GameVM _game;
        protected readonly AmfObject _items;
        protected readonly XmlNamedVector4 _xml;
        protected readonly HashSet<string> _gameProperties = new HashSet<string>();

        protected NamedVector4VM(GameVM game, AmfObject items, XmlNamedVector4 xml)
        {
            _xml = xml;
            _game = game;
            _items = items;
        }

        public new string Name
        {
            get { return _xml.Name; }
        }

        public string Comment
        {
            get { return _xml.Description; }
        }

        public Visibility CommentVisibility
        {
            get { return String.IsNullOrEmpty(Comment) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public HashSet<string> GameVMProperties
        {
            get { return _gameProperties; }
        }

        public AmfObject GetObject()
        {
            return _items.Select(x => x.ValueAsObject).FirstOrDefault(x => IsObject(x));
        }

        public virtual bool IsOwned
        {
            get { return GetObject() != null; }
            set
            {
                var pair = _items.FirstOrDefault(x => IsObject(x.ValueAsObject));
                if ((pair != null) == value) return;

                if (value)
                {
                    var obj = new AmfObject(AmfTypes.Array);
                    InitializeObject(obj);
                    obj["value1"] = _xml.Value1;
                    obj["value2"] = _xml.Value2;
                    obj["value3"] = _xml.Value3;
                    obj["value4"] = _xml.Value4;
                    _items.Push(obj);
                }
                else
                {
                    _items.Pop((int)pair.Key);
                }
                OnPropertyChanged("Value1");
                OnPropertyChanged("Value2");
                OnPropertyChanged("Value3");
                OnPropertyChanged("Value4");
                OnSavePropertyChanged();
                OnIsOwnedChanged();
            }
        }

        protected virtual void OnIsOwnedChanged()
        {
        }

        /// <summary>
        /// Gets the first value, or 0 if the perk/status/key item is not owned.
        /// </summary>
        public double Value1
        {
            get { return GetDouble("value1"); }
            set { SetDoubleOrIntValue("value1", value); }
        }

        /// <summary>
        /// Gets the second value, or 0 if the perk/status/key item is not owned.
        /// </summary>
        public double Value2
        {
            get { return GetDouble("value2"); }
            set { SetDoubleOrIntValue("value2", value); }
        }

        /// <summary>
        /// Gets the third value, or 0 if the perk/status/key item is not owned.
        /// </summary>
        public double Value3
        {
            get { return GetDouble("value3"); }
            set { SetDoubleOrIntValue("value3", value); }
        }

        /// <summary>
        /// Gets the fourth value, or 0 if the perk/status/key item is not owned.
        /// </summary>
        public double Value4
        {
            get { return GetDouble("value4"); }
            set { SetDoubleOrIntValue("value4", value); }
        }

        public string Type1
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Type1)) return "Int";
                return _xml.Type1;
            }
        }

        public string Type2
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Type2)) return "Int";
                return _xml.Type2;
            }
        }

        public string Type3
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Type3)) return "Int";
                return _xml.Type3;
            }
        }

        public string Type4
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Type4)) return "Int";
                return _xml.Type4;
            }
        }

        public string Label1
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Label1)) return "Value 1";
                return _xml.Label1;
            }
        }

        public string Label2
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Label2)) return "Value 2";
                return _xml.Label2;
            }
        }

        public string Label3
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Label3)) return "Value 3";
                return _xml.Label3;
            }
        }

        public string Label4
        {
            get
            {
                if (String.IsNullOrEmpty(_xml.Label4)) return "Value 4";
                return _xml.Label4;
            }
        }

        public string GetString(string name)
        {
            var obj = GetObject();
            if (obj == null) return null;
            return obj.GetString(name);
        }

        public bool GetBool(string name)
        {
            var obj = GetObject();
            if (obj == null) return false;
            return obj.GetBool(name);
        }

        public int GetInt(string name)
        {
            var obj = GetObject();
            if (obj == null) return 0;
            return obj.GetInt(name);
        }

        public double GetDouble(string name)
        {
            var obj = GetObject();
            if (obj == null) return 0;
            var value = obj.GetDouble(name);
            if (Double.IsNaN(value)) return 0.0;
            return value;
        }

        void SetDoubleOrIntValue(string key, double value, [CallerMemberName] string propertyName = null)
        {
            if (value == (int)value) SetValue(key, (int)value, propertyName);
            else SetValue(key, (double)value, propertyName);
        }

        public bool SetValue(object key, object value, [CallerMemberName] string propertyName = null)
        {
            var obj = GetObject();
            if (obj == null) return false;
            return SetValue(obj, key, value, propertyName);
        }

        virtual public bool Match(string str)
        {
            if (str == null || str.Length < 3) return true;

            int index = (Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Comment ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }

        protected override void OnSavePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnSavePropertyChanged(propertyName);
            NotifyGameVM();
        }

        protected abstract void InitializeObject(AmfObject obj);
        protected abstract bool IsObject(AmfObject obj);
        protected abstract void NotifyGameVM();

        public override string ToString()
        {
            return Name;
        }

        int IComparable.CompareTo(object obj)
        {
            NamedVector4VM bObj = (NamedVector4VM)obj;
            if (this != bObj)
            {
                string a = Name;
                string b = bObj.Name;
                return a.CompareTo(b);
            }
            return 0;
        }

    }
}
