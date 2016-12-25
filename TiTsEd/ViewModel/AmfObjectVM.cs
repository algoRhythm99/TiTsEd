using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public class AmfObjectVM : SortedDictionary<string, object>
    {
        public static NaturalSortComparer<AmfObjectVM> NSComparer = new NaturalSortComparer<AmfObjectVM>();

        public AmfObjectVM(AmfObject file)
            : base(NSComparer)
        {
            BuildTree(this, file);
        }

        public void BuildTree(SortedDictionary<string, object> dic, AmfObject o)
        {
            if (null != o)
            {
                var ot = o.AmfType;
                if (AmfTypes.Null != ot)
                {
                    foreach (AmfPair pair in o.Enumerate())
                    {
                        var val = pair.Value;
                        var kv = pair.Key.ToString().Trim();
                        var valO = pair.ValueAsObject;
                        if (null != valO)
                        {
                            var vt = valO.AmfType.ToString();
                            var key = String.Format("{0} [{1}]", kv, vt);
                            var d = new SortedDictionary<string, object>(NSComparer);
                            BuildTree(d, valO);
                            dic.Add(key, d);
                        }
                        else
                        {
                            var key = kv;
                            dic.Add(key, val);
                        }
                    }
                }
            }
        }

    }
}
