using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiTsEd.Model
{
    public class Creature : TiTSSaveableClass
    {

        public Creature() : base()
        { }

        public Creature(AmfObject saveObject) : base(saveObject)
        { }

        public String OriginalRace
        {
            get { return SaveableObject.GetString("originalRace"); }
            set { SaveableObject["originalRace"] = value; }
        }

        public ItemSlotClass MeleeWeapon
        {
            get
            {
                if (null == _meleeWeapon)
                {
                    _meleeWeapon = new ItemSlotClass(MeleeWeaponObject);
                }
                return _meleeWeapon;
            }

            set
            {
                MeleeWeaponObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject MeleeWeaponObject
        {
            get
            {
                var obj = SaveableObject.GetObj("meleeWeapon");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["meleeWeapon"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["meleeWeapon"] = value;
            }
        }

        public ItemSlotClass RangedWeapon
        {
            get
            {
                if (null == _rangedWeapon)
                {
                    _rangedWeapon = new ItemSlotClass(RangedWeaponObject);
                }
                return _rangedWeapon;
            }
            set
            {
                RangedWeaponObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject RangedWeaponObject
        {
            get
            {
                var obj = SaveableObject.GetObj("rangedWeapon");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["rangedWeapon"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["rangedWeapon"] = value;
            }
        }

        public ItemSlotClass Armor
        {
            get
            {
                if (null == _armor)
                {
                    _armor = new ItemSlotClass(ArmorObject);
                }
                return _armor;
            }
            set
            {
                ArmorObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject ArmorObject
        {
            get
            {
                var obj = SaveableObject.GetObj("armor");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["armor"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["armor"] = value;
            }
        }

        public ItemSlotClass Accessory
        {
            get
            {
                if (null == _accessory)
                {
                    _accessory = new ItemSlotClass(AccessoryObject);
                }
                return _accessory;
            }
            set
            {
                AccessoryObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject AccessoryObject
        {
            get
            {
                var obj = SaveableObject.GetObj("accessory");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["accessory"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["accessory"] = value;
            }
        }

        public ItemSlotClass Shield
        {
            get
            {
                if (null == _shield)
                {
                    _shield = new ItemSlotClass(ShieldObject);
                }
                return _shield;
            }
            set
            {
                ShieldObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject ShieldObject
        {
            get
            {
                var obj = SaveableObject.GetObj("shield");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["shield"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["shield"] = value;
            }
        }

        public ItemSlotClass UpperUnderGarment
        {
            get
            {
                if (null == _upperUndergarment)
                {
                    _upperUndergarment = new ItemSlotClass(UpperUnderGarmentObject);
                }
                return _upperUndergarment;
            }
            set
            {
                UpperUnderGarmentObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject UpperUnderGarmentObject
        {
            get
            {
                var obj = SaveableObject.GetObj("upperUndergarment");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["upperUndergarment"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["upperUndergarment"] = value;
            }
        }

        public ItemSlotClass LowerUnderGarment
        {
            get
            {
                if (null == _lowerUndergarment)
                {
                    _lowerUndergarment = new ItemSlotClass(LowerUnderGarmentyObject);
                }
                return _lowerUndergarment;
            }
            set
            {
                LowerUnderGarmentyObject = (null == value) ? null : value.SaveableObject;
            }
        }

        public AmfObject LowerUnderGarmentyObject
        {
            get
            {
                var obj = SaveableObject.GetObj("lowerUndergarment");
                if (null == obj)
                {
                    obj = new ItemSlotClass().SaveableObject;
                    SaveableObject["lowerUndergarment"] = obj;
                }
                return obj;
            }
            set
            {
                SaveableObject["lowerUndergarment"] = value;
            }
        }

        #region Private Fields
        [NonSerialized]
        private ItemSlotClass _meleeWeapon;
        [NonSerialized]
        private ItemSlotClass _rangedWeapon;
        [NonSerialized]
        private ItemSlotClass _armor;
        [NonSerialized]
        private ItemSlotClass _shield;
        [NonSerialized]
        private ItemSlotClass _accessory;
        [NonSerialized]
        private ItemSlotClass _upperUndergarment;
        [NonSerialized]
        private ItemSlotClass _lowerUndergarment;
        #endregion
    }
}
