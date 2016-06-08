using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TiTsEd.Model;

namespace TiTsEd.ViewModel {
    public sealed partial class GameVM : ObjectVM {

        public bool IsMale {
            get {
                return !IsFemale;
            }
        }

        public bool IsFemale {
            get {
                /* Decide what gender the character is for saving... */
                AmfObject arr = Character.GetObj("statusEffects");
                for (int i = 0; i < arr.DenseCount; ++i) {
                    if (arr[i] != null) {
                        if (((AmfObject)arr[i])["storageName"].ToString() == "Force Fem Gender") {
                            return true;
                        } else if (((AmfObject)arr[i])["storageName"].ToString() == "Force Male Gender") {
                            return false;
                        }
                    }
                }
                //base it off cock/vags
                bool vag = Character.Vaginas.Count > 0;
                bool cock = Character.Cocks.Count > 0;
                if (vag && !cock) {
                    return true;
                } else if (cock && !vag) {
                    return false;
                }
                //base it off feminitity
                if (Character.Feminity >= 50) {
                    return true;
                }
                return false;
            }
        }

        // Public helper for the various subordinate body part view models (e.g. CockVM)
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) {
            OnPropertyChanged(propertyName);
        }

        public void BeforeSerialization() {
            //TODO add "H"
            SetValue("playerGender", IsFemale ? "F" : "M");
            Character.BeforeSerialization();
        }
    }
}
