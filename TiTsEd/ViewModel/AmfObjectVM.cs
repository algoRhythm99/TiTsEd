using System;
using System.ComponentModel;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public class AmfObjectVM : ObjectVM
    {
        private readonly string _keyOverride;
        private readonly string _valueOverride;
        private readonly AmfPair _keyPair;
        private BindingList<AmfObjectVM> _children;

        public AmfObjectVM(AmfObject obj, AmfPair keyPair)
            : base(obj)
        {
            _keyPair = keyPair;
        }

        public AmfObjectVM(AmfObject obj, string key, string value)
            : base(obj)
        {
            _keyOverride = key;
            _valueOverride = value;
        }


        public string Key
        {
            get
            {
                if (null != _keyPair)
                {
                    return _keyPair.Key.ToString();
                }
                else
                {
                    if (String.IsNullOrEmpty(_keyOverride))
                    {
                        if (null != GetAmfObject())
                        {
                            return GetAmfObject().AmfType.ToString();
                        }
                    }
                    else
                    {
                        return _keyOverride;
                    }
                    return "";
                }
            }
        }

        public string Value
        {
            get
            {
                if (null != _keyPair)
                {
                    if (null == GetAmfObject())
                    {
                        //return String.Format("{0} [{1}]", _keyPair.Value.ToString(), typeString);
                        return _keyPair.Value?.ToString() ?? "";
                    }
                    else
                    {
                        var classInstance = GetAmfObject().GetString("classInstance");
                        var typeString = String.IsNullOrEmpty(classInstance) ? GetAmfObject().AmfType.ToString() : classInstance;
                        typeString = String.Format("<{0}>", typeString);
                        if (GetAmfObject().IsArray)
                        {
                            return String.Format("{0} {1}", typeString, ChildrenCountDisplay);
                        }
                        else
                        {
                            return String.Format("{0}", typeString);
                        }
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(_valueOverride))
                    {
                        if (null == GetAmfObject())
                        {
                            return "";
                        }
                        else
                        {
                            return GetAmfObject().ToString();
                        }
                    }
                    else
                    {
                        return _valueOverride;
                    }
                }
            }
        }

        public BindingList<AmfObjectVM> Children
        {
            get
            {
                if (null == GetAmfObject())
                {
                    return null;
                }
                else
                {
                    if (GetAmfObject().HasChildren)
                    {
                        if (null == _children)
                        {
                            _children = new BindingList<AmfObjectVM>();
                            var enumerator = GetAmfObject().GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                _children.Add(new AmfObjectVM(enumerator.Current.ValueAsObject, enumerator.Current));
                            }
                        }
                    }
                }
                return _children;
            }
        }

        public string ChildrenCountDisplay
        {
            get
            {
                string childDisplay = "";
                if (null != GetAmfObject())
                {
                    if (GetAmfObject().HasChildren)
                    {
                        if (GetAmfObject().IsArray)
                        {
                            childDisplay = String.Format("[{0}]", Children.Count);
                        }
                    }
                }
                return childDisplay;
            }
        }
    }
}
