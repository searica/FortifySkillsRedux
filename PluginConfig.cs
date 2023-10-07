using BepInEx.Configuration;
using ServerSync;
using System.Collections.Generic;
using System.Linq;

namespace FortifySkillsRedux
{
    internal class PluginConfig
    {

        private static ConfigFile configFile;

        private static readonly ConfigSync configSync = new(Plugin.PluginGuid)
        {
            DisplayName = Plugin.PluginName,
            CurrentVersion = Plugin.PluginVersion,
            MinimumRequiredVersion = Plugin.PluginVersion
        };

        public const string MainSection = "\u200BGlobal";
        public const string MechanicsSection = "General";
        public const string SkillsSection = "IndividualSkills";
        
        public static ConfigEntry<bool> IsModEnabled { get; private set; }
        public static ConfigEntry<bool> LockConfiguration { get; private set; }
        public static ConfigEntry<float> XPMult { get; private set; }
        public static ConfigEntry<float> FortifyLevelRate { get; private set; }
        public static ConfigEntry<float> FortifyMaxRate { get; private set; }
        public static ConfigEntry<bool> EnableIndividualSettings { get; private set; }

        public static Dictionary<string, ConfigEntry<float>> SkillConfigEntries = new();

        private static readonly AcceptableValueList<bool> AcceptableToggleValuesList = new(new bool[] { false, true });


        internal static ConfigEntry<T> BindConfig<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, description);

            // ServerSync Settings
            SyncedConfigEntry < T > syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        // Server Sync Locking Config
        internal static ConfigEntry<bool> BindLockingConfig(string group, string name, bool value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<bool> configEntry = configFile.Bind(group, name, value, description);

            SyncedConfigEntry<bool> syncedConfigEntry = configSync.AddLockingConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }


        internal static ConfigEntry<T> BindConfig<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => BindConfig(group, name, value, new ConfigDescription(description), synchronizedSetting);


        public static void Init(ConfigFile config)
        {
            configFile = config;
            configFile.SaveOnConfigSet = false;
        }

        public static void SetUpConfig()
        {
            IsModEnabled = BindConfig(
                MainSection,
                "EnableMod",
                true,
                new ConfigDescription(
                    "Globally enable or disable this mod (restart required).",
                    AcceptableToggleValuesList
                )
             );

            LockConfiguration = BindLockingConfig(
                MainSection,
                "LockConfiguration",
                true,
                new ConfigDescription(
                    "If true, the configuration is locked and can be changed by server admins only.",
                    AcceptableToggleValuesList
                )
            );

            XPMult = BindConfig(
                MechanicsSection,
                "XPMult",
                1.5f,
                new ConfigDescription("Used to control the rate at which the active level increases, 1=base game, 1.5=50% bonus xp awarded, 0.8=20% less xp awarded. Default:1.5",
                new AcceptableValueRange<float>(0.0f, 10f))
            );

            FortifyLevelRate = BindConfig(
                MechanicsSection,
                "FortifyXPPerLevelRate",
                0.1f,
                new ConfigDescription("Used to control the rate at which the fortified skill XP increases PER LEVEL behind the active level. 0.1=Will gain 10% XP for every level behind the active level. Note that this is a perctange of the XP earned towards the active skill after the XPMult value has been applied. Default:0.1",
                new AcceptableValueRange<float>(0.0f, 1f))
            );

            FortifyMaxRate = BindConfig(
                MechanicsSection,
                "FortifyMaxXPRate",
                0.8f,
                new ConfigDescription("Used to control the maximum rate of XP earned for the fortified skill. Caps FortifyXPPerLevelRate. Values less than 1 mean the fortify skill will always increase more slowly than the active level. 0.8=Will gain a max of 80% of the XP gained for the active skill. Default 0.8",
                new AcceptableValueRange<float>(0.0f, 2.0f))
            );

            EnableIndividualSettings = BindConfig(
                MechanicsSection,
                "EnableIndividualSettings",
                false,
                "Used to toggle whether the Global XPMult value is used for all skills or if the inidividual configuration settings are used."
            );

            // Create config entries for individual skills in the base game
#if DEBUG
            Log.LogInfo($"{Skills.s_allSkills.Count()} SkillTypes are defined in base game.");
#endif
            foreach (var skilltype in Skills.s_allSkills)
            {
                string skillName = skilltype.ToString();

                if (skillName != null && skillName != "None" && skillName != "All")
                {
#if DEBUG
                    Log.LogInfo($"Adding {skillName} to config.");
#endif
                    SkillConfigEntries.Add(
                        skillName,
                        BindConfig(
                            SkillsSection,
                            $"{skillName}_XPMult",
                            1.5f,
                            new ConfigDescription(
                                $"XP Multiplier for {skillName} skill. Only used if EnableIndividualSettings is set to true",
                                new AcceptableValueRange<float>(0.0f, 10.0f)
                            )
                        )
                    );
                }
            }
            Save();
        }

        public static void Save()
        {
            configFile.Save();
        }
    }
}
