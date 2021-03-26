using System.Collections.Generic;
using System.Linq;
using TiTsEd.Model;

namespace TiTsEd.ViewModel
{
    public class CreatureVM : ObjectVM
    {
        public CreatureVM(GameVM game, AmfObject obj)
            : base(obj)
        {

            Game = game;

            // Perks
            var xmlPerks = XmlData.Current.PerkGroups.SelectMany(x => x.Perks).ToArray();
            var unknownPerkGroup = XmlData.Current.PerkGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(PerksArray, xmlPerks, unknownPerkGroup.Perks);
            UpdatePerks();

            // KeyItems
            var xmlKeys = XmlData.Current.KeyItemGroups.SelectMany(x => x.KeyItems).ToArray();
            var unknownKeyItemGroup = XmlData.Current.KeyItemGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(KeyItemsArray, xmlKeys, unknownKeyItemGroup.KeyItems);
            UpdateKeyItems();

            // Statuses
            var xmlStatusEffects = XmlData.Current.StatusEffectGroups.SelectMany(x => x.StatusEffects).ToArray();
            var unknownStatusEffectsGroup = XmlData.Current.StatusEffectGroups.Last();
            GameVM.ImportUnknownStorageClassEntries(StatusEffectsArray, xmlStatusEffects, unknownStatusEffectsGroup.StatusEffects);
            UpdateStatusEffects();
       }
    
        public GameVM Game { get; set; }

        public new string Name
        {
            get { return GetString("short"); }
            set { SetValue("short", value); }
        }


        public int HP
        {
            get { return GetInt("HPRaw"); }
            set { SetValue("HPRaw", value); }
        }

        public int MaxHP
        {
            get { return 0; }
        }

        public int HPMod
        {
            get { return GetInt("HPMod"); }
            set { SetValue("HPMod", value); }
        }

        public int Shields
        {
            get { return GetInt("shieldsRaw"); }
            set { SetValue("shieldsRaw", value); }
        }

        public int MaxShields
        {
            get { return 0; }
        }

        public AmfObject PerksArray
        {
            get { return GetObj("perks") ?? new AmfObject(AmfTypes.Array); }
        }

        public List<PerkGroupVM> PerkGroups { get; set; }

        public void UpdatePerks()
        {
            if (null == PerkGroups)
            {
                PerkGroups = new List<PerkGroupVM>();
            }
            var perksArray = PerksArray;
            foreach (var xmlGroup in XmlData.Current.PerkGroups)
            {
                var perkVMs = xmlGroup.Perks.OrderBy(x => x.Name).Select(x => new PerkVM(this, perksArray, x)).ToArray();
                foreach (var perkVM in perkVMs)
                {
                    int indx = Game.AllPerks.FindIndex(f => f.Name == perkVM.Name);
                    if (-1 == indx)
                    {
                        Game.AllPerks.Add(perkVM);
                    }
                }

                var groupVM = new PerkGroupVM(this, xmlGroup.Name, perkVMs);
                int idx = PerkGroups.FindIndex(f => f.Name == groupVM.Name);
                if (-1 == idx)
                {
                    PerkGroups.Add(groupVM);
                }
            }
        }

        public void UpdatePerksVisibility()
        {
            foreach (var group in PerkGroups)
            {
                group.Update();
            }
        }

        public bool HasPerk(string perkName)
        {
            var perk = GetPerk(perkName);
            return (null != perk) ? perk.IsOwned : false;
        }

        public void OnPerkAddedOrRemoved(string name, bool isOwned)
        {
            // Grants/removes the appropriate bonuses when a perk is added or removed.
            // We do not add stats however since the user can already change them easily.
            switch (name)
            {
                default:
                    break;
            }
            OnSavePropertyChanged("PerkGroups");
        }

        /// <summary>
        /// Returns the perk with the specified name (even if not owned by the character) AND registers a dependency between the caller property and this perk.
        /// That way, anytime the perk is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public PerkVM GetPerk(string name, string propertyName = null)
        {
            var perk = Game.AllPerks.First(x => x.Name == name);
            if (null != perk)
            {
                if (null != propertyName)
                {
                    perk.GameVMProperties.Add(propertyName);
                }
                return perk;
            }
            return null;
        }

        // Whenever a PerkVM, FlagVM, or StatusVM is modified, it notifies GameVM with those functions so that it updates its dependent properties.
        // See also GetPerk, GetFlag, and GetStatus.
        public void OnPerkChanged(string name)
        {
            foreach (var prop in Game.AllPerks.First(x => x.Name == name).GameVMProperties)
            {
                OnPropertyChanged(prop);
            }
            OnSavePropertyChanged("PerkGroups");
        }


        public AmfObject StatusEffectsArray
        {
            get { return GetObj("statusEffects") ?? new AmfObject(AmfTypes.Array); }
        }

        public List<StatusGroupVM> StatusEffectGroups { get; set; }

        public void UpdateStatusEffects()
        {
            if (null == StatusEffectGroups)
            {
                StatusEffectGroups = new List<StatusGroupVM>();
            }
            var charStatuses = StatusEffectsArray;
            foreach (var xmlGroup in XmlData.Current.StatusEffectGroups)
            {
                var statusVMs = xmlGroup.StatusEffects.OrderBy(x => x.Name).Select(x => new StatusEffectVM(this, charStatuses, x)).ToArray();
                foreach (var statusVM in statusVMs)
                {
                    int indx = Game.AllStatusEffects.FindIndex(f => f.Name == statusVM.Name);
                    if (-1 == indx)
                    {
                        Game.AllStatusEffects.Add(statusVM);
                    }
                }

                var groupVM = new StatusGroupVM(this, xmlGroup.Name, statusVMs);
                int idx = StatusEffectGroups.FindIndex(f => f.Name == groupVM.Name);
                if (-1 == idx)
                {
                    StatusEffectGroups.Add(groupVM);
                }
            }
        }

        public void UpdateStatusEffectsVisibility()
        {
            foreach (var group in StatusEffectGroups)
            {
                group.Update();
            }
        }

        public bool HasStatusEffect(string statusEffectName)
        {
            var statusEffect = GetStatus(statusEffectName);
            return statusEffect != null ? statusEffect.IsOwned : false;
        }

        /// <summary>
        /// Returns the status with the specified name (even if not owned by the character) AND registers a dependency between the caller property and this status.
        /// That way, anytime the status is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public StatusEffectVM GetStatus(string name, string propertyName = null)
        {
            var status = Game.AllStatusEffects.First(x => x.Name == name);
            if (null != status)
            {
                if (null != propertyName)
                {
                    status.GameVMProperties.Add(propertyName);
                }
                return status;
            }
            return null;
        }

        public void RemoveStatus(string name)
        {
            var status = GetStatus(name);
            if (null != status)
            {
                if (status.IsOwned)
                {
                    status.IsOwned = false;
                }
            }
            OnSavePropertyChanged("StatusEffectGroups");
        }

        public void OnStatusAddedOrRemoved(string name, bool isOwned)
        {
            // Grants/removes the appropriate bonuses when a status is added or removed.
            // We do not add stats however since the user can already change them easily.
            switch (name)
            {
                default:
                    break;
            }
            OnSavePropertyChanged("StatusEffectGroups");
        }

        public void OnStatusChanged(string name)
        {
            foreach (var prop in Game.AllStatusEffects.First(x => x.Name == name).GameVMProperties)
            {
                OnPropertyChanged(prop);
            }
            OnSavePropertyChanged("StatusEffectGroups");
        }

        public AmfObject KeyItemsArray
        {
            get { return GetObj("keyItems") ?? new AmfObject(AmfTypes.Array); }
        }

        public List<KeyItemGroupVM> KeyItemGroups { get; set; }

        public void UpdateKeyItems()
        {
            if (null == KeyItemGroups)
            {
                KeyItemGroups = new List<KeyItemGroupVM>();
            }
            var charKeyItems = KeyItemsArray;
            foreach (var xmlGroup in XmlData.Current.KeyItemGroups)
            {
                var keyItemVMs = xmlGroup.KeyItems.OrderBy(x => x.Name).Select(x => new KeyItemVM(this, charKeyItems, x)).ToArray();
                foreach (var keyItemVM in keyItemVMs)
                {
                    int indx = Game.AllKeyItems.FindIndex(f => f.Name == keyItemVM.Name);
                    if (-1 == indx)
                    {
                        Game.AllKeyItems.Add(keyItemVM);
                    }
                }

                var groupVM = new KeyItemGroupVM(this, xmlGroup.Name, keyItemVMs);
                int idx = KeyItemGroups.FindIndex(f => f.Name == groupVM.Name);
                if (-1 == idx)
                {
                    KeyItemGroups.Add(groupVM);
                }
            }
        }

        public void UpdateKeyItemsVisibility()
        {
            foreach (var group in KeyItemGroups)
            {
                group.Update();
            }
        }

        public bool HasKeyItem(string keyItemName)
        {
            var keyItem = GetKeyItem(keyItemName);
            return (null != keyItem) ? keyItem.IsOwned : false;
        }

        public void OnKeyItemAddedOrRemoved(string name, bool isOwned)
        {
            switch (name)
            {
                default:
                    break;
            }
            OnSavePropertyChanged("KeyItemGroups");
        }

        /// <summary>
        /// Returns the key item with the specified name (even if not owned by the character) AND registers a dependency between the caller property and this key item.
        /// That way, anytime the key item is modified, OnPropertyChanged will be raised for the caller property.
        /// </summary>
        public KeyItemVM GetKeyItem(string name, string propertyName = null)
        {
            var keyItem = Game.AllKeyItems.First(x => x.Name == name);
            if (null != keyItem)
            {
                if (null != propertyName)
                {
                    keyItem.GameVMProperties.Add(propertyName);
                }
                return keyItem;
            }
            return null;
        }

        public void OnKeyItemChanged(string name)
        {
            foreach (var prop in Game.AllKeyItems.First(x => x.Name == name).GameVMProperties)
            {
                OnPropertyChanged(prop);
            }
            OnSavePropertyChanged("KeyItemGroups");
        }

        public int ItemStatsBonusByField(string statName)
        {
            int bonus = 0;

            return bonus;
        }
    }
}
