using System;
using System.Collections.Generic;

using System.Text;
using TiTsEd.Model;
using TiTsEd.Common;

namespace TiTsEd.ViewModel {
    public sealed class BreastArrayVM : ArrayVM<BreastsVM> {
        public BreastArrayVM(CharacterVM character, AmfObject obj)
            : base(obj, x => new BreastsVM(character, x)) {
        }

        protected override AmfObject CreateNewObject() {
            var obj = new AmfObject(AmfTypes.Object);
            
            obj["breastRatingRaw"] = 0;
            obj["breastRatingMod"] = 0;
            obj["breastRatingHoneypotMod"] = 0;
            obj["breastRatingLactationMod"] = 0;
            obj["breasts"] = 2;
            obj["fullness"] = 0;
            obj["nippleType"] = 0;
            obj["areolaFlags"] = new AmfObject(AmfTypes.Array);
            obj["classInstance"] = "classes::BreastRowClass";
            return obj;
        }
    }

    public class BreastsVM : ObjectVM {
        public BreastsVM(CharacterVM character, AmfObject obj)
            : base(obj) {

            _character = character;
        }

        private CharacterVM _character { get; set; }

        public double Rating {
            get { return GetDouble("breastRatingRaw"); }
            set {
                SetValue("breastRatingRaw", value);
                OnPropertyChanged("Description");
            }
        }

        public double RatingMod {
            get { return GetDouble("breastRatingMod"); }
            set
            {
                SetValue("breastRatingMod", value);
                OnPropertyChanged("Description");
            }
        }

        public double RatingLactationMod {
            get { return GetDouble("breastRatingLactationMod"); }
            set {
                SetValue("breastRatingLactationMod", value);
                OnPropertyChanged("Description");
            }
        }

        public double RatingHoneypotMod {
            get { return GetDouble("breastRatingHoneypotMod"); }
            set {
                SetValue("breastRatingHoneypotMod", value);
                OnPropertyChanged("Description");
            }
        }

        public double CurrentRating {
            get {
                var num = Rating + RatingMod + RatingLactationMod + RatingHoneypotMod;
                return (num < 0) ? 0 : num;
            }
        }


        public int MaxRating {
            get { return 199; }
        }

        public int BreastCount {
            get { return GetInt("breasts"); }
            set {
                SetValue("breasts", value);
                OnPropertyChanged("Description");
            }
        }

        public int NippleType {
            get { return GetInt("nippleType"); }
            set { SetValue("nippleType", value); }
        }

        public XmlEnum[] NippleTypes {
            get {
                return XmlData.Current.Body.NippleTypes;
            }
        }

        public List<FlagItem> AreolaFlags
        {
            get { return getFlagList(GetObj("areolaFlags"), XmlData.Current.Body.AreolaFlags); }
        }

        public double Fullness {
            get { return GetDouble("fullness"); }
            set { SetValue("fullness", value); }
        }

        public String Description {
            get {
                return (int)BreastCount + " " + CupDescription + " breasts.";
            }
        }

        private string CupDescription {
            get {
                double check = CurrentRating;
                if(check < 1) return "flat";
                else if(check < 2) return "A-cup";
                else if(check < 3) return "B-cup";
                else if(check < 4) return "C-cup";
                else if(check < 5) return "D-cup";
                else if(check < 6) return "DD-cup";
                else if(check < 7) return "big DD-cup";
                else if(check < 8) return "E-cup";
                else if(check < 9) return "big E-cup";
                else if(check < 10) return "EE-cup";
                else if(check < 11) return "big EE-cup";
                else if(check < 12) return "F-cup";
                else if(check < 13) return "big F-cup";
                else if(check < 14) return "FF-cup";
                else if(check < 15) return "big FF-cup";
                else if(check < 16) return "G-cup";
                else if(check < 17) return "big G-cup";
                else if(check < 18) return "GG-cup";
                else if(check < 19) return "big GG-cup";
                else if(check < 20) return "H-cup";
                else if(check < 21) return "big H-cup";
                else if(check < 22) return "HH-cup";
                else if(check < 23) return "big HH-cup";
                else if(check < 24) return "HHH-cup";
                else if(check < 25) return "I-cup";
                else if(check < 26) return "big I-cup";
                else if(check < 27) return "II-cup";
                else if(check < 28) return "big II-cup";
                else if(check < 29) return "J-cup";
                else if(check < 30) return "big J-cup";
                else if(check < 31) return "JJ-cup";
                else if(check < 32) return "big JJ-cup";
                else if(check < 33) return "K-cup";
                else if(check < 34) return "big K-cup";
                else if(check < 35) return "KK-cup";
                else if(check < 36) return "big KK-cup";
                else if(check < 37) return "L-cup";
                else if(check < 38) return "big L-cup";
                else if(check < 39) return "LL-cup";
                else if(check < 40) return "big LL-cup";
                else if(check < 41) return "M-cup";
                else if(check < 42) return "big M-cup";
                else if(check < 43) return "MM-cup";
                else if(check < 44) return "big MM-cup";
                else if(check < 45) return "MMM-cup";
                else if(check < 46) return "large MMM-cup";
                else if(check < 47) return "N-cup";
                else if(check < 48) return "large N-cup";
                else if(check < 49) return "NN-cup";
                else if(check < 50) return "large NN-cup";
                else if(check < 51) return "O-cup";
                else if(check < 52) return "large O-cup";
                else if(check < 53) return "OO-cup";
                else if(check < 54) return "large OO-cup";
                else if(check < 55) return "P-cup";
                else if(check < 56) return "large P-cup";
                else if(check < 57) return "PP-cup";
                else if(check < 58) return "large PP-cup";
                else if(check < 59) return "Q-cup";
                else if(check < 60) return "large Q-cup";
                else if(check < 61) return "QQ-cup";
                else if(check < 62) return "large QQ-cup";
                else if(check < 63) return "R-cup";
                else if(check < 64) return "large R-cup";
                else if(check < 65) return "RR-cup";
                else if(check < 66) return "large RR-cup";
                else if(check < 67) return "S-cup";
                else if(check < 68) return "large S-cup";
                else if(check < 69) return "SS-cup";
                else if(check < 70) return "large SS-cup";
                else if(check < 71) return "T-cup";
                else if(check < 72) return "large T-cup";
                else if(check < 73) return "TT-cup";
                else if(check < 74) return "large TT-cup";
                else if(check < 75) return "U-cup";
                else if(check < 76) return "large U-cup";
                else if(check < 77) return "UU-cup";
                else if(check < 78) return "large UU-cup";
                else if(check < 79) return "V-cup";
                else if(check < 80) return "large V-cup";
                else if(check < 81) return "VV-cup";
                else if(check < 82) return "large VV-cup";
                else if(check < 83) return "W-cup";
                else if(check < 84) return "large W-cup";
                else if(check < 85) return "WW-cup";
                else if(check < 86) return "large WW-cup";
                else if(check < 87) return "X-cup";
                else if(check < 88) return "large X-cup";
                else if(check < 89) return "XX-cup";
                else if(check < 90) return "large XX-cup";
                else if(check < 91) return "Y-cup";
                else if(check < 92) return "large Y-cup";
                else if(check < 93) return "YY-cup";
                else if(check < 94) return "large YY-cup";
                else if(check < 95) return "Z-cup";
                else if(check < 96) return "large Z-cup";
                else if(check < 97) return "ZZ-cup";
                else if(check < 98) return "large ZZ-cup";
                else if(check < 99) return "ZZZ-cup";
                else if(check < 100) return "large ZZZ-cup";
                else if(check < 101) return "hyper A-cup";
                else if(check < 102) return "hyper B-cup";
                else if(check < 103) return "hyper C-cup";
                else if(check < 104) return "hyper D-cup";
                else if(check < 105) return "hyper DD-cup";
                else if(check < 106) return "hyper big DD-cup";
                else if(check < 107) return "hyper E-cup";
                else if(check < 108) return "hyper big E-cup";
                else if(check < 109) return "hyper EE-cup";
                else if(check < 110) return "hyper big EE-cup";
                else if(check < 111) return "hyper F-cup";
                else if(check < 112) return "hyper big F-cup";
                else if(check < 113) return "hyper FF-cup";
                else if(check < 114) return "hyper big FF-cup";
                else if(check < 115) return "hyper G-cup";
                else if(check < 116) return "hyper big G-cup";
                else if(check < 117) return "hyper GG-cup";
                else if(check < 118) return "hyper big GG-cup";
                else if(check < 119) return "hyper H-cup";
                else if(check < 120) return "hyper big H-cup";
                else if(check < 121) return "hyper HH-cup";
                else if(check < 122) return "hyper big HH-cup";
                else if(check < 123) return "hyper HHH-cup";
                else if(check < 124) return "hyper I-cup";
                else if(check < 125) return "hyper big I-cup";
                else if(check < 126) return "hyper II-cup";
                else if(check < 127) return "hyper big II-cup";
                else if(check < 128) return "hyper J-cup";
                else if(check < 129) return "hyper big J-cup";
                else if(check < 130) return "hyper JJ-cup";
                else if(check < 131) return "hyper big JJ-cup";
                else if(check < 132) return "hyper K-cup";
                else if(check < 133) return "hyper big K-cup";
                else if(check < 134) return "hyper KK-cup";
                else if(check < 135) return "hyper big KK-cup";
                else if(check < 136) return "hyper L-cup";
                else if(check < 137) return "hyper big L-cup";
                else if(check < 138) return "hyper LL-cup";
                else if(check < 139) return "hyper big LL-cup";
                else if(check < 140) return "hyper M-cup";
                else if(check < 141) return "hyper big M-cup";
                else if(check < 142) return "hyper MM-cup";
                else if(check < 143) return "hyper big MM-cup";
                else if(check < 144) return "hyper MMM-cup";
                else if(check < 145) return "hyper large MMM-cup";
                else if(check < 146) return "hyper N-cup";
                else if(check < 147) return "hyper large N-cup";
                else if(check < 148) return "hyper NN-cup";
                else if(check < 149) return "hyper large NN-cup";
                else if(check < 150) return "hyper O-cup";
                else if(check < 151) return "hyper large O-cup";
                else if(check < 152) return "hyper OO-cup";
                else if(check < 153) return "hyper large OO-cup";
                else if(check < 154) return "hyper P-cup";
                else if(check < 155) return "hyper large P-cup";
                else if(check < 156) return "hyper PP-cup";
                else if(check < 157) return "hyper large PP-cup";
                else if(check < 158) return "hyper Q-cup";
                else if(check < 159) return "hyper large Q-cup";
                else if(check < 160) return "hyper QQ-cup";
                else if(check < 161) return "hyper large QQ-cup";
                else if(check < 162) return "hyper R-cup";
                else if(check < 163) return "hyper large R-cup";
                else if(check < 164) return "hyper RR-cup";
                else if(check < 165) return "hyper large RR-cup";
                else if(check < 166) return "hyper S-cup";
                else if(check < 167) return "hyper large S-cup";
                else if(check < 168) return "hyper SS-cup";
                else if(check < 169) return "hyper large SS-cup";
                else if(check < 170) return "hyper T-cup";
                else if(check < 171) return "hyper large T-cup";
                else if(check < 172) return "hyper TT-cup";
                else if(check < 173) return "hyper large TT-cup";
                else if(check < 174) return "hyper U-cup";
                else if(check < 175) return "hyper large U-cup";
                else if(check < 176) return "hyper UU-cup";
                else if(check < 177) return "hyper large UU-cup";
                else if(check < 178) return "hyper V-cup";
                else if(check < 179) return "hyper large V-cup";
                else if(check < 180) return "hyper VV-cup";
                else if(check < 181) return "hyper large VV-cup";
                else if(check < 182) return "hyper W-cup";
                else if(check < 183) return "hyper large W-cup";
                else if(check < 184) return "hyper WW-cup";
                else if(check < 185) return "hyper large WW-cup";
                else if(check < 186) return "hyper X-cup";
                else if(check < 187) return "hyper large X-cup";
                else if(check < 188) return "hyper XX-cup";
                else if(check < 189) return "hyper large XX-cup";
                else if(check < 190) return "hyper Y-cup";
                else if(check < 191) return "hyper large Y-cup";
                else if(check < 192) return "hyper YY-cup";
                else if(check < 193) return "hyper large YY-cup";
                else if(check < 194) return "hyper Z-cup";
                else if(check < 195) return "hyper large Z-cup";
                else if(check < 196) return "hyper ZZ-cup";
                else if(check < 197) return "hyper large ZZ-cup";
                else if(check < 198) return "hyper ZZZ-cup";
                else if(check < 199) return "hyper large ZZZ-cup";
                return "Jacques00-cup";
            }
        }

    }
}
