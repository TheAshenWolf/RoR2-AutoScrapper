using BepInEx;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;

namespace AutoScrapper
{
    /// <summary>
    /// Default setup for the plugin
    /// </summary>
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
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

        /// Link to the plugin's config file
        public AutoScrapperConfig config;

        /// <summary>
        /// Scrapping.OnEnter is called when the player opens a scrapper.
        /// </summary>
        private void Awake()
        {
            On.RoR2.ItemCatalog.SetItemRelationships += ItemCatalog_SetItemRelationships;
            On.RoR2.Interactor.PerformInteraction += Interactor_PerformInteraction;

            if (RiskOfOptionsCompatibility.Enabled)
            {
                RiskOfOptionsCompatibility.SetModDescriptionToken("AUTO_SCRAPPER_MOD_DESCRIPTION");
                RiskOfOptionsCompatibility.SetModIcon();
            }

        }

        /// <summary>
        /// Don't forget to unsubscribe from the event when the object is destroyed to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            On.RoR2.ItemCatalog.SetItemRelationships -= ItemCatalog_SetItemRelationships;
            On.RoR2.Interactor.PerformInteraction -= Interactor_PerformInteraction;
            config?.OnDestroy();
        }

        /// <summary>
        /// Called when the user successfully interacts with an interactable object.
        /// We only care about the scrapper, which is why we check for the name as the first thing.
        /// interactable.name is the name of the GameObject, so it doesn't fall under localization.
        /// </summary>
        private void Interactor_PerformInteraction(On.RoR2.Interactor.orig_PerformInteraction orig, Interactor self,
            GameObject interactable)
        {
            if (interactable.name.StartsWith("Scrapper"))
            {
                CharacterBody localBody = self.GetComponent<CharacterBody>();
                if (localBody == null)
                {
                    Debug.LogWarning("AutoScrapper: Local body is null. Cannot scrap items automatically.");
                    orig(self, interactable);
                    return;
                }

                // Get the player's inventory
                Inventory inventory = localBody.inventory;

                // We track whether an item was scrapped or not.
                bool itemScrapped = false;

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
                    if (itemLimit <= -1)
                        continue;

                    // If the item count is less than or equal to the limit, we scrap it.
                    itemScrapped |= ScrapItem(inventory, itemId, itemCount - itemLimit);
                }

                // If an item was scrapped and the config says to keep the scrapper closed, we return here.
                if (itemScrapped && config.KeepScrapperClosed)
                    return;
            }

            // Open the scrapper as normal.
            orig(self, interactable);
        }

        /// <summary>
        /// Scraps given item in the player's inventory.
        /// </summary>
        /// <param name="playerInventory">Inventory to take items from</param>
        /// <param name="itemIndex">The item to scrap</param>
        /// <param name="count">Amount of the item to scrap</param>
        /// <returns>True if an item was scrapped</returns>
        private bool ScrapItem(Inventory playerInventory, ItemIndex itemIndex, int count)
        {
            // If there is nothing to scrap (or we somehow got a negative number), we can't scrap it.
            if (count <= 0)
                return false;

            // If the item is a scrap item, we can't scrap it.
            if (Utility.IsScrap(itemIndex))
                return false;

            // First we need to get the item definition to find out the tier of the item, which determines the scrap type.
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);

            // If the item cannot be removed, we can't scrap it.
            if (!itemDef.canRemove)
                return false;

            // If the item is consumed, we can't scrap it.
            if (itemDef.isConsumed)
                return false;
            
            // If the item is blacklisted, ignore it.
            if (Utility.BLACKLIST.Contains(itemDef.name))
                return false;

            // We get the item tier (we don't need to get it if the previous checks failed).
            ItemTier itemTier = itemDef.tier;

            // We get the scrap index for the given item tier.
            ItemIndex scrapIndex = Utility.GetScrapItemIndex(itemTier);

            // Scrap index being None means the item is of a tier that cannot be scrapped.
            if (scrapIndex == ItemIndex.None)
                return false;

            // We remove the item from the player's inventory - only if everything else succeeded.
            playerInventory.RemoveItem(itemIndex, count);
            // Then we give the player the scrap item for the given tier.
            playerInventory.GiveItem(scrapIndex, count);

            // We return true to indicate that we scrapped an item.
            return true;
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