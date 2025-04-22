using System;
using System.Collections.ObjectModel;
using System.Linq;
using BepInEx;
using EntityStates.Scrapper;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using PickupPickerController = On.RoR2.PickupPickerController;

namespace ExamplePlugin
{
    /// <summary>
    /// Default setup for the plugin
    /// </summary>
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class AutoScrapper : BaseUnityPlugin
    {
        /// <summary>
        /// Just some default stuff for the plugin to function
        /// </summary>
        public const string PLUGIN_GUID = PLUGIN_AUTHOR + "." + PLUGIN_NAME;

        public const string PLUGIN_AUTHOR = "TheAshenWolf";
        public const string PLUGIN_NAME = "AutoScrapper";
        public const string PLUGIN_VERSION = "0.0.1";

        // Link to the plugin's config file
        public AutoScrapperConfig config;

        /// <summary>
        /// Scrapping.OnEnter is called when the player opens a scrapper.
        /// </summary>
        private void Awake()
        {
            On.EntityStates.Scrapper.Scrapping.OnEnter += Scrapping_OnEnter; // TODO: This is actually not being called
            On.RoR2.ItemCatalog.SetItemRelationships += ItemCatalog_SetItemRelationships;
        }

        /// <summary>
        /// Don't forget to unsubscribe from the event when the object is destroyed to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            On.EntityStates.Scrapper.Scrapping.OnEnter -= Scrapping_OnEnter;
            On.RoR2.ItemCatalog.SetItemRelationships -= ItemCatalog_SetItemRelationships;
            config?.OnDestroy();
        }

        /// <summary>
        /// Called when the player opens a scrapper.
        /// Scraps all items in the player's inventory that are above the limit set in the config.
        /// </summary>
        private void Scrapping_OnEnter(On.EntityStates.Scrapper.Scrapping.orig_OnEnter orig, Scrapping self)
        {
            // TODO: This is most likely not correct. I need to find a better way to detect when the scrapper is opened.
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            
            // Get the player's inventory
            Inventory inventory = localUser.cachedBody.inventory;
            for (int i = 0, count = inventory.itemAcquisitionOrder.Count; i < count; i++)
            {
                // itemAcquisitionOrder and itemStacks are two arrays representing which item corresponds to which stack.
                // We have to map both of these items to know how much of which item is present.
                int itemCount = inventory.itemStacks[i];
        
                // Trying to scrap 0 of an item could cause issues; we prevent that by skipping
                if (itemCount == 0) continue;
                
                // However, we want to access the array as late as possible.
                // We don't need to check the id if there are no items anyway.
                ItemIndex itemId = inventory.itemAcquisitionOrder[i];
        
                // We get the limit from the config
                int itemLimit = config.GetLimit(itemId);
                
                // If the limit is -1, we don't scrap the item.
                if (itemLimit == -1)
                    continue;
                
                // Lastly, we check if the item count is higher than the limit...
                if (itemLimit < itemCount)
                {
                    // ... and scrap items over the limit if the limit is lower than the amount of items in the inventory.
                    ScrapItem(inventory, itemId, itemCount - itemLimit);
                }
            }
        }

        /// <summary>
        /// Scraps given item in the player's inventory.
        /// </summary>
        /// <param name="playerInventory">Inventory to take items from</param>
        /// <param name="itemIndex">The item to scrap</param>
        /// <param name="count">Amount of the item to scrap</param>
        private void ScrapItem(Inventory playerInventory, ItemIndex itemIndex, int count)
        {
            // First we need to get the item definition to find out the tier of the item, which determines the scrap type.
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);

            // If the item cannot be removed, we can't scrap it.
            if (!itemDef.canRemove)
                return;

            // If the item is consumed, we can't scrap it.
            if (itemDef.isConsumed)
                return;

            // We get the item tier (we don't need to get it if the previous checks failed).
            ItemTier itemTier = itemDef.tier;

            // We get the scrap index for the given item tier.
            ItemIndex scrapIndex = Utility.GetScrapItemIndex(itemTier);

            // Scrap index being None means the item is of a tier that cannot be scrapped.
            if (scrapIndex == ItemIndex.None)
                return;

            // We remove the item from the player's inventory - only if everything else succeeded.
            playerInventory.RemoveItem(itemIndex, count);
            // Then we give the player the scrap item for the given tier.
            playerInventory.GiveItem(scrapIndex, count);
        }

        /// <summary>
        /// We cannot simply "setup" the config as it is dynamic.
        /// For this reason we need to call the setup method after all equipment was loaded.
        /// </summary>
        private void ItemCatalog_SetItemRelationships(On.RoR2.ItemCatalog.orig_SetItemRelationships orig,
            ItemRelationshipProvider[] newProviders)
        {
            orig(newProviders);
            config = new AutoScrapperConfig();
        }
    }
}