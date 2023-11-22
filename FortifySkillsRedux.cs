﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System.Reflection;
using FortifySkillsRedux.Configs;
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FortifySkillsRedux
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
    [NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Patch)]
    internal sealed class FortifySkillsRedux : BaseUnityPlugin
    {
        public const string PluginName = "FortifySkillsRedux";
        internal const string Author = "Searica";
        public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.0.7";

        private static readonly string MainSection = ConfigManager.SetStringPriority("Global", 2);
        private static readonly string Mechanics = ConfigManager.SetStringPriority("Mechanics", 1);
        private static readonly string SkillsSection = ConfigManager.SetStringPriority("IndividualSkills", 0);

        internal static ConfigEntry<float> XPMult { get; private set; }
        internal static ConfigEntry<float> FortifyLevelRate { get; private set; }
        internal static ConfigEntry<float> FortifyXPRateMax { get; private set; }
        internal static ConfigEntry<bool> EnableIndividualSettings { get; private set; }
        internal static ConfigEntry<float> ModdedSkillXPMult { get; private set; }

        internal static Dictionary<string, ConfigEntry<float>> SkillConfigEntries = new();

        private static bool ShouldSave = false;

        public void Awake()
        {
            Log.Init(Logger);

            ConfigManager.Init(PluginGUID, Config, false);
            SetUpConfigEntries();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);

            Game.isModded = true;

            ConfigManager.SetupWatcher();
            ConfigManager.CheckForConfigManager();
            ConfigManager.OnConfigWindowClosed += delegate
            {
                if (ShouldSave)
                {
                    ConfigManager.Save();
                    ShouldSave = false;
                }
            };
        }

        public void OnDestroy()
        {
            ConfigManager.Save();
        }

        internal static void SetUpConfigEntries()
        {
            Log.Verbosity = ConfigManager.BindConfig(
                MainSection,
                "Verbosity",
                LogLevel.Low,
                "Low will log basic information about the mod. Medium will log information that is useful for troubleshooting. High will log a lot of information, do not set it to this without good reason as it will slow down your game.",
                synced: false
            );
            Log.Verbosity.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };

            EnableIndividualSettings = ConfigManager.BindConfig(
                MainSection,
                "IndividualSettings",
                false,
                "Used to toggle whether the XPMult value from the Mechanics section is used for all skills or if the XPMult values from the IndividualSKills section are used for each vanilla skill."
            );
            EnableIndividualSettings.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };

            XPMult = ConfigManager.BindConfig(
                Mechanics,
                "XPMult",
                1.5f,
                "Used to control the rate at which the active level increases, 1=base game, 1.5=50% bonus xp awarded, 0.8=20% less xp awarded.",
                new AcceptableValueRange<float>(0.0f, 10f)
            );
            XPMult.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };

            FortifyLevelRate = ConfigManager.BindConfig(
                Mechanics,
                "FortifyXPPerLevelRate",
                0.1f,
                "Used to control the rate at which the fortified skill XP increases PER LEVEL behind the active level. 0.1=Will gain 10% XP for every level behind the active level. Note that this is a percentage of the XP earned towards the active skill before any XP multipliers have been applied.",
                new AcceptableValueRange<float>(0.0f, 1f)
            );
            FortifyLevelRate.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };

            FortifyXPRateMax = ConfigManager.BindConfig(
                Mechanics,
                "FortifyXPRateMax",
                0.8f,
                "Used to control the maximum rate of XP earned for the fortified skill. Caps FortifyXPPerLevelRate. Values less than 1 mean the fortify skill will always increase more slowly than the active level. 0.8=Will gain a max of 80% of the XP gained for the active skill.",
                new AcceptableValueRange<float>(0.0f, 2.0f)
            );
            FortifyXPRateMax.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };

            // Create config entries for individual skills in the base game
            Log.LogInfo($"{Skills.s_allSkills.Count()} SkillTypes are defined in base game.", LogLevel.Medium);

            foreach (var skillType in Skills.s_allSkills)
            {
                string skillName = skillType.ToString();

                if (skillName != null && skillType != Skills.SkillType.None && skillType != Skills.SkillType.All)
                {
                    Log.LogInfo($"Adding {skillName} to config.", LogLevel.Medium);
                    var configEntry = ConfigManager.BindConfig(
                        SkillsSection,
                        ConfigManager.SetStringPriority($"{skillName}_XPMult", 1),
                        1.5f,
                        $"XP Multiplier for {skillName} skill. Only used if IndividualSettings is set to true",
                        new AcceptableValueRange<float>(0.0f, 10.0f)
                    );
                    configEntry.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };
                    SkillConfigEntries.Add(skillName, configEntry);
                }
            }

            ModdedSkillXPMult = ConfigManager.BindConfig(
                SkillsSection,
                "ModdedSkill_XPMult",
                1.0f,
                "XP Multiplier for skills added by mods (default value is 1.0 since most skill mods have their own XP multiplier settings). " +
                "Only used if IndividualSettings is set to true.",
                new AcceptableValueRange<float>(0.0f, 10f)
            );
            ModdedSkillXPMult.SettingChanged += delegate { if (!ShouldSave) ShouldSave = true; };

            ConfigManager.Save();
        }
    }

    /// <summary>
    ///     Log level to control output to BepInEx log
    /// </summary>
    internal enum LogLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    /// <summary>
    ///     Helper class for properly logging from static contexts.
    /// </summary>
    internal static class Log
    {
        #region Verbosity

        internal static ConfigEntry<LogLevel> Verbosity { get; set; }
        internal static LogLevel VerbosityLevel => Verbosity.Value;

        #endregion Verbosity

        private static ManualLogSource logSource;

        private const BindingFlags AllBindings =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        internal static void Init(ManualLogSource logSource)
        {
            Log.logSource = logSource;
        }

        internal static void LogDebug(object data) => logSource.LogDebug(data);

        internal static void LogError(object data) => logSource.LogError(data);

        internal static void LogFatal(object data) => logSource.LogFatal(data);

        internal static void LogInfo(object data, LogLevel level = LogLevel.Low)
        {
            if (Verbosity is null || VerbosityLevel >= level)
            {
                logSource.LogInfo(data);
            }
        }

        internal static void LogMessage(object data) => logSource.LogMessage(data);

        internal static void LogWarning(object data) => logSource.LogWarning(data);

        #region Logging Unity Objects

        internal static void LogGameObject(GameObject prefab, bool includeChildren = false)
        {
            LogInfo("***** " + prefab.name + " *****");
            foreach (Component compo in prefab.GetComponents<Component>())
            {
                LogComponent(compo);
            }

            if (!includeChildren) { return; }

            LogInfo("***** " + prefab.name + " (children) *****");
            foreach (Transform child in prefab.transform)
            {
                LogInfo($" - {child.gameObject.name}");
                foreach (Component compo in child.gameObject.GetComponents<Component>())
                {
                    LogComponent(compo);
                }
            }
        }

        internal static void LogComponent(Component compo)
        {
            LogInfo($"--- {compo.GetType().Name}: {compo.name} ---");

            PropertyInfo[] properties = compo.GetType().GetProperties(AllBindings);
            foreach (var property in properties)
            {
                LogInfo($" - {property.Name} = {property.GetValue(compo)}");
            }

            FieldInfo[] fields = compo.GetType().GetFields(AllBindings);
            foreach (var field in fields)
            {
                LogInfo($" - {field.Name} = {field.GetValue(compo)}");
            }
        }

        #endregion Logging Unity Objects
    }
}