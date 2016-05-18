using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed partial class GameVM : ObjectVM {
        public GameVM(AmfFile file, GameVM previousVM)
            : base(file) {

        }

        static void ImportMissingNamedVectors(AmfObject cocItems, IEnumerable<XmlNamedVector4> xmlItems, string cocNameProperty, Func<AmfObject, String> descriptionGetter = null, IList<XmlNamedVector4> targetXmlList = null) {
            if(targetXmlList == null) targetXmlList = (IList<XmlNamedVector4>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach(var pair in cocItems) {
                var name = pair.ValueAsObject.GetString(cocNameProperty);
                if(xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlNamedVector4 { Name = name };
                if(descriptionGetter != null) xml.Description = descriptionGetter(pair.ValueAsObject);
                targetXmlList.Add(xml);
            }
        }

        private Dictionary<AmfObject, List<FlagItem>> flagData;
        private List<FlagItem> getFlagList(AmfObject obj, XmlEnum[] data) {
            if(flagData == null) {
                flagData = new Dictionary<AmfObject, List<FlagItem>>();
            }
            List<FlagItem> flags = null;
            if(flagData.ContainsKey(obj)) {
                flags = flagData[obj];
            } else {
                flags = new List<FlagItem>();
                foreach(XmlEnum e in data) {
                    flags.Add(new FlagItem(obj, e));
                }
                flagData.Add(obj, flags);
            }
            return flags;
        }

        AmfObject pc = null;
        private AmfObject getPC() {
            if(pc != null) {
                return pc;
            }
            var chars = GetObj("characters");
            return pc = chars.GetObj("PC");
        }

        #region GeneralPage
        public string Name {
            get { return getPC().GetString("short"); }
            set {
                SetValue(getPC(), "short", value, null);
                SetValue(getPC(), "uniqueName", value, null);
                SetValue("saveName", value);
            }
        }

        public string Notes {
            get { return GetString("saveNotes"); }
            set { SetValue("saveNotes", String.IsNullOrWhiteSpace(value) ? "No notes available." : value); }
        }

        public int Credits {
            get { return getPC().GetInt("credits"); }
            set { SetValue(getPC(), "credits", value, null); }
        }

        public int Days {
            get { return GetInt("daysPassed"); }
            set { SetValue("daysPassed", value); }
        }

        public int Hours {
            get { return GetInt("currentHours"); }
            set { SetValue("currentHours", value); }
        }

        public int Minutes {
            get { return GetInt("currentMinutes"); }
            set { SetValue("currentMinutes", value); }
        }

        public int Physique {
            get { return getPC().GetInt("physiqueRaw"); }
            set { SetValue(getPC(), "physiqueRaw", value, null); }
        }

        public int Reflexes {
            get { return getPC().GetInt("reflexesRaw"); }
            set { SetValue(getPC(), "reflexesRaw", value, null); }
        }

        public int Aim {
            get { return getPC().GetInt("aimRaw"); }
            set { SetValue(getPC(), "aimRaw", value, null); }
        }

        public int Intelligence {
            get { return getPC().GetInt("intelligenceRaw"); }
            set { SetValue(getPC(), "intelligenceRaw", value, null); }
        }

        public int Willpower {
            get { return getPC().GetInt("willpowerRaw"); }
            set { SetValue(getPC(), "willpowerRaw", value, null); }
        }

        public double Libido {
            get { return getPC().GetDouble("libidoRaw"); }
            set { SetValue(getPC(), "libidoRaw", value, null); }
        }

        public double MaxCoreStat {
            get { return Level * 5; }
        }

        public int HP {
            get { return getPC().GetInt("HPRaw"); }
            set { SetValue(getPC(), "HPRaw", value, null); }
        }

        public int Level {
            get { return getPC().GetInt("level"); }
            set {
                SetValue(getPC(), "level", value, null);
                OnPropertyChanged("MaxXP");
                OnPropertyChanged("MaxXPLabel");
                OnPropertyChanged("MaxCoreStat");
                OnPropertyChanged("MaxHP");
            }
        }

        public int CharacterClass {
            get { return getPC().GetInt("characterClass"); }
            set { SetValue(getPC(), "characterClass", value, null); }
        }

        private int HPMod {
            get { return getPC().GetInt("HPMod"); }
        }

        public int PerkPoints {
            get { return getPC().GetInt("unclaimedClassPerks"); }
            set { SetValue(getPC(), "unclaimedClassPerks", value, null); }
        }

        public int StatPoints {
            get { return getPC().GetInt("unspentStatPoints"); }
            set { SetValue(getPC(), "unspentStatPoints", value, null); }
        }

        public int XP {
            get { return getPC().GetInt("XPRaw"); }
            set { SetValue(getPC(), "XPRaw", value, null); }
        }

        public int MaxXP {
            get { return Level * Level * Level * Level * 100; }
        }

        public String MaxXPLabel {
            get {
                if(MaxXP > 1000) {
                    return MaxXP / 100 + "k";
                }
                return "" + MaxXP;
            }
        }

        public int Personality {
            get { return getPC().GetInt("personality"); }
            set {
                SetValue(getPC(), "personality", value, null);
                OnPropertyChanged("PersonalityTip");
            }
        }

        public String PersonalityTip {
            get {
                if(Personality <= 33) return "Nice";
                if(Personality <= 66) return "Mischievous";
                return "Ass";
            }
        }

        public int MaxHP {
            get {
                var bonus = 0;
                //TODO check items for fortification effects and add to bonus

                var maxhp = 15 + (Level - 1) * 15 + HPMod + bonus;

                //class mercenary
                if(CharacterClass == 0) maxhp += Level * 5;
                //class engineer
                if(CharacterClass == 2) maxhp -= Level * 5;

                //TODO check status conditions

                return maxhp;
            }
        }

        public double Lust {
            get { return getPC().GetDouble("lustRaw"); }
            set { SetValue(getPC(), "lustRaw", value, null); }
        }

        public int MaxLust {
            get { return 100; }
        }

        public int Energy {
            get { return getPC().GetInt("energyRaw"); }
            set { SetValue(getPC(), "energyRaw", value, null); }
        }

        public int MaxEnergy {
            get { return 100; }
        }

        #endregion

        #region BodyPage
        public int Tallness {
            get { return getPC().GetInt("tallness"); }
            set { SetValue(getPC(), "tallness", value, null); }
        }
        public int Thickness {
            get { return getPC().GetInt("thickness"); }
            set { SetValue(getPC(), "thickness", value, null); }
        }
        public int Feminity {
            get { return getPC().GetInt("femininity"); }
            set { SetValue(getPC(), "femininity", value, null); }
        }
        public int Tone {
            get { return getPC().GetInt("tone"); }
            set { SetValue(getPC(), "tone", value, null); }
        }
        public int SkinType {
            get { return getPC().GetInt("skinType"); }
            set { SetValue(getPC(), "skinType", value, null); }
        }
        public String SkinTone {
            get { return getPC().GetString("skinTone"); }
            set { SetValue(getPC(), "skinTone", value, null); }
        }
        public String SkinAccent {
            get { return getPC().GetString("skinAccent"); }
            set { SetValue(getPC(), "skinAccent", value, null); }
        }
        public List<FlagItem> SkinFlags {
            get { return getFlagList(getPC().GetObj("skinFlags"), XmlData.Current.Body.SkinFlags); }
        }
        public String FurColor {
            get { return getPC().GetString("furColor"); }
            set { SetValue(getPC(), "furColor", value, null); }
        }
        public String ScaleColor {
            get { return getPC().GetString("scaleColor"); }
            set { SetValue(getPC(), "scaleColor", value, null); }
        }

        public int AntennaeCount {
            get { return getPC().GetInt("antennae"); }
            set { SetValue(getPC(), "antennae", value, null); }
        }
        public int AntennaeType {
            get { return getPC().GetInt("antennaeType"); }
            set { SetValue(getPC(), "antennaeType", value, null); }
        }

        public int HornCount {
            get { return getPC().GetInt("horns"); }
            set { SetValue(getPC(), "horns", value, null); }
        }
        public int HornLength {
            get { return getPC().GetInt("hornLength"); }
            set { SetValue(getPC(), "hornLength", value, null); }
        }
        public int HornType {
            get { return getPC().GetInt("hornType"); }
            set { SetValue(getPC(), "hornType", value, null); }
        }

        public int HairLength {
            get { return getPC().GetInt("hairLength"); }
            set { SetValue(getPC(), "hairLength", value, null); }
        }
        public String HairColor {
            get { return getPC().GetString("hairColor"); }
            set { SetValue(getPC(), "hairColor", value, null); }
        }
        public String HairStyle {
            get { return getPC().GetString("hairStyle"); }
            set { SetValue(getPC(), "hairStyle", value, null); }
        }
        public int HairType {
            get { return getPC().GetInt("hairType"); }
            set { SetValue(getPC(), "hairType", value, null); }
        }
        public int FaceType {
            get { return getPC().GetInt("faceType"); }
            set { SetValue(getPC(), "faceType", value, null); }
        }
        public List<FlagItem> FaceFlags {
            get { return getFlagList(getPC().GetObj("faceFlags"), XmlData.Current.Body.FaceFlags); }
        }
        public int EarType {
            get { return getPC().GetInt("earType"); }
            set { SetValue(getPC(), "earType", value, null); }
        }
        public int EyeType {
            get { return getPC().GetInt("eyeType"); }
            set { SetValue(getPC(), "eyeType", value, null); }
        }
        public String EyeColor {
            get { return getPC().GetString("eyeColor"); }
            set { SetValue(getPC(), "eyeColor", value, null); }
        }
        public int TongueType {
            get { return getPC().GetInt("tongueType"); }
            set { SetValue(getPC(), "tongueType", value, null); }
        }
        public List<FlagItem> TongueFlags {
            get { return getFlagList(getPC().GetObj("tongueFlags"), XmlData.Current.Body.TongueFlags); }
        }

        public int LipSize {
            get { return getPC().GetInt("lipMod"); }
            set { SetValue(getPC(), "lipMod", value, null); }
        }
        public String LipColor {
            get { return getPC().GetString("lipColor"); }
            set { SetValue(getPC(), "lipColor", value, null); }
        }
        public int BeardLength {
            get { return getPC().GetInt("beardLength"); }
            set { SetValue(getPC(), "beardLength", value, null); }
        }
        public int BeardStyle {
            get { return getPC().GetInt("beardStyle"); }
            set { SetValue(getPC(), "beardStyle", value, null); }
        }
        //------------------------------------------------------------
        public int HipRating {
            get { return getPC().GetInt("hipRatingRaw"); }
            set {
                SetValue(getPC(), "hipRatingRaw", value, null);
                OnPropertyChanged("HipRatingTip");
            }
        }
        public string HipRatingTip {
            get {
                if(HipRating >= 20) return IsMale ? "inhumanly-wide" : "broodmother";
                if(HipRating >= 15) return IsMale ? "voluptuous" : "child-bearing";
                if(HipRating >= 10) return IsMale ? "wide" : "curvy";
                if(HipRating >= 6) return IsMale ? "ample" : "girly";
                if(HipRating >= 4) return "well-formed";
                if(HipRating >= 2) return "slender";
                return "boyish";
            }
        }

        public int ButtRating {
            get { return getPC().GetInt("buttRatingRaw"); }
            set {
                SetValue(getPC(), "buttRatingRaw", value, null);
                OnPropertyChanged("ButtRatingTip");
            }
        }

        public string ButtRatingTip {
            get {
                if(ButtRating >= 20) return "colossal";
                if(ButtRating >= 16) return "huge";
                if(ButtRating >= 13) return "voluminous";
                if(ButtRating >= 10) return "spacious";
                if(ButtRating >= 8) return "substantial";
                if(ButtRating >= 6) return "shapely";
                if(ButtRating >= 4) return "regular";
                if(ButtRating >= 2) return "compact";
                return "very small";
            }
        }

        public int BellyRating {
            get { return getPC().GetInt("bellyRatingRaw"); }
            set { SetValue(getPC(), "bellyRatingRaw", value, null); }
        }

        public int ArmType {
            get { return getPC().GetInt("armType"); }
            set { SetValue(getPC(), "armType", value, null); }
        }

        public List<FlagItem> ArmFlags {
            get { return getFlagList(getPC().GetObj("armFlags"), XmlData.Current.Body.ArmFlags); }
        }

        public int LegCount {
            get { return getPC().GetInt("legCount"); }
            set { SetValue(getPC(), "legCount", value, null); }
        }

        public int LegType {
            get { return getPC().GetInt("legType"); }
            set { SetValue(getPC(), "legType", value, null); }
        }

        public List<FlagItem> LegFlags {
            get { return getFlagList(getPC().GetObj("legFlags"), XmlData.Current.Body.LegFlags); }
        }

        public int WingCount {
            get { return getPC().GetInt("wingCount"); }
            set { SetValue(getPC(), "wingCount", value, null); }
        }
        public int WingType {
            get { return getPC().GetInt("wingType"); }
            set { SetValue(getPC(), "wingType", value, null); }
        }

        public int TailCount {
            get { return getPC().GetInt("tailCount"); }
            set { SetValue(getPC(), "tailCount", value, null); }
        }

        public int TailType {
            get { return getPC().GetInt("tailType"); }
            set { SetValue(getPC(), "tailType", value, null); }
        }

        public int TailGenital {
            get { return getPC().GetInt("tailGenital"); }
            set { SetValue(getPC(), "tailGenital", value, null); }
        }

        public int TailGenitalRace {
            get { return getPC().GetInt("tailGenitalArg"); }
            set { SetValue(getPC(), "tailGenitalArg", value, null); }
        }

        public String TailGenitalColor {
            get { return getPC().GetString("tailGenitalColor"); }
            set { SetValue(getPC(), "tailGenitalColor", value, null); }
        }

        public List<FlagItem> TailFlags {
            get { return getFlagList(getPC().GetObj("tailFlags"), XmlData.Current.Body.TailFlags); }
        }

        #endregion
    }
}
