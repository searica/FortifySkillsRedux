using HarmonyLib;
using static Skills;
using System.Collections.Generic;
using Logging;

namespace FortifySkillsRedux.Patches;

/// <summary>
///     Reset Fortify Skills when resetcharacter console command is used
/// </summary>
[HarmonyPatch(typeof(Player))]
internal static class OnDeathPatches
{
    /// <summary>
    ///     Prevents skill loss on death by modifying Vanilla settings. Allows this
    ///     mod to override the skill loss of mods like HardCore presets.
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(Player.OnDeath))]
    private static void OnDeath()
    {
        if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathSkillsReset))
        {
            ZoneSystem.instance.m_globalKeysEnums.Remove(GlobalKeys.DeathSkillsReset);
        }
    }

    [HarmonyFinalizer]
    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(nameof(Player.OnDeath))]
    private static void OnDeathFinalizer(Player __instance)
    {
        Log.LogInfo("Finalizing skills on death", Log.InfoLevel.Medium);
        if (!__instance || !__instance.m_skills)
        {
            return;
        }

        Skills skills = __instance.m_skills;
        foreach (KeyValuePair<SkillType, Skill> pair in skills.m_skillData)
        {
            if (FortifySkillData.s_FortifySkills.ContainsKey(pair.Key))
            {
                FortifySkillData fortify = FortifySkillData.s_FortifySkills[pair.Key];

                Log.LogInfo($"Setting {fortify.SkillName} to fortify level: {fortify.FortifyLevel}", Log.InfoLevel.Medium);

                pair.Value.m_level = fortify.FortifyLevel;
                pair.Value.m_accumulator = 0f;
            }
        }
    }
}
