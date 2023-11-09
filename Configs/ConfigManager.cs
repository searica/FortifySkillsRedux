using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using FortifySkillsRedux.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine;

namespace FortifySkillsRedux.Configs
{
    internal class ConfigManager
    {
        private static readonly string ConfigFileName = FortifySkillsRedux.PluginGUID + ".cfg";

        private static readonly string ConfigFileFullPath = string.Concat(
            Paths.ConfigPath,
            Path.DirectorySeparatorChar,
            ConfigFileName
        );

        private static ConfigFile configFile;
        private static BaseUnityPlugin ConfigurationManager;
        private const string ConfigManagerGUID = "com.bepis.bepinex.configurationmanager";

        #region Events

        /// <summary>
        ///     Event triggered after a the in-game configuration manager is closed.
        /// </summary>
        internal static event Action OnConfigWindowClosed;

        /// <summary>
        ///     Safely invoke the <see cref="OnConfigWindowClosed"/> event
        /// </summary>
        private static void InvokeOnConfigWindowClosed()
        {
            OnConfigWindowClosed?.SafeInvoke();
        }

        /// <summary>
        ///     Event triggered after the file watcher reloads the configuration file.
        /// </summary>
        internal static event Action OnConfigFileReloaded;

        /// <summary>
        ///     Safely invoke the <see cref="OnConfigFileReloaded"/> event
        /// </summary>
        private static void InvokeOnConfigFileReloaded()
        {
            OnConfigFileReloaded?.SafeInvoke();
        }

        #endregion Events

        #region LoggerLevel

        internal enum LoggerLevel
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        internal static ConfigEntry<LoggerLevel> Verbosity { get; private set; }
        internal static LoggerLevel VerbosityLevel => Verbosity.Value;
        internal static bool IsVerbosityLow => Verbosity.Value >= LoggerLevel.Low;
        internal static bool IsVerbosityMedium => Verbosity.Value >= LoggerLevel.Medium;
        internal static bool IsVerbosityHigh => Verbosity.Value >= LoggerLevel.High;

        #endregion LoggerLevel

        #region BindConfig

        internal static ConfigEntry<T> BindConfig<T>(
            string section,
            string name,
            T value,
            string description,
            AcceptableValueBase acceptVals = null,
            bool synced = true
        )
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            ConfigEntry<T> configEntry = configFile.Bind(
                section,
                name,
                value,
                new ConfigDescription(
                    extendedDescription,
                    acceptVals,
                    synced ? AdminConfig : ClientConfig
                )
            );
            return configEntry;
        }

        private static readonly ConfigurationManagerAttributes AdminConfig = new() { IsAdminOnly = true };
        private static readonly ConfigurationManagerAttributes ClientConfig = new() { IsAdminOnly = false };
        private const char ZWS = '\u200B';

        /// <summary>
        ///     Prepends Zero-Width-Space to set ordering of configuration sections
        /// </summary>
        /// <param name="sectionName">Section name</param>
        /// <param name="priority">Number of ZWS chars to prepend</param>
        /// <returns></returns>
        private static string SetStringPriority(string sectionName, int priority)
        {
            if (priority == 0) { return sectionName; }
            return new string(ZWS, priority) + sectionName;
        }

        internal static string GetExtendedDescription(string description, bool synchronizedSetting)
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        #endregion BindConfig

        private static readonly string MainSection = SetStringPriority("Global", 2);
        private static readonly string Mechanics = SetStringPriority("Mechanics", 1);
        private static readonly string SkillsSection = SetStringPriority("IndividualSkills", 0);

        internal static ConfigEntry<float> XPMult { get; private set; }
        internal static ConfigEntry<float> FortifyLevelRate { get; private set; }
        internal static ConfigEntry<float> FortifyXPRateMax { get; private set; }
        internal static ConfigEntry<bool> EnableIndividualSettings { get; private set; }
        internal static ConfigEntry<float> ModdedSkillXPMult { get; private set; }

        internal static Dictionary<string, ConfigEntry<float>> SkillConfigEntries = new();

        internal static void Init(ConfigFile config)
        {
            configFile = config;
            configFile.SaveOnConfigSet = false;
        }

        public static void SetUpConfig()
        {
            Verbosity = BindConfig(
                MainSection,
                "Verbosity",
                LoggerLevel.Low,
                "Low will log basic information about the mod. Medium will log information that is useful for troubleshooting. High will log a lot of information, do not set it to this without good reason as it will slow down your game."
            );

            EnableIndividualSettings = BindConfig(
                MainSection,
                "IndividualSettings",
                false,
                "Used to toggle whether the XPMult value from the Mechanics section is used for all skills or if the XPMult values from the IndividualSKills section are used for each vanilla skill."
            );

            XPMult = BindConfig(
                Mechanics,
                "XPMult",
                1.5f,
                "Used to control the rate at which the active level increases, 1=base game, 1.5=50% bonus xp awarded, 0.8=20% less xp awarded.",
                new AcceptableValueRange<float>(0.0f, 10f)
            );

            FortifyLevelRate = BindConfig(
                Mechanics,
                "FortifyXPPerLevelRate",
                0.1f,
                "Used to control the rate at which the fortified skill XP increases PER LEVEL behind the active level. 0.1=Will gain 10% XP for every level behind the active level. Note that this is a percentage of the XP earned towards the active skill before any XP multipliers have been applied.",
                new AcceptableValueRange<float>(0.0f, 1f)
            );

            FortifyXPRateMax = BindConfig(
                Mechanics,
                "FortifyXPRateMax",
                0.8f,
                "Used to control the maximum rate of XP earned for the fortified skill. Caps FortifyXPPerLevelRate. Values less than 1 mean the fortify skill will always increase more slowly than the active level. 0.8=Will gain a max of 80% of the XP gained for the active skill.",
                new AcceptableValueRange<float>(0.0f, 2.0f)
            );

            // Create config entries for individual skills in the base game
            if (IsVerbosityMedium)
            {
                Log.LogInfo($"{Skills.s_allSkills.Count()} SkillTypes are defined in base game.");
            }

            foreach (var skilltype in Skills.s_allSkills)
            {
                string skillName = skilltype.ToString();

                if (skillName != null && skillName != "None" && skillName != "All")
                {
                    if (IsVerbosityMedium)
                    {
                        Log.LogInfo($"Adding {skillName} to config.");
                    }
                    SkillConfigEntries.Add(
                    skillName,
                    BindConfig(
                        SkillsSection,
                        SetStringPriority($"{skillName}_XPMult", 1),
                        1.5f,
                        $"XP Multiplier for {skillName} skill. Only used if EnableIndividualSettings is set to true",
                        new AcceptableValueRange<float>(0.0f, 10.0f)
                    )
                );
                }
            }

            ModdedSkillXPMult = BindConfig(
                SkillsSection,
                "ModdedSkill_XPMult",
                1.0f,
                "XP Multiplier for skills added by mods (default value is 1.0 since most skill mods have their own XP multiplier settings). Only used if EnableIndividualSettings is set to true.",
                new AcceptableValueRange<float>(0.0f, 10f)
            );
            Save();
        }

        /// <summary>
        ///     Sets SaveOnConfigSet to false and returns
        ///     the value prior to calling this method.
        /// </summary>
        /// <returns></returns>
        private static bool DisableSaveOnConfigSet()
        {
            var val = configFile.SaveOnConfigSet;
            configFile.SaveOnConfigSet = false;
            return val;
        }

        /// <summary>
        ///     Set the value for the SaveOnConfigSet field.
        /// </summary>
        /// <param name="value"></param>
        internal static void SaveOnConfigSet(bool value)
        {
            configFile.SaveOnConfigSet = value;
        }

        /// <summary>
        ///     Save config file to disk.
        /// </summary>
        internal static void Save()
        {
            configFile.Save();
        }

        #region FileWatcher

        internal static void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReloadConfigFile;
            watcher.Created += ReloadConfigFile;
            watcher.Renamed += ReloadConfigFile;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReloadConfigFile(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) { return; }
            try
            {
                Log.LogInfo("Reloading config file");

                // turn off saving on config entry set
                var saveOnConfigSet = DisableSaveOnConfigSet();
                configFile.Reload();
                SaveOnConfigSet(saveOnConfigSet); // reset config saving state
                InvokeOnConfigFileReloaded(); // fire event
            }
            catch
            {
                Log.LogError($"There was an issue loading your {ConfigFileName}");
                Log.LogError("Please check your config entries for spelling and format!");
            }
        }

        #endregion FileWatcher

        #region ConfigManager

        /// <summary>
        ///     Checks for in-game configuration manager and
        ///     sets up OnConfigWindowClosed event if it is present
        /// </summary>
        internal static void CheckForConfigManager()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return;
            }

            if (Chainloader.PluginInfos.TryGetValue(ConfigManagerGUID, out PluginInfo configManagerInfo) && configManagerInfo.Instance)
            {
                ConfigurationManager = configManagerInfo.Instance;
                Log.LogDebug("Configuration manager found, hooking DisplayingWindowChanged");

                EventInfo eventinfo = ConfigurationManager.GetType().GetEvent("DisplayingWindowChanged");

                if (eventinfo != null)
                {
                    Action<object, object> local = new(OnConfigManagerDisplayingWindowChanged);
                    Delegate converted = Delegate.CreateDelegate(
                        eventinfo.EventHandlerType,
                        local.Target,
                        local.Method
                    );
                    eventinfo.AddEventHandler(ConfigurationManager, converted);
                }
            }
        }

        private static void OnConfigManagerDisplayingWindowChanged(object sender, object e)
        {
            PropertyInfo pi = ConfigurationManager.GetType().GetProperty("DisplayingWindow");
            bool ConfigurationManagerWindowShown = (bool)pi.GetValue(ConfigurationManager, null);

            if (!ConfigurationManagerWindowShown)
            {
                InvokeOnConfigWindowClosed();
            }
        }

        #endregion ConfigManager
    }
}