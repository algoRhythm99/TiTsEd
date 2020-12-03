using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiTsEd.Common;

namespace TiTsEd.Model
{
    public enum AmfTypes
    {
        Undefined     = 0x00,
        Null          = 0x01,
        False         = 0x02,
        True          = 0x03,
        Integer       = 0x04,
        Double        = 0x05,
        String        = 0x06,
        XmlDoc        = 0x07,
        Date          = 0x08,
        Array         = 0x09,
        Object        = 0x0A,
        Xml           = 0x0B,
        ByteArray     = 0x0C,
        VectorInt     = 0x0D,
        VectorUInt    = 0x0E,
        VectorDouble  = 0x0F,
        VectorGeneric = 0x10,
        Dictionary    = 0x11,
    }

    public sealed class AmfNull
    {
        public static readonly AmfNull Instance = new AmfNull();

        private AmfNull()
        {
        }

        public override string ToString()
        {
            return "<Null>";
        }
    }

    public sealed class AmfTrait
    {
        public AmfTrait()
        {
        }

        public String[] Properties { get; set; }
        public bool IsExternalizable { get; set; }
        public bool IsDynamic { get; set; }
        public bool IsEnum { get; set; }
        public string Name { get; set; }
    }

    public sealed class AmfPair
    {
        public AmfPair()
        {
        }

        public AmfPair(Object name, Object value)
        {
            Key = name;
            Value = value;
        }

        public Object Key { get; set; }
        public Object Value { get; set; }
        public AmfObject ValueAsObject { get { return Value as AmfObject; } }

        public override string ToString()
        {
            return Key == null ? "<Undefined>" : Key.ToString();
        }
    }

    public class AmfObject : IEnumerable<AmfPair>
    {
        readonly Dictionary<Object, Object> _associativePart = new Dictionary<Object, Object>();
        readonly Dictionary<Int32, Object> _sparsePart = new Dictionary<Int32, Object>();
        readonly List<Object> _densePart = new List<Object>();

        public AmfObject(AmfTypes type)
        {
            AmfType = type;

            if (type == AmfTypes.Object)
            {
                Trait = new AmfTrait { Name = "", IsDynamic = true, Properties = new string[0] };
            }
        }

        public AmfTrait Trait { get; set; }
        public AmfTypes AmfType { get; private set; }
        public string GenericElementType { get; set; }
        public bool IsFixedVector { get; set; }
        public bool HasWeakKeys { get; set; }

        public int DenseCount
        {
            get { return _densePart.Count; }
        }

        public int Count
        {
            get { return _associativePart.Count + _sparsePart.Count + _densePart.Count; }
        }

        public bool IsSparse
        {
            get { return _associativePart.Count != 0 || _sparsePart.Count != 0; }
        }

        public bool IsEnum
        {
            get { return Trait != null && Trait.IsEnum; }
        }

        public int EnumValue
        {
            get { return GetInt("value"); }
            set { this["value"] = value; }
        }

        public object this[object key]
        {
            get
            {
                int index;
                if (IsIndex(key, out index))
                {
                    if (IsDenseIndex(index)) return _densePart[index];
                    else return _sparsePart.GetValueOrDefault(index);
                }
                else
                {
                    key = NormalizeAssociativeKey(key);
                    return _associativePart.GetValueOrDefault(key);
                }
            }
            set
            {
                if (value == null)
                {
                    RemoveKey(key);
                    return;
                }

                int index;
                if (IsIndex(key, out index))
                {
                    if (IsDenseIndex(index)) _densePart[index] = value;
                    else if (index == _densePart.Count) Push(value);
                    else _sparsePart[index] = value;
                }
                else
                {
                    key = NormalizeAssociativeKey(key);
                    _associativePart[key] = value;
                }
            }
        }

        bool RemoveKey(object key)
        {
            int index;
            if (IsIndex(key, out index))
            {
                if (IsDenseIndex(index))
                {
                    RemoveDenseIndex(index);
                    return true;
                }
                else
                {
                    return _sparsePart.Remove(index);
                }
            }
            else
            {
                key = NormalizeAssociativeKey(key);
                return _associativePart.Remove(key);
            }
        }

        void RemoveDenseIndex(int index)
        {
            // We're going to remove 4, so we need to add 5 and higher to the associative part
            for (int i = index + 1; i < _densePart.Count; ++i)
            {
                _sparsePart.Add(i, _densePart[i]);
            }

            // Remove 4 and higher from the dense part
            _densePart.RemoveRange(index, _densePart.Count - index);
        }

        public double GetDouble(Object key, double? defaultValue = null)
        {
            var value = this[key];
            if (value == null) return defaultValue.Value;
            if (value is string) return Double.Parse((string)value);
            if (value is double) return (double)value;
            if (value is bool) return (bool)value ? 1 : 0;
            if (value is uint) return (double)(uint)value;
            if (value is int) return (double)(int)value;
            throw new InvalidOperationException("No conversion available");
        }

        public int GetInt(Object key, int? defaultValue = null)
        {
            var value = this[key];
            if (value == null) return (defaultValue != null) ? defaultValue.Value : 0;
            if (value is string) return Int32.Parse((string)value);
            if (value is double) return (int)(double)value;
            if (value is bool) return (bool)value ? 1 : 0;
            if (value is uint) return (int)(uint)value;
            if (value is int) return (int)value;
            throw new InvalidOperationException("No conversion available");
        }

        public string GetString(Object key)
        {
            var value = this[key];
            if (value == null) return null;
            if (value is AmfNull) return null;
            if (value is string) return (string)value;
            if (value is AmfObject)
            {
                var obj = (AmfObject)value;
                if (obj.IsEnum) return obj.EnumValue.ToString();
            }
            return value.ToString();
        }

        public bool GetBool(Object key, bool? defaultValue = null)
        {
            var value = this[key];
            if (value == null) return (defaultValue != null) ? defaultValue.Value : false;
            if (value is bool) return (bool)value;
            if (value is uint) return (uint)value == 0;
            if (value is int) return (int)value == 0;
            if (value as string == "false") return false;
            return true;
        }

        public AmfObject GetObj(Object key)
        {
            key = NormalizeAssociativeKey(key);
            return this[key] as AmfObject;
        }

        public bool Contains(Object key)
        {
            int index;
            if (IsIndex(key, out index))
            {
                if (IsDenseIndex(index)) return true;
                else return _sparsePart.ContainsKey(index);
            }
            else
            {
                key = NormalizeAssociativeKey(key);
                return _associativePart.ContainsKey(key);
            }
        }

        public bool Pop(int index)
        {
            // Remove from dense part
            if (IsDenseIndex(index))
            {
                _densePart.RemoveAt(index);
                return true;
            }

            // Remove from sparse part
            if (!_sparsePart.Remove(index)) return false;

            // Shift following items
            DecrementSparseIndicesGreaterThan(index);
            return true;
        }

        void DecrementSparseIndicesGreaterThan(int index)
        {
            if (_sparsePart.Count == 0) return;

            var pairsToShift = _sparsePart.Where(x => x.Key > index).ToArray();
            foreach (var pair in pairsToShift) _sparsePart.Remove(pair.Key);
            foreach (var pair in pairsToShift) _sparsePart.Add(pair.Key - 1, pair.Value);
        }

        public void Push(object value)
        {
            // Note: thanks to consistency, we know that there is no item in the associative part at index #DenseCount
            _densePart.Add(value);
            if (_sparsePart.Count == 0) return;   // Optimization for deserialization

            MergeSparsePartIntoDensePart();
        }

        void MergeSparsePartIntoDensePart()
        {
            // Before we had 0-4 and 6, we added 5, so we merge 6 and higher into the dense part
            object nextDenseValue;
            int nextDenseKey = _densePart.Count;

            while (_sparsePart.TryGetValue(nextDenseKey, out nextDenseValue))
            {
                _sparsePart.Remove(nextDenseKey);
                _densePart.Add(nextDenseValue);
                nextDenseKey++;
            }
        }

        public void Move(int sourceIndex, int destIndex)
        {
            if (destIndex == sourceIndex) return;
            // No change on the index: the shift caused by the removal is compensated by the fact that we need to increment the index since we want to insert "after".

            if (sourceIndex < 0 || sourceIndex >= _densePart.Count) throw new ArgumentOutOfRangeException();
            if (destIndex < 0 || destIndex >= _densePart.Count) throw new ArgumentOutOfRangeException();

            var value = _densePart[sourceIndex];
            _densePart.RemoveAt(sourceIndex);
            _densePart.Insert(destIndex, value);
        }

        public void SortDensePart(Comparison<Object> comparison)
        {
            _densePart.Sort(comparison);
        }

        public Object[] GetDensePart()
        {
            return _densePart.ToArray();
        }

        public IEnumerable<AmfPair> GetSparseAndAssociativePairs()
        {
            foreach (var entry in _sparsePart) yield return new AmfPair(entry.Key, entry.Value);
            foreach (var entry in _associativePart) yield return new AmfPair(entry.Key, entry.Value);
        }

        public IEnumerable<AmfPair> Enumerate()
        {
            foreach (var entry in _sparsePart) yield return new AmfPair(entry.Key, entry.Value);
            foreach (var entry in _associativePart) yield return new AmfPair(entry.Key, entry.Value);
            for (int i = 0; i < _densePart.Count; i++) yield return new AmfPair(i, _densePart[i]);
        }

        IEnumerator<AmfPair> IEnumerable<AmfPair>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<AmfPair> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        bool IsDenseIndex(int index)
        {
            return index >= 0 && index < _densePart.Count;
        }

        bool IsIndex(object key, out int index)
        {
            if (key == null)
            {
                index = 0;
                return false;
            }
            if (key is int)
            {
                index = (int)key;
                return true;
            }
            if (key is string)
            {
                return Int32.TryParse((string)key, out index);
            }
            if (key is bool)
            {
                index = (bool)key ? 1 : 0;
                return true;
            }

            var str = key.ToString();
            return Int32.TryParse(str, out index);
        }

        public static bool AreSame(Object x, Object y)
        {
            if (x == null) return (y == null);
            if (y == null) return false;

            var xType = System.Type.GetTypeCode(x.GetType());
            var yType = System.Type.GetTypeCode(y.GetType());

            if (xType == yType) return object.Equals(x, y);
            if (TryConvertForEqualityComparison(ref x, xType)) return AreSame(x, y);
            if (TryConvertForEqualityComparison(ref y, yType)) return AreSame(x, y);
            return false;
        }

        static bool TryConvertForEqualityComparison(ref object x, TypeCode type)
        {
            switch(type)
            {
                case TypeCode.Boolean:
                    x = (bool)x ? 1 : 0;
                    return true;

                case TypeCode.Char:
                case TypeCode.DateTime:

                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:

                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    x = x.ToString();
                    return true;

                default:
                    return false;
            }
        }

        object NormalizeAssociativeKey(Object key)
        {
            if (AmfType == AmfTypes.Dictionary) return key;
            return key.ToString();
        }

        public AmfObject clone() {
            AmfObject obj = new AmfObject(this.AmfType);
            foreach(var v in _associativePart) {
                if(v.Value != null) {
                    if(v.Value.GetType() == typeof(AmfObject)) {
                        obj._associativePart.Add(v.Key, ((AmfObject)v.Value).clone());
                    } else {
                        obj._associativePart.Add(v.Key, v.Value);
                    }
                } else {
                    obj._associativePart.Add(v.Key, v.Value);
                }
            }
            foreach(var v in _densePart) {
                if(v != null) {
                    if(v.GetType() == typeof(AmfObject)) {
                        obj._densePart.Add(((AmfObject)v).clone());
                    } else {
                        obj._densePart.Add(v);
                    }
                } else {
                    obj._densePart.Add(v);
                }
            }
            foreach(var v in _sparsePart) {
                if(v.Value != null) {
                    if(v.Value.GetType() == typeof(AmfObject)) {
                        obj._sparsePart.Add(v.Key, ((AmfObject)v.Value).clone());
                    } else {
                        obj._sparsePart.Add(v.Key, v.Value);
                    }
                } else {
                    obj._sparsePart.Add(v.Key, v.Value);
                }
            }

            if(Trait != null) {
                obj.Trait.IsDynamic = Trait.IsDynamic;
                obj.Trait.IsEnum = Trait.IsEnum;
                obj.Trait.IsExternalizable = Trait.IsExternalizable;
                obj.Trait.Name = Trait.Name;
                obj.Trait.Properties = (string[])Trait.Properties.Clone();
            }
            obj.GenericElementType = GenericElementType;
            obj.IsFixedVector = IsFixedVector;
            obj.HasWeakKeys = HasWeakKeys;
            
            return obj;
        }
    }

    public class AmfXmlType
    {
        public bool IsDocument { get; set; }
        public string Content { get; set; }
    }

    public static class AmfHelpers
    {
        public static bool FlagsHasFlag(AmfObject flagObj, GLOBAL.FLAGS flag)
        {
            return FlagsHasFlag(flagObj, (int)flag);
        }

        public static bool FlagsHasFlag(AmfObject flagObj, int flag)
        {
            if (null != flagObj)
            {
                int i = 0;
                int id = 0;
                while ((id = flagObj.GetInt(i++, -1234)) != -1234)
                {
                    if (id == flag)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
