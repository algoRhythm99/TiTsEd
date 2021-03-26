using System;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
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

        public void UpdatePregnancyStatuses()
        {
            foreach (var pregType in XmlData.Current.Body.PregnancyTypes)
            {
                switch (pregType)
                {
                    case "BothriocPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Bothrioc Eggs");
                        }
                        break;
                    case "DeepQueenPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Queen Pregnancy End");
                            _character.RemoveStatus("Queen Pregnancy State");
                        }
                        break;
                    case "MilodanPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Milodan Pregnancy Ends");
                        }
                        break;
                    case "NyreaEggPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Nyrea Eggs Messages Available");
                        }
                        break;
                    case "RiyaPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Riya Spawn Reflex Mod");
                            _character.RemoveStatus("Riya Spawn Pregnancy Ends");
                        }
                        break;
                    case "RoyalEggPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Royal Eggs Messages Available");
                        }
                        break;
                    case "SeraSpawnPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Sera Spawn Reflex Mod");
                            _character.RemoveStatus("Sera Spawn Pregnancy Ends");
                        }
                        break;
                    case "SydianPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Sydian Pregnancy Ends");
                        }
                        break;
                    case "SiegwulfeEggnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Siegwulfe Eggnancy Stage");
                        }
                        break;
                    case "VenusPitcherFertilizedSeedCarrier":
                    case "VenusPitcherSeedCarrier":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Venus Pitcher Egg Incubation Finished");
                            _character.RemoveStatus("Venus Pitcher Seed Residue");
                        }
                        break;
                    case "ZaaltPregnancy":
                        if (0 == PregCountByType(pregType))
                        {
                            _character.RemoveStatus("Zaalt Pregnancy Ends");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Delete(int index) {
            base.Delete(index);
            Create();
            var last = (Count - 1) >= 0 ? Count - 1 : 0;
            if (last != index)
            {
                MoveItemToIndex(last, index);
            }
            UpdatePregnancyStatuses();
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
            obj["classInstance"] = "classes::PregnancyData";
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
            get {
                string val = GetString("pregnancyType");
                if ("".Equals(val))
                {
                    val = "(None)";
                }
                return val;
            }
            set {
                if (value != PregnancyType)
                {
                    string val = value;
                    if ("(None)".Equals(val))
                    {
                        val = "";
                        Reset();
                        OnPropertyChanged(null);
                    }
                    _character.PregnancyData.UpdatePregnancyStatuses();
                    SetValue("pregnancyType", val);
                    OnPropertyChanged("Description");
                }
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
