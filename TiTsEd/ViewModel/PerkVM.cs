﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public sealed class PerkGroupVM : BindableBase
    {
        readonly GameVM _game;

        public PerkGroupVM(GameVM game, string name, PerkVM[] perks)
        {
            _game = game;
            Name = name;
            Perks = new UpdatableCollection<PerkVM>(perks.Where(x => x.Match(_game.PerkSearchText)));
        }

        public new string Name
        {
            get;
            private set;
        }

        public UpdatableCollection<PerkVM> Perks
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

    public sealed class PerkVM : NamedVector4VM
    {
        public PerkVM(GameVM game, AmfObject perksArray, XmlNamedVector4 xml)
            : base(game, perksArray, xml)
        {
        }

        protected override void InitializeObject(AmfObject obj)
        {
            StorageClassVM.initialize(obj);
            obj["storageName"] = _xml.Name;
        }

        protected override bool IsObject(AmfObject obj)
        {
            var storageName = obj.GetString("storageName");
            StorageClassVM.fixup(obj);
            return storageName == _xml.Name;
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
