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

namespace TiTsEd.Model
{
    public sealed class XmlData
    {
        // Kind of hacky I suppose, but for something this simple it beats creating a discriminated union
        // or juggling a filename list/enum pair
        public static class Files
        {
            public const string TiTs = "TiTsEd.Data.xml";
            public static readonly IEnumerable<string> All = new string[] { TiTs };
        }

        private static Dictionary<string, XmlDataSet> _files = new Dictionary<string, XmlDataSet>();

        private static string _selectedFile { get; set; }

        public static void Select(string xmlFile) { _selectedFile = xmlFile; }

        public static XmlDataSet Current { get { return _files[_selectedFile]; } }

        public static XmlLoadingResult LoadXml(string xmlFile)
        {
            try
            {
                var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                path = Path.Combine(path, xmlFile);

                using (var stream = File.OpenRead(path))
                {
                    XmlSerializer s = new XmlSerializer(typeof(XmlDataSet));
                    var fileData = s.Deserialize(stream) as XmlDataSet;

                    //var unknownPerks = new XmlPerkGroup { Name = "Unknown", Perks = new List<XmlNamedVector4>() };
                    //fileData.PerkGroups.Add(unknownPerks);

                    //var unknownItems = new XmlItemGroup { Name = "Unknown", Items = new List<XmlItem>(), Category = ItemCategories.Unknown };
                    //fileData.ItemGroups.Add(unknownItems);

                    _files.Add(xmlFile, fileData);
                    if (_files.Count == 1) Select(xmlFile);

                    return XmlLoadingResult.Success;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return XmlLoadingResult.NoPermission;
            }
            catch (SecurityException)
            {
                return XmlLoadingResult.NoPermission;
            }
            catch (FileNotFoundException)
            {
                return XmlLoadingResult.MissingFile;
            }
        }
    }

    [XmlRoot("TiTsEd")]
    public sealed class XmlDataSet
    {
        [XmlElement("General")]
        public XmlGeneralSet General { get; set; }

        //remove all these until we have something we need them for
    }

    public sealed class XmlGeneralSet
    {
        [XmlArray, XmlArrayItem("ClassType")]
        public XmlEnum[] ClassTypes { get; set; }
    }

    public sealed class XmlBodySet
    {
        [XmlArray, XmlArrayItem("SkinType")]
        public XmlEnum[] SkinTypes { get; set; }
        [XmlArray, XmlArrayItem("SkinTone")]
        public String[] SkinTones { get; set; }
        [XmlArray, XmlArrayItem("SkinAdjective")]
        public String[] SkinAdjectives { get; set; }
        [XmlArray, XmlArrayItem("SkinDescription")]
        public String[] SkinDescriptions { get; set; }
        [XmlArray, XmlArrayItem("FurColor")]
        public String[] FurColors { get; set; }
        [XmlArray, XmlArrayItem("HairType")]
        public XmlEnum[] HairTypes { get; set; }
        [XmlArray, XmlArrayItem("HairColor")]
        public String[] HairColors { get; set; }
        [XmlArray, XmlArrayItem("BeardType")]
        public XmlEnum[] BeardTypes { get; set; }

        [XmlArray, XmlArrayItem("FaceType")]
        public XmlEnum[] FaceTypes { get; set; }
        [XmlArray, XmlArrayItem("TongueType")]
        public XmlEnum[] TongueTypes { get; set; }
        [XmlArray, XmlArrayItem("EyeType")]
        public XmlEnum[] EyeTypes { get; set; }
        [XmlArray, XmlArrayItem("EarType")]
        public XmlEnum[] EarTypes { get; set; }
        [XmlArray, XmlArrayItem("HornType")]
        public XmlEnum[] HornTypes { get; set; }
        [XmlArray, XmlArrayItem("AntennaeType")]
        public XmlEnum[] AntennaeTypes { get; set; }

        [XmlArray, XmlArrayItem("ArmType")]
        public XmlEnum[] ArmTypes { get; set; }
        [XmlArray, XmlArrayItem("TailType")]
        public XmlEnum[] TailTypes { get; set; }
        [XmlArray, XmlArrayItem("WingType")]
        public XmlEnum[] WingTypes { get; set; }
        [XmlArray, XmlArrayItem("WingDescription")]
        public String[] WingDescriptions { get; set; }
        [XmlArray, XmlArrayItem("LowerBodyType")]
        public XmlEnum[] LowerBodyTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingType")]
        public XmlEnum[] PiercingTypes { get; set; }
        [XmlArray, XmlArrayItem("PiercingMaterial")]
        public String[] PiercingMaterials { get; set; }

        [XmlArray, XmlArrayItem("CockType")]
        public XmlEnum[] CockTypes { get; set; }
        [XmlArray, XmlArrayItem("CockSockType")]
        public XmlItem[] CockSockTypes { get; set; }
        [XmlArray, XmlArrayItem("VaginaType")]
        public XmlEnum[] VaginaTypes { get; set; }
        [XmlArray, XmlArrayItem("VaginalWetnessLevel")]
        public XmlEnum[] VaginalWetnessLevels { get; set; }
        [XmlArray, XmlArrayItem("VaginalLoosenessLevel")]
        public XmlEnum[] VaginalLoosenessLevels { get; set; }
        [XmlArray, XmlArrayItem("AnalLoosenessLevel")]
        public XmlEnum[] AnalLoosenessLevels { get; set; }
        [XmlArray, XmlArrayItem("AnalWetnessLevel")]
        public XmlEnum[] AnalWetnessLevels { get; set; }

        [XmlArray, XmlArrayItem("PregnancyType")]
        public XmlEnum[] PregnancyTypes { get; set; }
        [XmlArray, XmlArrayItem("AnalPregnancyType")]
        public XmlEnum[] AnalPregnancyTypes { get; set; }
        [XmlArray, XmlArrayItem("EggPregnancyType")]
        public XmlEnum[] EggPregnancyTypes { get; set; }
    }

    [Flags]
    public enum ItemCategories
    {
        Other = 1,
        Weapon = 2,
        Armor = 4,
        ArmorCursed = 8,
        Shield = 16,
        Undergarment = 32,
        Jewelry = 64,
        Unknown = 128,
        All = Other | Weapon | Armor | ArmorCursed | Shield | Undergarment | Jewelry | Unknown,
    }

    public sealed class XmlItemGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ItemCategories Category { get; set; }

        [XmlElement("Item")]
        public List<XmlItem> Items { get; set; }

        public override string ToString()
        {
            return Name;
        }
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

    public sealed class XmlEnum
    {
        [XmlAttribute]
        public int ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlIgnore]
        public bool IsGrayedOut { get; set; }

        public override string ToString()
        {
            return ID + " - " + Name;
        }
    }

    public sealed class XmlItem
    {
        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        public override string ToString()
        {
            return ID + " | " + Name;
        }
    }

    public sealed class XmlName
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }

        public override string ToString()
        {
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

    public sealed class XmlPropCount
    {
        [XmlAttribute]
        public string Version { get; set; }
        [XmlAttribute]
        public int Count { get; set; }

        public override string ToString()
        {
            return Version + " - " + Count;
        }
    }

    public enum XmlLoadingResult
    {
        Success,
        InvalidFile,
        NoPermission,
        MissingFile,
    }
}
