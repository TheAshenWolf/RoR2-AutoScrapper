using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using RoR2;

namespace ExamplePlugin;

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
        
        _limits = new Dictionary<ItemIndex, int>(whiteItems.Length + greenItems.Length + redItems.Length + yellowItems.Length);
    }
    
    private void BindEvents()
    {
        // We bind the event to reload the config when it is reloaded
        _config.ConfigReloaded += OnConfigReloaded;
    }
    
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

    private void CreateItemGroupConfigs(string section, ItemIndex[] items, Dictionary<ItemIndex, ConfigEntry<int>> itemConfig)
    {
        // First we gather all items for given section
        int itemCount = items.Length;
        
        // Then we iterate over all items in the section
        for (int i = 0; i < itemCount; i++)
        {
            // We get the item definition
            ItemDef item = ItemCatalog.GetItemDef(items[i]);

            // Then we create a config entry for each item. We do not use translation here as this needs to be persistent for everyone regardless of locale.
            itemConfig[item.itemIndex] = _config.Bind(section, item.nameToken, -1, GetDescription(item));
        }
    }

    private ConfigDescription GetDescription(ItemDef item)
    {
        return new ConfigDescription(
            $"{Language.GetString(item.nameToken)} amount to keep before scrapping. \n" +
            $"> {Language.GetString(item.descriptionToken).Sanitize()} \n" +
            $"0 = scrap all, -1 = don't scrap");
    }
    
    /// <summary>
    /// When the config reloads, we need to update the limits for each item
    /// so the values stay up to date.
    /// </summary>
    private void OnConfigReloaded(object sender, EventArgs e)
    {
        // First we need to clear the limits dictionary
        _limits.Clear();
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
}