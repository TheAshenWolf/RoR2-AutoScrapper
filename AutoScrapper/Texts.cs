using BepInEx.Configuration;
using R2API;
using RoR2;

namespace AutoScrapper
{
    public static class Texts
    {
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
                 {Utility.COLOR_TEXT}{Language.GetStringFormatted("AUTO_SCRAPPER_AMOUNT_OF_X_TO_KEEP", Utility.GetFormattedName(item))}</color>

                 <i>{Language.GetString(item.descriptionToken)}</i>

                 {Utility.COLOR_TEXT}0 = scrap all, -1 = don't scrap</color>
                 """);
        }

        public static string CreateDescriptionToken(ItemDef item)
        {
            string itemDescription = Language.GetString(item.descriptionToken);
            string itemName = Utility.GetFormattedName(item);

            string description = Language.GetStringFormatted("AUTO_SCRAPPER_AMOUNT_OF_X_TO_KEEP", itemName);
            description += "/n/n<i>" + itemDescription + "</i>";
            description += "/n/n" + Utility.COLOR_TEXT + Language.GetString("AUTO_SCRAPPER_NUMBER_EXPLANATION") +
                           "</color>";

            string newToken = item.nameToken + "_AUTO_SCRAPPER_DESCRIPTION";
            LanguageAPI.Add(newToken, description, Language.currentLanguageName);

            return newToken;
        }
    }
}