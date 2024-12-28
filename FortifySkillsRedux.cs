using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using System.Reflection;
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;
using Configs;
using Logging;

namespace FortifySkillsRedux;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
[NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Patch)]
[SynchronizationMode(AdminOnlyStrictness.IfOnServer)]
internal sealed class FortifySkillsRedux : BaseUnityPlugin
{
    public const string PluginName = "FortifySkillsRedux";
    internal const string Author = "Searica";
    public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
    public const string PluginVersion = "1.5.0";

    private const string MainSection = "Global";
    private const string Mechanics = "Mechanics";
    private const string ModdedSkills = "Modded Skill Settings";

    public static FortifySkillsRedux Instance;
    internal class SkillConfig
    {
        public ConfigEntry<float> ActiveSkillXPMult;
        public ConfigEntry<float> FortifySkillMaxXPRate;
        public ConfigEntry<float> FortifySkillXPPerLevel;
    }

    internal SkillConfig GlobalSkillConfig { get; private set; }

    internal ConfigEntry<bool> EnableIndividualSettings { get; private set; }

    internal ConfigEntry<bool> KeepAllItemsOnDeath { get; private set; }
    internal ConfigEntry<bool> KeepEquippedItemsOnDeath { get; private set; }
    internal SkillConfig ModdedSkillConfig { get; private set; }

    internal Dictionary<Skills.SkillType, SkillConfig> SkillConfigsMap = [];

    private bool ShouldSave = false;

    public void Awake()
    {
        Instance = this;
        Log.Init(Logger);

        Config.Init(PluginGUID, false);
        SetUpConfigEntries();
        Config.Save();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        Game.isModded = true;

        Config.SetupWatcher();
        Config.CheckForConfigManager();
        ConfigFileManager.OnConfigWindowClosed += delegate
        {
            if (ShouldSave)
            {
                Config.Save();
                ShouldSave = false;
            }
        };
    }

    public void OnDestroy()
    {
        Config.Save();
    }

    internal void SetUpConfigEntries()
    {
        Log.Verbosity = Config.BindConfigInOrder(
            MainSection,
            "Verbosity",
            Log.InfoLevel.Low,
            "Low will log basic information about the mod. Medium will log information that is useful for troubleshooting. High will log a lot of information, do not set it to this without good reason as it will slow down your game.",
            synced: false
        );
        Log.Verbosity.SettingChanged += delegate { if (!ShouldSave) { ShouldSave = true; } };

        KeepAllItemsOnDeath = Config.Bind(
            MainSection,
            "Keep All Items on Death",
            false,
            "Whether to keep all items on death."
        );

        KeepEquippedItemsOnDeath = Config.Bind(
            MainSection,
            "Keep Equipped Items on Death",
            false,
            "Whether to keep your equiped items when you die."
        );

        EnableIndividualSettings = Config.BindConfigInOrder(
            MainSection,
            "Use Individual Settings",
            false,
            "If enabled, use the config settings for each individual Vanilla skill and the Modded skill config settings for all skills added by mods. If disabled use the config setting from the Mechanics section for all skills."
        );
        EnableIndividualSettings.SettingChanged += delegate { if (!ShouldSave) { ShouldSave = true; } };

        GlobalSkillConfig = BindSkillConfig(Mechanics);

        // Create config entries for individual skills in the base game
        Log.LogInfo($"{Skills.s_allSkills.Count()} SkillTypes are defined in base game.", Log.InfoLevel.Medium);
        foreach (Skills.SkillType skillType in Skills.s_allSkills)
        {
            if (skillType == Skills.SkillType.None) { continue; }
            if (skillType == Skills.SkillType.All) { continue; }

            string skillName = skillType.ToString();
            if (skillName == null)
            {
                continue;
            }

            Log.LogInfo($"Adding {skillName} to config.", Log.InfoLevel.Medium);
            SkillConfig skillConfig = BindSkillConfig(skillName);
            SkillConfigsMap.Add(skillType, skillConfig);
        }

        ModdedSkillConfig = BindSkillConfig(ModdedSkills);
    }

    internal SkillConfig BindSkillConfig(string section)
    {
        SkillConfig skillConfig = new()
        {
            ActiveSkillXPMult = Config.BindConfigInOrder(
                section,
                "Active Skill XP Multiplier",
                1.5f,
                "Controls XP gained for the active skill level. 1 = base game XP, 1.5 = 50% bonus XP, 0.8 = 20% less XP.",
                new AcceptableValueRange<float>(0.0f, 10f)
            ),
            FortifySkillMaxXPRate = Config.BindConfigInOrder(
                section,
                "Max Fortify Skill XP Rate",
                0.8f,
                "Controls maximum rate of XP earned for the fortified skill as a percentage of vanilla XP rates." +
                "Values below 1 mean that fortified skills will always increase slower than vanilla skills." +
                "Values above 1 mean that fortified skills can increase faster than vanilla skills if your active skill level is high enough.",
                new AcceptableValueRange<float>(0.0f, 2.0f)
            ),
            FortifySkillXPPerLevel = Config.BindConfigInOrder(
                section,
                "Fortify Skill XP Per Level",
                0.1f,
                "Controls XP gained for the fortified skill. For every level the active skill is above the fortified skill increase" +
                "the percentage of XP gained for the fortified skill by this amount up to Max Fortify Skill XP Rate.",
                new AcceptableValueRange<float>(0.0f, 1f)
            ),

        };
        skillConfig.ActiveSkillXPMult.SettingChanged += delegate { if (!ShouldSave) { ShouldSave = true; } };
        skillConfig.FortifySkillXPPerLevel.SettingChanged += delegate { if (!ShouldSave) { ShouldSave = true; } };
        skillConfig.FortifySkillMaxXPRate.SettingChanged += delegate { if (!ShouldSave) { ShouldSave = true; } };
        return skillConfig;
    }

    /// <summary>
    ///     Gets the multiplier for the active skill XP.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    internal float GetActiveSkillXPMult(Skills.Skill skill)
    {
        if (!EnableIndividualSettings.Value)
        {
            return GlobalSkillConfig.ActiveSkillXPMult.Value;
        }

        Skills.SkillType skillType = skill.m_info.m_skill;
        if (SkillConfigsMap.ContainsKey(skillType))
        {
            return SkillConfigsMap[skillType].ActiveSkillXPMult.Value;
        }
        else
        {
            return ModdedSkillConfig.ActiveSkillXPMult.Value;
        }
    }

    /// <summary>
    ///     Gets the percentage of XP gained per level that the active skill is
    ///     greater than the fortify skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    internal float GetFortifyXPPerLevel(Skills.Skill skill)
    {
        if (!EnableIndividualSettings.Value)
        {
            return GlobalSkillConfig.FortifySkillXPPerLevel.Value;
        }

        Skills.SkillType skillType = skill.m_info.m_skill;
        if (SkillConfigsMap.ContainsKey(skillType))
        {
            return SkillConfigsMap[skillType].FortifySkillXPPerLevel.Value;
        }
        else
        {
            return ModdedSkillConfig.FortifySkillXPPerLevel.Value;
        }
    }

    /// <summary>
    ///     Gets the XP rate of fortified skill relative to vanilla XP rates.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    internal float GetFortityMaxXPRate(Skills.Skill skill)
    {
        if (!EnableIndividualSettings.Value)
        {
            return GlobalSkillConfig.FortifySkillMaxXPRate.Value;
        }

        Skills.SkillType skillType = skill.m_info.m_skill;
        if (SkillConfigsMap.ContainsKey(skillType))
        {
            return SkillConfigsMap[skillType].FortifySkillMaxXPRate.Value;
        }
        else
        {
            return ModdedSkillConfig.FortifySkillMaxXPRate.Value;
        }
    }
}
