using System;
using System.Collections.Generic;

using System.Text;
using TiTsEd.Model;
using TiTsEd.Common;

namespace TiTsEd.ViewModel {
    public sealed class CockArrayVM : ArrayVM<CockVM> {
        public CockArrayVM(CharacterVM character, AmfObject obj)
            : base(obj, x => new CockVM(character, x)) {
        }

        protected override AmfObject CreateNewObject() {
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

    public class CockVM : ObjectVM {
        public CockVM(CharacterVM character, AmfObject obj)
            : base(obj) {

            _character = character;
        }

        private CharacterVM _character { get; set; }

        public double Length {
            get { return GetDouble("cLengthRaw"); }
            set {
                SetValue("cLengthRaw", value);
                OnPropertyChanged("Description");
            }
        }

        public int LengthMod {
            get { return GetInt("cLengthMod"); }
            set {
                SetValue("cLengthMod", value);
                OnPropertyChanged("Description");
            }
        }

        public double EffectiveLength {
            get { return Length + LengthMod; }
        }

        public string LengthTip {
            get { return Extensions.GetFeetAndCentimetersDescription(EffectiveLength); }
        }

        public double ThicknessRatio {
            get { return GetDouble("cThicknessRatioRaw"); }
            set {
                SetValue("cThicknessRatioRaw", value);
                OnPropertyChanged("ThicknessTip");
            }
        }

        public int ThicknessRatioMod {
            get { return GetInt("cThicknessRatioMod"); }
            set {
                SetValue("cThicknessRatioMod", value);
                OnPropertyChanged("ThicknessTip");
            }
        }

        public double EffectiveThicknessRatio {
            get { return ThicknessRatio + ThicknessRatioMod; }
        }

        public string ThicknessTip {
            get {
                var thicknessValue = Math.Round((EffectiveLength / 6) * EffectiveThicknessRatio, 2);
                return Extensions.GetFeetAndCentimetersDescription(thicknessValue); 
            }
        }

        public int Pierced {
            get { return GetInt("pierced"); }
            set { SetValue("pierced", value); }
        }

        public int Virgin {
            get { return GetInt("virgin"); }
            set { SetValue("virgin", value); }
        }

        public double KnotMultiplier {
            get { return GetDouble("knotMultiplier"); }
            set { SetValue("knotMultiplier", value); }
        }

        public double FlaccidMultiplier {
            get { return GetDouble("flaccidMultiplier"); }
            set { SetValue("flaccidMultiplier", value); }
        }

        public string CockColor {
            get { return GetString("cockColor"); }
            set {
                SetValue("cockColor", value);
                OnPropertyChanged("Description");
            }
        }

        public string[] CockColors {
            get {
                return XmlData.Current.Body.SkinTones;
            }
        }

        public int CockType {
            get { return GetInt("cType"); }
            set {
                SetValue("cType", value);
                OnPropertyChanged("Description");
            }
        }

        public XmlEnum[] CockTypes {
            get {
                return XmlData.Current.Body.CockTypes;
            }
        }

        public List<FlagItem> CockFlags {
            get { return getFlagList(GetObj("cockFlags"), XmlData.Current.Body.CockFlags); }
        }

        public String Description {
            get {
                string output = "a ";

                output += CockColor + " ";

                output += EffectiveLength + "\" ";

                string type = "unknown";
                foreach (var vtype in CockTypes) {
                    if (vtype.ID == CockType) {
                        type = vtype.Name.ToLower();
                    }
                }

                return output + type + " cock.";
            }
        }
    }
}
