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

        /// <summary>
        /// Adds an option with custom translation support.
        /// </summary>
        /// <param name="option">Option to add</param>
        /// <param name="profileIndex">Index of the profile to add for</param>
        /// <param name="profileName">Name of the profile to add for</param>
        /// <param name="customNameToken">Custom name translation token</param>
        /// <param name="customDescriptionToken">Custom description translation token</param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddOptionCustomTranslation(BaseOption option, int profileIndex, string profileName,
            string customNameToken, string customDescriptionToken)
        {
            ModSettingsManager.AddOption(option, Utility.GetProfileGUID(profileIndex),
                profileName, customNameToken, customDescriptionToken);
        }

        /// <summary>
        /// Adds an option without custom translation support.
        /// </summary>
        /// <param name="option">Option to add</param>
        /// <param name="profileIndex">Index of the profile to add for</param>
        /// <param name="profileName">Name of the profile to add for</param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddOptionNoTranslation(BaseOption option, int profileIndex, string profileName)
        {
            ModSettingsManager.AddOption(option, Utility.GetProfileGUID(profileIndex),
                profileName);
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static BaseOption AddIntOption(int profileIndex, ConfigEntry<int> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool restartRequired = false)
        {
            BaseOption option = new IntFieldOption(configEntry, restartRequired);

            if (SupportsCustomTranslation)
            {
                AddOptionCustomTranslation(option, profileIndex, profileName, customNameToken,
                    customDescriptionToken);
            }
            else
            {
                AddOptionNoTranslation(option, profileIndex, profileName);
            }

            return option;
        }

        /// <summary>
        /// Creates a new Bool option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static BaseOption AddBoolOption(int profileIndex, ConfigEntry<bool> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool requiresRestart = false)
        {
            BaseOption option = new CheckBoxOption(configEntry, requiresRestart);

            if (SupportsCustomTranslation)
            {
                AddOptionCustomTranslation(option, profileIndex, profileName, customNameToken,
                    customDescriptionToken);
            }
            else
            {
                AddOptionNoTranslation(option, profileIndex, profileName);
            }

            return option;
        }


        /// <summary>
        /// Creates a new Enum (Dropdown selection) option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static BaseOption AddDropdownOption<T>(int profileIndex, ConfigEntry<T> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool requiresRestart = false)
            where T : Enum
        {
            BaseOption option = new ChoiceOption(configEntry, requiresRestart);

            // TODO: This call does not work until Risk of Options 2.8.5 is released.
            if (false /*SupportsCustomTranslation*/)
            {
                AddOptionCustomTranslation(option, profileIndex, profileName, customNameToken,
                    customDescriptionToken);
            }
            else
            {
                AddOptionNoTranslation(option, profileIndex, profileName);
            }

            return option;
        }


        /// <summary>
        /// Creates a new String option for the given config entry.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static BaseOption AddStringOption(int profileIndex, ConfigEntry<string> configEntry, string profileName,
            string customNameToken, string customDescriptionToken, bool requiresRestart = false)
        {
            BaseOption option = new StringInputFieldOption(configEntry, requiresRestart);

            if (SupportsCustomTranslation)
            {
                AddOptionCustomTranslation(option, profileIndex, profileName, customNameToken,
                    customDescriptionToken);
            }
            else
            {
                AddOptionNoTranslation(option, profileIndex, profileName);
            }

            return option;
        }
    }
}