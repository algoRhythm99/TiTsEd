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

namespace TiTsEd.ViewModel
{
    public sealed partial class GameVM : ObjectVM
    {
        public GameVM(AmfFile file, GameVM previousVM)
            : base(file)
        {

        }

        static void ImportMissingNamedVectors(AmfObject cocItems, IEnumerable<XmlNamedVector4> xmlItems, string cocNameProperty, Func<AmfObject, String> descriptionGetter = null, IList<XmlNamedVector4> targetXmlList = null)
        {
            if (targetXmlList == null) targetXmlList = (IList<XmlNamedVector4>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in cocItems)
            {
                var name = pair.ValueAsObject.GetString(cocNameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlNamedVector4 { Name = name };
                if (descriptionGetter != null) xml.Description = descriptionGetter(pair.ValueAsObject);
                targetXmlList.Add(xml);
            }
        }

        AmfObject pc = null;
        private AmfObject getPC()
        {
            if (pc != null)
            {
                return pc;
            }
            var chars = GetObj("characters");
            return pc = chars.GetObj("PC");
        }


        #region GeneralPage
        public string Name
        {
            get { return getPC().GetString("short"); }
            set {
                SetValue(getPC(), "short", value);
                SetValue(getPC(), "uniqueName", value);
                SetValue("saveName", value);
            }
        }

        public string Notes
        {
            get { return GetString("saveNotes"); }
            set { SetValue("saveNotes", String.IsNullOrWhiteSpace(value) ? "No notes available." : value); }
        }

        public int Credits
        {
            get { return getPC().GetInt("credits"); }
            set { SetValue(getPC(), "credits", value); }
        }

        public int Days
        {
            get { return GetInt("daysPassed"); }
            set { SetValue("daysPassed", value); }
        }

        public int Hours
        {
            get { return GetInt("currentHours"); }
            set { SetValue("currentHours", value); }
        }

        public int Minutes
        {
            get { return GetInt("currentMinutes"); }
            set { SetValue("currentMinutes", value); }
        }

        public int Physique
        {
            get { return getPC().GetInt("physiqueRaw"); }
            set { SetValue(getPC(), "physiqueRaw", value); }
        }

        public int Reflexes
        {
            get { return getPC().GetInt("reflexesRaw"); }
            set { SetValue(getPC(), "reflexesRaw", value); }
        }

        public int Aim
        {
            get { return getPC().GetInt("aimRaw"); }
            set { SetValue(getPC(), "aimRaw", value); }
        }

        public int Intelligence
        {
            get { return getPC().GetInt("intelligenceRaw"); }
            set { SetValue(getPC(), "intelligenceRaw", value); }
        }

        public int Willpower
        {
            get { return getPC().GetInt("willpowerRaw"); }
            set { SetValue(getPC(), "willpowerRaw", value); }
        }

        public double Libido
        {
            get { return getPC().GetDouble("libidoRaw"); }
            set { SetValue(getPC(), "libidoRaw", value); }
        }

        public double MaxCoreStat
        {
            get { return Level * 5; }
        }

        public int HP
        {
            get { return getPC().GetInt("HPRaw"); }
            set { SetValue(getPC(), "HPRaw", value); }
        }

        public int Level
        {
            get { return getPC().GetInt("level"); }
            set {
                SetValue(getPC(), "level", value);
                OnPropertyChanged("MaxXP");
                OnPropertyChanged("MaxXPLabel");
                OnPropertyChanged("MaxCoreStat");
                OnPropertyChanged("MaxHP");
            }
        }

        public int CharacterClass
        {
            get { return getPC().GetInt("characterClass"); }
            set { SetValue(getPC(), "characterClass", value); }
        }

        private int HPMod
        {
            get { return getPC().GetInt("HPMod"); }
        }

        public int PerkPoints
        {
            get { return getPC().GetInt("unclaimedClassPerks"); }
            set { SetValue(getPC(), "unclaimedClassPerks", value); }
        }

        public int StatPoints
        {
            get { return getPC().GetInt("unspentStatPoints"); }
            set { SetValue(getPC(), "unspentStatPoints", value); }
        }

        public int XP
        {
            get { return getPC().GetInt("XPRaw"); }
            set { SetValue(getPC(), "XPRaw", value); }
        }

        public int MaxXP
        {
            get { return Level * Level * Level * Level * 100; }
        }

        public String MaxXPLabel
        {
            get
            {
                if (MaxXP > 1000)
                {
                    return MaxXP / 100 + "k";
                }
                return "" + MaxXP;
            }
        }

        public int Personality
        {
            get { return getPC().GetInt("personality"); }
            set { 
                SetValue(getPC(), "personality", value);
                OnPropertyChanged("PersonalityTip");
            }
        }

        public String PersonalityTip
        {
            get {
                if (Personality <= 33) return "Nice";
                if (Personality <= 66) return "Mischievous";
                return "Ass";
            }
        }

        public int MaxHP
        {
            get
            {
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

        public double Lust
        {
            get { return getPC().GetDouble("lustRaw"); }
            set { SetValue(getPC(), "lustRaw", value); }
        }

        public int MaxLust
        {
            get { return 100; }
        }

        public int Energy
        {
            get { return getPC().GetInt("energyRaw"); }
            set { SetValue(getPC(), "energyRaw", value); }
        }

        public int MaxEnergy
        {
            get { return 100; }
        }

        #endregion

        #region BodyPage
        public int Tallness
        {
            get { return getPC().GetInt("tallness"); }
            set { SetValue(getPC(), "tallness", value); }
        }
        public int Thickness
        {
            get { return getPC().GetInt("thickness"); }
            set { SetValue(getPC(), "thickness", value); }
        }
        public int Feminity
        {
            get { return getPC().GetInt("femininity"); }
            set { SetValue(getPC(), "femininity", value); }
        }
        public int Tone
        {
            get { return getPC().GetInt("tone"); }
            set { SetValue(getPC(), "tone", value); }
        }
        public int SkinType
        {
            get { return getPC().GetInt("skinType"); }
            set { SetValue(getPC(), "skinType", value); }
        }
        public String SkinTone
        {
            get { return getPC().GetString("skinTone"); }
            set { SetValue(getPC(), "skinTone", value); }
        }
        public String FurColor
        {
            get { return getPC().GetString("furColor"); }
            set { SetValue(getPC(), "furColor", value); }
        }


        public int HairLength
        {
            get { return getPC().GetInt("hairLength"); }
            set { SetValue(getPC(), "hairLength", value); }
        }
        public String HairColor
        {
            get { return getPC().GetString("hairColor"); }
            set { SetValue(getPC(), "hairColor", value); }
        }
        public int HairType
        {
            get { return getPC().GetInt("hairType"); }
            set { SetValue(getPC(), "hairType", value); }
        }
        public int FaceType
        {
            get { return getPC().GetInt("faceType"); }
            set { SetValue(getPC(), "faceType", value); }
        }
        public int EarType
        {
            get { return getPC().GetInt("earType"); }
            set { SetValue(getPC(), "earType", value); }
        }
        public int EyeType
        {
            get { return getPC().GetInt("eyeType"); }
            set { SetValue(getPC(), "eyeType", value); }
        }
        public String EyeColor
        {
            get { return getPC().GetString("eyeColor"); }
            set { SetValue(getPC(), "eyeColor", value); }
        }
        public int TongueType
        {
            get { return getPC().GetInt("tongueType"); }
            set { SetValue(getPC(), "tongueType", value); }
        }

        #endregion
    }
}
