using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public class CharacterVM : ObjectVM {
        private ItemContainerVM _inventory;

        public CharacterVM(GameVM game, AmfObject obj)
            : base(obj) {

            Game = game;

            Breasts = new BreastArrayVM(game, GetObj("breastRows"));
            Vaginas = new VaginaArrayVM(game, GetObj("vaginas"));
            Cocks = new CockArrayVM(game, GetObj("cocks"));
            Ass = new VaginaVM(game, GetObj("ass"));

            List<String> types = new List<String>();
            foreach (XmlItemType type in XmlData.Current.ItemTypes) {
                types.Add(type.Name);
            }

            var containers = new List<ItemContainerVM>();
            _inventory = new ItemContainerVM(this, "Inventory", types);
            containers.Add(_inventory);
            UpdateInventory();

            // Complete slots creation
            ItemContainers = new UpdatableCollection<ItemContainerVM>(containers);

        }

        public void BeforeSerialization() {
            CleanupInventory();
        }

        public GameVM Game { get; set; }

        public BreastArrayVM Breasts { get; private set; }
        public VaginaArrayVM Vaginas { get; private set; }
        public CockArrayVM Cocks { get; private set; }
        public VaginaVM Ass { get; private set; }

        public string GenderId {
            get {
                var gender = "N";
                if (Vaginas.Count > 0 && Cocks.Count > 0) {
                    gender = "H";
                } else if (Cocks.Count > 0 && Feminity >= 50) {
                    gender = "T";
                } else if (Vaginas.Count > 0 && Feminity < 50) {
                    gender = "C";
                } else if (Cocks.Count > 0) {
                    gender = "M";
                } else if (Vaginas.Count > 0) {
                    gender = "F";
                }
                return gender;
            }
        }

        #region GeneralPage
        public new string Name {
            get { return GetString("short"); }
            set {
                SetValue("short", value);
                //SetValue("uniqueName", value);
                if (Game.IsPC) {
                    Game.SetValue("saveName", value);
                }
            }
        }

        public int Credits {
            get { return GetInt("credits"); }
            set { SetValue("credits", value); }
        }

        public int Physique {
            get { return GetInt("physiqueRaw"); }
            set { SetValue("physiqueRaw", value);
                OnPropertyChanged("PhysiquePercent");
            }
        }

        public int PhysiqueMod {
            get { return GetInt("physiqueMod"); }
            set { SetValue("physiqueMod", value);
                OnPropertyChanged("PhysiquePercent");
            }
        }

        public int EffectivePhysique {
            get { return Physique + PhysiqueMod; }
        }

        public double PhysiquePercent {
            get { return Math.Round((EffectivePhysique / MaxCoreStat) * 100); }
        }

        public int Reflexes {
            get { return GetInt("reflexesRaw"); }
            set { SetValue("reflexesRaw", value);
                OnPropertyChanged("ReflexesPercent");
            }
        }

        public int ReflexesMod {
            get { return GetInt("reflexesMod"); }
            set { SetValue("reflexesMod", value);
                OnPropertyChanged("ReflexesPercent");
            }
        }

        public int EffectiveReflexes {
            get { return Reflexes + ReflexesMod; }
        }

        public double ReflexesPercent {
            get { return Math.Round((EffectiveReflexes / MaxCoreStat) * 100); }
        }

        public int Aim {
            get { return GetInt("aimRaw"); }
            set { SetValue("aimRaw", value);
                OnPropertyChanged("AimPercent");
            }
        }

        public int AimMod {
            get { return GetInt("aimMod"); }
            set { SetValue("aimMod", value);
                OnPropertyChanged("AimPercent");
            }
        }

        public int EffectiveAim {
            get { return Aim + AimMod; }
        }

        public double AimPercent {
            get { return Math.Round((EffectiveAim / MaxCoreStat) * 100); }
        }

        public int Intelligence {
            get { return GetInt("intelligenceRaw"); }
            set { SetValue("intelligenceRaw", value);
                OnPropertyChanged("IntelligencePercent");
            }
        }

        public int IntelligenceMod {
            get { return GetInt("intelligenceMod"); }
            set { SetValue("intelligenceMod", value);
                OnPropertyChanged("IntelligencePercent");
            }
        }

        public int EffectiveIntelligence {
            get { return Intelligence + IntelligenceMod; }
        }

        public double IntelligencePercent {
            get { return Math.Round((EffectiveIntelligence / MaxCoreStat) * 100); }
        }

        public int Willpower {
            get { return GetInt("willpowerRaw"); }
            set { SetValue("willpowerRaw", value);
                OnPropertyChanged("WillpowerPercent");
            }
        }

        public int WillpowerMod {
            get { return GetInt("willpowerMod"); }
            set { SetValue("willpowerMod", value);
                OnPropertyChanged("WillpowerPercent");
            }
        }

        public int EffectiveWillpower {
            get { return Willpower + WillpowerMod; }
        }

        public double WillpowerPercent {
            get { return Math.Round((EffectiveWillpower / MaxCoreStat) * 100); }
        }

        public double Libido {
            get { return GetDouble("libidoRaw"); }
            set { SetValue("libidoRaw", value); }
        }

        public int LibidoMod {
            get { return GetInt("libidoMod"); }
            set { SetValue("libidoMod", value); }
        }

        public int MaxLibido {
            get { return 100; }
        }

        public double MaxCoreStat {
            get { return Level * 5; }
        }

        public int HP {
            get { return GetInt("HPRaw"); }
            set { SetValue("HPRaw", value); }
        }

        private int HPMod {
            get { return GetInt("HPMod"); }
        }

        public int MaxHP {
            get {
                var bonus = 0;
                //TODO check items for fortification effects and add to bonus

                var maxhp = 15 + (Level - 1) * 15 + HPMod + bonus;

                //class mercenary
                if (CharacterClass == 0) maxhp += Level * 5;
                //class engineer
                if (CharacterClass == 2) maxhp -= Level * 5;

                //TODO check status conditions

                return maxhp;
            }
        }

        public int Level {
            get { return GetInt("level"); }
            set {
                SetValue("level", value);
                OnPropertyChanged("MaxXP");
                OnPropertyChanged("MaxXPLabel");
                OnPropertyChanged("MaxCoreStat");
                OnPropertyChanged("MaxHP");
            }
        }

        public int CharacterClass {
            get { return GetInt("characterClass"); }
            set { SetValue("characterClass", value); }
        }

        public int PerkPoints {
            get { return GetInt("unclaimedClassPerks"); }
            set { SetValue("unclaimedClassPerks", value); }
        }

        public int StatPoints {
            get {
                if (Game.IsPC) {
                    return GetInt("unspentStatPoints");
                }
                return 0;
            }
            set {
                if (Game.IsPC) {
                    SetValue("unspentStatPoints", value);
                }
            }
        }

        public int XP {
            get { return GetInt("XPRaw"); }
            set { SetValue("XPRaw", value); }
        }

        public int MaxXP {
            get { return Level * Level * Level * Level * 100; }
        }

        public String MaxXPLabel {
            get {
                if (MaxXP > 1000) {
                    return MaxXP / 100 + "k";
                }
                return "" + MaxXP;
            }
        }

        public double Lust {
            get { return GetDouble("lustRaw"); }
            set { SetValue("lustRaw", value); }
        }

        public int LustMod {
            get { return GetInt("lustMod"); }
            set { SetValue("lustMod", value); }
        }

        public int MaxLust {
            get {
                int max = 100;
                var perk = Game.GetPerk("Inhuman Desire");
                if (perk.IsOwned) {
                    max += (int) perk.Value1;
                }
                return max;
            }
        }

        public int Energy {
            get { return GetInt("energyRaw"); }
            set { SetValue("energyRaw", value); }
        }

        public int EnergyMod {
            get { return GetInt("energyMod"); }
            set { SetValue("energyMod", value); }
        }

        public int MaxEnergy {
            get {
                int max = 100;
                var perk = Game.GetPerk("Heroic Reserves");
                if (perk.IsOwned) {
                    max += 33;
                }
                return max;
            }
        }

        public int Personality {
            get { return GetInt("personality"); }
            set {
                SetValue("personality", value);
                OnPropertyChanged("PersonalityTip");
            }
        }

        public String PersonalityTip {
            get {
                if (Personality <= 33) return "Nice";
                if (Personality <= 66) return "Mischievous";
                return "Ass";
            }
        }

        public int Exhibitionism {
            get { return GetInt("exhibitionismRaw"); }
            set { SetValue("exhibitionismRaw", value); }
        }

        public int Tallness {
            get { return GetInt("tallness"); }
            set { SetValue("tallness", value); }
        }

        public int Thickness {
            get { return GetInt("thickness"); }
            set { SetValue("thickness", value); }
        }

        public int Feminity {
            get { return GetInt("femininity"); }
            set { SetValue("femininity", value); }
        }

        public int Tone {
            get { return GetInt("tone"); }
            set { SetValue("tone", value); }
        }

        public int SkinType {
            get { return GetInt("skinType"); }
            set { SetValue("skinType", value); }
        }

        public String SkinTone {
            get { return GetString("skinTone"); }
            set { SetValue("skinTone", value); }
        }

        public String SkinAccent {
            get { return GetString("skinAccent"); }
            set { SetValue("skinAccent", value); }
        }

        public List<FlagItem> SkinFlags {
            get { return getFlagList(GetObj("skinFlags"), XmlData.Current.Body.SkinFlags); }
        }

        public String FurColor {
            get { return GetString("furColor"); }
            set { SetValue("furColor", value); }
        }

        public String ScaleColor {
            get { return GetString("scaleColor"); }
            set { SetValue("scaleColor", value); }
        }

        public int AlcoholTolerance {
            get {
                var toleranceStat = Game.GetStatus("Tolerance");
                return (int)toleranceStat.Value1;
            }
            set {
                var toleranceStat = Game.GetStatus("Tolerance");
                if (!toleranceStat.IsOwned) {
                    toleranceStat.IsOwned = true;
                }
                toleranceStat.Value1 = value;
            }
        }

        #endregion

        #region HeadPage
        public int AntennaeCount {
            get { return GetInt("antennae"); }
            set { SetValue("antennae", value); }
        }
        public int AntennaeType {
            get { return GetInt("antennaeType"); }
            set { SetValue("antennaeType", value); }
        }

        public int HornCount {
            get { return GetInt("horns"); }
            set { SetValue("horns", value); }
        }
        public int HornLength {
            get { return GetInt("hornLength"); }
            set { SetValue("hornLength", value); }
        }
        public int HornType {
            get { return GetInt("hornType"); }
            set { SetValue("hornType", value); }
        }

        public double HairLength {
            get { return GetDouble("hairLength"); }
            set { SetValue("hairLength", value); }
        }
        public String HairColor {
            get { return GetString("hairColor"); }
            set { SetValue("hairColor", value); }
        }
        public String HairStyle {
            get {
                if (GetString("hairStyle") == "null") {
                    return "";
                }
                return GetString("hairStyle");
            }
            set {
                if (value == null || value.Length == 0) {
                    SetValue("hairStyle", "null");
                } else {
                    SetValue("hairStyle", value);
                }
                
            }
        }
        public int HairType {
            get { return GetInt("hairType"); }
            set { SetValue("hairType", value); }
        }
        public int FaceType {
            get { return GetInt("faceType"); }
            set { SetValue("faceType", value); }
        }
        public List<FlagItem> FaceFlags {
            get { return getFlagList(GetObj("faceFlags"), XmlData.Current.Body.FaceFlags); }
        }
        public int EarLength {
            get { return GetInt("earLength"); }
            set { SetValue("earLength", value); }
        }
        public bool EarLengthEnabled {
            get {
                //lookup ear type
                XmlEnum datum = XmlData.LookupEnumByID(XmlData.Current.Body.EarTypes, EarType);
                if (datum != null) {
                    //check if in ear length
                    foreach (string name in XmlData.Current.Body.EarLengthEnables) {
                        if (name.Equals(datum.Name)) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        public int EarType {
            get { return GetInt("earType"); }
            set {
                SetValue("earType", value);
                OnPropertyChanged("EarLengthEnabled");
            }
        }
        public int EyeType {
            get { return GetInt("eyeType"); }
            set { SetValue("eyeType", value); }
        }
        public String EyeColor {
            get { return GetString("eyeColor"); }
            set { SetValue("eyeColor", value); }
        }
        public int TongueType {
            get { return GetInt("tongueType"); }
            set { SetValue("tongueType", value); }
        }
        public List<FlagItem> TongueFlags {
            get { return getFlagList(GetObj("tongueFlags"), XmlData.Current.Body.TongueFlags); }
        }

        public int LipSize {
            get { return GetInt("lipMod"); }
            set { SetValue("lipMod", value); }
        }
        public String LipColor {
            get { return GetString("lipColor"); }
            set { SetValue("lipColor", value); }
        }
        public int BeardLength {
            get { return GetInt("beardLength"); }
            set { SetValue("beardLength", value); }
        }
        public int BeardStyle {
            get { return GetInt("beardStyle"); }
            set { SetValue("beardStyle", value); }
        }

        #endregion

        #region BodyPage

        public double Elasticity {
            get { return GetDouble("elasticity"); }
            set { SetValue("elasticity", value); }
        }

        public int HipRating {
            get { return GetInt("hipRatingRaw"); }
            set {
                SetValue("hipRatingRaw", value);
                OnPropertyChanged("HipRatingTip");
            }
        }

        public int HipRatingMod {
            get { return GetInt("hipRatingMod"); }
            set {
                SetValue("hipRatingMod", value);
                OnPropertyChanged("HipRatingTip");
            }
        }

        public int EffectiveHipRating {
            get { return HipRating + HipRatingMod; }
        }

        public string HipRatingTip {
            get {
                var isMale = Feminity < 50;
                if (EffectiveHipRating >= 20) return isMale ? "inhumanly-wide" : "broodmother";
                if (EffectiveHipRating >= 15) return isMale ? "voluptuous" : "child-bearing";
                if (EffectiveHipRating >= 10) return isMale ? "wide" : "curvy";
                if (EffectiveHipRating >= 6) return isMale ? "ample" : "girly";
                if (EffectiveHipRating >= 4) return "well-formed";
                if (EffectiveHipRating >= 2) return "slender";
                return "boyish";
            }
        }

        public int ButtRating {
            get { return GetInt("buttRatingRaw"); }
            set {
                SetValue("buttRatingRaw", value);
                OnPropertyChanged("ButtRatingTip");
            }
        }

        public int ButtRatingMod {
            get { return GetInt("buttRatingMod"); }
            set {
                SetValue("buttRatingMod", value);
                OnPropertyChanged("ButtRatingTip");
            }
        }

        public int EffectiveButtRating {
            get { return ButtRating + ButtRatingMod; }
        }

        public string ButtRatingTip {
            get {
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

        public int BellyRating {
            get { return GetInt("bellyRatingRaw"); }
            set { SetValue("bellyRatingRaw", value); }
        }

        public int BellyRatingMod {
            get { return GetInt("bellyRatingMod"); }
            set { SetValue("bellyRatingMod", value); }
        }

        public int EffectiveBellyRating {
            get { return BellyRating + BellyRatingMod; }
        }

        public int ArmType {
            get { return GetInt("armType"); }
            set { SetValue("armType", value); }
        }

        public List<FlagItem> ArmFlags {
            get { return getFlagList(GetObj("armFlags"), XmlData.Current.Body.ArmFlags); }
        }

        public int LegCount {
            get { return GetInt("legCount"); }
            set { SetValue("legCount", value); }
        }

        public int LegType {
            get { return GetInt("legType"); }
            set { SetValue("legType", value); }
        }

        public List<FlagItem> LegFlags {
            get { return getFlagList(GetObj("legFlags"), XmlData.Current.Body.LegFlags); }
        }

        public int WingCount {
            get { return GetInt("wingCount"); }
            set { SetValue("wingCount", value); }
        }

        public int WingType {
            get { return GetInt("wingType"); }
            set { SetValue("wingType", value); }
        }

        public int TailCount {
            get { return GetInt("tailCount"); }
            set { SetValue("tailCount", value); }
        }

        public int TailType {
            get { return GetInt("tailType"); }
            set {
                SetValue("tailType", value);

                TailGenital = 0;
                XmlEnum datum = XmlData.LookupEnumByID(XmlData.Current.Body.TailTypes, value);
                if (datum != null) {
                    var name = datum.Name;
                    if (name == "Cuntsnake") {
                        datum = XmlData.LookupEnumByName(XmlData.Current.Body.TailGenitalTypes, "Vagina");
                        if(datum != null) {
                            TailGenital = datum.ID;
                        }
                    }
                    else if (name == "Cockvine") {
                        datum = XmlData.LookupEnumByName(XmlData.Current.Body.TailGenitalTypes, "Cock");
                        if (datum != null) {
                            TailGenital = datum.ID;
                        }
                    }
                }
            }
        }

        public bool TailGenitalEnable {
            get { return TailGenital != 0; }
        }

        public int TailGenital {
            get { return GetInt("tailGenital"); }
            set {
                SetValue("tailGenital", value);
                OnPropertyChanged("TailGenitalEnable");
            }
        }

        public int TailGenitalRace {
            get { return GetInt("tailGenitalArg"); }
            set { SetValue("tailGenitalArg", value); }
        }

        public String TailGenitalColor {
            get { return GetString("tailGenitalColor"); }
            set { SetValue("tailGenitalColor", value); }
        }

        public List<FlagItem> TailFlags {
            get { return getFlagList(GetObj("tailFlags"), XmlData.Current.Body.TailFlags); }
        }

        public int GenitalSpot {
            get { return GetInt("genitalSpot"); }
            set { SetValue("genitalSpot", value); }
        }

        public String NippleColor {
            get { return GetString("nippleColor"); }
            set { SetValue("nippleColor", value); }
        }

        public int NipplesPerBreast {
            get { return GetInt("nipplesPerBreast"); }
            set { SetValue("nipplesPerBreast", value); }
        }

        public double NippleLengthRatio {
            get { return GetDouble("nippleLengthRatio"); }
            set { SetValue("nippleLengthRatio", value); }
        }

        public double NippleWidthRatio {
            get { return GetDouble("nippleWidthRatio"); }
            set { SetValue("nippleWidthRatio", value); }
        }

        public int DickNippleType {
            get { return GetInt("dickNippleType"); }
            set { SetValue("dickNippleType", value); }
        }

        public int DickNippleMultiplier {
            get { return GetInt("dickNippleMultiplier"); }
            set { SetValue("dickNippleMultiplier", value); }
        }

        public int MilkType {
            get { return GetInt("milkType"); }
            set { SetValue("milkType", value); }
        }

        public double MilkFullness {
            get { return GetDouble("milkFullness"); }
            set { SetValue("milkFullness", value); }
        }

        public int MilkRate {
            get { return GetInt("milkRate"); }
            set { SetValue("milkRate", value); }
        }

        public int MilkMultiplier {
            get { return GetInt("milkMultiplier"); }
            set { SetValue("milkMultiplier", value); }
        }

        public int MilkStorageMultiplier {
            get { return GetInt("milkStorageMultiplier"); }
            set { SetValue("milkStorageMultiplier", value); }
        }

        #endregion

        #region GenitalPage

        public int GirlCumType {
            get { return GetInt("girlCumType"); }
            set { SetValue("girlCumType", value); }
        }

        public double Fertility {
            get { return GetDouble("fertilityRaw"); }
            set { SetValue("fertilityRaw", value); }
        }

        public int FertilityMod {
            get { return GetInt("fertilityMod"); }
            set { SetValue("fertilityMod", value); }
        }

        public double ClitLength {
            get { return GetDouble("clitLength"); }
            set { SetValue("clitLength", value); }
        }

        public bool AnalVirgin {
            get { return GetBool("analVirgin"); }
            set {
                SetValue("analVirgin", value);
                Ass.Hymen = value;
            }
        }

        public bool VaginalVirgin {
            get { return GetBool("vaginalVirgin"); }
            set { SetValue("vaginalVirgin", value); }
        }

        public bool CockVirgin {
            get { return GetBool("cockVirgin"); }
            set { SetValue("cockVirgin", value); }
        }

        public int CumType {
            get { return GetInt("cumType"); }
            set { SetValue("cumType", value); }
        }

        public int CumMultiplier {
            get { return GetInt("cumMultiplierRaw"); }
            set { SetValue("cumMultiplierRaw", value); }
        }

        public int CumMultiplierMod {
            get { return GetInt("cumMultiplierMod"); }
            set { SetValue("cumMultiplierMod", value); }
        }

        public double CumQuality {
            get { return GetDouble("cumQualityRaw"); }
            set { SetValue("cumQualityRaw", value); }
        }

        public int CumQualityMod {
            get { return GetInt("cumQualityMod"); }
            set { SetValue("cumQualityMod", value); }
        }

        public int Balls {
            get { return GetInt("balls"); }
            set { SetValue("balls", value); }
        }

        public double BallSize {
            get { return GetDouble("ballSizeRaw"); }
            set { SetValue("ballSizeRaw", value); }
        }

        public int BallFullness {
            get { return GetInt("ballFullness"); }
            set { SetValue("ballFullness", value); }
        }

        public int BallEfficiency {
            get { return GetInt("ballEfficiency"); }
            set { SetValue("ballEfficiency", value); }
        }

        public int RefractoryRate {
            get { return GetInt("refractoryRate"); }
            set { SetValue("refractoryRate", value); }
        }

        #endregion

        #region MiscPage

        public String Affinity {
            get { return GetString("affinity"); }
            set { SetValue("affinity", value); }
        }

        public String OriginalRace {
            get { return GetString("originalRace"); }
            set { SetValue("originalRace", value); }
        }

        public string ImpregnationType {
            get { return GetString("impregnationType"); }
            set { SetValue("impregnationType", value); }
        }

        public int PregnancyMultiplierRaw {
            get { return GetInt("pregnancyMultiplierRaw"); }
            set { SetValue("pregnancyMultiplierRaw", value); }
        }

        public int PregnancyMultiplierMod {
            get { return GetInt("pregnancyMultiplierMod"); }
            set { SetValue("pregnancyMultiplierMod", value); }
        }

        public int PregnancyIncubationBonusMotherRaw {
            get { return GetInt("pregnancyIncubationBonusMotherRaw"); }
            set { SetValue("pregnancyIncubationBonusMotherRaw", value); }
        }

        public int PregnancyIncubationBonusMotherMod {
            get { return GetInt("pregnancyIncubationBonusMotherMod"); }
            set { SetValue("pregnancyIncubationBonusMotherMod", value); }
        }

        public int PregnancyIncubationBonusFatherRaw {
            get { return GetInt("pregnancyIncubationBonusFatherRaw"); }
            set { SetValue("pregnancyIncubationBonusFatherRaw", value); }
        }

        public int PregnancyIncubationBonusFatherMod {
            get { return GetInt("pregnancyIncubationBonusFatherMod"); }
            set { SetValue("pregnancyIncubationBonusFatherMod", value); }
        }

        #endregion

        #region ItemPage
        public UpdatableCollection<ItemContainerVM> ItemContainers { get; private set; }

        public void UpdateItemList() {
            //need to find a way to make search only apply to currently selected item somehow
            foreach(var slot in _inventory.Slots) {
                slot.UpdateItemGroups();
            }
        }

        public void UpdateInventory() {
            _inventory.Clear();
            const int maxSlots = 10;
            AmfObject inv = GetObj("inventory");
            for (int i = 0; i < maxSlots; ++i) {
                AmfObject item = (AmfObject)inv[i];
                if(item == null) {
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

        public void CleanupInventory() {
            //shift all the items around in the inventory
            const int maxSlots = 10;
            AmfObject inv = GetObj("inventory");
            AmfObject nInv = new AmfObject(AmfTypes.Array);
            for (int i = 0; i < maxSlots; ++i) {
                AmfObject item = (AmfObject)inv[i];
                if (item == null) {
                    continue;
                }
                if (item.GetString("classInstance") != XmlItem.Empty.ID) {
                    nInv.Push(item);
                }
            }
            SetValue("inventory", nInv);
        }

        #endregion

        #region KeyItemsPage

        public AmfObject KeyItemsArray {
            get { return GetObj("keyItems"); }
        }

        public List<KeyItemGroupVM> KeyItemGroups { get; set; }

        #endregion

        #region PerksPage

        public AmfObject PerksArray {
            get { return GetObj("perks"); }
        }

        public List<PerkGroupVM> PerkGroups { get; set; }

        #endregion

        #region RawPage

        public AmfObject StatusEffectsArray {
            get { return GetObj("statusEffects"); }
        }

        public UpdatableCollection<StatusEffectVM> StatusEffects { get; set; }

        #endregion
    }
}
