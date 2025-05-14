using BepInEx.Configuration;
using RoR2;

namespace AutoScrapper
{
    public static class Texts
    {
        public const string KEEP_SCRAPPER_CLOSED =
            """
            If this setting is enabled, the scrapper will not open if it automatically scrapped items. 
            
            You can always open it with a second interaction.
            """;

        public const string MOD_ENABLED =
            """
            Who likes restarting the game just to see what mod does what, right?
            Just untick this box and the mod won't do anything.

            This setting overrides <b>all</b> other settings.
            """;

        public const string SCRAP_EVERYTHING =
            """
            If this setting is enabled, all items will be scrapped.

            This setting overrides all individual item settings.
            """;
        
        public const string PROFILE_OVERRIDE =
            """
            This setting allows you to quickly swap between different profiles.
            The main configs settings are used if "None" is selected.
            """;

        public const string PROFILE_RENAME =
            """
            The name of the profile. This is used in the RiskOfOptions menu.
             
            <color=red>Restart is required for this setting to take effect.</color>
            """;
        
        /// <summary>
        /// To help with readability, this method creates an identical description for each item.
        /// <example>
        /// [name] amount to keep before scrapping. <br/>
        /// > [item_description] <br/>
        /// 0 = scrap all, -1 = don't scrap
        /// </example>
        /// </summary>
        /// <param name="item">The item definition to use in description creation</param>
        public static ConfigDescription GetDescription(ItemDef item)
        {
            return new ConfigDescription(
                $"""
                 {Utility.GetFormattedName(item)} {Utility.COLOR_TEXT}amount to keep before scrapping.</color>

                 <i>{Language.GetString(item.descriptionToken)}</i>

                 {Utility.COLOR_TEXT}0 = scrap all, -1 = don't scrap</color>
                 """);
        }
    }
}