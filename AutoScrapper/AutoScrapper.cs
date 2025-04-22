using BepInEx;
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
            On.RoR2.ItemCatalog.SetItemRelationships += ItemCatalog_SetItemRelationships;
            On.RoR2.PickupPickerController.OnDisplayBegin += PickupPickerPanel_OnDisplayBegin;
        }

        /// <summary>
        /// Don't forget to unsubscribe from the event when the object is destroyed to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            On.RoR2.ItemCatalog.SetItemRelationships -= ItemCatalog_SetItemRelationships;
            On.RoR2.PickupPickerController.OnDisplayBegin -= PickupPickerPanel_OnDisplayBegin;
            config?.OnDestroy();
        }

        /// <summary>
        /// This method is getting called whenever a selection UI (like the one scrapper uses) is opened.
        /// All we need from it is to know when it was opened (the call in general) and who opened it (LocalUser).
        /// </summary>
        private void PickupPickerPanel_OnDisplayBegin(PickupPickerController.orig_OnDisplayBegin orig,
            RoR2.PickupPickerController pickupPickerController, NetworkUIPromptController networkUIPromptController,
            LocalUser user, CameraRigController cameraRigController)
        {
            // TODO: This gets called in every item selection, doesn't it?
            // TODO: Check if it is actually a scrapper.

            // Get the player's inventory
            Inventory inventory = user.cachedBody.inventory;

            // We have to go backwards, as Removing an item actually removes it from the array.
            // This is not exactly performance friendly, but it works.
            // Sadly, we cannot really change that.
            for (int i = inventory.itemAcquisitionOrder.Count - 1; i >= 0; i--)
            {
                // itemAcquisitionOrder tells us which items we need to check.
                // By using GetItemCount, we can check how many items of the given type we have.
                ItemIndex itemId = inventory.itemAcquisitionOrder[i];
                int itemCount = inventory.GetItemCount(itemId);

                // Trying to scrap 0 of an item could cause issues; we prevent that by skipping
                if (itemCount == 0)
                    continue;

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

            // TODO: If we call this here, the scrapper displays the items that were just scrapped.
            // TODO: We should probably check for the scrapper interaction itself and call that if we scrapped anything.
            orig(pickupPickerController, networkUIPromptController, user, cameraRigController);
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