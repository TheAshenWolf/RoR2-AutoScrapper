using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AutoScrapper
{
    /// <summary>
    /// CompatibilityAPI for RiskOfOptions
    /// </summary>
    public static class RiskOfOptionsCompatibility
    {
        private static bool? _isEnabled;

        /// <summary>
        /// Returns true if the RiskOfOptions plugin is installed and enabled.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _isEnabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                return _isEnabled.Value;
            }
        }
        
        /// <summary>
        /// Sets the mod description token for the mod.
        /// Token is used by the localization system to get the correct description for the mod.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetModDescriptionToken(string descriptionToken)
        {
            ModSettingsManager.SetModDescriptionToken(descriptionToken, AutoScrapper.PLUGIN_GUID, AutoScrapper.PLUGIN_NAME);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetModIcon(Sprite icon)
        {
            ModSettingsManager.SetModIcon(icon, AutoScrapper.PLUGIN_GUID, AutoScrapper.PLUGIN_NAME);
        }

        /// <summary>
        /// Creates a new Int option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddIntOption(ConfigEntry<int> configEntry)
        {
            ModSettingsManager.AddOption(new IntFieldOption(configEntry), AutoScrapper.PLUGIN_GUID, AutoScrapper.PLUGIN_NAME);
        }
        
        /// <summary>
        /// Creates a new Bool option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddBoolOption(ConfigEntry<bool> configEntry)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(configEntry), AutoScrapper.PLUGIN_GUID, AutoScrapper.PLUGIN_NAME);
        }
    }
}
