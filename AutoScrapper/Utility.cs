using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using Path = System.IO.Path;

namespace AutoScrapper
{
    /// <summary>
    /// Class containing static utility methods.
    /// </summary>
    public static class Utility
    {
        public const string COLOR_TEXT = "<color=#DDDDDD>";
        public const string COLOR_WHITE = "<color=#FFFFFF>";
        public const string COLOR_GREEN = "<color=#00FF00>";
        public const string COLOR_RED = "<color=#FF0000>";
        public const string COLOR_YELLOW = "<color=#FFFF00>";
        public const string COLOR_PLAYER = "<color=#2083fc>";

        public const int ALT_PROFILE_COUNT = 3;
        public const int PROFILE_COUNT = ALT_PROFILE_COUNT + 1;

        // We need this for compatibility reasons. While simply deleting user config would be the easier way,
        // the intention here is to keep the player happy.
        public static readonly string OLD_CONFIG_PATH = Paths.ConfigPath + "\\TheAshenWolf.AutoScrapper.cfg";
        public static readonly string MAIN_CONFIG_PATH = Paths.ConfigPath + "\\TheAshenWolf.AutoScrapper\\Main.cfg";

        /// <summary>
        /// Returns the config path for the given profile index.
        /// This should be only used with indices 1+, as the main config has a different path.
        /// </summary>
        public static string ConfigPath(int profileIndex) =>
            Paths.ConfigPath + "\\TheAshenWolf.AutoScrapper\\Profile" + profileIndex + ".cfg";


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
            return index == RoR2Content.Items.ScrapWhite.itemIndex || index == RoR2Content.Items.ScrapGreen.itemIndex ||
                   index == RoR2Content.Items.ScrapRed.itemIndex || index == RoR2Content.Items.ScrapYellow.itemIndex;
        }

        /// <summary>
        /// Returns a translated name of the item with the color corresponding to its tier.
        /// </summary>
        public static string GetFormattedName(ItemDef item)
        {
            string color = item.tier switch
            {
                ItemTier.Tier1 => COLOR_WHITE,
                ItemTier.Tier2 => COLOR_GREEN,
                ItemTier.Tier3 => COLOR_RED,
                ItemTier.Boss => COLOR_YELLOW,
                _ => COLOR_WHITE
            };

            return $"{color}{Language.GetString(item.nameToken)}</color>";
        }

        /// <summary>
        /// Reports the results of scrapping into the chat window.
        /// </summary>
        public static void ReportResults(string userName, ScrapperReportCount count)
        {
            List<string> parts = count.GetReportParts();

            int partsCount = parts.Count;
            if (partsCount == 0)
                return;

            string result = COLOR_PLAYER + userName + "</color> " + COLOR_TEXT +
                            Language.GetString("AUTO_SCRAPPER_AUTOMAGICALLY_SCRAPPED") + " ";

            if (partsCount == 1)
                result += parts[0] + ".";
            else if (partsCount == 2)
                result += parts[0] + " " + Language.GetString("AUTO_SCRAPPER_AND") + " " + parts[1] + ".";
            else if (partsCount > 2)
            {
                for (int i = 0; i < partsCount; i++)
                {
                    if (i > 0)
                    {
                        if (i == partsCount - 1)
                            result += ", " + Language.GetString("AUTO_SCRAPPER_AND") + " ";
                        else
                            result += ", ";
                    }

                    result += parts[i];
                }

                result += ".";
            }

            result += "</color>";

            Chat.SimpleChatMessage chat = new Chat.SimpleChatMessage();
            chat.baseToken = result;

            Chat.SendBroadcastChat(chat);
        }

        /// <summary>
        /// Returns an altered mod guid for RiskOfOptions.
        /// This is used inside the identifier string - no spaces allowed
        /// </summary>
        public static string GetProfileGUID(int profileIndex)
        {
            if (profileIndex == 0)
            {
                return AutoScrapper.PLUGIN_GUID;
            }

            return AutoScrapper.PLUGIN_GUID + "_" + profileIndex;
        }

        /// <summary>
        /// If an old config exists, we want to move it to the new location and delete it.
        /// This is to ensure that the player does not lose their config settings.
        /// </summary>
        public static void EnsureConfigCompatibilityWithOldVersion()
        {
            string directoryPath = Path.GetDirectoryName(MAIN_CONFIG_PATH);

            // We get an error if this directory doesn't exist.
            if (!Directory.Exists(directoryPath))
            {
                if (directoryPath == null)
                    throw new DirectoryNotFoundException("AutoScrapper: Config directory path is null");
                Directory.CreateDirectory(directoryPath);
            }

            // We need to check if the old config file exists
            if (File.Exists(OLD_CONFIG_PATH))
            {
                // We need to copy the old config file to the new location
                File.Copy(OLD_CONFIG_PATH, MAIN_CONFIG_PATH, true);

                // We need to delete the old config file
                File.Delete(OLD_CONFIG_PATH);
            }
        }
    }
}