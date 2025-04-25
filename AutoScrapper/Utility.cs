using RoR2;

namespace AutoScrapper;

/// <summary>
/// Class containing static utility methods.
/// </summary>
public static class Utility
{
    /// <summary>
    /// Some special items that shouldn't be scrapped still bypass the filters. We list them here.
    /// </summary>
    public static readonly string[] BLACKLIST = new[]
    {
            "ArtifactKey",
    };

    /// <summary>
    /// Returns the ItemIndex of the scrap item corresponding to the given ItemTier. <br/>
    /// Returns <see cref="ItemIndex.None"/> if the tier is not supported. This should be
    /// used to prevent scrapping items that are not supported.
    /// </summary>
    /// <param name="tier">Tier of the scrap to return</param>
    public static ItemIndex GetScrapItemIndex(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Tier1 => RoR2Content.Items.ScrapWhite.itemIndex,
            ItemTier.Tier2 => RoR2Content.Items.ScrapGreen.itemIndex,
            ItemTier.Tier3 => RoR2Content.Items.ScrapRed.itemIndex,
            ItemTier.Boss => RoR2Content.Items.ScrapYellow.itemIndex,
            _ => ItemIndex.None
        };
    }

    /// <summary>
    /// Returns whether the given ItemIndex is a scrap item. <br/>
    /// </summary>
    public static bool IsScrap(ItemIndex index)
    {
        return index == RoR2Content.Items.ScrapWhite.itemIndex || index == RoR2Content.Items.ScrapGreen.itemIndex || index == RoR2Content.Items.ScrapRed.itemIndex || index == RoR2Content.Items.ScrapYellow.itemIndex;
    }

    /// <summary>
    /// Returns a translated name of the item with the color corresponding to its tier.
    /// </summary>
    public static string GetFormattedName(ItemDef item)
    {
        string color = item.tier switch
        {
                ItemTier.Tier1 => "<color=#FFFFFF>",
                ItemTier.Tier2 => "<color=#00FF00>",
                ItemTier.Tier3 => "<color=#FF0000>",
                ItemTier.Boss => "<color=#FFFF00>",
                _ => "<color=#FFFFFF>"
        };

        return $"{color}{Language.GetString(item.nameToken)}</color>";
    }
}