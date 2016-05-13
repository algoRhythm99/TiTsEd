using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public sealed partial class GameVM : ObjectVM
    {
        public GameVM(AmfFile file, GameVM previousVM)
            : base(file)
        {
            // Import missing items
            var unknownItemGroup = XmlData.Current.ItemGroups.Last();
        }

        static void ImportMissingNamedVectors(AmfObject cocItems, IEnumerable<XmlNamedVector4> xmlItems, string cocNameProperty, Func<AmfObject, String> descriptionGetter = null, IList<XmlNamedVector4> targetXmlList = null)
        {
            if (targetXmlList == null) targetXmlList = (IList<XmlNamedVector4>)xmlItems;
            var xmlNames = new HashSet<String>(xmlItems.Select(x => x.Name));

            foreach (var pair in cocItems)
            {
                var name = pair.ValueAsObject.GetString(cocNameProperty);
                if (xmlNames.Contains(name)) continue;
                xmlNames.Add(name);

                var xml = new XmlNamedVector4 { Name = name };
                if (descriptionGetter != null) xml.Description = descriptionGetter(pair.ValueAsObject);
                targetXmlList.Add(xml);
            }
        }

        AmfObject pc = null;
        private AmfObject getPC()
        {
            if (pc != null)
            {
                return pc;
            }
            var chars = GetObj("characters");
            return pc = chars.GetObj("PC");
        }

        public string Name
        {
            get { return getPC().GetString("short"); }
            set {
                SetValue(getPC(), "short", value);
                SetValue("saveName", value);
            }
        }

        public string Notes
        {
            get { return GetString("saveNotes"); }
            set { SetValue("saveNotes", String.IsNullOrWhiteSpace(value) ? "No notes available." : value); }
        }

        public int Credits
        {
            get { return getPC().GetInt("credits"); }
            set { SetValue(getPC(), "credits", value); }
        }

        public int Days
        {
            get { return GetInt("daysPassed"); }
            set { SetValue("daysPassed", value); }
        }

        public int Hours
        {
            get { return GetInt("currentHours"); }
            set { SetValue("currentHours", value); }
        }

        public int Minutes
        {
            get { return GetInt("currentMinutes"); }
            set { SetValue("currentMinutes", value); }
        }

        public int Physique
        {
            get { return getPC().GetInt("physiqueRaw"); }
            set { SetValue(getPC(), "physiqueRaw", value); }
        }

        public int Reflexes
        {
            get { return getPC().GetInt("reflexesRaw"); }
            set { SetValue(getPC(), "reflexesRaw", value); }
        }

        public int Aim
        {
            get { return getPC().GetInt("aimRaw"); }
            set { SetValue(getPC(), "aimRaw", value); }
        }

        public int Intelligence
        {
            get { return getPC().GetInt("intelligenceRaw"); }
            set { SetValue(getPC(), "intelligenceRaw", value); }
        }

        public int Willpower
        {
            get { return getPC().GetInt("willpowerRaw"); }
            set { SetValue(getPC(), "willpowerRaw", value); }
        }

        public double Libido
        {
            get { return getPC().GetDouble("libidoRaw"); }
            set { SetValue(getPC(), "libidoRaw", value); }
        }

        public int HP
        {
            get { return getPC().GetInt("HPRaw"); }
            set { SetValue(getPC(), "HPRaw", value); }
        }

        public int MaxHP
        {
            get
            {
                return 10000;
            }
        }

        public double Lust
        {
            get { return getPC().GetDouble("lustRaw"); }
            set { SetValue(getPC(), "lustRaw", value); }
        }

        public int MaxLust
        {
            get { return 100; }
        }

        public int Energy
        {
            get { return getPC().GetInt("energyRaw"); }
            set { SetValue(getPC(), "energyRaw", value); }
        }

        public int MaxEnergy
        {
            get { return 100; }
        }
    }
}
