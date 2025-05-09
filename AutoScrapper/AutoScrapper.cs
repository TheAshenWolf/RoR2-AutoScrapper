using System.Collections.Generic;
using System.Linq;
using BepInEx;
using On.RoR2.UI.MainMenu;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace AutoScrapper;

[BepInDependency(ItemAPI.PluginGUID)]
[BepInDependency(LanguageAPI.PluginGUID)]
[BepInDependency(NetworkingAPI.PluginGUID)]
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
    public const string PLUGIN_VERSION = "0.2.0";

    /// Link to the plugin's config file
    public AutoScrapperConfig config;
    private bool _configInitialized = false;

    /// <summary>
    /// Scrapping.OnEnter is called when the player opens a scrapper.
    /// </summary>
    [Client]
    private void Awake()
    {
        MainMenuController.Awake += MainMenuController_Awake;
        On.RoR2.Interactor.AttemptInteraction += Interactor_AttemptInteraction;

        if (RiskOfOptionsCompatibility.Enabled)
        {
            RiskOfOptionsCompatibility.SetModDescriptionToken("AUTO_SCRAPPER_MOD_DESCRIPTION");
            RiskOfOptionsCompatibility.SetModIcon();
        }
        
        NetworkingAPI.RegisterMessageType<ScrapSync>();
    }

    /// <summary>
    /// Don't forget to unsubscribe from the event when the object is destroyed to prevent memory leaks.
    /// </summary>
    [Client]
    private void OnDestroy()
    {
        MainMenuController.Awake -= MainMenuController_Awake;
        On.RoR2.Interactor.AttemptInteraction -= Interactor_AttemptInteraction;
    }

    /// <summary>
    /// We cannot simply "setup" the config as it is dynamic.
    /// For this reason we need to call the setup method after all equipment was loaded.
    /// We do this as soon as the main menu is loaded. This also makes sure all modded items are loaded.
    /// </summary>
    [Client]
    private void MainMenuController_Awake(MainMenuController.orig_Awake orig, RoR2.UI.MainMenu.MainMenuController self)
    {
        orig(self);

        if (_configInitialized) return;
        config = new AutoScrapperConfig();
        _configInitialized = true;
    }
    
    /// <summary>
    /// Called when the user successfully interacts with an interactable object.
    /// We only care about the scrapper, which is why we check for the name as the first thing.
    /// interactable.name is the name of the GameObject, so it doesn't fall under localization.
    /// </summary>
    [Client]
    private void Interactor_AttemptInteraction(On.RoR2.Interactor.orig_AttemptInteraction orig, Interactor self,
        GameObject interactable)
    {
        // Only run on client
        // 1. Check if the mod is enabled
        // 2. Attempt to scrap all items generating KVP<string, int> for each item
        //     2.a - We will be adding scrap: n for each item scrapped
        //     2.b - We will be adding item: -n for each item scrapped
        // 3. Send the message to the server
        // 4. If the config says to keep the scrapper closed, we return here.
        // 5. Open the scrapper as normal.

        if (!config.ModEnabled)
        {
            orig(self, interactable);
            return;
        }

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
            ScrapperReportCount reportCount = new ScrapperReportCount();
            Dictionary<ItemIndex, int> itemsToRemove = new Dictionary<ItemIndex, int>();

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

                ItemDef itemDef = ItemCatalog.GetItemDef(itemId);
                int count = itemCount - itemLimit;

                // If the item count is less than or equal to the limit, we scrap it.
                if (ScrapItem(itemsToRemove, itemDef, count))
                {
                    reportCount.Add(itemDef.tier, count);
                    itemScrapped = true;
                }
            }

            // If an item was scrapped and the config says to keep the scrapper closed, we return here.
            if (itemScrapped)
            {
                new ScrapSync(localBody.networkIdentity.netId, itemsToRemove).Send(NetworkDestination
                    .Server);

                if (config.KeepScrapperClosed)
                    return;
            }
        }

        // Open the scrapper as normal.
        orig(self, interactable);
    }

    /// <summary>
    /// Scraps given item in the player's inventory.
    /// </summary>
    /// <param name="itemsToRemove">Dictionary reference to the items to remove</param>
    /// <param name="itemDef">Definition of the item to scrap</param>
    /// <param name="count">Amount of the item to scrap</param>
    /// <returns>True if an item was scrapped</returns>
    [Client]
    private bool ScrapItem(Dictionary<ItemIndex, int> itemsToRemove, ItemDef itemDef,
        int count)
    {
        // If there is nothing to scrap (or we somehow got a negative number), we can't scrap it.
        if (count <= 0)
            return false;

        // If the item is a scrap item, we can't scrap it.
        if (Utility.IsScrap(itemDef.itemIndex))
            return false;

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

        itemsToRemove.Add(itemDef.itemIndex, count);

        // We return true to indicate that we scrapped an item.
        return true;
    }
}