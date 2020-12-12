using System;
using System.Collections.Generic;

using System.Text;
using TiTsEd.Model;
using TiTsEd.Common;

namespace TiTsEd.ViewModel
{
    public sealed class VaginaArrayVM : ArrayVM<VaginaVM>
    {
        public VaginaArrayVM(CharacterVM character, AmfObject obj)
            : base(obj, x => new VaginaVM(character, x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Object);

            obj["type"] = 0;
            obj["hymen"] = true;
            obj["clits"] = 1;
            obj["vaginaColor"] = "pink";

            obj["wetnessRaw"] = 1;
            obj["wetnessMod"] = 0;

            obj["loosenessRaw"] = 1;
            obj["loosenessMod"] = 0;
            obj["minLooseness"] = 1;

            obj["bonusCapacity"] = 0;
            obj["shrinkCounter"] = 0;
            obj["vagooFlags"] = new AmfObject(AmfTypes.Array);

            obj["labiaPierced"] = 0;
            obj["labiaPLong"] = "";
            obj["labiaPShort"] = "";

            obj["clitPierced"] = 0;
            obj["clitPLong"] = "";
            obj["clitPShort"] = "";


            obj["classInstance"] = "classes::VaginaClass";

            return obj;
        }
    }

    public class VaginaVM : ObjectVM
    {
        public VaginaVM(CharacterVM character, AmfObject obj)
            : base(obj)
        {
            _character = character;
        }

        private CharacterVM _character { get; set; }

        public int Clits
        {
            get { return GetInt("clits"); }
            set { SetValue("clits", value); }
        }

        public bool Hymen
        {
            get { return GetBool("hymen"); }
            set
            {
                SetValue("hymen", value);
                OnPropertyChanged("Description");
            }
        }

        public double Looseness
        {
            get { return GetDouble("loosenessRaw"); }
            set
            {
                SetValue("loosenessRaw", value);
                UpdateCalculatedValues();
            }
        }

        public double LoosenessMod
        {
            get { return GetDouble("loosenessMod"); }
            set
            {
                SetValue("loosenessMod", value);
                UpdateCalculatedValues();
            }
        }

        public double EffectiveLooseness
        {
            get
            {
                var val = Looseness + LoosenessMod;
                if (val < 0.5)
                {
                    return 0.5;
                }
                if (val > 5)
                {
                    return 5;
                }
                return val;
            }
        }

        public double MinLooseness
        {
            get { return GetDouble("minLooseness"); }
            set
            {
                SetValue("minLooseness", value);
                OnPropertyChanged("Looseness");
                UpdateCalculatedValues();
            }
        }

        public double Wetness
        {
            get { return GetDouble("wetnessRaw"); }
            set
            {
                SetValue("wetnessRaw", value);
                UpdateCalculatedValues();
            }
        }

        public double WetnessMod
        {
            get { return GetDouble("wetnessMod"); }
            set
            {
                SetValue("wetnessMod", value);
                UpdateCalculatedValues();
            }
        }

        public double EffectiveWetness
        {
            get
            {
                var val = Wetness + WetnessMod;
                if (HasFlag(GLOBAL.FLAGS.FLAG_LUBRICATED))
                {
                    val = val + 2;
                }
                if (val < 0)
                {
                    return 0;
                }
                return val;
            }
        }

        public double BonusCapacity
        {
            get { return GetDouble("bonusCapacity"); }
            set { SetValue("bonusCapacity", value); UpdateCalculatedValues(); }
        }

        public double VaginaCapacity
        {
            get
            {
                double capacity = 20;
                capacity = capacity * (((EffectiveLooseness * 5.0) + 1) / 3.0);
                capacity = capacity + BonusCapacity;
                if (_character.HasStatusEffect("Soak"))
                {
                    capacity = capacity + 150;
                }
                capacity = capacity * ((EffectiveWetness + 4) / 5.0);
                capacity = capacity * _character.Elasticity;
                if (_character.IsTaur)
                {
                    capacity = capacity + 400;
                }
                return capacity;
            }
        }

        public string VaginaCapacityTip
        {
            get
            {
                return Extensions.GetCubicInchesOrCentimetersDescription(VaginaCapacity);
            }
        }

        public int ShrinkCounter
        {
            get { return GetInt("shrinkCounter"); }
            set { SetValue("shrinkCounter", value); }
        }

        public string VaginaColor
        {
            get { return GetString("vaginaColor"); }
            set
            {
                SetValue("vaginaColor", value);
                OnPropertyChanged("Description");
            }
        }

        public string[] VaginaColors
        {
            get
            {
                return XmlData.Current.Body.SkinTones;
            }
        }

        public int VaginaType
        {
            get { return GetInt("type"); }
            set
            {
                if (value != VaginaType)
                {
                    SetValue("type", value);
                    UpdateFlags(value, this.VaginaFlags, "VaginaFlags");
                    OnPropertyChanged("Description");
                }
            }
        }

        protected override List<string> GetDefaultFlags(int vType, string propertyName = null)
        {
            List<string> defaultFlags = new List<string>();
            switch (vType)
            {
                case 74: //Mouthgina
                    defaultFlags.Add("Tongued");
                    break;
                case 18: //Gabilani
                case 55: //Nyrea
                    defaultFlags.Add("Lubricated");
                    break;
                case 65: //Gryvain
                    defaultFlags.Add("Nubby");
                    break;
                case 67: //Flower
                    defaultFlags.Add("Aphrodisiac");
                    break;
                case 0:  //Human
                case 1:  //Equine
                case 3:  //Canine
                case 5:  //Vulpine
                case 6:  //Bee
                case 10: //Avian
                case 13: //Naga
                case 16: //Gooey
                case 19: //Shark
                case 20: //Suula
                case 24: //Kui-tan
                case 44: //Lapinara
                case 46: //Vanae
                case 49: //Leithan
                case 51: //Synthetic
                case 72: //Swine
                default:
                    //All of the above have no default flags, so do nothing
                    break;
            }
            return defaultFlags;
        }


        public XmlEnum[] VaginaTypes
        {
            get
            {
                return XmlData.Current.Body.VaginaTypes;
            }
        }

        public AmfObject FlagsObj
        {
            get
            {
                return GetObj("vagooFlags");
            }
        }

        public List<FlagItem> VaginaFlags
        {
            get { return getFlagList(FlagsObj, XmlData.Current.Body.VaginaFlags); }
        }

        public List<FlagItem> AssFlags
        {
            get { return getFlagList(FlagsObj, XmlData.Current.Body.AssFlags); }
        }

        public bool HasFlag(GLOBAL.FLAGS flag)
        {
            return AmfHelpers.FlagsHasFlag(FlagsObj, flag);
        }

        public String Description
        {
            get
            {
                return String.Format( "a {0}{1} {2} vagina"
                                    , ( Hymen ) ? "virgin " : ""
                                    , VaginaColor
                                    , XmlData.EnumIDToName(VaginaType, VaginaTypes).ToLower() );
            }
        }

        private void UpdateCalculatedValues()
        {
            OnPropertyChanged("EffectiveWetness");
            OnPropertyChanged("VaginaCapacity");
            OnPropertyChanged("VaginaCapacityTip");
        }
    }
}
