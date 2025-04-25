using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using RoR2;

namespace AutoScrapper
{
    /// <summary>
    /// Config for the AutoScrapper
    /// </summary>
    public class AutoScrapperConfig
    {
        private ConfigFile _config;

        // We use a dictionary so we can easily look up the item's config by its index
        public static Dictionary<ItemIndex, ConfigEntry<int>> whiteItemConfig;
        public static Dictionary<ItemIndex, ConfigEntry<int>> greenItemConfig;
        public static Dictionary<ItemIndex, ConfigEntry<int>> redItemConfig;
        public static Dictionary<ItemIndex, ConfigEntry<int>> yellowItemConfig;

        // We use a dictionary to cache the limits for each item
        // This way we don't need to access the config file every time we open a scrapper
        private Dictionary<ItemIndex, int> _limits;
        
        // General configuration
        ConfigEntry<bool> _keepScrapperClosedConfig;
        
        // We use these arrays to store the items for each tier
        public ItemIndex[] whiteItems;
        public ItemIndex[] greenItems;
        public ItemIndex[] redItems;
        public ItemIndex[] yellowItems;

        /// <summary>
        /// Upon construction, we set up the config and bind the events.
        /// </summary>
        public AutoScrapperConfig()
        {
            SetupConfig();
            BindEvents();
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
            
            // First we gather all white items
            whiteItemConfig = new Dictionary<ItemIndex, ConfigEntry<int>>(whiteItems.Length);
            CreateItemGroupConfigs("White Items", whiteItems, whiteItemConfig);

            // Then we gather all green items
            greenItemConfig = new Dictionary<ItemIndex, ConfigEntry<int>>(greenItems.Length);
            CreateItemGroupConfigs("Green Items", greenItems, greenItemConfig);

            // Then we gather all red items
            redItemConfig = new Dictionary<ItemIndex, ConfigEntry<int>>(redItems.Length);
            CreateItemGroupConfigs("Red Items", redItems, redItemConfig);

            // Then we gather all yellow items
            yellowItemConfig = new Dictionary<ItemIndex, ConfigEntry<int>>(yellowItems.Length);
            CreateItemGroupConfigs("Yellow Items", yellowItems, yellowItemConfig);

            _limits = new Dictionary<ItemIndex, int>(whiteItems.Length + greenItems.Length + redItems.Length +
                                                     yellowItems.Length);

            // When the game initializes, the config cache is empty. We need to call OnConfigReloaded manually to fill it.
            OnConfigReloaded(null, null);
        }

        private void BindEvents()
        {
            // We bind the event to reload the config when it is reloaded
            _config.ConfigReloaded += OnConfigReloaded;
        }

        /// <summary>
        /// We need to unbind the event when the mod is destroyed
        /// As this class is a custom one, we need this public so we can call it from <see cref="AutoScrapper"/>
        /// </summary>
        public void OnDestroy()
        {
            // Unbind the event to prevent memory leaks
            _config.ConfigReloaded -= OnConfigReloaded;
        }


        /// <summary>
        /// Gathers all item IDs for scrappable items
        /// </summary>
        private void GatherItems()
        {
            whiteItems = ItemCatalog.tier1ItemList.ToArray();
            greenItems = ItemCatalog.tier2ItemList.ToArray();
            redItems = ItemCatalog.tier3ItemList.ToArray();
            yellowItems = ItemCatalog.allItemDefs.Where(def => def.tier == ItemTier.Boss).Select(def => def.itemIndex)
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
                ItemDef item = ItemCatalog.GetItemDef(items[i]);

                // There is no point in creating a config entry for items that cannot be removed, are consumed or are hidden
                if (!item.canRemove || item.isConsumed || item.hidden)
                    continue;

                // Then we create a config entry for each item. We do not use translation here as this needs to be persistent for everyone regardless of locale.
                ConfigEntry<int> config = _config.Bind(section, item.name, -1, GetDescription(item));
                itemConfig[item.itemIndex] = config;
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
        /// When the config reloads, we need to update the limits for each item
        /// so the values stay up to date.
        /// </summary>
        private void OnConfigReloaded(object sender, EventArgs e)
        {
            // First we need to clear the limits dictionary
            _limits.Clear();

            // Then we update the limits for each item
            foreach ((ItemIndex key, ConfigEntry<int> _) in whiteItemConfig)
            {
                _limits[key] = whiteItemConfig.TryGetValue(key, out ConfigEntry<int> entry) ? entry.Value : -1;
            }

            foreach ((ItemIndex key, ConfigEntry<int> _) in greenItemConfig)
            {
                _limits[key] = greenItemConfig.TryGetValue(key, out ConfigEntry<int> entry) ? entry.Value : -1;
            }

            foreach ((ItemIndex key, ConfigEntry<int> _) in redItemConfig)
            {
                _limits[key] = redItemConfig.TryGetValue(key, out ConfigEntry<int> entry) ? entry.Value : -1;
            }

            foreach ((ItemIndex key, ConfigEntry<int> _) in yellowItemConfig)
            {
                _limits[key] = yellowItemConfig.TryGetValue(key, out ConfigEntry<int> entry) ? entry.Value : -1;
            }
        }

        /// <summary>
        /// A getter for the item limit for given item index.
        /// </summary>
        public int GetLimit(ItemIndex index)
        {
            return _limits.GetValueOrDefault(index, -1);
        }
        
        /// <summary>
        /// Gets the config entry for whether the scrapper should remain closed after
        /// automatically scrapping.
        /// </summary>
        public bool KeepScrapperClosed => _keepScrapperClosedConfig.Value;
    }
}