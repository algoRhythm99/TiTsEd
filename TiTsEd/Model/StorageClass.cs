using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiTsEd.Model
{
    public class StorageClass : TiTSSaveableClass
    {
        public StorageClass() : base()
        {
            StorageName = "";
            Tooltip = "";
            IconName = "";
            IconShade = 0xFFFFFF;
            MinutesLeft = 0;
            Value1 = 0;
            Value2 = 0;
            Value3 = 0;
            Value4 = 0;
            Hidden = true;
            CombatOnly = false;
            ClassInstance = "classes::StorageClass";
        }

        public StorageClass(AmfObject saveObject) : base(saveObject)
        { }

        public String StorageName
        {
            get { return SaveableObject.GetString("storageName"); }
            set { SaveableObject["storageName"] = value; }
        }

        public String Tooltip
        {
            get { return SaveableObject.GetString("tooltip"); }
            set { SaveableObject["tooltip"] = value; }
        }

        public String IconName
        {
            get { return SaveableObject.GetString("iconName"); }
            set { SaveableObject["iconName"] = value; }
        }

        public int IconShade
        {
            get { return SaveableObject.GetInt("iconShade", 0); }
            set { SaveableObject["iconShade"] = value; }
        }

        public int MinutesLeft
        {
            get { return SaveableObject.GetInt("minutesLeft", 0); }
            set { SaveableObject["minutesLeft"] = value; }
        }

        public bool Hidden
        {
            get { return SaveableObject.GetBool("hidden", false); }
            set { SaveableObject["hidden"] = value; }
        }

        public bool CombatOnly
        {
            get { return SaveableObject.GetBool("combatOnly", false); }
            set { SaveableObject["combatOnly"] = value; }
        }

        public double Value1
        {
            get { return SaveableObject.GetDouble("value1", 0); }
            set { SaveableObject["value1"] = value; }
        }

        public double Value2
        {
            get { return SaveableObject.GetDouble("value2", 0); }
            set { SaveableObject["value2"] = value; }
        }

        public double Value3
        {
            get { return SaveableObject.GetDouble("value3", 0); }
            set { SaveableObject["value3"] = value; }
        }

        public double Value4
        {
            get { return SaveableObject.GetDouble("value4", 0); }
            set { SaveableObject["value4"] = value; }
        }

    }

}
