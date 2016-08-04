using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TiTsEd.Model {
    public sealed class XmlData {
        // Kind of hacky I suppose, but for something this simple it beats creating a discriminated union
        // or juggling a filename list/enum pair
        public static class Files {
            public const string TiTs = "TiTsEd.Data.xml";
            public static readonly IEnumerable<string> All = new string[] { TiTs };
        }

        private static Dictionary<string, XmlDataSet> _files = new Dictionary<string, XmlDataSet>();

        private static string _selectedFile { get; set; }

        public static void Select(string xmlFile) { _selectedFile = xmlFile; }

        public static XmlDataSet Current { get { return _files[_selectedFile]; } }

        public static XmlLoadingResult LoadXml(string xmlFile) {
            try {
                var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                path = Path.Combine(path, xmlFile);

                using (var stream = File.OpenRead(path)) {
                    XmlSerializer s = new XmlSerializer(typeof(XmlDataSet));
                    var fileData = s.Deserialize(stream) as XmlDataSet;

                    var unknownPerks = new XmlPerkGroup { Name = "Unknown", Perks = new List<XmlNamedVector4>() };
                    fileData.PerkGroups.Add(unknownPerks);

                    //var unknownItems = new XmlItemGroup { Name = "Unknown", Items = new List<XmlItem>(), Category = ItemCategories.Unknown };
                    //fileData.ItemGroups.Add(unknownItems);

                    _files.Add(xmlFile, fileData);
                    if (_files.Count == 1) Select(xmlFile);

                    return XmlLoadingResult.Success;
                }
            } catch (UnauthorizedAccessException) {
                return XmlLoadingResult.NoPermission;
            } catch (SecurityException) {
                return XmlLoadingResult.NoPermission;
            } catch (FileNotFoundException) {
                return XmlLoadingResult.MissingFile;
            }
        }

        /// <summary>
        /// Get's the first enum in the set of enums that matches the given id.
        /// </summary>
        /// <param name="data">set of enums to search</param>
        /// <param name="id">id value to match</param>
        /// <returns>the matching XmlEnum or null if not found</returns>
        public static XmlEnum LookupEnumByID(XmlEnum[] data, int id) {
            foreach (XmlEnum datum in data) {
                if (datum.ID == id) {
                    return datum;
                }
            }
            return null;
        }

        /// <summary>
        /// Get's the first enum in the set of enums that matches the given name.
        /// </summary>
        /// <param name="data">set of enums to search</param>
        /// <param name="name">name value to match</param>
        /// <returns>the matching XmlEnum or null if not found</returns>
        public static XmlEnum LookupEnumByName(XmlEnum[] data, string name) {
            if (String.IsNullOrEmpty(name)) {
                return null;
            }
            foreach (XmlEnum datum in data) {
                if (name.Equals(datum.Name)) {
                    return datum;
                }
            }
            return null;
        }
    }

    [XmlRoot("TiTsEd")]
    public sealed class XmlDataSet {
        [XmlElement("General")]
        public XmlGeneralSet General { get; set; }

        [XmlElement("Body")]
        public XmlBodySet Body { get; set; }

        [XmlArray("Items"), XmlArrayItem("Item")]
        public XmlItem[] Items { get; set; }

        [XmlArray("Perks"), XmlArrayItem("PerkGroup")]
        public List<XmlPerkGroup> PerkGroups { get; set; }
    }

    public sealed class XmlGeneralSet {
        [XmlArray, XmlArrayItem("ClassType")]
        public XmlEnum[] ClassTypes { get; set; }

        [XmlArray, XmlArrayItem("CopyTag")]
        public string[] CopyTags { get; set; }
    }

    public sealed class XmlBodySet {
        [XmlArray, XmlArrayItem("SkinType")]
        public XmlEnum[] SkinTypes { get; set; }

        [XmlArray, XmlArrayItem("SkinFlag")]
        public XmlEnum[] SkinFlags { get; set; }

        [XmlArray, XmlArrayItem("SkinTone")]
        public string[] SkinTones { get; set; }

        [XmlArray, XmlArrayItem("HairType")]
        public XmlEnum[] HairTypes { get; set; }

        [XmlArray, XmlArrayItem("HairColor")]
        public string[] HairColors { get; set; }

        [XmlArray, XmlArrayItem("HairStyle")]
        public string[] HairStyles { get; set; }

        [XmlArray, XmlArrayItem("FaceType")]
        public XmlEnum[] FaceTypes { get; set; }

        [XmlArray, XmlArrayItem("FaceFlag")]
        public XmlEnum[] FaceFlags { get; set; }

        [XmlArray, XmlArrayItem("EyeType")]
        public XmlEnum[] EyeTypes { get; set; }

        [XmlArray, XmlArrayItem("EarType")]
        public XmlEnum[] EarTypes { get; set; }

        [XmlArray, XmlArrayItem("EarLengthEnable")]
        public string[] EarLengthEnables { get; set; }

        [XmlArray, XmlArrayItem("TongueType")]
        public XmlEnum[] TongueTypes { get; set; }

        [XmlArray, XmlArrayItem("TongueFlag")]
        public XmlEnum[] TongueFlags { get; set; }

        [XmlArray, XmlArrayItem("AntennaeType")]
        public XmlEnum[] AntennaeTypes { get; set; }

        [XmlArray, XmlArrayItem("HornType")]
        public XmlEnum[] HornTypes { get; set; }

        [XmlArray, XmlArrayItem("BeardStyle")]
        public XmlEnum[] BeardStyles { get; set; }

        [XmlArray, XmlArrayItem("ArmType")]
        public XmlEnum[] ArmTypes { get; set; }

        [XmlArray, XmlArrayItem("ArmFlag")]
        public XmlEnum[] ArmFlags { get; set; }

        [XmlArray, XmlArrayItem("LegType")]
        public XmlEnum[] LegTypes { get; set; }

        [XmlArray, XmlArrayItem("LegFlag")]
        public XmlEnum[] LegFlags { get; set; }

        [XmlArray, XmlArrayItem("WingType")]
        public XmlEnum[] WingTypes { get; set; }

        [XmlArray, XmlArrayItem("TailType")]
        public XmlEnum[] TailTypes { get; set; }

        [XmlArray, XmlArrayItem("TailFlag")]
        public XmlEnum[] TailFlags { get; set; }

        [XmlArray, XmlArrayItem("TailGenitalType")]
        public XmlEnum[] TailGenitalTypes { get; set; }

        [XmlArray, XmlArrayItem("TailGenitalRaceType")]
        public XmlEnum[] TailGenitalRaceTypes { get; set; }

        [XmlArray, XmlArrayItem("GenitalSpotType")]
        public XmlEnum[] GenitalSpotTypes { get; set; }

        [XmlArray, XmlArrayItem("MilkType")]
        public XmlEnum[] MilkTypes { get; set; }

        [XmlArray, XmlArrayItem("CumType")]
        public XmlEnum[] CumTypes { get; set; }

        [XmlArray, XmlArrayItem("GirlCumType")]
        public XmlEnum[] GirlCumTypes { get; set; }

        [XmlArray, XmlArrayItem("NippleType")]
        public XmlEnum[] NippleTypes { get; set; }

        [XmlArray, XmlArrayItem("DickNippleType")]
        public XmlEnum[] DickNippleTypes { get; set; }


        [XmlArray, XmlArrayItem("VaginaType")]
        public XmlEnum[] VaginaTypes { get; set; }

        [XmlArray, XmlArrayItem("VaginaFlag")]
        public XmlEnum[] VaginaFlags { get; set; }


        [XmlArray, XmlArrayItem("CockType")]
        public XmlEnum[] CockTypes { get; set; }

        [XmlArray, XmlArrayItem("CockFlag")]
        public XmlEnum[] CockFlags { get; set; }

    }

    public sealed class XmlPerkGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Perk")]
        public List<XmlNamedVector4> Perks { get; set; }


        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class XmlEnum {
        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlIgnore]
        public bool IsGrayedOut { get; set; }

        public override string ToString() {
            return ID + " - " + Name;
        }
    }

    public sealed class XmlItem {
        public static XmlItem Empty = new XmlItem("classes.Items.Miscellaneous::EmptySlot", "<empty>", "Other", 0);

        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public int Stack { get; set; }

        public XmlItem() { }

        public XmlItem(string id, string name, string type, int stack) {
            ID = id;
            Name = name;
            Type = type;
            Stack = stack;
        }

        public string LongName {
            get {
                StringBuilder buf = new StringBuilder();
                var className = ID.Substring(ID.LastIndexOf(':') + 1);
                bool lastUpper = false;
                char lastChr = '\0';
                foreach (var chr in className) {
                    if (Char.IsUpper(chr)) {
                        if (lastUpper) {
                            buf.Append('.');
                        } else if (lastChr != 'I' || chr != 'I') {
                            buf.Append(' ');
                        }
                        if (chr != 'I') {
                            lastUpper = true;
                        }
                    } else {
                        lastUpper = false;
                    }
                    buf.Append(chr);
                    lastChr = chr;
                }
                return buf.ToString();
            }
        }

        public override string ToString() {
            return Name;
        }
    }

    [Flags]
    public enum ItemCategories {
        Other = 1,
        MeleeWeapon = 2,
        RangedWeapon = 4,
        Clothing = 8,
        Armor = 8,
        Shield = 16,
        UpperUndergarment = 32,
        LowerUndergarment = 64,
        Accessory = 128,
        Consumable = 256,
        Gadget = 512,
        All = Other | MeleeWeapon | RangedWeapon | Armor |
            Clothing | Shield | UpperUndergarment | LowerUndergarment |
            Accessory | Consumable | Gadget,
    }

    public sealed class XmlName {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public sealed class XmlNamedVector4
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public double Value1 { get; set; }
        [XmlAttribute]
        public double Value2 { get; set; }
        [XmlAttribute]
        public double Value3 { get; set; }
        [XmlAttribute]
        public double Value4 { get; set; }

        [XmlAttribute]
        public string Type1 { get; set; }
        [XmlAttribute]
        public string Type2 { get; set; }
        [XmlAttribute]
        public string Type3 { get; set; }
        [XmlAttribute]
        public string Type4 { get; set; }

        [XmlAttribute]
        public string Label1 { get; set; }
        [XmlAttribute]
        public string Label2 { get; set; }
        [XmlAttribute]
        public string Label3 { get; set; }
        [XmlAttribute]
        public string Label4 { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class XmlPropCount {
        [XmlAttribute]
        public string Version { get; set; }
        [XmlAttribute]
        public int Count { get; set; }

        public override string ToString() {
            return Version + " - " + Count;
        }
    }

    public enum XmlLoadingResult {
        Success,
        InvalidFile,
        NoPermission,
        MissingFile,
    }
}
