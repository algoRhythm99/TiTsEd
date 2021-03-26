using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiTsEd.Common
{
    public class NaturalSortComparer<T> : IComparer<string>, IDisposable
    {
        private bool isAscending;
        private Regex _Regex = new Regex(@"(\d+)|(\D+)");

        public NaturalSortComparer(bool inAscendingOrder = true)
        {
            this.isAscending = inAscendingOrder;
        }

        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            var xb = x ?? "";
            return xb.CompareTo(y);
        }

        #endregion

        #region IComparer<string> Members

        int IComparer<string>.Compare(string x, string y)
        {
            var list1 = _Regex.Matches(x).Cast<Match>().Select(m => m.Value.Trim()).ToList();

            var list2 = _Regex.Matches(y).Cast<Match>().Select(m => m.Value.Trim()).ToList();

            var min = Math.Min(list1.Count, list2.Count);
            int comp = 0;

            for (int i = 0; i < min; i++)
            {
                int intx, inty;

                if (int.TryParse(list1[i], out intx) && int.TryParse(list2[i], out inty))
                {
                    comp = intx - inty;
                }
                else
                {
                    comp = String.Compare(list1[i], list2[i]);
                }
                if (comp != 0)
                {
                    return comp;
                }
            }

            return list1.Count - list2.Count;
        }

        private static int PartCompare(string left, string right)
        {
            int x, y;
            if (!int.TryParse(left, out x))
            {
                return left.CompareTo(right);
            }
            if (!int.TryParse(right, out y))
            {
                return left.CompareTo(right);
            }
            return x.CompareTo(y);
        }

        #endregion

        private Dictionary<string, string[]> table = new Dictionary<string, string[]>();

        public void Dispose()
        {
            table.Clear();
            table = null;
        }
    }
}
