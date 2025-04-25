using RoR2;
using System.Collections.Generic;

namespace AutoScrapper
{
    /// <summary>
    /// Contains a list of item IDs and their default quantities.
    /// Each list is used for a different purpose.
    /// </summary>
    public static class ConfigOverrides
    {
        /// <summary>
        /// These settings will be used as overrides for the default settings.
        /// ItemId : DefaultLimit
        /// </summary>
        public static Dictionary<string, int> defaultOverrides = new Dictionary<string, int>()
        {
                // White items
                {"BleedOnHit", 10}, // Tri-tip Dagger - bleed on hit
                {"CritGlasses", 9}, // Lens-Maker's Glasses - crit chance
                {"StickyBomb", 20}, // Sticky bomb - % chance to drop a bomb on hit
                
                // Green Items
                {"Bandolier", 3}, // Bandolier - ammo box chance
                {"BonusGoldPackOnKill", 25}, // Ghor's Tome - Monsters have chance to drop treasure on kill
                {"EquipmentMagazine", 254} // Fuel cell - additional equipment charge
        };
    }
}
