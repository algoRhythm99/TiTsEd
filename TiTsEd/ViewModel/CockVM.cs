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
            
            obj["breastRatingRaw"] = 0;
            obj["breastRatingMod"] = 0;
            obj["breastRatingHoneypotMod"] = 0;
            obj["breasts"] = 2;
            obj["fullness"] = 0;
            obj["nippleType"] = 0;
            obj["breastRatingLactationMod"] = 0;
            obj["classInstance"] = "classes::BreastRowClass";
            return obj;
        }
    }

    public class CockVM : ObjectVM {
        public CockVM(GameVM game, AmfObject obj)
            : base(obj) {

            _game = game;
        }

        public GameVM _game { get; set; }
    }
}
