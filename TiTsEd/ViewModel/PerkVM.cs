using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public sealed class PerkArrayVM : ArrayVM<PerkVM>
    {
        public PerkArrayVM(GameVM game, AmfObject obj)
            : base(obj, x => new PerkVM(game, x))
        {
        }

        protected override AmfObject CreateNewObject()
        {
            var obj = new AmfObject(AmfTypes.Object);

            PerkVM.initialize(obj);

            return obj;
        }
    }

    public class PerkVM : ObjectVM
    {
        public PerkVM(GameVM game, AmfObject obj)
            : base(obj)
        {
            _game = game;
        }

        public GameVM _game { get; set; }

        public static void initialize(AmfObject obj, bool reset=true)
        {
            if (null == obj)
            {
                obj = new AmfObject(AmfTypes.Object);
            }
            if (reset)
            {
                obj["storageName"] = "";
                obj["tooltip"] = "";
                obj["iconName"] = "";
                obj["iconShade"] = 16777215;
                obj["minutesLeft"] = 0;
                obj["value1"] = 0;
                obj["value2"] = 0;
                obj["value3"] = 0;
                obj["value4"] = 0;
                obj["hidden"] = true;
                obj["combatOnly"] = false;

                obj["classInstance"] = "classes::StorageClass";
            }
        }

        public static void fixup(AmfObject obj)
        {
            PerkVM.initialize(obj, false);
            // Fixes saves which have NaNs for some perk values
            if (double.IsNaN(obj.GetDouble("value1"))) obj["value1"] = 0;
            if (double.IsNaN(obj.GetDouble("value2"))) obj["value2"] = 0;
            if (double.IsNaN(obj.GetDouble("value3"))) obj["value3"] = 0;
            if (double.IsNaN(obj.GetDouble("value4"))) obj["value4"] = 0;
        }

        public string Name
        {
            get { return GetString("storageName"); }
            set
            {
                SetValue("storageName", value);
            }
        }

        public string IconName
        {
            get { return GetString("iconName"); }
            set
            {
                SetValue("iconName", value);
            }
        }

        public int IconShade
        {
            get { return GetInt("iconShade"); }
            set
            {
                SetValue("iconShade", value);
            }
        }

        public int MinutesLeft
        {
            get { return GetInt("minutesLeft"); }
            set
            {
                SetValue("minutesLeft", value);
            }
        }

        public string Description
        {
            get { return GetString("tooltip"); }
            set
            {
                SetValue("tooltip", value);
            }
        }

        public double Value1
        {
            get { return GetDouble("value1"); }
            set
            {
                SetValue("value1", value);
            }
        }

        public double Value2
        {
            get { return GetDouble("value2"); }
            set
            {
                SetValue("value2", value);
            }
        }

        public double Value3
        {
            get { return GetDouble("value3"); }
            set
            {
                SetValue("value3", value);
            }
        }

        public double Value4
        {
            get { return GetDouble("value4"); }
            set
            {
                SetValue("value4", value);
            }
        }

        public bool IsHidden
        {
            get { return GetBool("hidden"); }
            set
            {
                SetValue("hidden", value);
            }
        }

        public bool IsCombatOnly
        {
            get { return GetBool("combatOnly"); }
            set
            {
                SetValue("combatOnly", value);
            }
        }
    }

    public sealed class PerkGroupVM : BindableBase
    {
        readonly GameVM _game;

        public PerkGroupVM(GameVM game, string name, PerkVectorVM[] perks)
        {
            _game = game;
            Name = name;
            Perks = new UpdatableCollection<PerkVectorVM>(perks.Where(x => x.Match(_game.PerkSearchText)));
        }

        public string Name
        {
            get;
            private set;
        }

        public UpdatableCollection<PerkVectorVM> Perks
        {
            get;
            private set;
        }

        public Visibility Visibility
        {
            get { return Perks.Count != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void Update()
        {
            Perks.Update();
            OnPropertyChanged("Visibility");
        }
    }

    public sealed class PerkVectorVM : NamedVector4VM
    {
        public PerkVectorVM(GameVM game, AmfObject perksArray, XmlNamedVector4 xml)
            : base(game, perksArray, xml)
        {
        }

        protected override void InitializeObject(AmfObject obj)
        {
            PerkVM.initialize(obj);
            obj["storageName"] = _xml.Name;
        }

        protected override bool IsObject(AmfObject obj)
        {
            var storageName = obj.GetString("storageName");
            PerkVM.fixup(obj);
            return storageName == _xml.Name;
        }

        public string Description
        {
            get { return GetString("tooltip"); }
            set
            {
                SetValue("tooltip", value);
            }
        }

        public string IconName
        {
            get { return GetString("iconName"); }
            set
            {
                SetValue("iconName", value);
            }
        }

        public int IconShade
        {
            get { return GetInt("iconShade"); }
            set
            {
                SetValue("iconShade", value);
            }
        }

        public int MinutesLeft
        {
            get { return GetInt("minutesLeft"); }
            set
            {
                SetValue("minutesLeft", value);
            }
        }

        public bool IsHidden
        {
            get { return GetBool("hidden"); }
            set
            {
                SetValue("hidden", value);
            }
        }

        public bool IsCombatOnly
        {
            get { return GetBool("combatOnly"); }
            set
            {
                SetValue("combatOnly", value);
            }
        }

        protected override void NotifyGameVM()
        {
            _game.OnPerkChanged(_xml.Name);
        }

        public override bool IsOwned
        {
            get { return GetObject() != null; }
            set
            {
                var pair = _items.FirstOrDefault(x => IsObject(x.ValueAsObject));
                if ((pair != null) == value) return;

                if (value)
                {
                    var obj = new AmfObject(AmfTypes.Object);
                    InitializeObject(obj);
                    obj["value1"] = _xml.Value1;
                    obj["value2"] = _xml.Value2;
                    obj["value3"] = _xml.Value3;
                    obj["value4"] = _xml.Value4;
                    obj["tooltip"] = _xml.Description;
                    //obj["hidden"] = _xml.Hidden;
                    //obj["combatOnly"] = _xml.CombatOnly;
                    _items.Push(obj);
                }
                else
                {
                    _items.Pop((int)pair.Key);
                }
                OnPropertyChanged("Value1");
                OnPropertyChanged("Value2");
                OnPropertyChanged("Value3");
                OnPropertyChanged("Value4");
                OnSavePropertyChanged();
                OnIsOwnedChanged();
            }
        }

        protected override void OnIsOwnedChanged()
        {
            _game.OnPerkAddedOrRemoved(_xml.Name, IsOwned);
        }
    }
}
