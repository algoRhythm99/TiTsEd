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
            AmfHelpers.BuildTree(this, file);
        }

    }
}
