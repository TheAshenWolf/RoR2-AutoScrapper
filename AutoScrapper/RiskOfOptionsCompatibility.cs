using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Reflection;
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

        /// <summary>
        /// Sets the mod icon for the mod.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetModIcon()
        {
            try
            {
                // Assembly location is the location of executing DLL
                string assemblyLocation = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                string fullName = new DirectoryInfo(assemblyLocation!).FullName;
                
                // We don't care how large the original texture is, as LoadImage overrides it anyways.
                // Bigger original texture would only mean more Garbage Collection.
                Texture2D iconTexture = new Texture2D(0, 0); 
                if (iconTexture.LoadImage(File.ReadAllBytes(Path.Combine(fullName, "icon.png"))))
                {
                    Sprite icon = Sprite.Create(iconTexture, new Rect(0.0f, 0.0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                    ModSettingsManager.SetModIcon(icon, AutoScrapper.PLUGIN_GUID, AutoScrapper.PLUGIN_NAME);
                }
                else
                    Debug.LogWarning("AutoScrapper: Failed to load icon.png");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AutoScrapper: Failed to load icon.png\n" + ex);
            }
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
