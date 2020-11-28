using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public sealed class ShipArrayVM : ArrayVM<ShipVM>
    {
        public ShipArrayVM(GameVM game, AmfObject obj)
            : base(obj, x => new ShipVM(game, x))
        {
        }

        public static AmfObject CreateNewShipObject()
        {
            var obj = new AmfObject(AmfTypes.Object);

            obj["classInstance"] = "classes::ShittyShip";

            obj["wardrobeSizRawe"]      = 10;
            obj["equipmentSizeRaw"]     = 10;
            obj["consumableSizeRaw"]    = 10;
            obj["valuablesSizeRaw"]     = 10;
            obj["toysSizeRaw"]          = 10;

            obj["modelDisplay"]     = "UNKNOWN";
            obj["captainDisplay"]   = "UNKNOWN";
            obj["factionDisplay"]   = "UNKNOWN";

            return obj;
        }

        protected override AmfObject CreateNewObject()
        {
            return CreateNewShipObject();
        }
    }

    public class ShipVM : CreatureVM
    {

        public ShipVM(GameVM game, AmfObject obj)
            : base(game, obj)
        {
            // Perks
            var xmlPerks = XmlData.Current.PerkGroups.SelectMany(x => x.Perks).ToArray();
            var unknownPerkGroup = XmlData.Current.PerkGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(PerksArray, xmlPerks, unknownPerkGroup.Perks);
            UpdatePerks();
        }

        public new string Name
        {
            get { return GetString("short"); }
            set
            {
                SetValue("short", value);
                OnPropertyChanged("Description");
            }
        }

        public new int Shields
        {
            get
            {
                int shields = 0;
                if (null != ShieldObj)
                {
                    shields += ShieldObj.GetInt("shields", 0);
                }
                return shields;
            }
            set
            {
                ShieldObj["shields"] = value;
            }
        }

        public new int MaxShields
        {
            get
            {
                return Shields;
            }
        }

        public double ShipAgility
        {
            get { return GetDouble("reflexesRaw"); }
            set
            {
                SetValue("reflexesRaw", value);
                OnPropertyChanged("EffectiveAgility");
            }
        }

        public double EffectiveAgility
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 20;
                return ShipAgility + bonus;
            }
        }

        public double ShipSpeed
        {
            get { return GetDouble("physiqueRaw"); }
            set
            {
                SetValue("physiqueRaw", value);
                OnPropertyChanged("EffectiveSpeed");
            }
        }

        public double EffectiveSpeed
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 20;
                return ShipSpeed + bonus;
            }
        }

        public double ShipPowerGeneration
        {
            get { return GetDouble("willpowerRaw"); }
            set
            {
                SetValue("willpowerRaw", value);
                OnPropertyChanged("EffectivePowerGeneration");
            }
        }

        public double EffectivePowerGeneration
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 20;
                return ShipPowerGeneration + bonus;
            }
        }

        public double ShipSensors
        {
            get { return GetDouble("aimRaw"); }
            set
            {
                SetValue("aimRaw", value);
                OnPropertyChanged("EffectiveSensors");
            }
        }

        public double EffectiveSensors
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 20;
                return ShipSensors + bonus;
            }
        }

        public double ShipSystems
        {
            get { return GetDouble("intelligenceRaw"); }
            set
            {
                SetValue("intelligenceRaw", value);
                OnPropertyChanged("EffectiveSystems");
            }
        }

        public double EffectiveSystems
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 20;
                return ShipSystems + bonus;
            }
        }

        public double ShipCrewCapacity
        {
            get { return GetDouble("shipCapacityRaw"); }
            set
            {
                SetValue("shipCapacityRaw", value);
                OnPropertyChanged("EffectiveCrewCapacity");
            }
        }

        public double EffectiveCrewCapacity
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 2;
                return (ShipCrewCapacity + bonus) - EquippedModulesCount;
            }
        }

        public double ShipGunCapacity
        {
            get { return GetDouble("shipGunCapacityRaw"); }
            set
            {
                SetValue("shipGunCapacityRaw", value);
                OnPropertyChanged("EffectiveGunCapacity");
            }
        }

        public double EffectiveGunCapacity
        {
            get
            {
                double bonus = EquippedItemCountByClass(0) * 20;
                return ShipGunCapacity + bonus;
            }
        }

        public double WardrobeSize
        {
            get { return GetDouble("wardrobeSizeRaw"); }
            set
            {
                SetValue("wardrobeSizeRaw", value);
            }
        }

        public double EquipmentSize
        {
            get { return GetDouble("equipmentSizeRaw"); }
            set
            {
                SetValue("equipmentSizeRaw", value);
            }
        }

        public double ConsumableSize
        {
            get { return GetDouble("consumableSizeRaw"); }
            set
            {
                SetValue("consumableSizeRaw", value);
            }
        }

        public double ValuablesSize
        {
            get { return GetDouble("valuablesSizeRaw"); }
            set
            {
                SetValue("valuablesSizeRaw", value);
            }
        }

        public double ToysSize
        {
            get { return GetDouble("toysSizeRaw"); }
            set
            {
                SetValue("toysSizeRaw", value);
            }
        }

        public string ModelDisplay
        {
            get { return GetString("modelDisplay"); }
            set { SetValue("modelDisplay", value); }
        }

        public string CaptainDisplay
        {
            get { return GetString("captainDisplay"); }
            set { SetValue("captainDisplay", value); }
        }

        public string FactionDisplay
        {
            get { return GetString("factionDisplay"); }
            set { SetValue("factionDisplay", value); }
        }

        public int EquippedItemCountByClass(int iClass)
        {
            return 0;
        }

        public int EquippedModulesCount
        {
            get
            {
                return InventoryObj.Count();
            }
        }

        public AmfObject InventoryObj
        {
            get
            {
                return GetObj("inventory");
            }
            set
            {
                SetValue("inventory", value);
                OnPropertyChanged("EquippedModulesCount");
                OnPropertyChanged("EffectiveCrewCapacity");
            }
        }

        public AmfObject ShieldObj
        {
            get
            {
                if (null == GetObj("shield"))
                {
                    SetValue("shield", NewShipShieldGenerator());
                }
                return GetObj("shield");
            }
        }

        public int ShieldDefense
        {
            get
            {
                return ShieldObj.GetInt("shieldDefense", 0);
            }
            set
            {
                ShieldObj["shieldDefense"] = value;
            }
        }

        public int EffectiveShieldDefense
        {
            get
            {
                int bonus = 0;
                return ShieldDefense + bonus;
            }
        }


        public AmfObject ArmorObj
        {
            get
            {
                if (null == GetObj("armor"))
                {
                    SetValue("armor", NewShipArmor());
                }
                return GetObj("armor");
            }
        }


        public int ArmorDefense
        {
            get
            {
                return ArmorObj.GetInt("defense", 25);
            }
            set
            {
                ArmorObj["defense"] = value;
            }
        }

        public int EffectiveArmorDefense
        {
            get
            {
                int bonus = 0;
                return ArmorDefense + bonus;
            }
        }


        public String Description
        {
            get
            {
                return Name;
            }
        }


        public static AmfObject NewShipShieldGenerator()
        {
            var item = new AmfObject(AmfTypes.Object);
            item["classInstance"] = "classes.ShittyShips.ShittyShipGear.Misc::ShipShield";
            item["shortName"] = "ShieldGen";
            item["longName"] = "shield generator";
            item["description"] = "a shield generator";
            item["quantity"] = 1;
            item["version"] = 1;
            item["shields"] = 1000;
            item["hasRandomProperties"] = true;
            return item;
        }


        public static AmfObject NewShipArmor()
        {
            var item = new AmfObject(AmfTypes.Object);
            item["classInstance"] = "classes.ShittyShips.ShittyShipGear.Misc::ShipArmor";
            item["shortName"] = "Armor";
            item["longName"] = "armor plating";
            item["description"] = "an armor plate";
            item["quantity"] = 1;
            item["version"] = 1;
            item["defense"] = 25;
            item["hasRandomProperties"] = true;
            return item;
        }
    }
}
