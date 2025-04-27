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
            
            if (white > 0)
                parts.Add(string.Format(Language.GetString("AUTO_SCRAPPER_WHITE_ITEMS"), white));
            if (green > 0)
                parts.Add(string.Format(Language.GetString("AUTO_SCRAPPER_GREEN_ITEMS"), green));
            if (red > 0)
                parts.Add(string.Format(Language.GetString("AUTO_SCRAPPER_RED_ITEMS"), red));
            if (yellow > 0)
                parts.Add(string.Format(Language.GetString("AUTO_SCRAPPER_YELLOW_ITEMS"), yellow));
            
            return parts;
        }
    }
}
