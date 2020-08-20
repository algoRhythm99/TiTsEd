using System;
using System.Collections.Generic;

using System.Text;
using TiTsEd.Model;
using TiTsEd.Common;

namespace TiTsEd.ViewModel
{
    public sealed class CockArrayVM : ArrayVM<CockVM>
    {
        public CockArrayVM(CharacterVM character, AmfObject obj)
            : base(obj, x => new CockVM(character, x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Object);

            obj["cLengthRaw"] = 5.0;
            obj["cLengthMod"] = 0;

            obj["cThicknessRatioRaw"] = 1.0;
            obj["cThicknessRatioMod"] = 0;

            obj["cType"] = 0;
            obj["cockColor"] = "pink";

            obj["knotMultiplier"] = 1.0;
            obj["flaccidMultiplier"] = 0.25;
            obj["virgin"] = true;

            obj["cockFlags"] = new AmfObject(AmfTypes.Array);
            obj["pierced"] = 0;

            obj["pLong"] = "";
            obj["pShort"] = "";
            obj["sock"] = "";

            obj["classInstance"] = "classes::CockClass";

            return obj;
        }
    }

    public class CockVM : ObjectVM
    {
        public CockVM(CharacterVM character, AmfObject obj)
            : base(obj)
        {

            _character = character;
        }

        private CharacterVM _character { get; set; }

        public double Length
        {
            get { return GetDouble("cLengthRaw"); }
            set
            {
                SetValue("cLengthRaw", value);
                OnPropertyChanged("Description");
            }
        }

        public int LengthMod
        {
            get { return GetInt("cLengthMod"); }
            set
            {
                SetValue("cLengthMod", value);
                OnPropertyChanged("Description");
            }
        }

        public double EffectiveLength
        {
            get { return Length + LengthMod; }
        }

        public string LengthTip
        {
            get { return Extensions.GetFeetAndCentimetersDescription(EffectiveLength); }
        }

        public double ThicknessRatio
        {
            get { return GetDouble("cThicknessRatioRaw"); }
            set
            {
                SetValue("cThicknessRatioRaw", value);
                OnPropertyChanged("ThicknessTip");
            }
        }

        public int ThicknessRatioMod
        {
            get { return GetInt("cThicknessRatioMod"); }
            set
            {
                SetValue("cThicknessRatioMod", value);
                OnPropertyChanged("ThicknessTip");
            }
        }

        public double EffectiveThicknessRatio
        {
            get { return ThicknessRatio + ThicknessRatioMod; }
        }

        public string ThicknessTip
        {
            get
            {
                var thicknessValue = Math.Round((EffectiveLength / 6) * EffectiveThicknessRatio, 2);
                return Extensions.GetFeetAndCentimetersDescription(thicknessValue);
            }
        }

        public int Pierced
        {
            get { return GetInt("pierced"); }
            set { SetValue("pierced", value); }
        }

        public int Virgin
        {
            get { return GetInt("virgin"); }
            set { SetValue("virgin", value); }
        }

        public double KnotMultiplier
        {
            get { return GetDouble("knotMultiplier"); }
            set { SetValue("knotMultiplier", value); }
        }

        public double FlaccidMultiplier
        {
            get { return GetDouble("flaccidMultiplier"); }
            set { SetValue("flaccidMultiplier", value); }
        }

        public string CockColor
        {
            get { return GetString("cockColor"); }
            set
            {
                SetValue("cockColor", value);
                OnPropertyChanged("Description");
            }
        }

        public string[] CockColors
        {
            get
            {
                return XmlData.Current.Body.SkinTones;
            }
        }

        public int CockType
        {
            get { return GetInt("cType"); }
            set
            {
                if (value != CockType)
                {
                    SetValue("cType", value);
                    UpdateFlags(value, this.CockFlags, "CockFlags");
                    OnPropertyChanged("Description");
                }
            }
        }

        protected override List<string> GetDefaultFlags(int cType, string propertyName = null)
        {
            List<string> defaultFlags = new List<string>();
            switch (cType)
            {
                case 0:  //Human
                case 10: //Avian
                case 19: //Shark
                case 20: //Suula
                case 43: //Sydian
                case 51: //Synthetic
                case 52: //Simii
                case 56: //Inhuman
                case 57: //Ovir
                case 61: //Saurian
                case 72: //Swine
                default:
                    //All of the above have no default flags, so do nothing
                    break;
                case 1:  //Equine
                    defaultFlags.Add("Blunt");
                    defaultFlags.Add("Flared");
                    defaultFlags.Add("Sheathed");
                    break;
                case 3:  //Canine
                case 5:  //Vulpine
                    defaultFlags.Add("Knotted");
                    defaultFlags.Add("Sheathed");
                    defaultFlags.Add("Tapered");
                    break;
                case 4:  //Feline
                    defaultFlags.Add("Nubby");
                    break;
                case 6:  //Bee
                    defaultFlags.Add("Smooth");
                    defaultFlags.Add("Foreskinned");
                    break;
                case 11: //Draconic
                    defaultFlags.Add("Tapered");
                    defaultFlags.Add("Knotted");
                    break;
                case 13: //Naga
                    defaultFlags.Add("Smooth");
                    defaultFlags.Add("Tapered");
                    break;
                case 15: //Demonic
                    defaultFlags.Add("Knotted");
                    defaultFlags.Add("Nubby");
                    break;
                case 17: //Kangaroo
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Tapered");
                    break;
                case 18: //Gabilani
                    defaultFlags.Add("Double Headed");
                    break;
                case 22: //Anemone
                    defaultFlags.Add("Aphrodisiac");
                    defaultFlags.Add("Stinger Base");
                    break;
                case 23: //Tentacle
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Flared");
                    defaultFlags.Add("Lubricated");
                    break;
                case 24: //Kui-tan
                    defaultFlags.Add("Knotted");
                    defaultFlags.Add("Sheathed");
                    defaultFlags.Add("Tapered");
                    break;
                case 34: //Venus Pitcher
                    defaultFlags.Add("Prehensile");
                    defaultFlags.Add("Tapered");
                    break;
                case 42: //Raskvel
                    defaultFlags.Add("Smooth");
                    break;
                case 46: //Vanae
                    defaultFlags.Add("Smooth");
                    defaultFlags.Add("Lubricated");
                    break;
                case 53: //Daynar
                    defaultFlags.Add("Tapered");
                    break;
                case 54: //Cockvine
                    defaultFlags.Add("Prehensile");
                    break;
                case 55: //Nyrea
                    defaultFlags.Add("Flared");
                    defaultFlags.Add("Knotted");
                    defaultFlags.Add("Ovipositor");
                    break;
                case 64: //Hradian
                    defaultFlags.Add("Foreskinned");
                    defaultFlags.Add("Flared");
                    break;
                case 65: //Gryvain
                    defaultFlags.Add("Knotted");
                    defaultFlags.Add("Scaled");
                    defaultFlags.Add("Ribbed");
                    break;
                case 93:  // Lion
                    defaultFlags.Add("Nubby");
                    defaultFlags.Add("Sheathed");
                    break;
            }
            return defaultFlags;
        }

        public XmlEnum[] CockTypes
        {
            get
            {
                return XmlData.Current.Body.CockTypes;
            }
        }

        public List<FlagItem> CockFlags
        {
            get { return getFlagList(GetObj("cockFlags"), XmlData.Current.Body.CockFlags); }
        }

        public String Description
        {
            get
            {
                return String.Format( "a {0} {1}\" {2} cock."
                                    , CockColor
                                    , EffectiveLength.ToString()
                                    , XmlData.EnumIDToName(CockType, CockTypes).ToLower() );
            }
        }
    }
}
