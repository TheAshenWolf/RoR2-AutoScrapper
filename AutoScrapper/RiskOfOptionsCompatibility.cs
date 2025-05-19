using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using PluginInfo = BepInEx.PluginInfo;

namespace AutoScrapper
{
    /// <summary>
    /// CompatibilityAPI for RiskOfOptions
    /// </summary>
    public static class RiskOfOptionsCompatibility
    {
        private static bool? _isEnabled;
        private static bool? _supportsCustomTranslation;

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

        public static bool SupportsCustomTranslation
        {
            get
            {
                if (!Enabled) return false;

                if (_supportsCustomTranslation == null)
                {
                    Version version = BepInEx.Bootstrap.Chainloader.PluginInfos["com.rune580.riskofoptions"].Metadata
                        .Version;
                    _supportsCustomTranslation = version == Version.Parse("2.8.3");
                }

                return _supportsCustomTranslation.Value;
            }
        }

        /// <summary>
        /// Sets the mod description token for the mod.
        /// Token is used by the localization system to get the correct description for the mod.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetModDescriptionToken(string descriptionToken)
        {
            ModSettingsManager.SetModDescriptionToken(descriptionToken, AutoScrapper.PLUGIN_GUID,
                AutoScrapper.PLUGIN_NAME);

            for (int i = 1; i < Utility.PROFILE_COUNT; i++)
            {
                ModSettingsManager.SetModDescriptionToken(Tokens.PROFILE_DESCRIPTION,
                    Utility.GetProfileGUID(i),
                    AutoScrapper.PLUGIN_NAME);
            }
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

                // We don't care how large the original texture is, as LoadImage overrides it anyway.
                // Bigger original texture would only mean more Garbage Collection.
                Texture2D iconTexture = new Texture2D(0, 0);
                if (iconTexture.LoadImage(File.ReadAllBytes(Path.Combine(fullName, "icon.png"))))
                {
                    Sprite icon = Sprite.Create(iconTexture,
                        new Rect(0.0f, 0.0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                    for (int profileIndex = 0; profileIndex < Utility.PROFILE_COUNT; profileIndex++)
                    {
                        ModSettingsManager.SetModIcon(icon, Utility.GetProfileGUID(profileIndex),
                            AutoScrapper.PLUGIN_NAME);
                    }
                }
                else
                    Debug.LogWarning("AutoScrapper: Failed to load icon.png");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("AutoScrapper: Failed to load icon.png\n" + ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddIntOption(int profileIndex, ConfigEntry<int> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool restartRequired = false)
        {
            if (SupportsCustomTranslation)
            {
                ModSettingsManager.AddOption(new IntFieldOption(configEntry, restartRequired),
                    Utility.GetProfileGUID(profileIndex),
                    profileName, customNameToken, customDescriptionToken);
            }
            else
            {
                ModSettingsManager.AddOption(new IntFieldOption(configEntry, restartRequired),
                    Utility.GetProfileGUID(profileIndex),
                    profileName);
            }
        }

        /// <summary>
        /// Creates a new Bool option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddBoolOption(int profileIndex, ConfigEntry<bool> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool requiresRestart = false)
        {
            if (SupportsCustomTranslation)
            {
                ModSettingsManager.AddOption(new CheckBoxOption(configEntry, requiresRestart),
                    Utility.GetProfileGUID(profileIndex),
                    profileName, customNameToken, customDescriptionToken);
            }
            else
            {
                ModSettingsManager.AddOption(new CheckBoxOption(configEntry, requiresRestart),
                    Utility.GetProfileGUID(profileIndex),
                    profileName);
            }
        }

        /// <summary>
        /// Creates a new Enum (Dropdown selection) option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddDropdownOption<T>(int profileIndex, ConfigEntry<T> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool requiresRestart = false)
            where T : Enum
        {
            if (SupportsCustomTranslation)
            {
                ModSettingsManager.AddOption(new ChoiceOption(configEntry, requiresRestart),
                    Utility.GetProfileGUID(profileIndex),
                    profileName, customNameToken, customDescriptionToken);
            }
            else
            {
                ModSettingsManager.AddOption(new ChoiceOption(configEntry, requiresRestart),
                    Utility.GetProfileGUID(profileIndex),
                    profileName);
            }
        }

        /// <summary>
        /// Creates a new String option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddStringOption(int profileIndex, ConfigEntry<string> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool requiresRestart = false)
        {
            if (SupportsCustomTranslation)
            {
                ModSettingsManager.AddOption(new StringInputFieldOption(configEntry, requiresRestart),
                    Utility.GetProfileGUID(profileIndex),
                    profileName, customNameToken, customDescriptionToken);
            }
            else
            {
                ModSettingsManager.AddOption(new StringInputFieldOption(configEntry, requiresRestart),
                    Utility.GetProfileGUID(profileIndex),
                    profileName);
            }
        }
    }
}