using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            var containers = new List<ItemContainerVM>();
            _inventory = new ItemContainerVM(this, "Inventory", ItemCategories.All);
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

        #region GeneralPage
        public string Name {
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
            set { SetValue("physiqueRaw", value); }
        }

        public int Reflexes {
            get { return GetInt("reflexesRaw"); }
            set { SetValue("reflexesRaw", value); }
        }

        public int Aim {
            get { return GetInt("aimRaw"); }
            set { SetValue("aimRaw", value); }
        }

        public int Intelligence {
            get { return GetInt("intelligenceRaw"); }
            set { SetValue("intelligenceRaw", value); }
        }

        public int Willpower {
            get { return GetInt("willpowerRaw"); }
            set { SetValue("willpowerRaw", value); }
        }

        public double Libido {
            get { return GetDouble("libidoRaw"); }
            set { SetValue("libidoRaw", value); }
        }

        public double MaxCoreStat {
            get { return Level * 5; }
        }

        public int HP {
            get { return GetInt("HPRaw"); }
            set { SetValue("HPRaw", value); }
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

        private int HPMod {
            get { return GetInt("HPMod"); }
        }

        public int PerkPoints {
            get { return GetInt("unclaimedClassPerks"); }
            set { SetValue("unclaimedClassPerks", value); }
        }

        public int StatPoints {
            get { return GetInt("unspentStatPoints"); }
            set { SetValue("unspentStatPoints", value); }
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

        public double Lust {
            get { return GetDouble("lustRaw"); }
            set { SetValue("lustRaw", value); }
        }

        public int MaxLust {
            get { return 100; }
        }

        public int Energy {
            get { return GetInt("energyRaw"); }
            set { SetValue("energyRaw", value); }
        }

        public int MaxEnergy {
            get { return 100; }
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

        public int HairLength {
            get { return GetInt("hairLength"); }
            set { SetValue("hairLength", value); }
        }
        public String HairColor {
            get { return GetString("hairColor"); }
            set { SetValue("hairColor", value); }
        }
        public String HairStyle {
            get { return GetString("hairStyle"); }
            set { SetValue("hairStyle", value); }
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

        public int HipRating {
            get { return GetInt("hipRatingRaw"); }
            set {
                SetValue("hipRatingRaw", value);
                OnPropertyChanged("HipRatingTip");
            }
        }
        public string HipRatingTip {
            get {
                var isMale = Feminity < 50;
                if (HipRating >= 20) return isMale ? "inhumanly-wide" : "broodmother";
                if (HipRating >= 15) return isMale ? "voluptuous" : "child-bearing";
                if (HipRating >= 10) return isMale ? "wide" : "curvy";
                if (HipRating >= 6) return isMale ? "ample" : "girly";
                if (HipRating >= 4) return "well-formed";
                if (HipRating >= 2) return "slender";
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

        public string ButtRatingTip {
            get {
                if (ButtRating >= 20) return "colossal";
                if (ButtRating >= 16) return "huge";
                if (ButtRating >= 13) return "voluminous";
                if (ButtRating >= 10) return "spacious";
                if (ButtRating >= 8) return "substantial";
                if (ButtRating >= 6) return "shapely";
                if (ButtRating >= 4) return "regular";
                if (ButtRating >= 2) return "compact";
                return "very small";
            }
        }

        public int BellyRating {
            get { return GetInt("bellyRatingRaw"); }
            set { SetValue("bellyRatingRaw", value); }
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


        public String NippleColor {
            get { return GetString("nippleColor"); }
            set { SetValue("nippleColor", value); }
        }

        public int NipplesPerBreast {
            get { return GetInt("nipplesPerBreast"); }
            set { SetValue("nipplesPerBreast", value); }
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

        public int MilkFullness {
            get { return GetInt("milkFullness"); }
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

        public double ClitLength {
            get { return GetDouble("clitLength"); }
            set { SetValue("clitLength", value); }
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

        public int CumQuality {
            get { return GetInt("cumQualityRaw"); }
            set { SetValue("cumQualityRaw", value); }
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

        #endregion

        #region ItemPage
        public UpdatableCollection<ItemContainerVM> ItemContainers { get; private set; }

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

    }
}
