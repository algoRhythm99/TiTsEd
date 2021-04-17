using System;
using System.Collections.Generic;
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
                UpdateCalculatedValues();
            }
        }

        public int LengthMod
        {
            get { return GetInt("cLengthMod"); }
            set
            {
                SetValue("cLengthMod", value);
                UpdateCalculatedValues();
            }
        }

        public double EffectiveLength
        {
            get
            {
                double val = Length + LengthMod;
                if (val < 0)
                {
                    return 0;
                }
                return val;
            }
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
                UpdateCalculatedValues();
            }
        }

        public int ThicknessRatioMod
        {
            get { return GetInt("cThicknessRatioMod"); }
            set
            {
                SetValue("cThicknessRatioMod", value);
                UpdateCalculatedValues();
            }
        }

        public double EffectiveThicknessRatio
        {
            get
            {
                double val = ThicknessRatio + ThicknessRatioMod;
                if (val < 0)
                {
                    return 0;
                }
                return val;
            }
        }

        public double Thickness
        {
            get
            {
                return (EffectiveLength / 6.0) * EffectiveThicknessRatio;
            }
        }

        public string ThicknessTip
        {
            get
            {
                return Extensions.GetFeetAndCentimetersDescription(Thickness);
            }
        }

        public double Volume
        {
            get
            {
                double radius = Thickness / 2.0;
                double cylinder = AS3_Math.PI * Math.Pow(radius, 2) * (EffectiveLength - radius);
                double tip = (4.0 / 3.0) * AS3_Math.PI * Math.Pow(radius, 3) / 2.0;
                if (HasFlag(GLOBAL.FLAGS.FLAG_BLUNT))
                {
                    tip = AS3_Math.PI * Math.Pow(radius, 3);
                }
                if (HasFlag(GLOBAL.FLAGS.FLAG_FLARED))
                {
                    tip *= 1.3;
                }
                if (HasFlag(GLOBAL.FLAGS.FLAG_TAPERED))
                {
                    tip *= 0.75;
                    cylinder = cylinder * 0.75;
                }
                if (HasFlag(GLOBAL.FLAGS.FLAG_DOUBLE_HEADED))
                {
                    tip = 2.0 * ((2.0/3.0) * AS3_Math.PI * Math.Pow(AS3_Math.SQRT2 * radius / 2.0, 3)) + (0.512 * AS3_Math.PI * Math.Pow(radius, 3));
                }
                return Math.Round((tip + cylinder) * 100.0) / 100.0;
            }
        }

        public double EffectiveVolume
        {
            get
            {
                double vol = Volume;
                if (HasFlag(GLOBAL.FLAGS.FLAG_LUBRICATED) || HasFlag(GLOBAL.FLAGS.FLAG_GOOEY))
                {
                    vol *= 0.75;
                }
                if (HasFlag(GLOBAL.FLAGS.FLAG_STICKY))
                {
                    vol *= 1.25;
                }
                return Math.Round(vol*100.0)/100.0;
            }
        }

        public string VolumeTip
        {
            get
            {
                return Extensions.GetCubicInchesOrCentimetersDescription(Volume);
            }
        }


        public string EffectiveVolumeTip
        {
            get
            {
                return Extensions.GetCubicInchesOrCentimetersDescription(EffectiveVolume);
            }
        }


        public double CockCapacity
        {
            get
            {
                return (Volume / 6.0) * _character.Elasticity;
            }
        }

        public string CockCapacityTip
        {
            get
            {
                return Extensions.GetCubicInchesOrCentimetersDescription(CockCapacity);
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

        public AmfObject FlagsObj
        {
            get
            {
                return GetObj("cockFlags");
            }
        }

        public List<FlagItem> CockFlags
        {
            get { return getFlagList(FlagsObj, XmlData.Current.Body.CockFlags); }
        }

        public bool HasFlag(GLOBAL.FLAGS flag)
        {
            return AmfHelpers.FlagsHasFlag(FlagsObj, flag);
        }

        public String Description
        {
            get
            {
                return String.Format( "a {0} {1}\" {2} cock"
                                    , CockColor
                                    , EffectiveLength.ToString()
                                    , XmlData.EnumIDToName(CockType, CockTypes).ToLower() );
            }
        }

        private void UpdateCalculatedValues()
        {

            OnPropertyChanged("CockCapacity");
            OnPropertyChanged("CockCapacityTip");
            OnPropertyChanged("Thickness");
            OnPropertyChanged("ThicknessTip");
            OnPropertyChanged("Volume");
            OnPropertyChanged("VolumeTip");
        }
    }
}
