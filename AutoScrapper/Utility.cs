using RoR2;

namespace AutoScrapper;

/// <summary>
/// Class containing static utility methods.
/// </summary>
public static class Utility
{
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
    /// Removes all tags from the given string to remove formatting used in-game.<br/>
    /// </summary>
    public static string Sanitize(this string text)
    {
        // Remove everything within <> tags, including the tags themselves, for every occurrence
        // and replace it with an empty string.
        text = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);
        
        // Remove new lines
        text = text.Replace("\n", string.Empty);

        return text;
    }
}