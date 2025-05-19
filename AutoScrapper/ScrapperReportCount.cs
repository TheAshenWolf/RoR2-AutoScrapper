using RoR2;
using System.Collections.Generic;

namespace AutoScrapper
{
    /// <summary>
    /// A container for the number of items scrapped by tier.
    /// </summary>
    public struct ScrapperReportCount
    {
        public int white;
        public int green;
        public int red;
        public int yellow;

        /// <summary>
        /// Adds <param name="count"/> to the count of the given <paramref name="tier"/>.
        /// </summary>
        public void Add(ItemTier tier, int count)
        {
            switch (tier)
            {
                case ItemTier.Tier1:
                    white += count;
                    break;
                case ItemTier.Tier2:
                    green += count;
                    break;
                case ItemTier.Tier3:
                    red += count;
                    break;
                case ItemTier.Boss:
                    yellow += count;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns a list of strings containing the number of items scrapped by tier.
        /// These still have to be connected in a meaningful way.
        /// </summary>
        public List<string> GetReportParts()
        {
            List<string> parts = new List<string>(4);

            // Strings here are in format "Scrapped {0} {1}[color] items</color>"
            // We use {1} to insert the color of the item based on our settings

            if (white > 0)
                parts.Add(Language.GetStringFormatted(white > 1 ? Tokens.WHITE_ITEMS_PLURAL : Tokens.WHITE_ITEMS, white,
                    Utility.COLOR_WHITE));
            if (green > 0)
                parts.Add(Language.GetStringFormatted(green > 1 ? Tokens.GREEN_ITEMS_PLURAL : Tokens.GREEN_ITEMS, green,
                    Utility.COLOR_GREEN));
            if (red > 0)
                parts.Add(Language.GetStringFormatted(red > 1 ? Tokens.RED_ITEMS_PLURAL : Tokens.RED_ITEMS, red,
                    Utility.COLOR_RED));
            if (yellow > 0)
                parts.Add(Language.GetStringFormatted(yellow > 1 ? Tokens.YELLOW_ITEMS_PLURAL : Tokens.YELLOW_ITEMS,
                    yellow, Utility.COLOR_YELLOW));

            return parts;
        }
    }
}