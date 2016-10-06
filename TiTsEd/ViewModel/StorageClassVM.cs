using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public class StorageClassVM : ObjectVM
    {
        public StorageClassVM(AmfObject obj)
            : base(obj)
        {
        }

        public static void initialize(AmfObject obj, bool reset = true)
        {
            if (null == obj)
            {
                obj = new AmfObject(AmfTypes.Object);
            }
            if (reset)
            {
                obj["storageName"] = "";
                obj["tooltip"] = "";
                obj["iconName"] = "";
                obj["iconShade"] = 16777215;
                obj["minutesLeft"] = 0;
                obj["value1"] = 0;
                obj["value2"] = 0;
                obj["value3"] = 0;
                obj["value4"] = 0;
                obj["hidden"] = true;
                obj["combatOnly"] = false;

                obj["classInstance"] = "classes::StorageClass";
            }
        }

        public static void fixup(AmfObject obj)
        {
            initialize(obj, false);
            // Fixes saves which have NaNs for some perk values
            if (double.IsNaN(obj.GetDouble("value1"))) obj["value1"] = 0;
            if (double.IsNaN(obj.GetDouble("value2"))) obj["value2"] = 0;
            if (double.IsNaN(obj.GetDouble("value3"))) obj["value3"] = 0;
            if (double.IsNaN(obj.GetDouble("value4"))) obj["value4"] = 0;
        }

        public new string Name
        {
            get { return GetString("storageName"); }
            set
            {
                SetValue("storageName", value);
            }
        }

        public string IconName
        {
            get { return GetString("iconName"); }
            set
            {
                SetValue("iconName", value);
            }
        }

        public int IconShade
        {
            get { return GetInt("iconShade"); }
            set
            {
                SetValue("iconShade", value);
            }
        }

        public int MinutesLeft
        {
            get { return GetInt("minutesLeft"); }
            set
            {
                SetValue("minutesLeft", value);
            }
        }

        public string Description
        {
            get { return GetString("tooltip"); }
            set
            {
                SetValue("tooltip", value);
            }
        }

        public double Value1
        {
            get { return GetDouble("value1"); }
            set
            {
                SetValue("value1", value);
            }
        }

        public double Value2
        {
            get { return GetDouble("value2"); }
            set
            {
                SetValue("value2", value);
            }
        }

        public double Value3
        {
            get { return GetDouble("value3"); }
            set
            {
                SetValue("value3", value);
            }
        }

        public double Value4
        {
            get { return GetDouble("value4"); }
            set
            {
                SetValue("value4", value);
            }
        }

        public bool IsHidden
        {
            get { return GetBool("hidden"); }
            set
            {
                SetValue("hidden", value);
            }
        }

        public bool IsCombatOnly
        {
            get { return GetBool("combatOnly"); }
            set
            {
                SetValue("combatOnly", value);
            }
        }
    }
}
