using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public class CharacterVM : CreatureVM
    {
        private ItemContainerVM _inventory;
        private ItemSlotVM _shield;
        private ItemSlotVM _meleeWeapon;
        private ItemSlotVM _rangedWeapon;
        private ItemSlotVM _accessory;
        private ItemSlotVM _armor;
        private ItemSlotVM _upperUndergarment;
        private ItemSlotVM _lowerUndergarment;
        private string _characterName;

        public enum CharacterClasses
        {
            Smuggler    = 0
          , Mercenary   = 1
          , Engineer    = 2
        }

        public CharacterVM(GameVM game, AmfObject obj)
            : base(game, obj)
        {

            // body parts
            Breasts = new BreastArrayVM(this, GetObj("breastRows"));
            Vaginas = new VaginaArrayVM(this, GetObj("vaginas"));
            Cocks = new CockArrayVM(this, GetObj("cocks"));
            Ass = new VaginaVM(this, GetObj("ass"));

            // Wombs
            PregnancyData = new PregnancyDataArrayVM(this, GetObj("pregnancyData") ?? new AmfObject(AmfTypes.Array));

            // Perks
            var xmlPerks = XmlData.Current.PerkGroups.SelectMany(x => x.Perks).ToArray();
            var unknownPerkGroup = XmlData.Current.PerkGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(PerksArray, xmlPerks, unknownPerkGroup.Perks);
            UpdatePerks();

            // KeyItems
            var xmlKeys = XmlData.Current.KeyItemGroups.SelectMany(x => x.KeyItems).ToArray();
            var unknownKeyItemGroup = XmlData.Current.KeyItemGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(KeyItemsArray, xmlKeys, unknownKeyItemGroup.KeyItems);
            UpdateKeyItems();

            // Statuses
            var xmlStatusEffects = XmlData.Current.StatusEffectGroups.SelectMany(x => x.StatusEffects).ToArray();
            var unknownStatusEffectsGroup = XmlData.Current.StatusEffectGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(StatusEffectsArray, xmlStatusEffects, unknownStatusEffectsGroup.StatusEffects);
            UpdateStatusEffects();

            // inventory
            List<String> types = new List<String>();
            foreach (XmlItemGroup type in XmlData.Current.ItemGroups)
            {
                types.Add(type.Name);
            }

            var containers = new List<ItemContainerVM>();
            _inventory = new ItemContainerVM(this, "Inventory", types);
            AmfObject inv = GetObj("inventory") ?? new AmfObject(AmfTypes.Array);
            var maxSlots = MaxInventoryItems;
            for (int i = 0; i < maxSlots; ++i)
            {
                AmfObject item = (AmfObject)inv[i];
                if (null != item)
                {
                    _inventory.Add(item);
                }
            }
            containers.Add(_inventory);
            GameVM.ImportUnknownItems(containers, types);
            UpdateInventory();

            // Complete slots creation
            ItemContainers = new UpdatableCollection<ItemContainerVM>(containers);

            // shield, weapons, accessory, armors
            _shield = new ItemSlotVM(this, GetObj("shield"), types);
            _meleeWeapon = new ItemSlotVM(this, GetObj("meleeWeapon"), types);
            _rangedWeapon = new ItemSlotVM(this, GetObj("rangedWeapon"), types);
            _armor = new ItemSlotVM(this, GetObj("armor"), types);
            _accessory = new ItemSlotVM(this, GetObj("accessory"), types);
            _upperUndergarment = new ItemSlotVM(this, GetObj("upperUndergarment"), types);
            _lowerUndergarment = new ItemSlotVM(this, GetObj("lowerUndergarment"), types);
            UpdateInventory();
        }

        public void UpdateAll(string name = null)
        {
            if (null != name)
            {
                CharacterName = name;
            }

            UpdateKeyItems();

            UpdatePerks();

            UpdateStatusEffects();
        }

        public void BeforeSerialization()
        {
            CleanupInventory();
        }

        public BreastArrayVM Breasts { get; private set; }
        public VaginaArrayVM Vaginas { get; private set; }
        public CockArrayVM Cocks { get; private set; }
        public VaginaVM Ass { get; private set; }

        public PregnancyDataArrayVM PregnancyData { get; private set; }

        public string GenderId
        {
            get
            {
                var gender = "N";
                if (Vaginas.Count > 0 && Cocks.Count > 0)
                {
                    gender = "H";
                }
                else if (Cocks.Count > 0 && Feminity >= 50)
                {
                    gender = "T";
                }
                else if (Vaginas.Count > 0 && Feminity < 50)
                {
                    gender = "C";
                }
                else if (Cocks.Count > 0)
                {
                    gender = "M";
                }
                else if (Vaginas.Count > 0)
                {
                    gender = "F";
                }
                return gender;
            }
        }

        public bool IsMale
        {
            get
            {
                return HasStatusEffect("Force Male Gender") || ("M" == GenderId) || (Feminity < 50);
            }
        }

        public bool IsFemale
        {
            get
            {
                return HasStatusEffect("Force Fem Gender") || ("F" == GenderId) || (Feminity > 50);
            }
        }

        public bool IsNeuter
        {
            get
            {
                return HasStatusEffect("Force It Gender") || ("N" == GenderId);
            }
        }

        #region GeneralPage

        public new string Name
        {
            get { return GetString("short"); }
            set
            {
                SetValue("short", value);
                //SetValue("uniqueName", value);
                if (Game.IsPC)
                {
                    Game.SetValue("saveName", value);
                }
            }
        }

        public string CharacterName
        {
            get { return _characterName; }
            set { _characterName = value; }
        }

        public double Credits
        {
            get { return GetDouble("credits"); }
            set { SetValue("credits", value); }
        }

        public double Physique
        {
            get { return GetDouble("physiqueRaw"); }
            set
            {
                SetValue("physiqueRaw", value);
                OnPropertyChanged("PhysiquePercent");
            }
        }

        public double PhysiqueMod
        {
            get { return GetDouble("physiqueMod"); }
            set
            {
                SetValue("physiqueMod", value);
                OnPropertyChanged("PhysiquePercent");
            }
        }

        public double EffectivePhysique
        {
            get { return Physique + PhysiqueMod; }
        }

        public double PhysiquePercent
        {
            get { return Math.Round((EffectivePhysique / MaxCoreStat) * 100); }
        }

        public double Reflexes
        {
            get { return GetDouble("reflexesRaw"); }
            set
            {
                SetValue("reflexesRaw", value);
                OnPropertyChanged("ReflexesPercent");
            }
        }

        public double ReflexesMod
        {
            get { return GetDouble("reflexesMod"); }
            set
            {
                SetValue("reflexesMod", value);
                OnPropertyChanged("ReflexesPercent");
            }
        }

        public double EffectiveReflexes
        {
            get { return Reflexes + ReflexesMod; }
        }

        public double ReflexesPercent
        {
            get { return Math.Round((EffectiveReflexes / MaxCoreStat) * 100); }
        }

        public double Aim
        {
            get { return GetDouble("aimRaw"); }
            set
            {
                SetValue("aimRaw", value);
                OnPropertyChanged("AimPercent");
            }
        }

        public double AimMod
        {
            get { return GetDouble("aimMod"); }
            set
            {
                SetValue("aimMod", value);
                OnPropertyChanged("AimPercent");
            }
        }

        public double EffectiveAim
        {
            get { return Aim + AimMod; }
        }

        public double AimPercent
        {
            get { return Math.Round((EffectiveAim / MaxCoreStat) * 100); }
        }

        public double Intelligence
        {
            get { return GetDouble("intelligenceRaw"); }
            set
            {
                SetValue("intelligenceRaw", value);
                OnPropertyChanged("IntelligencePercent");
            }
        }

        public double IntelligenceMod
        {
            get { return GetDouble("intelligenceMod"); }
            set
            {
                SetValue("intelligenceMod", value);
                OnPropertyChanged("IntelligencePercent");
            }
        }

        public double EffectiveIntelligence
        {
            get { return Intelligence + IntelligenceMod; }
        }

        public double IntelligencePercent
        {
            get { return Math.Round((EffectiveIntelligence / MaxCoreStat) * 100); }
        }

        public double Willpower
        {
            get { return GetDouble("willpowerRaw"); }
            set
            {
                SetValue("willpowerRaw", value);
                OnPropertyChanged("WillpowerPercent");
            }
        }

        public double WillpowerMod
        {
            get { return GetDouble("willpowerMod"); }
            set
            {
                SetValue("willpowerMod", value);
                OnPropertyChanged("WillpowerPercent");
            }
        }

        public double EffectiveWillpower
        {
            get { return Willpower + WillpowerMod; }
        }

        public double WillpowerPercent
        {
            get { return Math.Round((EffectiveWillpower / MaxCoreStat) * 100); }
        }

        public double Libido
        {
            get { return GetDouble("libidoRaw"); }
            set { SetValue("libidoRaw", value); }
        }

        public double LibidoMod
        {
            get { return GetDouble("libidoMod"); }
            set { SetValue("libidoMod", value); }
        }

        public int MaxLibido
        {
            get { return 100; }
        }

        public double MaxCoreStat
        {
            get { return Level * 5; }
        }


        public new int MaxShields
        {
            get
            {
                int maxShields = 0;
                int shields = (Shield != null) ? Shield.Xml.GetFieldValueAsInt("shields") : 0;
                if (shields > 0)
                {
                    maxShields += shields;

                    maxShields += (MeleeWeapon != null) ? MeleeWeapon.Xml.GetFieldValueAsInt("shields") : 0;
                    maxShields += (RangedWeapon != null) ? RangedWeapon.Xml.GetFieldValueAsInt("shields") : 0;

                    maxShields += (Armor != null) ? Armor.Xml.GetFieldValueAsInt("shields") : 0;
                    maxShields += (UpperUndergarment != null) ? UpperUndergarment.Xml.GetFieldValueAsInt("shields") : 0;
                    maxShields += (LowerUndergarment != null) ? LowerUndergarment.Xml.GetFieldValueAsInt("shields") : 0;

                    if (HasPerk("Shield Tweaks"))
                    {
                        maxShields += Level * 2;
                    }
                    if (HasPerk("Shield Booster"))
                    {
                        maxShields += Level * 8;
                    }
                    if (HasPerk("Attack Drone"))
                    {
                        maxShields += (Level * 3);
                    }

                    //Debuffs!
                    if (HasStatusEffect("Rusted Emitters"))
                    {
                        maxShields = (int)Math.Round(maxShields * 0.75);
                    }
                }
                return maxShields;
            }
        }

        public new int MaxHP
        {
            get
            {
                var bonus = 0;
                //TODO check items for fortification effects and add to bonus

                double maxhp = 15 + (Level - 1) * 15 + HPMod + bonus;

                // class bonuses
                switch ((CharacterClasses)CharacterClass)
                {

                    case CharacterClasses.Mercenary:
                        maxhp += Level * 5;
                        break;
                    case CharacterClasses.Engineer:
                        maxhp -= Level * 5;
                        break;
                }

                /// status effects
                if (HasStatusEffect("Heart Tea"))
                {
                    maxhp = maxhp * 1.1;
                }
                var status = GetStatus("Well-Groomed");
                if ((null != status) && status.IsOwned)
                {
                    if (status.Value1 != 0)
                    {
                        maxhp = maxhp * status.Value1;
                    }
                }

                return (int)maxhp;
            }
        }

        public int Level
        {
            get { return GetInt("level"); }
            set
            {
                SetValue("level", value);
                OnPropertyChanged("MaxXP");
                OnPropertyChanged("MaxXPLabel");
                OnPropertyChanged("MaxCoreStat");
                OnPropertyChanged("MaxHP");
                OnPropertyChanged("MaxShields");
            }
        }

        public int MaxLevel
        {
            get { return 10; }
        }

        public int CharacterClass
        {
            get { return GetInt("characterClass"); }
            set { SetValue("characterClass", value); }
        }

        public int PerkPoints
        {
            get { return GetInt("unclaimedClassPerks"); }
            set { SetValue("unclaimedClassPerks", value); }
        }

        public int StatPoints
        {
            get
            {
                if (Game.IsPC)
                {
                    return GetInt("unspentStatPoints");
                }
                return 0;
            }
            set
            {
                if (Game.IsPC)
                {
                    SetValue("unspentStatPoints", value);
                }
            }
        }

        public double XP
        {
            get { return GetDouble("XPRaw"); }
            set { SetValue("XPRaw", value); }
        }

        public int MaxXP
        {
            get
            {
                int level = Level;
                double xpMax = level * level * level * level * 100;
                if (level >= 5)
                {
                    xpMax *= 1.5;
                }
                if (level >= 8)
                {
                    xpMax *= 1.5;
                }
                return (int)xpMax;
            }
        }

        public String MaxXPLabel
        {
            get
            {
                int maxXP = MaxXP;
                if (maxXP > 1000000)
                {
                    double val = (maxXP / 1000000);
                    return val + "m";
                }
                else if (maxXP > 1000)
                {
                    double val = (maxXP / 100);
                    return val + "k";
                }
                return "" + maxXP;
            }
        }

        public double Lust
        {
            get { return GetDouble("lustRaw"); }
            set { SetValue("lustRaw", value); }
        }

        public double LustMod
        {
            get { return GetDouble("lustMod"); }
            set { SetValue("lustMod", value); }
        }

        public int MaxLust
        {
            get
            {
                int max = 100;
                var perk = GetPerk("Inhuman Desire", "MaxLust");
                if (perk.IsOwned)
                {
                    max += (int)perk.Value1;
                }
                if (HasPerk("Corrupted") && (100 + max < 200))
                {
                    max = 200;
                }
                return max;
            }
        }

        public double Taint
        {
            get { return GetDouble("taintRaw"); }
            set { SetValue("taintRaw", value); }
        }

        public double TaintMod
        {
            get { return GetDouble("taintMod"); }
            set { SetValue("taintMod", value); }
        }

        public double Energy
        {
            get { return GetDouble("energyRaw"); }
            set { SetValue("energyRaw", value); }
        }

        public double EnergyMod
        {
            get { return GetDouble("energyMod"); }
            set { SetValue("energyMod", value); }
        }

        public int MaxEnergy
        {
            get
            {
                int max = 100;
                var perk = GetPerk("Heroic Reserves", "MaxEnergy");
                if (perk.IsOwned)
                {
                    max += 33;
                }
                return max;
            }
        }

        public int Personality
        {
            get { return GetInt("personality"); }
            set
            {
                SetValue("personality", value);
                OnPropertyChanged("PersonalityTip");
            }
        }

        public String PersonalityTip
        {
            get
            {
                if (Personality <= 33) return "Nice";
                if (Personality <= 66) return "Mischievous";
                return "Ass";
            }
        }

        public double Exhibitionism
        {
            get { return GetDouble("exhibitionismRaw"); }
            set { SetValue("exhibitionismRaw", value); }
        }

        public int Tallness
        {
            get { return GetInt("tallness"); }
            set { SetValue("tallness", value); }
        }

        public int Thickness
        {
            get { return GetInt("thickness"); }
            set { SetValue("thickness", value); }
        }

        public int Feminity
        {
            get { return GetInt("femininity"); }
            set
            {
                SetValue("femininity", value);
                OnPropertyChanged("LipRating");
                OnPropertyChanged("LipRatingTip");
            }
        }

        public int Tone
        {
            get { return GetInt("tone"); }
            set { SetValue("tone", value); }
        }

        public int SkinType
        {
            get { return GetInt("skinType"); }
            set
            {
                if (value != SkinType)
                {
                    SetValue("skinType", value);
                    UpdateFlags(value, this.SkinFlags, "SkinFlags");
                }
            }
        }

        protected override List<string> GetDefaultFlags(int type, string propertyName = null)
        {
            if (propertyName.Equals("SkinFlags"))
                return GetDefaultSkinFlags(type);
            else if (propertyName.Equals("FaceFlags"))
                return GetDefaultFaceFlags(type);
            else if (propertyName.Equals("TongueFlags"))
                return GetDefaultTongueFlags(type);
            else if (propertyName.Equals("ArmFlags"))
                return GetDefaultArmFlags(type);
            else if (propertyName.Equals("LegFlags"))
                return GetDefaultLegFlags(type);
            else if (propertyName.Equals("TailFlags"))
                return GetDefaultTailFlags(type);
            else
                return null;
        }

        private List<string> GetDefaultSkinFlags(int sType)
        {
            List<string> defaultFlags = new List<string>();
            switch (sType)
            {
                case 2: //Scales
                case 4: //Chitin
                case 5: //Feathers
                default:
                    //All of the above have no default flags, so do nothing
                    break;
                case 0: //Skin
                case 6: //Latex
                case 7: //Plant
                    defaultFlags.Add("Smooth");
                    break;
                case 1: //Fur
                    defaultFlags.Add("Fluffy");
                    defaultFlags.Add("Thick");
                    break;
                case 3: //Goo
                    defaultFlags.Add("Squishy");
                    defaultFlags.Add("Lubricated");
                    defaultFlags.Add("Absorbent");
                    break;
                case 8: //Bark
                    defaultFlags.Add("Thick");
                    break;

            }
            return defaultFlags;
        }

        public String SkinTone
        {
            get { return GetString("skinTone"); }
            set { SetValue("skinTone", value); }
        }

        public String SkinAccent
        {
            get { return GetString("skinAccent"); }
            set { SetValue("skinAccent", value); }
        }

        public List<FlagItem> SkinFlags
        {
            get { return getFlagList(GetObj("skinFlags"), XmlData.Current.Body.SkinFlags); }
        }

        public String FurColor
        {
            get { return GetString("furColor"); }
            set { SetValue("furColor", value); }
        }

        public String ScaleColor
        {
            get { return GetString("scaleColor"); }
            set { SetValue("scaleColor", value); }
        }

        public int AlcoholTolerance
        {
            get
            {
                var toleranceStat = GetStatus("Tolerance", "AlcoholTolerance");
                return (int)toleranceStat.Value1;
            }
            set
            {
                var toleranceStat = GetStatus("Tolerance", "AlcoholTolerance");
                if (!toleranceStat.IsOwned)
                {
                    toleranceStat.IsOwned = true;
                }
                toleranceStat.Value1 = value;
            }
        }

        #endregion

        #region HeadPage
        public int AntennaeCount
        {
            get { return GetInt("antennae"); }
            set { SetValue("antennae", value); }
        }

        public int AntennaeType
        {
            get { return GetInt("antennaeType"); }
            set
            {
                SetValue("antennaeType", value);
                OnPropertyChanged("AntennaOptionsEnabled");
            }
        }

        public bool AntennaOptionsEnabled
        {
            get { return (0 != AntennaeType); }
        }

        public int HornCount
        {
            get { return GetInt("horns"); }
            set { SetValue("horns", value); }
        }

        public int HornLength
        {
            get { return GetInt("hornLength"); }
            set { SetValue("hornLength", value); }
        }

        public int HornType
        {
            get { return GetInt("hornType"); }
            set
            {
                SetValue("hornType", value);
                OnPropertyChanged("HornOptionsEnabled");
            }
        }

        public bool HornOptionsEnabled
        {
            get { return (0 != HornType); }
        }

        public double HairLength
        {
            get { return GetDouble("hairLength"); }
            set { SetValue("hairLength", value); }
        }

        public String HairColor
        {
            get { return GetString("hairColor"); }
            set { SetValue("hairColor", value); }
        }

        public String HairStyle
        {
            get
            {
                if (GetString("hairStyle") == "null")
                {
                    return "";
                }
                return GetString("hairStyle");
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    SetValue("hairStyle", "null");
                }
                else
                {
                    SetValue("hairStyle", value);
                }
            }
        }

        public int HairType
        {
            get { return GetInt("hairType"); }
            set { SetValue("hairType", value); }
        }

        public int FaceType
        {
            get { return GetInt("faceType"); }
            set
            {
                if (value != FaceType)
                {
                    SetValue("faceType", value);
                    UpdateFlags(value, this.FaceFlags, "FaceFlags");
                }
            }
        }

        private List<string> GetDefaultFaceFlags(int fType)
        {
            List<string> defaultFlags = new List<string>();
            switch (fType)
            {
                case 5:  //Vulpine
                case 9:  //Lapine
                case 10: //Avian
                case 11: //Draconic
                case 12: //Lizan
                case 17: //Kangaroo
                case 19: //Shark
                case 24: //Kui-tan
                case 25: //Human Masked
                case 26: //Mouse
                case 27: //Mouseman
                case 72: //Swine
                case 76: //Sheep
                case 77: //Lesser Panda
                default:
                    //All of the above have no default flags, so do nothing
                    break;
                case 0:  //Human
                case 39: //Naleen
                    defaultFlags.Add("Smooth");
                    break;
                case 1:  //Equine
                case 2:  //Bovine
                case 50: //Goat
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Muzzled");
                    break;
                case 3:  //Canine
                case 4:  //Feline
                case 20: //Suula
                case 40: //Panda
                case 45: //Badger
                case 71: //Worg
                case 75: //Lupine
                    defaultFlags.Add("Muzzled");
                    break;
                case 14: //Frog
                    defaultFlags.Add("Smooth");
                    break;
                case 18: //Gabilani
                case 81: //Mothrine
                    defaultFlags.Add("Angular");
                    break;
                case 66: //Korgonne
                    defaultFlags.Add("Muzzled");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Fluffy");
                    break;
                case 82: //Frostwyrm
                    defaultFlags.Add("Angular");
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Muzzled");
                    break;
            }
            return defaultFlags;
        }

        public List<FlagItem> FaceFlags
        {
            get { return getFlagList(GetObj("faceFlags"), XmlData.Current.Body.FaceFlags); }
        }

        public List<FlagItem> EarFlags
        {
            get { return getFlagList(GetObj("earFlags"), XmlData.Current.Body.EarFlags); }
        }

        public double EarLength
        {
            get { return GetDouble("earLength"); }
            set { SetValue("earLength", value); }
        }

        public bool EarLengthEnabled
        {
            get
            {
                //lookup ear type
                XmlEnum datum = XmlData.LookupEnumByID(XmlData.Current.Body.EarTypes, EarType);
                if (datum != null)
                {
                    //check if in ear length
                    foreach (string name in XmlData.Current.Body.EarLengthEnables)
                    {
                        if (name.Equals(datum.Name))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public int EarType
        {
            get { return GetInt("earType"); }
            set
            {
                SetValue("earType", value);
                OnPropertyChanged("EarLengthEnabled");
            }
        }

        public int EyeType
        {
            get { return GetInt("eyeType"); }
            set { SetValue("eyeType", value); }
        }

        public String EyeColor
        {
            get { return GetString("eyeColor"); }
            set { SetValue("eyeColor", value); }
        }

        public int TongueType
        {
            get { return GetInt("tongueType"); }
            set
            {
                if (value != TongueType)
                {
                    SetValue("tongueType", value);
                    UpdateFlags(value, this.TongueFlags, "TongueFlags");
                }
            }
        }

        private List<string> GetDefaultTongueFlags(int tType)
        {
            List<string> defaultFlags = new List<string>();
            switch (tType)
            {
                case 0:  //Human
                case 1:  //Equine
                case 2:  //Bovine
                case 3:  //Canine
                case 5:  //Vulpine
                case 9:  //Lapine
                case 10: //Avian
                case 11: //Draconic
                case 13: //Naga
                case 14: //Frog
                case 16: //Gooey
                case 40: //Panda
                case 45: //Badger

                default:
                    //All of the above have no default flags, so do nothing
                    break;
                case 6:  //Bee
                case 81: //Mothrine
                    defaultFlags.Add("Hollow");
                    defaultFlags.Add("Long");
                    break;
                case 15: //Demonic
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Tapered");
                    break;
                case 23: //Tentacle
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Lubricated");
                    break;
                case 42: //Raskvel
                case 82: //Frostwyrm
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Lubricated");
                    defaultFlags.Add("Squishy");
                    break;
                case 4:  //Feline
                case 49: //Leithan
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Prehensile");
                    break;
                case 57: //Ovir
                case 66: //Korgonne
                case 75: //Lupine
                    defaultFlags.Add("Long");
                    break;
            }
            return defaultFlags;
        }

        public List<FlagItem> TongueFlags
        {
            get { return getFlagList(GetObj("tongueFlags"), XmlData.Current.Body.TongueFlags); }
        }

        public int LipSize
        {
            get { return GetInt("lipMod"); }
            set
            {
                SetValue("lipMod", value);
                OnPropertyChanged("LipRating");
                OnPropertyChanged("LipRatingTip");
            }
        }

        public int LipRating
        {
            get { return LipSize + (Feminity / 25); }
        }

        public string LipRatingTip
        {
            get
            {
                string result = "";
                int lips = LipRating;
                if (lips <= 1)
                {
                    result = "pencil-thin";
                }
                else if (lips <= 2)
                {
                    result = "supple";
                }
                else if (lips <= 3)
                {
                    result = "plump";
                }
                else if (lips <= 4)
                {
                    result = "luscious";
                }
                else if (lips <= 5)
                {
                    result = "swollen";
                }
                else if (lips <= 6)
                {
                    result = "exquisitely large";
                }
                else if (lips <= 7)
                {
                    result = "bloated";
                }
                else if (lips <= 8)
                {
                    result = "whorish";
                }
                else
                {
                    result = "scylla-tier";
                }
                return result;
            }
        }

        public String LipColor
        {
            get { return GetString("lipColor"); }
            set { SetValue("lipColor", value); }
        }

        public double BeardLength
        {
            get { return GetDouble("beardLength"); }
            set { SetValue("beardLength", value); }
        }

        public int BeardType
        {
            get { return GetInt("beardType"); }
            set { SetValue("beardType", value); }
        }

        public int BeardStyle
        {
            get { return GetInt("beardStyle"); }
            set { SetValue("beardStyle", value); }
        }

        #endregion

        #region BodyPage

        public bool HasGills
        {
            get { return GetBool("gills"); }
            set { SetValue("gills", value); }
        }

        public double Elasticity
        {
            get { return GetDouble("elasticity"); }
            set { SetValue("elasticity", value); }
        }

        public int HipRating
        {
            get { return GetInt("hipRatingRaw"); }
            set
            {
                SetValue("hipRatingRaw", value);
                OnPropertyChanged("HipRatingTip");
            }
        }

        public int HipRatingMod
        {
            get { return GetInt("hipRatingMod"); }
            set
            {
                SetValue("hipRatingMod", value);
                OnPropertyChanged("HipRatingTip");
            }
        }

        public int EffectiveHipRating
        {
            get { return HipRating + HipRatingMod; }
        }

        public string HipRatingTip
        {
            get
            {
                var isMale = IsMale;
                if (EffectiveHipRating >= 20) return isMale ? "inhumanly-wide" : "broodmother";
                if (EffectiveHipRating >= 15) return isMale ? "voluptuous" : "child-bearing";
                if (EffectiveHipRating >= 10) return isMale ? "wide" : "curvy";
                if (EffectiveHipRating >= 6) return isMale ? "ample" : "girly";
                if (EffectiveHipRating >= 4) return "well-formed";
                if (EffectiveHipRating >= 2) return "slender";
                return "boyish";
            }
        }

        public int ButtRating
        {
            get { return GetInt("buttRatingRaw"); }
            set
            {
                SetValue("buttRatingRaw", value);
                OnPropertyChanged("ButtRatingTip");
            }
        }

        public int ButtRatingMod
        {
            get { return GetInt("buttRatingMod"); }
            set
            {
                SetValue("buttRatingMod", value);
                OnPropertyChanged("ButtRatingTip");
            }
        }

        public int EffectiveButtRating
        {
            get { return ButtRating + ButtRatingMod; }
        }

        public string ButtRatingTip
        {
            get
            {
                if (EffectiveButtRating >= 20) return "colossal";
                if (EffectiveButtRating >= 16) return "huge";
                if (EffectiveButtRating >= 13) return "voluminous";
                if (EffectiveButtRating >= 10) return "spacious";
                if (EffectiveButtRating >= 8) return "substantial";
                if (EffectiveButtRating >= 6) return "shapely";
                if (EffectiveButtRating >= 4) return "regular";
                if (EffectiveButtRating >= 2) return "compact";
                return "very small";
            }
        }

        public int BellyRating
        {
            get { return GetInt("bellyRatingRaw"); }
            set { SetValue("bellyRatingRaw", value); }
        }

        public int BellyRatingMod
        {
            get { return GetInt("bellyRatingMod"); }
            set { SetValue("bellyRatingMod", value); }
        }

        public int EffectiveBellyRating
        {
            get { return BellyRating + BellyRatingMod; }
        }

        public int ArmType
        {
            get { return GetInt("armType"); }
            set
            {
                if (value != ArmType)
                {
                    SetValue("armType", value);
                    UpdateFlags(value, this.ArmFlags, "ArmFlags");
                }
            }
        }

        private List<string> GetDefaultArmFlags(int aType)
        {
            List<string> defaultFlags = new List<string>();
            switch (aType)
            {
                case 0:  //Human
                case 1:  //Equine
                case 5:  //Vulpine
                case 7:  //Arachnid
                case 8:  //Drider
                case 9:  //Lapine
                case 10: //Avian
                case 14: //Frog
                case 19: //Shark
                case 23: //Tentacle
                case 52: //Simii
                case 53: //Daynar
                case 67: //Flower
                case 77: //Lesser Panda
                default:
                    //All of the above have no default flags, so do nothing
                    break;
                case 3:  //Canine
                case 4:  //Feline
                case 24: //Kui-tan
                case 40: //Panda
                case 45: //Badger
                case 75: //Lupine
                    defaultFlags.Add("Furred");
                    break;
                case 6:  //Bee
                case 49: //Leithan
                case 55: //Nyrea
                case 60: //Myr
                case 81: //Mothrine
                    defaultFlags.Add("Chitinous");
                    break;
                case 15: //Demonic
                    defaultFlags.Add("Chitinous");
                    defaultFlags.Add("Spiked");
                    break;
                case 57: //Ovir
                    defaultFlags.Add("Scaled");
                    break;
                case 66: //Korgonne
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Paws");
                    break;
                case 20: //Suula
                    defaultFlags.Add("Feathered");
                    break;
                case 82: //Frostwyrm
                    defaultFlags.Add("Scaled");
                    defaultFlags.Add("Smooth");
                    break;
            }
            return defaultFlags;
        }

        public List<FlagItem> ArmFlags
        {
            get { return getFlagList(GetObj("armFlags"), XmlData.Current.Body.ArmFlags); }
        }

        public int LegCount
        {
            get { return GetInt("legCount"); }
            set { SetValue("legCount", value); }
        }

        public int LegType
        {
            get { return GetInt("legType"); }
            set
            {
                if (value != LegType)
                {
                    SetValue("legType", value);
                    UpdateFlags(value, this.LegFlags, "LegFlags");
                }
            }
        }

        private List<string> GetDefaultLegFlags(int lType)
        {
            List<string> defaultFlags = new List<string>();
            switch (lType)
            {

                case 4:  //Feline
                case 5:  //Vulpine
                case 7:  //Arachnid
                case 8:  //Drider
                case 9:  //Lapine
                case 10: //Avian
                case 11: //Draconic
                case 13: //Naga
                case 17: //Kangaroo
                case 19: //Shark
                case 21: //Deer
                case 23: //Tentacle
                case 32: //MLP
                case 45: //Badger
                case 52: //Simii
                case 53: //Daynar
                case 55: //Nyrea
                case 72: //Swine
                case 76: //Sheep
                case 77: //Lesser Panda
                default:
                    //All of the above have no default flags, so do nothing
                    break;
                case 0:  //Human
                    defaultFlags.Add("Plantigrade");
                    break;
                case 1:  //Equine
                case 2:  //Bovine
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Hooves");
                    break;
                case 3:  //Canine
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Paws");
                    break;
                case 6:  //Bee
                case 81: //Mothrine
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Chitinous");
                    defaultFlags.Add("Smooth");
                    break;
                case 12: //Lizan
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Scaled");
                    defaultFlags.Add("Paws");
                    break;
                case 14: //Frog
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Smooth");
                    defaultFlags.Add("Sticky");
                    break;
                case 15: //Demonic
                    defaultFlags.Add("Plantigrade");
                    break;
                case 16: //Gooey
                    defaultFlags.Add("Amorphous");
                    break;
                case 24: //Kui-tan
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Fluffy");
                    break;
                case 35: //Succubus
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Heels");
                    break;
                case 40: //Panda
                case 75: //Lupine
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Paws");
                    break;
                case 42: //Raskvel
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Scaled");
                    break;
                case 50: //Goat
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Hooves");
                    break;
                case 20: //Suula
                case 57: //Ovir
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Scaled");
                    break;
                case 60: //Myr
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Chitinous");
                    break;
                case 65: //Gryvain
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Scaled");
                    break;
                case 66: //Korgonne
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Paws");
                    break;
                case 82: //Frostwyrm
                    defaultFlags.Add("Digitigrade");
                    defaultFlags.Add("Furred");
                    break;
                case 85: //Janeria
                    defaultFlags.Add("Amorphous");
                    defaultFlags.Add("Plantigrade");
                    defaultFlags.Add("Smooth");
                    break;

            }
            return defaultFlags;
        }

        public List<FlagItem> LegFlags
        {
            get { return getFlagList(GetObj("legFlags"), XmlData.Current.Body.LegFlags); }
        }

        public int WingCount
        {
            get { return GetInt("wingCount"); }
            set { SetValue("wingCount", value); }
        }

        public int WingType
        {
            get { return GetInt("wingType"); }
            set
            {
                SetValue("wingType", value);
                OnPropertyChanged("WingOptionsEnabled");
            }
        }

        public bool WingOptionsEnabled
        {
            get { return (0 != WingType); }
        }

        public int TailCount
        {
            get { return GetInt("tailCount"); }
            set { SetValue("tailCount", value); }
        }

        public int TailType
        {
            get { return GetInt("tailType"); }
            set
            {
                if (value != TailType)
                {
                    SetValue("tailType", value);
                    UpdateFlags(value, this.TailFlags, "TailFlags");
                }
            }
        }

        private List<string> GetDefaultTailFlags(int tType)
        {
            List<string> defaultFlags = new List<string>();
            switch (tType)
            {
                case 0:  //None
                case 7:  //Arachnid
                case 8:  //Drider
                case 9:  //Lapine
                case 10: //Avian
                case 11: //Draconic
                case 13: //Naga
                case 17: //Kangaroo
                case 19: //Shark
                case 21: //Deer
                case 23: //Tentacle
                case 26: //Mouse
                case 29: //Floppy Dog
                case 43: //Sydian
                case 51: //Synthetic
                case 52: //Simii
                case 53: //Daynar
                case 72: //Swine
                case 76: //Sheep
                case 77: //Lesser Panda
                default:
                    //All of the above have no default flags, so do nothing
                    break;

                case 1:  //Equine
                    defaultFlags.Add("Long");
                    break;
                case 2:  //Bovine
                    defaultFlags.Add("Fluffy");
                    defaultFlags.Add("Long");
                    break;
                case 3:  //Canine
                case 5:  //Vulpine
                case 24: //Kui-tan
                case 75: //Lupine
                    defaultFlags.Add("Fluffy");
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Long");
                    break;
                case 4:  //Feline
                    defaultFlags.Add("Furred");
                    defaultFlags.Add("Long");
                    break;
                case 6:  //Bee
                    defaultFlags.Add("Chitinous");
                    defaultFlags.Add("Smooth");
                    defaultFlags.Add("Stinger Tip");
                    break;
                case 12: //Lizan
                case 42: //Raskvel
                    defaultFlags.Add("Scaled");
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Long");
                    break;
                case 14: //Frog
                    defaultFlags.Add("Smooth");
                    defaultFlags.Add("Sticky");
                    break;
                case 15: //Demonic
                case 46: //Vanae
                case 54: //Cockvine
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Long");
                    break;
                case 33: //Cuntsnake
                    defaultFlags.Add("Thick");
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Long");
                    break;
                case 40: //Panda
                case 45: //Badger
                    defaultFlags.Add("Fluffy");
                    defaultFlags.Add("Furred");
                    break;
                case 57: //Ovir
                    defaultFlags.Add("Scaled");
                    break;
                case 60: //Myr
                    defaultFlags.Add("Chitinous");
                    defaultFlags.Add("Smooth");
                    break;
                case 65: //Gryvain
                case 82: //Frostwyrm
                    defaultFlags.Add("Scaled");
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Tapered");
                    break;
                case 66: //Korgonne
                    defaultFlags.Add("Fluffy");
                    defaultFlags.Add("Furred");
                    break;
                case 20: //Suula
                    defaultFlags.Add("Long");
                    defaultFlags.Add("Scaled");
                    break;
            }
            return defaultFlags;
        }

        public int TailGenital
        {
            get { return GetInt("tailGenital"); }
            set
            {
                SetValue("tailGenital", value);
            }
        }

        public int TailGenitalRace
        {
            get { return GetInt("tailGenitalArg"); }
            set { SetValue("tailGenitalArg", value); }
        }

        public int TailVenom
        {
            get { return GetInt("tailVenom"); }
            set { SetValue("tailVenom", value); }
        }

        public int TailRecharge
        {
            get { return GetInt("tailRecharge"); }
            set { SetValue("tailRecharge", value); }
        }

        public String TailGenitalColor
        {
            get { return GetString("tailGenitalColor"); }
            set { SetValue("tailGenitalColor", value); }
        }

        public List<FlagItem> TailFlags
        {
            get { return getFlagList(GetObj("tailFlags"), XmlData.Current.Body.TailFlags); }
        }

        public int GenitalSpot
        {
            get { return GetInt("genitalSpot"); }
            set { SetValue("genitalSpot", value); }
        }

        public String NippleColor
        {
            get { return GetString("nippleColor"); }
            set { SetValue("nippleColor", value); }
        }

        public int NipplesPerBreast
        {
            get { return GetInt("nipplesPerBreast"); }
            set { SetValue("nipplesPerBreast", value); }
        }

        public double NippleLengthRatio
        {
            get { return GetDouble("nippleLengthRatio"); }
            set { SetValue("nippleLengthRatio", value); }
        }

        public double NippleWidthRatio
        {
            get { return GetDouble("nippleWidthRatio"); }
            set { SetValue("nippleWidthRatio", value); }
        }

        public int DickNippleType
        {
            get { return GetInt("dickNippleType"); }
            set { SetValue("dickNippleType", value); }
        }

        public int DickNippleMultiplier
        {
            get { return GetInt("dickNippleMultiplier"); }
            set { SetValue("dickNippleMultiplier", value); }
        }

        public int MilkType
        {
            get { return GetInt("milkType"); }
            set { SetValue("milkType", value); }
        }

        public double MilkFullness
        {
            get { return GetDouble("milkFullness"); }
            set { SetValue("milkFullness", value); }
        }

        public int MilkRate
        {
            get { return GetInt("milkRate"); }
            set { SetValue("milkRate", value); }
        }

        public int MilkMultiplier
        {
            get { return GetInt("milkMultiplier"); }
            set { SetValue("milkMultiplier", value); }
        }

        public int MilkStorageMultiplier
        {
            get { return GetInt("milkStorageMultiplier"); }
            set { SetValue("milkStorageMultiplier", value); }
        }

        #endregion

        #region GenitalPage

        public int GirlCumType
        {
            get { return GetInt("girlCumType"); }
            set { SetValue("girlCumType", value); }
        }

        public double GirlCumMultiplier
        {
            get { return GetDouble("girlCumMultiplierRaw"); }
            set { SetValue("girlCumMultiplierRaw", value); }
        }

        public double GirlCumMultiplierMod
        {
            get { return GetDouble("girlCumMultiplierMod"); }
            set { SetValue("girlCumMultiplierMod", value); }
        }

        public double Fertility
        {
            get { return GetDouble("fertilityRaw"); }
            set { SetValue("fertilityRaw", value); }
        }

        public int FertilityMod
        {
            get { return GetInt("fertilityMod"); }
            set { SetValue("fertilityMod", value); }
        }

        public double ClitLength
        {
            get { return GetDouble("clitLength"); }
            set { SetValue("clitLength", value); }
        }

        public bool AnalVirgin
        {
            get { return GetBool("analVirgin"); }
            set
            {
                SetValue("analVirgin", value);
                Ass.Hymen = value;
            }
        }

        public bool VaginalVirgin
        {
            get { return GetBool("vaginalVirgin"); }
            set { SetValue("vaginalVirgin", value); }
        }

        public bool CockVirgin
        {
            get { return GetBool("cockVirgin"); }
            set { SetValue("cockVirgin", value); }
        }

        public int CumType
        {
            get { return GetInt("cumType"); }
            set { SetValue("cumType", value); }
        }

        public double CumMultiplier
        {
            get { return GetDouble("cumMultiplierRaw"); }
            set { SetValue("cumMultiplierRaw", value); }
        }

        public int CumMultiplierMod
        {
            get { return GetInt("cumMultiplierMod"); }
            set { SetValue("cumMultiplierMod", value); }
        }

        public double CumQuality
        {
            get { return GetDouble("cumQualityRaw"); }
            set { SetValue("cumQualityRaw", value); }
        }

        public int CumQualityMod
        {
            get { return GetInt("cumQualityMod"); }
            set { SetValue("cumQualityMod", value); }
        }

        public int Balls
        {
            get { return GetInt("balls"); }
            set { SetValue("balls", value); }
        }

        public double BallSize
        {
            get { return GetDouble("ballSizeRaw"); }
            set { SetValue("ballSizeRaw", value); }
        }

        public double BallSizeMod
        {
            get { return GetDouble("ballSizeMod"); }
            set { SetValue("ballSizeMod", value); }
        }

        public double BallFullness
        {
            get { return GetDouble("ballFullness"); }
            set { SetValue("ballFullness", value); }
        }

        public double BallEfficiency
        {
            get { return GetDouble("ballEfficiency"); }
            set { SetValue("ballEfficiency", value); }
        }

        public double RefractoryRate
        {
            get { return GetDouble("refractoryRate"); }
            set { SetValue("refractoryRate", value); }
        }

        #endregion

        #region MiscPage

        public String Affinity
        {
            get { return GetString("affinity"); }
            set { SetValue("affinity", value); }
        }

        public String OriginalRace
        {
            get { return GetString("originalRace"); }
            set { SetValue("originalRace", value); }
        }

        public string ImpregnationType
        {
            get { return GetString("impregnationType"); }
            set { SetValue("impregnationType", value); }
        }

        public int PregnancyMultiplierRaw
        {
            get { return GetInt("pregnancyMultiplierRaw"); }
            set { SetValue("pregnancyMultiplierRaw", value); }
        }

        public int PregnancyMultiplierMod
        {
            get { return GetInt("pregnancyMultiplierMod"); }
            set { SetValue("pregnancyMultiplierMod", value); }
        }

        public int PregnancyIncubationBonusMotherRaw
        {
            get { return GetInt("pregnancyIncubationBonusMotherRaw"); }
            set { SetValue("pregnancyIncubationBonusMotherRaw", value); }
        }

        public int PregnancyIncubationBonusMotherMod
        {
            get { return GetInt("pregnancyIncubationBonusMotherMod"); }
            set { SetValue("pregnancyIncubationBonusMotherMod", value); }
        }

        public int PregnancyIncubationBonusFatherRaw
        {
            get { return GetInt("pregnancyIncubationBonusFatherRaw"); }
            set { SetValue("pregnancyIncubationBonusFatherRaw", value); }
        }

        public int PregnancyIncubationBonusFatherMod
        {
            get { return GetInt("pregnancyIncubationBonusFatherMod"); }
            set { SetValue("pregnancyIncubationBonusFatherMod", value); }
        }

        public int MaxWombCount
        {
            get
            {
                int count = 1;
                count += Vaginas.Count;
                return count;
            }
        }

        #endregion

        #region ItemPage
        public UpdatableCollection<ItemContainerVM> ItemContainers { get; private set; }

        public int MaxInventoryItems
        {
            get
            {
                int max = 10;
                var status = GetStatus("Backpack Upgrade");
                if (status.IsOwned)
                {
                    max += (int)status.Value1;
                }
                if (HasPerk("Hidden Loot"))
                {
                    max += 2;
                }
                if (null != Accessory)
                {
                    if ("Cargobot" == Accessory.Name)
                    {
                        max += 2;
                    }
                }
                if (null != Armor)
                {
                    if ("I.Coat" == Armor.Name)
                    {
                        max++;
                    }
                }
                return max;
            }
        }

        public void UpdateItemList()
        {
            //need to find a way to make search only apply to currently selected item somehow
            foreach (var slot in _inventory.Slots)
            {
                slot.UpdateItemGroups();
            }
        }

        public void UpdateInventory()
        {
            _inventory.Clear();
            AmfObject inv = GetObj("inventory");
            var maxSlots = MaxInventoryItems;
            for (int i = 0; i < maxSlots; ++i)
            {
                AmfObject item = (AmfObject)inv[i];
                if (item == null)
                {
                    //just add an empty item, we'll fix it later maybe
                    item = new AmfObject(AmfTypes.Object);
                    item["classInstance"] = XmlItem.Empty.ID;
                    item["shortName"] = XmlItem.Empty.Name;
                    item["quantity"] = 0;
                    item["version"] = 1;
                    inv.Push(item);
                }
                _inventory.Add(item);
            }
        }

        public void CleanupInventory()
        {
            //shift all the items around in the inventory
            AmfObject inv = GetObj("inventory");
            var maxSlots = MaxInventoryItems;
            AmfObject nInv = new AmfObject(AmfTypes.Array);
            for (int i = 0; i < maxSlots; ++i)
            {
                AmfObject item = (AmfObject)inv[i];
                if (item == null)
                {
                    continue;
                }
                if (item.GetString("classInstance") != XmlItem.Empty.ID)
                {
                    nInv.Push(item);
                }
            }
            SetValue("inventory", nInv);
        }

        public ItemSlotVM Shield
        {
            get { return _shield; }
        }

        public ItemSlotVM MeleeWeapon
        {
            get { return _meleeWeapon; }
        }

        public ItemSlotVM RangedWeapon
        {
            get { return _rangedWeapon; }
        }

        public ItemSlotVM Accessory
        {
            get { return _accessory; }
        }

        public ItemSlotVM Armor
        {
            get { return _armor; }
        }

        public ItemSlotVM UpperUndergarment
        {
            get { return _upperUndergarment; }
        }

        public ItemSlotVM LowerUndergarment
        {
            get { return _lowerUndergarment; }
        }

        #endregion

        #region KeyItemsPage

        public new void OnKeyItemAddedOrRemoved(string name, bool isOwned)
        {
            switch (name)
            {
                default:
                    break;
            }
        }

        #endregion

        #region PerksPage

        public new void OnPerkAddedOrRemoved(string name, bool isOwned)
        {
            // Grants/removes the appropriate bonuses when a perk is added or removed.
            // We do not add stats however since the user can already change them easily.
            switch (name)
            {
                case "Shield Tweaks":
                case "Shield Booster":
                case "Attack Drone":
                    OnPropertyChanged("MaxShields");
                    break;
                case "Inhuman Desire":
                    OnPropertyChanged("MaxLust");
                    break;
                case "Heroic Reserves":
                    OnPropertyChanged("MaxEnergy");
                    break;
                case "Hidden Loot":
                    OnPropertyChanged("MaxInventoryItems");
                    UpdateInventory();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region RawPage

        public new void OnStatusAddedOrRemoved(string name, bool isOwned)
        {
            // Grants/removes the appropriate bonuses when a status is added or removed.
            // We do not add stats however since the user can already change them easily.
            switch (name)
            {
                case "Force Fem Gender":
                    if (isOwned)
                    {
                        // there can be only one!
                        RemoveStatus("Force It Gender");
                        RemoveStatus("Force Male Gender");
                    }
                    OnPropertyChanged("IsMale");
                    OnPropertyChanged("IsFemale");
                    OnPropertyChanged("IsNeuter");
                    break;
                case "Force It Gender":
                    if (isOwned)
                    {
                        // there can be only one!
                        RemoveStatus("Force Fem Gender");
                        RemoveStatus("Force Male Gender");
                    }
                    OnPropertyChanged("IsMale");
                    OnPropertyChanged("IsFemale");
                    OnPropertyChanged("IsNeuter");
                    break;
                case "Force Male Gender":
                    if (isOwned)
                    {
                        // there can be only one!
                        RemoveStatus("Force Fem Gender");
                        RemoveStatus("Force It Gender");
                    }
                    OnPropertyChanged("IsMale");
                    OnPropertyChanged("IsFemale");
                    OnPropertyChanged("IsNeuter");
                    break;
                case "Rusted Emitters":
                    OnPropertyChanged("MaxShields");
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
