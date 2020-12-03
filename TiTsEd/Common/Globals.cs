using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiTsEd.Common
{
    public static class GLOBAL
    {
        public enum FLAGS
        {
            FLAG_INVALID = 0,
            FLAG_LONG = 1,
            FLAG_PREHENSILE = 2,
            FLAG_LUBRICATED = 3,
            FLAG_FLUFFY = 4,
            FLAG_SQUISHY = 5,
            FLAG_SMOOTH = 6,
            FLAG_TAPERED = 7,
            FLAG_FLARED = 8,
            FLAG_KNOTTED = 9,
            FLAG_BLUNT = 10,
            FLAG_APHRODISIAC_LACED = 11,
            FLAG_STICKY = 12,
            FLAG_THICK = 13,
            FLAG_MUZZLED = 14,
            FLAG_ANGULAR = 15,
            FLAG_PLANTIGRADE = 16,
            FLAG_DIGITIGRADE = 17,
            FLAG_TENDRIL = 18,
            FLAG_AMORPHOUS = 19,
            FLAG_FURRED = 20,
            FLAG_SCALED = 21,
            FLAG_HOOVES = 22,
            FLAG_PAWS = 23,
            FLAG_HEELS = 24,
            FLAG_OVIPOSITOR = 25,
            FLAG_SHEATHED = 26,
            FLAG_TAILCOCK = 27,
            FLAG_STINGER_BASED = 28,
            FLAG_STINGER_TIPPED = 29,
            FLAG_NUBBY = 30,
            FLAG_FORESKINNED = 31,
            FLAG_HOLLOW = 32,
            FLAG_RIBBED = 33,
            FLAG_CHITINOUS = 34,
            FLAG_FEATHERED = 35,
            FLAG_DOUBLE_HEADED = 36,
            FLAG_GOOEY = 37,
            FLAG_SPIKED = 38,
            FLAG_ABSORBENT = 39,
            FLAG_SLIGHTLY_PUMPED = 40,
            FLAG_PUMPED = 41,
            FLAG_TAILCUNT = 42,
            FLAG_BEAK = 43,
            FLAG_CORKSCREWED = 44,
            FLAG_TONGUE = 45,
            FLAG_FRECKLED = 46,
            FLAG_PLUGGED = 47,
            FLAG_SCALED_PRETTY = 48,
            FLAG_SHORT = 49,
            FLAG_FLOPPY = 50,
            FLAG_HYPER_PUMPED = 51,
            FLAG_HEART_SHAPED = 52,
            FLAG_STAR_SHAPED = 53,
            FLAG_FLOWER_SHAPED = 54
        };

        public static readonly string[] FLAG_NAMES = {
              "OFFSET -- INVALID"
            , "Long"
            , "Prehensile"
            , "Lubricated"
            , "Fluffy"
            , "Squishy"
            , "Smooth"
            , "Tapered"
            , "Flared"
            , "Knotted"
            , "Blunt"
            , "Aphrodisiac"
            , "Sticky"
            , "Thick"
            , "Muzzled"
            , "Angular"
            , "Plantigrade"
            , "Digitgrade"
            , "Tendril"
            , "Amorphous"
            , "Furred"
            , "Scaled"
            , "Hooves"
            , "Paws"
            , "Heels"
            , "Ovipositor"
            , "Sheathed"
            , "Tailcock"
            , "Stinger Base"
            , "Stinger Tip"
            , "Nubby"
            , "Foreskinned"
            , "Hollow"
            , "Ribbed"
            , "Chitinous"
            , "Feathered"
            , "Double Headed"
            , "Gooey"
            , "Spiked"
            , "Absorbent"
            , "Slightly Pumped"
            , "Fully Pumped"
            , "Tailcunt"
            , "Beaked"
            , "Corkscrewed"
            , "Tongued"
            , "Freckled"
            , "Plugged"
            , "Partially Scaled"
            , "Short"
            , "Floppy"
            , "Hyper Pumped"
            , "Heart-Shaped"
            , "Star-Shaped"
            , "Flower-Shaped"
        };

        public static string FlagDisplayName(FLAGS flag)
        {
            int idx = (int)flag;
            return FLAG_NAMES[idx];
        }

        public static string FlagDisplayName(int flag)
        {
            string displayName = String.Format("ID:{0}:Unknown", flag);
            if (Enum.IsDefined(typeof(FLAGS), flag))
            {
                displayName = FLAG_NAMES[flag];
            }
            return displayName;
        }

        public static int FlagIDFromName(string name)
        {
            int idx = -1;
            if (!String.IsNullOrEmpty(name))
            {
                idx = Array.IndexOf(FLAG_NAMES, name);
            }
            return idx;
        }

        public static FLAGS FlagFromName(string name)
        {
            FLAGS flag = FLAGS.FLAG_INVALID;
            if (!String.IsNullOrEmpty(name))
            {
                int id = FlagIDFromName(name);
                if (Enum.IsDefined(typeof(FLAGS), id))
                {
                    flag = (FLAGS)FlagIDFromName(name);
                }
            }
            return flag;
        }


        public enum TYPES
        {
            TYPE_HUMAN = 0,
            TYPE_EQUINE = 1,
            TYPE_BOVINE = 2,
            TYPE_CANINE = 3,
            TYPE_FELINE = 4,
            TYPE_VULPINE = 5,
            TYPE_BEE = 6,
            TYPE_ARACHNID = 7,
            TYPE_DRIDER = 8,
            TYPE_LAPINE = 9,
            TYPE_AVIAN = 10,
            TYPE_DRACONIC = 11,
            TYPE_LIZAN = 12,
            TYPE_SNAKE = 13,
            TYPE_NAGA = GLOBAL.TYPES.TYPE_SNAKE,
            TYPE_CENTAUR = GLOBAL.TYPES.TYPE_EQUINE,
            TYPE_FROG = 14,
            TYPE_DEMONIC = 15,
            TYPE_GOOEY = 16,
            TYPE_KANGAROO = 17,
            TYPE_GABILANI = 18,
            TYPE_SHARK = 19,
            TYPE_SIREN = 20,
            TYPE_SUULA = GLOBAL.TYPES.TYPE_SIREN,
            TYPE_DEER = 21,
            TYPE_ANEMONE = 22,
            TYPE_TENTACLE = 23,
            TYPE_TANUKI = 24,
            TYPE_KUITAN = 24,
            TYPE_HUMANMASKED = 25,
            TYPE_MOUSE = 26,
            TYPE_MOUSEMAN = 27,
            TYPE_DOVE = 28,
            TYPE_DOGGIE = 29,
            TYPE_DRYAD = 30,
            TYPE_DRAGONFLY = 31,
            TYPE_MLP = 32,
            TYPE_CUNTSNAKE = 33,
            TYPE_VENUSPITCHER = 34,
            TYPE_SUCCUBUS = 35,
            TYPE_SMALLBEE = 36,
            TYPE_SMALLDEMONIC = 37,
            TYPE_SMALLDRACONIC = 38,
            TYPE_NALEEN_FACE = 39,
            TYPE_NALEEN = GLOBAL.TYPES.TYPE_NALEEN_FACE,
            TYPE_PANDA = 40,
            TYPE_MIMBRANE = 41,
            TYPE_RASKVEL = 42,
            TYPE_SYDIAN = 43,
            TYPE_LAPINARA = 44,
            TYPE_BADGER = 45,
            TYPE_VANAE = 46,
            TYPE_VANAE_MAIDEN = 47,
            TYPE_VANAE_HUNTRESS = 48,
            TYPE_LEITHAN = 49,
            TYPE_GOAT = 50,
            TYPE_SYNTHETIC = 51,
            TYPE_SIMII = 52,
            TYPE_DAYNAR = 53,
            TYPE_COCKVINE = 54,
            TYPE_NYREA = 55,
            TYPE_INHUMAN = 56,
            TYPE_OVIR = 57,
            TYPE_SYLVAN = 58,
            TYPE_DARK_SYLVAN = 59,
            TYPE_MYR = 60,
            TYPE_SAURIAN = 61,
            TYPE_RHINO = 62,
            TYPE_NARWHAL = 63,
            TYPE_HRAD = 64,
            TYPE_GRYVAIN = 65,
            TYPE_KORGONNE = 66,
            TYPE_FLOWER = 67,
            TYPE_WATERQUEEN = 68,
            TYPE_BOTHRIOC = 69,
            TYPE_MILODAN = 70,
            TYPE_WORG = 71,
            TYPE_SWINE = 72,
            TYPE_QUAD_LAPINE = 73,
            TYPE_MOUTHGINA = 74,
            TYPE_LUPINE = 75,
            TYPE_SHEEP = 76,
            TYPE_REDPANDA = 77,
            TYPE_RAHN = 78,
            TYPE_THRAGGEN = 79,
            TYPE_ADREMMALEX = 80,
            TYPE_MOTHRINE = 81,
            TYPE_FROSTWYRM = 82,
            TYPE_SANDWORM = 83,
            TYPE_SANDWORM_PARASITE = 84,
            TYPE_JANERIA = 85,
            TYPE_ROEHM = 86,
            TYPE_XHELARFOG = 87,
            TYPE_SAURMORIAN = 88,
            TYPE_HYENA = 89,
            TYPE_DZAAN = 90,
            TYPE_ZAIKA = 91,
            TYPE_CUNDARIAN = 92,
            TYPE_LION = 93
        };

        public static readonly string[] TYPE_NAMES = {
              "Human"
            , "Equine"
            , "Bovine"
            , "Canine"
            , "Feline"
            , "Vulpine"
            , "Bee"
            , "Arachnid"
            , "Drider"
            , "Lapine"
            , "Avian"
            , "Draconic"
            , "Lizan"
            , "Naga"
            , "Frog"
            , "Demonic"
            , "Gooey"
            , "Kangaroo"
            , "Gabilani"
            , "Shark"
            , "Suula"
            , "Deer"
            , "Anemone"
            , "Tentacle"
            , "Kui-tan"
            , "Human Masked"
            , "Mouse"
            , "Mouseman"
            , "Dove"
            , "Floppy Dog"
            , "Dryad"
            , "Dragonfly"
            , "MLP"
            , "Cuntsnake"
            , "Venus Pitcher"
            , "Succubus"
            , "Small Bee"
            , "Small Demonic"
            , "Small Draconic"
            , "Naleen"
            , "Panda"
            , "Mimbrane"
            , "Raskvel"
            , "Sydian"
            , "Lapinara"
            , "Badger"
            , "Vanae"
            , "Vanae Maiden"
            , "Vanae Huntress"
            , "Leithan"
            , "Goat"
            , "Synthetic"
            , "Simii"
            , "Daynar"
            , "Cockvine"
            , "Nyrea"
            , "Inhuman"
            , "Ovir"
            , "Sylvan"
            , "Dark Sylvan"
            , "Myr"
            , "Saurian"
            , "Rhino"
            , "Narwhal"
            , "Hradian"
            , "Gryvain"
            , "Korgonne"
            , "Flower"
            , "Water Queen"
            , "Bothrioc"
            , "Milodan"
            , "Worg"
            , "Swine"
            , "Quad Lapine"
            , "Mouthgina"
            , "Lupine"
            , "Sheep"
            , "Lesser Panda"
            , "Rahn"
            , "Thraggen"
            , "Adremmalex"
            , "Mothrine"
            , "Frostwyrm"
            , "Sand Worm"
            , "Butt Bug"
            , "Janeria"
            , "Roehm"
            , "Xhelarfog"
            , "Saurmorian"
            , "Hyena"
            , "Dzaan"
            , "Zaika"
            , "Cundarian"
            , "Lion"
        };


        public static string TypeDisplayName(TYPES typesType)
        {
            int idx = (int)typesType;
            return TYPE_NAMES[idx];
        }

        public static string TypeDisplayName(int type)
        {
            string displayName = String.Format("ID:{0}:Unknown", type);
            if (Enum.IsDefined(typeof(TYPES), type))
            {
                displayName = TYPE_NAMES[type];
            }
            return displayName;
        }

        public static int TypeIDFromName(string name)
        {
            int idx = 0;
            if (!String.IsNullOrEmpty(name))
            {
                idx = Array.IndexOf(TYPE_NAMES, name);
                if (idx < 0)
                {
                    idx = 0;
                }
            }
            return idx;
        }

        public static TYPES TypeFromName(string name)
        {
            TYPES typeType = TYPES.TYPE_HUMAN;
            if (!String.IsNullOrEmpty(name))
            {
                typeType = (TYPES)TypeIDFromName(name);
            }
            return typeType;
        }
    }
}
