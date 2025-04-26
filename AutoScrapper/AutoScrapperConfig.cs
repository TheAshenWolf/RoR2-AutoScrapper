using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace AutoScrapper
{
    /// <summary>
    /// Config for the AutoScrapper
    /// </summary>
    public class AutoScrapperConfig
    {
        private ConfigFile _config;

        /// We use a dictionary so we can easily look up the item's config by its index
        private Dictionary<ItemIndex, ConfigEntry<int>> _configEntries;
        
        // General configuration
        ConfigEntry<bool> _keepScrapperClosedConfig;
        
        // We use these arrays to store the items for each tier
        private ItemIndex[] _whiteItems;
        private ItemIndex[] _greenItems;
        private ItemIndex[] _redItems;
        private ItemIndex[] _yellowItems;

        /// <summary>
        /// Upon construction, we set up the config and bind the events.
        /// </summary>
        public AutoScrapperConfig()
        {
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
            if (RiskOfOptionsCompatibility.Enabled)
                RiskOfOptionsCompatibility.AddBoolOption(_keepScrapperClosedConfig);
            
            // We count the total amount of items and create a dictionary for the config entries
            int itemsTotal = _whiteItems.Length + _greenItems.Length + _redItems.Length + _yellowItems.Length;
            _configEntries = new Dictionary<ItemIndex, ConfigEntry<int>>(itemsTotal);

            CreateItemGroupConfigs("White Items", _whiteItems, _configEntries);
            CreateItemGroupConfigs("Green Items", _greenItems, _configEntries);
            CreateItemGroupConfigs("Red Items", _redItems, _configEntries);
            CreateItemGroupConfigs("Yellow Items", _yellowItems, _configEntries);
        }

        /// <summary>
        /// Gathers all item IDs for scrappable items
        /// </summary>
        private void GatherItems()
        {
            _whiteItems = ItemCatalog.tier1ItemList.ToArray();
            _greenItems = ItemCatalog.tier2ItemList.ToArray();
            _redItems = ItemCatalog.tier3ItemList.ToArray();
            _yellowItems = ItemCatalog.allItemDefs.Where(def => def.tier == ItemTier.Boss).Select(def => def.itemIndex)
                .ToArray();
        }

        /// <summary>
        /// To avoid code repetition, this method creates a config entry for each item in the given section.
        /// </summary>
        /// <param name="section">Name of the section - same for each entry</param>
        /// <param name="items">An array of item ids. This method looks up required information itself</param>
        /// <param name="itemConfig">Link to an existing dictionary with the config. We save config data here</param>
        private void CreateItemGroupConfigs(string section, ItemIndex[] items,
            Dictionary<ItemIndex, ConfigEntry<int>> itemConfig)
        {
            // First we gather all items for given section
            int itemCount = items.Length;

            // Then we iterate over all items in the section
            for (int i = 0; i < itemCount; i++)
            {
                // We get the item definition
                ItemIndex itemIndex = items[i];
                ItemDef item = ItemCatalog.GetItemDef(itemIndex);

                // There is no point in creating a config entry for items that cannot be removed, are consumed or are hidden
                if (!item.canRemove || item.isConsumed || item.hidden)
                    continue;

                // We don't want scrap in the config
                if (Utility.IsScrap(itemIndex))
                    continue;

                if (Utility.BLACKLIST.Contains(item.name))
                    continue;

                int defaultValue = ConfigOverrides.defaultOverrides.GetValueOrDefault(item.name, -1);

                // Then we create a config entry for each item. We do not use translation here as this needs to be persistent for everyone regardless of locale.
                ConfigEntry<int> config = _config.Bind(section, item.name, defaultValue, GetDescription(item));
                itemConfig[itemIndex] = config;
                if (RiskOfOptionsCompatibility.Enabled)
                    RiskOfOptionsCompatibility.AddIntOption(config);
            }
        }

        /// <summary>
        /// To help with readability, this method creates an identical description for each item.
        /// <example>
        /// [name] amount to keep before scrapping. <br/>
        /// > [item_description] <br/>
        /// 0 = scrap all, -1 = don't scrap
        /// </example>
        /// </summary>
        /// <param name="item">The item definition to use in description creation</param>
        private ConfigDescription GetDescription(ItemDef item)
        {
            return new ConfigDescription(
                $"{Utility.GetFormattedName(item)} <color=#DDDDDD>amount to keep before scrapping.</color> \n\n" +
                $"<i>{Language.GetString(item.descriptionToken)}</i> \n\n" +
                $"<color=#DDDDDD>0 = scrap all, -1 = don't scrap</color>");
        }
        
        /// <summary>
        /// A getter for the item limit for given item index.
        /// </summary>
        public int GetLimit(ItemIndex index)
        {
            return _configEntries.GetValueOrDefault(index, null)?.Value ?? -1;
        }
        
        /// <summary>
        /// Gets the config entry for whether the scrapper should remain closed after
        /// automatically scrapping.
        /// </summary>
        public bool KeepScrapperClosed => _keepScrapperClosedConfig.Value;
    }
}