using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class CockArrayVM : ArrayVM<CockVM> {
        public CockArrayVM(GameVM game, AmfObject obj)
            : base(obj, x => new CockVM(game, x)) {
        }

        protected override AmfObject CreateNewObject() {
            var obj = new AmfObject(AmfTypes.Object);

            obj["cLengthRaw"] = 5;
            obj["cLengthMod"] = 0;

            obj["cThicknessRatioRaw"] = 1.0;
            obj["cThicknessRatioMod"] = 0;

            obj["cType"] = 0;
            obj["cockColor"] = "pink";

            obj["knotMultiplier"] = 1;
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
        public CockVM(GameVM game, AmfObject obj)
            : base(obj) {

            _game = game;
        }

        public GameVM _game { get; set; }

        public int Length {
            get { return GetInt("cLengthRaw"); }
            set {
                SetValue("cLengthRaw", value);
                OnPropertyChanged("Description");
            }
        }

        public double ThicknessRatio {
            get { return GetDouble("cThicknessRatioRaw"); }
            set {
                SetValue("cThicknessRatioRaw", value);
                OnPropertyChanged("ThicknessTip");
            }
        }

        public string ThicknessTip {
            get {
                return Length/6*ThicknessRatio +"\"";
            }
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

                output += Length + "\" ";

                string type = "unknown";
                foreach(var vtype in CockTypes) {
                    if(vtype.ID == CockType) {
                        type = vtype.Name.ToLower();
                    }
                }

                return output + type + " cock.";
            }
        }
    }
}
