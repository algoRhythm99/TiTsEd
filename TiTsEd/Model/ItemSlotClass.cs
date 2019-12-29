using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiTsEd.Model
{
    public class ItemSlotClass : TiTSSaveableClass
    {
        public static readonly ItemSlotClass EmptyItem = new ItemSlotClass();

        public ItemSlotClass() : base()
        {
            ClassInstance = XmlItem.Empty.ID;
            ShortName = XmlItem.Empty.Name;
            Quantity = 0;
            Version = 1;
        }

        public ItemSlotClass(AmfObject saveObject) : base(saveObject)
        { }

        public String ShortName
        {
            get { return SaveableObject.GetString("shortName"); }
            set { SaveableObject["shortName"] = value; }
        }

        public String LongName
        {
            get { return SaveableObject.GetString("longName"); }
            set { SaveableObject["longName"] = value; }
        }

        public String Description
        {
            get { return SaveableObject.GetString("description"); }
            set { SaveableObject["description"] = value; }
        }

        public String Tooltip
        {
            get { return SaveableObject.GetString("tooltip"); }
            set { SaveableObject["tooltip"] = value; }
        }

        public int Version
        {
            get { return SaveableObject.GetInt("version", 0); }
            set { SaveableObject["version"] = value; }
        }

        public int Quantity
        {
            get { return SaveableObject.GetInt("quantity", 0); }
            set { SaveableObject["quantity"] = value; }
        }

        public int StackSize
        {
            get { return SaveableObject.GetInt("stackSize", 0); }
            set { SaveableObject["stackSize"] = value; }
        }

        public double Defense
        {
            get { return SaveableObject.GetDouble("defense",0); }
            set { SaveableObject["defense"] = value; }
        }

        public double ShieldDefense
        {
            get { return SaveableObject.GetDouble("shieldDefense", 0); }
            set { SaveableObject["shieldDefense"] = value; }
        }

        public double Shields
        {
            get { return SaveableObject.GetDouble("shields",0); }
            set { SaveableObject["shields"] = value; }
        }

        public double Attack
        {
            get { return SaveableObject.GetDouble("attack",0); }
            set { SaveableObject["attack"] = value; }
        }

        public double CritBonus
        {
            get { return SaveableObject.GetDouble("critBonus",0); }
            set { SaveableObject["critBonus"] = value; }
        }

        public double Sexiness
        {
            get { return SaveableObject.GetDouble("sexiness",0); }
            set { SaveableObject["sexiness"] = value; }
        }

        public double Evasion
        {
            get { return SaveableObject.GetDouble("evasion",0); }
            set { SaveableObject["evasion"] = value; }
        }

        public double Fortification
        {
            get { return SaveableObject.GetDouble("fortification",0); }
            set { SaveableObject["fortification"] = value; }
        }

        public bool IsUsable
        {
            get { return SaveableObject.GetBool("isUsable",false); }
            set { SaveableObject["isUsable"] = value; }
        }

        public bool CombatUsable
        {
            get { return SaveableObject.GetBool("combatUsable",false); }
            set { SaveableObject["combatUsable"] = value; }
        }

        public bool HardLightEquipped
        {
            get { return SaveableObject.GetBool("hardLightEquipped", false); }
            set { SaveableObject["hardLightEquipped"] = value; }
        }

        public bool HasRandomProperties
        {
            get { return SaveableObject.GetBool("hasRandomProperties", false); }
            set { SaveableObject["hasRandomProperties"] = value; }
        }

    }
}
