using System;
using System.Collections.Generic;
using System.ComponentModel;
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

                    var unknownPerks = new XmlPerkGroup { Name = "Unknown", Perks = new List<XmlStorageClass>() };
                    fileData.PerkGroups.Add(unknownPerks);

                    var unknownKeyItems = new XmlKeyItemGroup { Name = "Unknown", KeyItems = new List<XmlStorageClass>() };
                    fileData.KeyItemGroups.Add(unknownKeyItems);

                    var unknownStatusEffects = new XmlStatusEffectGroup { Name = "Unknown", StatusEffects = new List<XmlStorageClass>() };
                    fileData.StatusEffectGroups.Add(unknownStatusEffects);

                    var unknownItems = new XmlItemGroup { Name = "Unknown", Items = new List<XmlItem>() };
                    fileData.ItemGroups.Add(unknownItems);

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

        public static string EnumIDToName(int typeID, XmlEnum[] data)
        {
            string type = "unknown";
            foreach (var vtype in data)
            {
                if (vtype.ID == typeID)
                {
                    type = vtype.Name;
                }
            }
            //String.Format("[ID#: {0}] <unknown>", typeID);
            return type;
        }
    }

    [XmlRoot("TiTsEd")]
    public sealed class XmlDataSet {
        [XmlElement("General")]
        public XmlGeneralSet General { get; set; }

        [XmlArray("CodexEntries"), XmlArrayItem("CodexEntry")]
        public List<XmlCodexEntry> CodexEntries { get; set; }

        [XmlElement("Body")]
        public XmlBodySet Body { get; set; }

        [XmlArray, XmlArrayItem("ItemType")]
        public XmlEnum[] ItemTypes { get; set; }

        [XmlArray("Items"), XmlArrayItem("ItemGroup")]
        public List<XmlItemGroup> ItemGroups { get; set; }

        /*
        [XmlArray("ShipGear"), XmlArrayItem("GearGroup")]
        public List<XmlItemGroup> ShipGearGroups { get; set; }
        */

        [XmlArray("Perks"), XmlArrayItem("PerkGroup")]
        public List<XmlPerkGroup> PerkGroups { get; set; }

        [XmlArray("KeyItems"), XmlArrayItem("KeyItemGroup")]
        public List<XmlKeyItemGroup> KeyItemGroups { get; set; }

        [XmlArray("StatusEffects"), XmlArrayItem("StatusEffectGroup")]
        public List<XmlStatusEffectGroup> StatusEffectGroups { get; set; }

        [XmlArray, XmlArrayItem("Flag")]
        public XmlEnum[] Flags { get; set; }
    }

    public sealed class XmlGeneralSet {
        [XmlArray, XmlArrayItem("Upbringing")]
        public XmlEnum[] Upbringings { get; set; }

        [XmlArray, XmlArrayItem("ClassType")]
        public XmlEnum[] ClassTypes { get; set; }

        [XmlArray, XmlArrayItem("OriginalRace")]
        public string[] OriginalRaces { get; set; }

        [XmlArray, XmlArrayItem("Affinity")]
        public string[] Affinities { get; set; }

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

        [XmlArray, XmlArrayItem("EyeColor")]
        public string[] EyeColors { get; set; }

        [XmlArray, XmlArrayItem("EarFlag")]
        public XmlEnum[] EarFlags { get; set; }

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

        [XmlArray, XmlArrayItem("PregnancyType")]
        public string[] PregnancyTypes { get; set; }

        [XmlArray, XmlArrayItem("AssFlag")]
        public XmlEnum[] AssFlags { get; set; }

        [XmlArray, XmlArrayItem("CockType")]
        public XmlEnum[] CockTypes { get; set; }

        [XmlArray, XmlArrayItem("CockFlag")]
        public XmlEnum[] CockFlags { get; set; }

        [XmlArray, XmlArrayItem("AreolaFlag")]
        public XmlEnum[] AreolaFlags { get; set; }

    }

    public sealed class XmlCodexEntry
    {
        [XmlText]
        public string Name { get; set; }

        public XmlCodexEntry() { }

        public XmlCodexEntry(string entryName)
        {
            Name = entryName;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class XmlPerkGroup {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Perk")]
        public List<XmlStorageClass> Perks { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public sealed class XmlKeyItemGroup {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("KeyItem")]
        public List<XmlStorageClass> KeyItems { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public sealed class XmlStatusEffectGroup {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("StatusEffect")]
        public List<XmlStorageClass> StatusEffects { get; set; }

        public override string ToString() {
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

    public sealed class XmlItemGroup {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Item")]
        public List<XmlItem> Items { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public sealed class XmlItem {
        public static XmlItem Empty = new XmlItem("classes.Items.Miscellaneous::EmptySlot", "<empty>", 0, "nothing");

        [XmlAttribute]
        public string ID { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string LongName { get; set; }
		[XmlAttribute]
        public string EditorName { get; set; }
        [XmlAttribute]
        public string Tooltip { get; set; }
        [XmlAttribute]
        public int Stack { get; set; }
        public int Version { get; set; }

        [XmlElement("ItemField")]
        public List<XmlObjectField> Fields { get; set; }

        public XmlItem() { }

        public XmlItem(string id, string name, int stack, string longName)
        {
            ID = id;
            Name = name;
            Stack = stack;
            LongName = longName;
        }

        public string DisplayName {
            get {
                return GetDisplayName(this);
            }
        }

        public override string ToString() {
            return Name;
        }

        public static string GetDisplayName(XmlItem item, string typeId = null, string longName = null) {
            if (item == Empty && typeId == Empty.ID) {
                return item.Name;
            }
			var _editorName = item.EditorName;
			if(null != _editorName) {
				//return the editor specific name that is NEVER EVER saved to a game save.
				//this is used for unique item variants that have no name differences in game.
                return _editorName;
			}
            var _longName = longName ?? item.LongName;
            if (null != _longName) {
                //skip the side show and just return the long name
                return _longName;
            }
            var typeName = typeId ?? item.ID;
            var className = _longName ?? typeName.Substring(typeName.LastIndexOf(':') + 1);
            StringBuilder buf = new StringBuilder();
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
            return buf.ToString().Trim();
        }

        public string GetFieldValue(string fieldName) {
            if (null != Fields) {
                foreach (var field in Fields) {
                    if (fieldName == field.Name) {
                        return field.Value;
                    }
                }
            }
            return null;
        }

        public int GetFieldValueAsInt(string fieldName) {
            var oVal = GetFieldValue(fieldName);
            if (oVal != null) {
                int iVal = 0;
                bool success = int.TryParse(oVal, out iVal);
                return success ? iVal : 0;
            } else {
                return 0;
            }
        }

        public bool GetFieldValueAsBool(string fieldName) {
            var oVal = GetFieldValue(fieldName);
            if (oVal != null) {
                bool bVal = false;
                bool success = bool.TryParse(oVal, out bVal);
                return success ? bVal : false;
            } else {
                return false;
            }
        }
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

    /// <summary>
    /// Directly correlates to an entry in an Amf Object.
    /// </summary>
    public sealed class XmlObjectField {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }

    public sealed class XmlStorageClass {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public string DescriptionLabel { get; set; }

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

        [XmlAttribute]
        public string Tooltip { get; set; }

        [XmlAttribute]
        public string Comment { get; set; }

        [XmlIgnore]
        public bool? IsHidden { get; set; }
        [XmlAttribute("IsHidden")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool XmlIsHidden { get { return IsHidden.Value; } set { IsHidden = value; } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool XmlIsHiddenSpecified { get { return IsHidden.HasValue; } }

        [XmlIgnore]
        public bool? IsCombatOnly { get; set; }
        [XmlAttribute("IsCombatOnly")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool XmlIsCombatOnly { get { return IsCombatOnly.Value; } set { IsCombatOnly = value; } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool XmlIsCombatOnlySpecified { get { return IsCombatOnly.HasValue; } }

        [XmlAttribute]
        public string IconName { get; set; }
        [XmlIgnore]
        public int? IconShade { get; set; }
        [XmlAttribute("IconShade")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int XmlIconShade { get { return IconShade.Value; } set { IconShade = value; } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool XmlIconShadeSpecified { get { return IconShade.HasValue; } }

        [XmlAttribute]
        public int MinutesLeft { get; set; }

        public override string ToString() {
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
        Unknown,
    }
}
