using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FortifySkillsRedux.Configs
{
    internal class Config
    {
        private static readonly string ConfigFileName = FortifySkillsRedux.PluginGuid + ".cfg";

        private static readonly string ConfigFileFullPath = string.Concat(
            Paths.ConfigPath,
            Path.DirectorySeparatorChar,
            ConfigFileName
        );

        private static ConfigFile configFile;

        private static readonly ConfigurationManagerAttributes AdminConfig = new() { IsAdminOnly = true };
        private static readonly ConfigurationManagerAttributes ClientConfig = new() { IsAdminOnly = false };

        internal enum LoggerLevel
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        internal const string MainSection = "\u200BGlobal";
        internal const string SkillsSection = "IndividualSkills";

        internal static ConfigEntry<LoggerLevel> Verbosity { get; private set; }
        internal static ConfigEntry<float> XPMult { get; private set; }
        internal static ConfigEntry<float> FortifyLevelRate { get; private set; }
        internal static ConfigEntry<float> FortifyXPRateMax { get; private set; }
        internal static ConfigEntry<bool> EnableIndividualSettings { get; private set; }
        internal static ConfigEntry<float> ModdedSkillXPMult { get; private set; }

        internal static Dictionary<string, ConfigEntry<float>> SkillConfigEntries = new();

        private static readonly AcceptableValueList<bool> AcceptableToggleValuesList = new(new bool[] { false, true });

        internal static LoggerLevel VerbosityLevel => Verbosity.Value;

        internal static bool IsVerbosityLow => Verbosity.Value >= LoggerLevel.Low;
        internal static bool IsVerbosityMedium => Verbosity.Value >= LoggerLevel.Medium;
        internal static bool IsVerbosityHigh => Verbosity.Value >= LoggerLevel.High;

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

        internal static string GetExtendedDescription(string description, bool synchronizedSetting)
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        internal static void Init(ConfigFile config)
        {
            configFile = config;
            configFile.SaveOnConfigSet = false;
        }

        internal static void Save()
        {
            configFile.Save();
        }

        internal static void SaveOnConfigSet(bool value)
        {
            configFile.SaveOnConfigSet = value;
        }

        public static void SetUpConfig()
        {
            XPMult = BindConfig(
                MainSection,
                "XPMult",
                1.5f,
                "Used to control the rate at which the active level increases, 1=base game, 1.5=50% bonus xp awarded, 0.8=20% less xp awarded. Default:1.5",
                new AcceptableValueRange<float>(0.0f, 10f)
            );

            FortifyLevelRate = BindConfig(
                MainSection,
                "FortifyXPPerLevelRate",
                0.1f,
                "Used to control the rate at which the fortified skill XP increases PER LEVEL behind the active level. 0.1=Will gain 10% XP for every level behind the active level. Note that this is a percentage of the XP earned towards the active skill after the XPMult value has been applied. Default:0.1",
                new AcceptableValueRange<float>(0.0f, 1f)
            );

            FortifyXPRateMax = BindConfig(
                MainSection,
                "FortifyXPRateMax",
                0.8f,
                "Used to control the maximum rate of XP earned for the fortified skill. Caps FortifyXPPerLevelRate. Values less than 1 mean the fortify skill will always increase more slowly than the active level. 0.8=Will gain a max of 80% of the XP gained for the active skill. Default 0.8",
                new AcceptableValueRange<float>(0.0f, 2.0f)
            );

            EnableIndividualSettings = BindConfig(
                MainSection,
                "EnableIndividualSettings",
                false,
                "Used to toggle whether the XPMult value from the Mechanics section is used for all skills or if the XPMult values from the IndividualSKills section are used for each vanilla skill.",
                AcceptableToggleValuesList
            );

            Verbosity = BindConfig(
                MainSection,
                "Verbosity",
                LoggerLevel.Low,
                "Low will log basic information about the mod. Medium will log information that is useful for troubleshooting. High will log a lot of information, do not set it to this without good reason as it will slow down your game."
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
                        $"{skillName}_XPMult",
                        1.5f,
                        $"XP Multiplier for {skillName} skill. Only used if EnableIndividualSettings is set to true",
                        new AcceptableValueRange<float>(0.0f, 10.0f)
                    )
                );
                }
            }

            ModdedSkillXPMult = BindConfig(
                SkillsSection,
                "ModdedSkillXPMult",
                1.0f,
                "XP Multiplier for skills added by mods (default value is 1.0 since most skill mods have their own XP multipier settings). Only used if EnableIndividualSettings is set to true.",
                new AcceptableValueRange<float>(0.0f, 10f)
            );
            Save();
        }

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
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Log.LogInfo("Reloading config file");
                var saveOnConfig = configFile.SaveOnConfigSet;
                configFile.SaveOnConfigSet = false;
                configFile.Reload();
                configFile.SaveOnConfigSet = saveOnConfig;
            }
            catch
            {
                Log.LogError($"There was an issue loading your {ConfigFileName}");
                Log.LogError("Please check your config entries for spelling and format!");
            }
        }
    }
}