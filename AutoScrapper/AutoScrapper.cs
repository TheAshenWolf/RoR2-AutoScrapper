using BepInEx;
using EntityStates.Scrapper;
using R2API;
using RoR2;

namespace ExamplePlugin
{
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
        
        /// <summary>
        /// Scrapping.OnEnter is called when the player opens a scrapper.
        /// </summary>
        private void Awake()
        {
            On.EntityStates.Scrapper.Scrapping.OnEnter += Scrapping_OnEnter;
        }

        /// <summary>
        /// Don't forget to unsubscribe from the event when the object is destroyed to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            On.EntityStates.Scrapper.Scrapping.OnEnter -= Scrapping_OnEnter;
        }

        /// <summary>
        /// Called when the player opens a scrapper.
        /// Scraps all items in the player's inventory that are above the limit set in the config.
        /// </summary>
        private void Scrapping_OnEnter(On.EntityStates.Scrapper.Scrapping.orig_OnEnter orig, Scrapping self)
        {
            orig(self);
            
            // Get the player's inventory
            Inventory inventory = self.characterBody.inventory;
            for (int i = 0, count = inventory.itemAcquisitionOrder.Count; i < count; i++)
            {
                // itemAcquisitionOrder and itemStacks are two arrays representing which item corresponds to which stack.
                // We have to map both of these items to know how much of which item is present.
                ItemIndex itemId = inventory.itemAcquisitionOrder[i];
                int itemCount = inventory.itemStacks[i];

                // Trying to scrap 0 of an item could cause issues; we prevent that by skipping
                if (itemCount == 0) continue;
                
                // For starters, we scrap every item in the inventory.
                ScrapItem(inventory, itemId, itemCount);
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
            ItemTier itemTier = itemDef.tier;
            
            // We remove the item from the player's inventory.
            playerInventory.RemoveItem(itemIndex, count);
            
            // Then we give the player the scrap item for the given tier.
            playerInventory.GiveItem(Utility.GetScrapItemIndex(itemTier), count);
        }
    }
}
