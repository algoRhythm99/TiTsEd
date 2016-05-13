using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public sealed partial class GameVM : ObjectVM
    {

        bool IsMale
        {
            get { return GetInt("gender", 0) <= 1; }
        }

        // Public helper for the various subordinate body part view models (e.g. CockVM)
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }

        public void BeforeSerialization()
        {

        }
    }
}
