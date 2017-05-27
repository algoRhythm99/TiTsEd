using System;
using System.Collections.Generic;

using System.Text;
using TiTsEd.Common;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed class PregnancyDataArrayVM : ArrayVM<PregnancyDataVM>
    {
        CharacterVM _character;

        public PregnancyDataArrayVM(CharacterVM character, AmfObject obj)
            : base(obj, x => new PregnancyDataVM(character, x)) {
            _character = character;
        }

        protected override AmfObject CreateNewObject() {
            var obj = new AmfObject(AmfTypes.Object);
            PregnancyDataVM.Reset(obj);
            return obj;
        }

        public int PregCountByType(string pregType) {
            int pregCount = 0;
            for (int i = 0; i < Count; i++) {
                var womb = this[i];
                if (pregType == womb.PregnancyType) {
                    pregCount++;
                }
            }
            return pregCount;
        }

        public override void Delete(int index) {
            base.Delete(index);
            foreach (var pregType in XmlData.Current.Body.PregnancyTypes) {
                switch (pregType) {
                    case "BothriocPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Bothrioc Eggs");
                        }
                        break;
                    case "RahnPregnancyBreedwell":
                        break;
                    case "CockvinePregnancy":
                        break;
                    case "DeepQueenPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Queen Pregnancy End");
                            _character.RemoveStatus("Queen Pregnancy State");
                        }
                        break;
                    case "EggTrainerCarryTraining":
                        break;
                    case "EggTrainerFauxPreg":
                        break;
                    case "KorgonnePregnancy":
                        break;
                    case "LapinaraPregnancy":
                        break;
                    case "MilodanPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Milodan Pregnancy Ends");
                        }
                        break;
                    case "NyreaEggPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Nyrea Eggs Messages Available");
                        }
                        break;
                    case "OvalastingEggPregnancy":
                        break;
                    case "OviliumEggPregnancy":
                        break;
                    case "PsychicTentacles":
                        break;
                    case "RenvraEggPregnancy":
                        break;
                    case "RenvraFullPregnancy":
                        break;
                    case "RiyaPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Riya Spawn Reflex Mod");
                            _character.RemoveStatus("Riya Spawn Pregnancy Ends");
                        }
                        break;
                    case "RoyalEggPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Royal Eggs Messages Available");
                        }
                        break;
                    case "SeraSpawnPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Sera Spawn Reflex Mod");
                            _character.RemoveStatus("Sera Spawn Pregnancy Ends");
                        }
                        break;
                    case "SydianPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Sydian Pregnancy Ends");
                        }
                        break;
                    case "VenusPitcherFertilizedSeedCarrier":
                    case "VenusPitcherSeedCarrier":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Venus Pitcher Egg Incubation Finished");
                            _character.RemoveStatus("Venus Pitcher Seed Residue");
                        }
                        break;
                    case "ZaaltPregnancy":
                        if (0 == PregCountByType(pregType)) {
                            _character.RemoveStatus("Zaalt Pregnancy Ends");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

    }

    public class PregnancyDataVM : ObjectVM {
        public PregnancyDataVM(CharacterVM character, AmfObject obj)
            : base(obj) {
            _character = character;
        }

        private CharacterVM _character { get; set; }

        public string SlotDescription { get; set; }

        public static void Reset(AmfObject obj) {
            obj["pregnancyType"] = "";
            obj["pregnancyIncubation"] = 0;
            obj["pregnancyQuantity"] = 0;
            obj["pregnancyIncubationMulti"] = 1.0;
            obj["pregnancyBellyRatingContribution"] = 0.0;
        }

        public void Reset() {
            Reset(GetAmfObject());
        }

        public int PregnancyQuantity {
            get { return GetInt("pregnancyQuantity"); }
            set {
                SetValue("pregnancyQuantity", value);
                OnPropertyChanged("Description");
            }
        }

        public int PregnancyIncubation {
            get { return GetInt("pregnancyIncubation"); }
            set { SetValue("pregnancyIncubation", value); }
        }

        public double PregnancyIncubationMulti {
            get { return GetDouble("pregnancyIncubationMulti"); }
            set { SetValue("pregnancyIncubationMulti", value); }
        }

        public double PregnancyBellyRatingContribution {
            get { return GetDouble("pregnancyBellyRatingContribution"); }
            set { SetValue("pregnancyBellyRatingContribution", value); }
        }

        public string PregnancyType  {
            get { return GetString("pregnancyType"); }
            set {
                SetValue("pregnancyType", value);
                OnPropertyChanged("Description");
            }
        }

        public string[] PregnancyTypes {
            get {
                return XmlData.Current.Body.PregnancyTypes;
            }
        }

        public String Description {
            get {
                return PregnancyType;
            }
        }
    }
}
