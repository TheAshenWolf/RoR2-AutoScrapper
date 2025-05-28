using System.Collections.Generic;
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
        private ConfigFile[] _configs;

        /// We use a dictionary so we can easily look up the item's config by its index
        public Dictionary<ItemIndex, ConfigEntry<int>>[] configEntries;

        // General configuration
        // Keeps the scrapper closed after auto-scrapping
        private ConfigEntry<bool> _keepScrapperClosedConfig;

        // Disables the whole functionality of the mod. Priority 0.
        private ConfigEntry<bool> _modEnabledConfig;

        // Scraps everything. Priority 1.
        private ConfigEntry<bool> _scrapEverythingConfig;

        private ConfigEntry<ProfileOverride> _profileOverrideConfig;
        private ConfigEntry<string>[] _profileNamesConfig;

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
            _configs = new ConfigFile[Utility.PROFILE_COUNT];
            _profileNamesConfig = new ConfigEntry<string>[Utility.ALT_PROFILE_COUNT];

            SetupConfig();

            // We moved these settings here, as getting the category name in them is painful.
            // The category name is determined by the first config to ever set it.
            if (RiskOfOptionsCompatibility.Enabled)
            {
                RiskOfOptionsCompatibility.SetModDescriptionToken(Tokens.MOD_DESCRIPTION);
                RiskOfOptionsCompatibility.SetModIcon();
            }
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

            // We don't want players to lose their configuration, so we move the config from the old to the new location.
            Utility.EnsureConfigCompatibilityWithOldVersion();

            // We create the config files - main config lives in the main directory, so the old config values are not lost.
            _configs[0] = new ConfigFile(Utility.MAIN_CONFIG_PATH, true);
            for (int i = 1; i < Utility.PROFILE_COUNT; i++)
            {
                // We create a new config file for each profile
                _configs[i] = new ConfigFile(Utility.ConfigPath(i), true);
            }

            ConfigFile mainConfig = _configs[0];

            // Generic mod settings - We get descriptions by using the english translation of the text
            _modEnabledConfig = mainConfig.Bind("General", "ModEnabled", true,
                new ConfigDescription(Language.english.GetLocalizedStringByToken(Tokens.MOD_ENABLED_DESC)
                    .SanitizeIfLocalized(RiskOfOptionsCompatibility.SupportsCustomTranslation)));

            _keepScrapperClosedConfig = mainConfig.Bind("General", "KeepScrapperClosed", true,
                new ConfigDescription(Language.english
                    .GetLocalizedStringByToken(Tokens.KEEP_SCRAPPER_CLOSED_DESC)
                    .SanitizeIfLocalized(RiskOfOptionsCompatibility.SupportsCustomTranslation)));

            _scrapEverythingConfig = mainConfig.Bind("General", "ScrapEverything", false,
                new ConfigDescription(Language.english.GetLocalizedStringByToken(Tokens.SCRAP_EVERYTHING_DESC)
                    .SanitizeIfLocalized(RiskOfOptionsCompatibility.SupportsCustomTranslation)));

            _profileOverrideConfig = mainConfig.Bind("General", "ProfileOverride", ProfileOverride.None,
                new ConfigDescription(Language.english.GetLocalizedStringByToken(Tokens.PROFILE_OVERRIDE_DESC)
                    .SanitizeIfLocalized(RiskOfOptionsCompatibility.SupportsCustomTranslation)));

            for (int i = 0; i < Utility.ALT_PROFILE_COUNT; i++)
            {
                // We create a new config entry for each profile
                _profileNamesConfig[i] = mainConfig.Bind("General", "ProfileName_" + (i + 1), "Profile " + (i + 1),
                    new ConfigDescription(Language.english
                        .GetLocalizedStringByToken(Tokens.PROFILE_RENAME_DESC)
                        .SanitizeIfLocalized(RiskOfOptionsCompatibility.SupportsCustomTranslation)));
            }

            if (RiskOfOptionsCompatibility.Enabled)
            {
                // Bool settings are only present in the main config.
                string mainConfigName = GetProfileName(0);
                RiskOfOptionsCompatibility.AddBoolOption(0, _modEnabledConfig, mainConfigName,
                    Tokens.MOD_ENABLED, Tokens.MOD_ENABLED_DESC);
                RiskOfOptionsCompatibility.AddBoolOption(0, _keepScrapperClosedConfig, mainConfigName,
                    Tokens.KEEP_SCRAPPER_CLOSED, Tokens.KEEP_SCRAPPER_CLOSED_DESC);
                RiskOfOptionsCompatibility.AddBoolOption(0, _scrapEverythingConfig, mainConfigName,
                    Tokens.SCRAP_EVERYTHING, Tokens.SCRAP_EVERYTHING_DESC);
                RiskOfOptionsCompatibility.AddDropdownOption(0, _profileOverrideConfig, mainConfigName,
                    Tokens.PROFILE_OVERRIDE, Tokens.PROFILE_OVERRIDE_DESC);

                for (int i = 0; i < Utility.ALT_PROFILE_COUNT; i++)
                {
                    RiskOfOptionsCompatibility.AddStringOption(0, _profileNamesConfig[i], mainConfigName,
                        Tokens.PROFILE_RENAME, Tokens.PROFILE_RENAME_DESC, true);
                }
            }

            // We count the total amount of items and create a dictionary for the config entries
            int itemsTotal = _whiteItems.Length + _greenItems.Length + _redItems.Length + _yellowItems.Length;
            configEntries = new Dictionary<ItemIndex, ConfigEntry<int>>[Utility.PROFILE_COUNT];
            for (int i = 0; i < Utility.PROFILE_COUNT; i++)
            {
                configEntries[i] = new Dictionary<ItemIndex, ConfigEntry<int>>(itemsTotal);
            }

            // Dictionary is a reference type; passing it as a parameter allows us to add into it
            CreateItemGroupConfigs(Language.english.GetLocalizedStringByToken(Tokens.CATEGORY_WHITE_ITEMS), _whiteItems, configEntries);
            CreateItemGroupConfigs(Language.english.GetLocalizedStringByToken(Tokens.CATEGORY_GREEN_ITEMS), _greenItems, configEntries);
            CreateItemGroupConfigs(Language.english.GetLocalizedStringByToken(Tokens.CATEGORY_RED_ITEMS), _redItems, configEntries);
            CreateItemGroupConfigs(Language.english.GetLocalizedStringByToken(Tokens.CATEGORY_YELLOW_ITEMS), _yellowItems, configEntries);
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
        /// <param name="itemConfigs">Link to an existing dictionary with the configs. We save config data here</param>
        private void CreateItemGroupConfigs(string section, ItemDef[] items,
            Dictionary<ItemIndex, ConfigEntry<int>>[] itemConfigs)
        {
            section = section.Sanitize();
            
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
                for (int profileIndex = 0; profileIndex < Utility.PROFILE_COUNT; profileIndex++)
                {
                    ConfigEntry<int> config = _configs[profileIndex]
                        .Bind(section, item.name, defaultValue,
                            Utility.GetConfigDescription(item, RiskOfOptionsCompatibility.SupportsCustomTranslation));
                    itemConfigs[profileIndex][item.itemIndex] = config;

                    if (RiskOfOptionsCompatibility.Enabled)
                        RiskOfOptionsCompatibility.AddIntOption(profileIndex, config, GetProfileName(profileIndex),
                            item.nameToken, Utility.CreateDescriptionToken(item));
                }
            }
        }

        /// <summary>
        /// A getter for the item limit for given item index.
        /// </summary>
        public int GetLimit(ItemIndex index)
        {
            return configEntries[(int)_profileOverrideConfig.Value].GetValueOrDefault(index, null)?.Value ?? -1;
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

        /// <summary>
        /// Gets the config entry for whether all items should be scrapped.
        /// </summary>
        public bool ScrapEverything => _scrapEverythingConfig.Value;

        /// <summary>
        /// The first index - 0, is always the main config.
        /// The other names can be retrieved from the main config. This array, however, is
        /// indexed only for alt profiles, which is why we have to subtract 1 when accessing the array.
        /// </summary>
        public string GetProfileName(int profileIndex)
        {
            if (profileIndex == 0) return AutoScrapper.PLUGIN_NAME;
            return AutoScrapper.PLUGIN_NAME + " | " + _profileNamesConfig[profileIndex - 1].Value;
        }
    }
}