using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class FlagVM : BindableBase {
        readonly string _name;
        readonly string _description;
        private object _flagValue;
        readonly AmfObject _flagsObject;
        readonly GeneralObjectVM _flags;
        readonly GameVM _game;

        public FlagVM(GameVM game, ref AmfObject flags, XmlEnum data) {
            _game = game;
            _name = data != null ? data.Name : "";
            _description = data != null ? data.Description : "";

            _flagsObject = flags;
            _flags = new GeneralObjectVM(flags);
            bool hasFlag = _flags.HasValue(_name);
            if (hasFlag) {
                _flagValue = _flagsObject[_name];
            }
            GameVMProperties = new HashSet<string>();

        }

        public HashSet<string> GameVMProperties {
            get;
            private set;
        }

        public new string Name {
            get { return _name; }
        }

        public string Description {
            get { return _description; }
        }

        public object Value {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        int? valueAsInt(string value) {
            int iValue;
            string _value = (value ?? "").Trim();
            bool converted = Int32.TryParse(_value, NumberStyles.Integer, CultureInfo.CurrentCulture, out iValue);
            if (converted) return iValue;
            converted = Int32.TryParse(_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out iValue);
            if (converted) return iValue;
            return null;
        }

        double? valueAsDouble(string value) {
            double fValue;
            string _value = (value ?? "").Trim();
            bool converted = Double.TryParse(_value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out fValue);
            if (converted) return fValue;
            converted = Double.TryParse(_value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fValue);
            if (converted) return fValue;
            return null;
        }

        bool? valueAsBool(string value) {
            string _value = (value ?? "").Trim();
            bool converted = String.Equals(_value, "true", StringComparison.InvariantCultureIgnoreCase);
            if (converted) return true;
            converted = String.Equals(_value, "false", StringComparison.InvariantCultureIgnoreCase);
            if (converted) return false;
            return null;
        }

        object GetValue() {
            string value = (_flagValue ?? "").ToString();

            int? iValue = valueAsInt(value);
            if (null != iValue) return iValue;

            double? fValue = valueAsDouble(value);
            if (null != fValue) return fValue;

            bool? bValue = valueAsBool(value);
            if (null != bValue) return bValue;

            if (value == "<null>") return AmfNull.Instance;
            return _flagValue;
        }

        public bool SetValue(object value) {
            string sValue = (value ?? "").ToString();
            int? iValue = valueAsInt(sValue);
            double? fValue = valueAsDouble(sValue);
            bool? bValue = valueAsBool(sValue);
            // Update value
            if (null != iValue) {
                _flagValue = iValue;
                if (!SetValue(_flagsObject, Name, iValue)) return false;
            } else if (null != fValue) {
                _flagValue = fValue;
                if (!SetValue(_flagsObject, Name, fValue)) return false;
            } else if (null != bValue) {
                _flagValue = bValue;
                if (!SetValue(_flagsObject, Name, bValue)) return false;
            } else {
                _flagValue = value;
                if (!SetValue(_flagsObject, Name, value)) return false;
            }
            return true;
        }

        public bool Match(string str) {
            if (str == null) return true;

            int index = (Name ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            index = (Description ?? "").IndexOf(str, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) return true;

            return false;
        }

        protected override void OnSavePropertyChanged([CallerMemberName] string propertyName = null) {
            base.OnSavePropertyChanged(propertyName);
            _game.OnFlagChanged(Name);
        }
    }
}
