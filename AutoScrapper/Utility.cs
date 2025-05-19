using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using R2API;
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
                            Language.GetString(Tokens.AUTOMAGICALLY_SCRAPPED) + " ";

            if (partsCount == 1)
                result += parts[0] + ".";
            else if (partsCount == 2)
                result += parts[0] + " " + Language.GetString(Tokens.AND) + " " + parts[1] + ".";
            else if (partsCount > 2)
            {
                for (int i = 0; i < partsCount; i++)
                {
                    if (i > 0)
                    {
                        if (i == partsCount - 1)
                            result += ", " + Language.GetString(Tokens.AND) + " ";
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
        
        public static string SanitizeIfLocalized(this string text, bool localizationSupported)
        {
            return localizationSupported ? text.Sanitize() : text;
        }

    /// <summary>
        /// To help with readability, this method creates an identical description for each item.
        /// <example>
        /// [name] amount to keep before scrapping. <br/>
        /// > [item_description] <br/>
        /// 0 = scrap all, -1 = don't scrap
        /// </example>
        /// </summary>
        /// <param name="item">The item definition to use in description creation</param>
        public static ConfigDescription GetConfigDescription(ItemDef item, bool customTranslationSupported)
        {
            if (customTranslationSupported)
                return new ConfigDescription(
                    $"Amount of {item.name} to keep before scrapping. \n0 = scrap all, -1 = don't scrap");
            return new ConfigDescription(
                $"""
                 {COLOR_TEXT}{Language.GetStringFormatted(Tokens.AMOUNT_OF_X_TO_KEEP, GetFormattedName(item))}</color>

                 <i>{Language.GetString(item.descriptionToken)}</i>

                 {COLOR_TEXT}0 = scrap all, -1 = don't scrap</color>
                 """);
        }

        /// <summary>
        /// Creates a description token and registers it with the localization system.
        /// </summary>
        /// <param name="item">Item to create a token for</param>
        public static string CreateDescriptionToken(ItemDef item)
        {
            string itemDescription = Language.GetString(item.descriptionToken);
            string itemName = GetFormattedName(item);

            string description = Language.GetStringFormatted(Tokens.AMOUNT_OF_X_TO_KEEP, itemName);
            description += "/n/n<i>" + itemDescription + "</i>";
            description += "/n/n" + COLOR_TEXT + Language.GetString(Tokens.NUMBER_EXPLANATION) +
                           "</color>";

            string newToken = item.nameToken + "_AUTO_SCRAPPER_DESCRIPTION";
            LanguageAPI.Add(newToken, description, Language.currentLanguageName);

            return newToken;
        }
    }
}