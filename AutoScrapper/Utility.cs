using RoR2;

namespace ExamplePlugin;

/// <summary>
/// Class containing static utility methods.
/// </summary>
public static class Utility
{
    /// <summary>
    /// Returns the ItemIndex of the scrap item corresponding to the given ItemTier.
    /// </summary>
    /// <param name="tier">Tier of the scrap to return</param>
    public static ItemIndex GetScrapItemIndex(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Tier1 => RoR2Content.Items.ScrapWhite.itemIndex,
            ItemTier.Tier2 => RoR2Content.Items.ScrapGreen.itemIndex,
            ItemTier.Tier3 => RoR2Content.Items.ScrapRed.itemIndex,
            ItemTier.Lunar => RoR2Content.Items.ScrapGreen.itemIndex,
            ItemTier.Boss => RoR2Content.Items.ScrapYellow.itemIndex,
            _ => RoR2Content.Items.ScrapWhite.itemIndex
        };
    }
}