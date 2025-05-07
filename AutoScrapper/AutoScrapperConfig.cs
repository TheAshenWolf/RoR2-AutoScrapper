using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine.Networking;

namespace AutoScrapper
{
    /// <summary>
    /// Config for the AutoScrapper
    /// </summary>
    public class AutoScrapperConfig
    {
        public Dictionary<NetworkInstanceId, Dictionary<string, bool>> clientGeneralConfigs;
        public Dictionary<NetworkInstanceId, Dictionary<string, int>> clientItemConfigs;

        private ConfigFile _config;

        /// We use a dictionary so we can easily look up the item's config by its index
        public Dictionary<ItemIndex, ConfigEntry<int>> configEntries;
        
        // General configuration
        private ConfigEntry<bool> _keepScrapperClosedConfig;
        private ConfigEntry<bool> _modEnabledConfig;
        // private ConfigEntry<bool> _reportEnabledConfig;
        
        // We use these arrays to store the items for each tier
        private ItemDef[] _whiteItems;
        private ItemDef[] _greenItems;
        private ItemDef[] _redItems;
        private ItemDef[] _yellowItems;

        /// <summary>
        /// Upon construction, we set up the config and bind the events.
        /// </summary>
        public AutoScrapperConfig()
        {
            clientGeneralConfigs = new Dictionary<NetworkInstanceId, Dictionary<string, bool>>();
            clientItemConfigs = new Dictionary<NetworkInstanceId, Dictionary<string, int>>();

            SetupConfig();
        }

        /// <summary>
        /// Sets up the configs for the AutoScrapper.
        /// As the item list is dynamic (for example with additional mods),
        /// we need to gather the items before setting up the config.
        /// </summary>
        private void SetupConfig()
        {
            // Fills our own item arrays with the items from the game.
            // We use these arrays so we don't need to gather the items every time
            GatherItems();

            // We create the config file
            _config = new ConfigFile(Paths.ConfigPath + "\\TheAshenWolf.AutoScrapper.cfg", true);

            // Generic mod settings
            _keepScrapperClosedConfig = _config.Bind("General", "KeepScrapperClosed", true,
                new ConfigDescription("If this setting is enabled, the scrapper will not open if it automatically scrapped items. \n"
                                      + "You can always open it with a second interaction."));
            
            _modEnabledConfig = _config.Bind("General", "ModEnabled", true,
                new ConfigDescription("Who likes restarting the game just to see what mod does what, right? \n"
                                      + "Just untick this box and the mod won't do anything."));
            
            // _reportEnabledConfig = _config.Bind("General", "ReportEnabled", true,
            //     new ConfigDescription("When you scrap items, the totals will be written into the chat window. If you don't want that, you can always disable it here."));

            if (RiskOfOptionsCompatibility.Enabled)
            {
                RiskOfOptionsCompatibility.AddBoolOption(_modEnabledConfig);
                RiskOfOptionsCompatibility.AddBoolOption(_keepScrapperClosedConfig);
                // RiskOfOptionsCompatibility.AddBoolOption(_reportEnabledConfig);
            }
            
            // We count the total amount of items and create a dictionary for the config entries
            int itemsTotal = _whiteItems.Length + _greenItems.Length + _redItems.Length + _yellowItems.Length;
            configEntries = new Dictionary<ItemIndex, ConfigEntry<int>>(itemsTotal);

            // Dictionary is a reference type; passing it as a parameter allows us to add into it
            CreateItemGroupConfigs("White Items", _whiteItems, configEntries);
            CreateItemGroupConfigs("Green Items", _greenItems, configEntries);
            CreateItemGroupConfigs("Red Items", _redItems, configEntries);
            CreateItemGroupConfigs("Yellow Items", _yellowItems, configEntries);
        }

        /// <summary>
        /// Gathers all item IDs for scrappable items
        /// </summary>
        private void GatherItems()
        {
            ItemDef[] itemDefinitions = ContentManager._itemDefs;
            int count = itemDefinitions.Length;
            
            List<ItemDef> whiteItems = new List<ItemDef>(count);
            List<ItemDef> greenItems = new List<ItemDef>(count);
            List<ItemDef> redItems = new List<ItemDef>(count);
            List<ItemDef> yellowItems = new List<ItemDef>(count);

            for (int i = 0; i < count; i++)
            {
                ItemDef itemDef = itemDefinitions[i];
                if (itemDef == null)
                    continue;
                
                switch (itemDef.tier)
                {
                    case ItemTier.Tier1:
                        whiteItems.Add(itemDef);
                        break;
                    case ItemTier.Tier2:
                        greenItems.Add(itemDef);
                        break;
                    case ItemTier.Tier3:
                        redItems.Add(itemDef);
                        break;
                    case ItemTier.Boss:
                        yellowItems.Add(itemDef);
                        break;
                    default:
                        continue;
                }
            }
            
            _whiteItems = whiteItems.ToArray();
            _greenItems = greenItems.ToArray();
            _redItems = redItems.ToArray();
            _yellowItems = yellowItems.ToArray();
        }

        /// <summary>
        /// To avoid code repetition, this method creates a config entry for each item in the given section.
        /// </summary>
        /// <param name="section">Name of the section - same for each entry</param>
        /// <param name="items">An array of item ids. This method looks up required information itself</param>
        /// <param name="itemConfig">Link to an existing dictionary with the config. We save config data here</param>
        private void CreateItemGroupConfigs(string section, ItemDef[] items,
            Dictionary<ItemIndex, ConfigEntry<int>> itemConfig)
        {
            // First we gather all items for given section
            int itemCount = items.Length;

            // Then we iterate over all items in the section
            for (int i = 0; i < itemCount; i++)
            {
                // We get the item definition
                ItemDef item = items[i];

                // There is no point in creating a config entry for items that cannot be removed, are consumed or are hidden
                if (!item.canRemove || item.isConsumed || item.hidden)
                    continue;

                // We don't want scrap in the config
                if (Utility.IsScrap(item.itemIndex))
                    continue;
                
                bool isBlacklisted = false;
                for (int blacklistIndex = 0, count = Utility.BLACKLIST.Length; blacklistIndex < count; blacklistIndex++)
                {
                    if (item.name == Utility.BLACKLIST[blacklistIndex])
                    {
                        isBlacklisted = true;
                        break;
                    }
                }

                if (isBlacklisted)
                    continue;

                int defaultValue = ConfigOverrides.defaultOverrides.GetValueOrDefault(item.name, -1);

                // Then we create a config entry for each item. We do not use translation here as this needs to be persistent for everyone regardless of locale.
                ConfigEntry<int> config = _config.Bind(section, item.name, defaultValue, Utility.GetDescription(item));
                itemConfig[item.itemIndex] = config;
                if (RiskOfOptionsCompatibility.Enabled)
                    RiskOfOptionsCompatibility.AddIntOption(config);
            }
        }
        
        /// <summary>
        /// A getter for the item limit for given item index.
        /// </summary>
        public int GetLimit(ItemIndex index)
        {
            return configEntries.GetValueOrDefault(index, null)?.Value ?? -1;
        }
        
        /// <summary>
        /// Gets the config entry for whether the scrapper should remain closed after
        /// automatically scrapping.
        /// </summary>
        public bool KeepScrapperClosed => _keepScrapperClosedConfig.Value;

        /// <summary>
        /// Gets the config entry for whether the mod is enabled.
        /// </summary>
        public bool ModEnabled => _modEnabledConfig.Value;
    }
}