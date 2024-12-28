using HarmonyLib;
using UnityEngine;
using Logging;

namespace FortifySkillsRedux.Patches;

[HarmonyPatch(typeof(Skills.Skill))]
public static class SkillPatch
{
    /// <summary>
    ///     Patch to raise XP for fortified skill before
    ///     XP multipliers are applied by this mod or other mods.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="factor"></param>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyPatch(nameof(Skills.Skill.Raise))]
    private static void RaiseFortifySkill(Skills.Skill __instance, ref float factor)
    {
        Log.LogInfo("Raising XP for Fortified skill", Log.InfoLevel.Medium);

        // calculate XP gained for the skill (used for getting fortified skill level XP)
        float baseXP = __instance.m_info.m_increseStep * factor;

        // Get or add FortifySkill data
        FortifySkillData fortSkill;
        if (FortifySkillData.s_FortifySkills.ContainsKey(__instance.m_info.m_skill))
        {
            fortSkill = FortifySkillData.s_FortifySkills[__instance.m_info.m_skill];
        }
        else
        {
            fortSkill = new FortifySkillData(__instance.m_info, __instance.m_level, 0f);
            FortifySkillData.s_FortifySkills[__instance.m_info.m_skill] = fortSkill;
        }

        // Early return if can't raise FortifySkill
        if (fortSkill.FortifyLevel >= 100f)
        {
            return;
        }

        // Compute new XP value
        float fortifyRate = FortifySkillsRedux.Instance.GetFortifyXPPerLevel(__instance);
        float fortifyMax = FortifySkillsRedux.Instance.GetFortityMaxXPRate(__instance);
        fortSkill.FortifyAccumulator += baseXP * Mathf.Clamp(
            (__instance.m_level - fortSkill.FortifyLevel) * fortifyRate, 0.0f, fortifyMax
        );
        Log.LogInfo("Fortify XP:" + fortSkill.FortifyAccumulator, Log.InfoLevel.Medium);

        // Level up FortifySkill if XP threshold reached
        if (fortSkill.FortifyAccumulator >= GetLevelUpXpRequirement(fortSkill.FortifyLevel))
        {
            LevelUpFortifySkill(fortSkill);
        }
    }

    /// <summary>
    ///     Level up FortifySkill and play effect/notify player.
    /// </summary>
    /// <param name="fortifySkill"></param>
    private static void LevelUpFortifySkill(FortifySkillData fortifySkill)
    {
        // Set level up message type
        MessageHud.MessageType type = (int)fortifySkill.FortifyLevel == 0 ? MessageHud.MessageType.Center : MessageHud.MessageType.TopLeft;

        // Level up Fortify skill
        fortifySkill.FortifyLevel = Mathf.Clamp(fortifySkill.FortifyLevel + 1f, 0f, 100f);
        fortifySkill.FortifyAccumulator = 0f;

        Log.LogInfo("Fortify level:" + fortifySkill.FortifyLevel, Log.InfoLevel.Medium);

        // Display level up effect
        Player player = Player.m_localPlayer;
        GameObject vfx_prefab = ZNetScene.instance.GetPrefab("vfx_ColdBall_launch");
        GameObject sfx_prefab = player.m_skillLevelupEffects.m_effectPrefabs[1].m_prefab;

        Object.Instantiate(vfx_prefab, player.GetHeadPoint(), Quaternion.Euler(-90f, 0, 0));
        Object.Instantiate(sfx_prefab, player.GetHeadPoint(), Quaternion.identity);

        // Display level up message
        player.Message(
            type,
            $"Fortified skill improved {fortifySkill.SkillName}: {(int)fortifySkill.FortifyLevel}",
            0,
            fortifySkill.Info.m_icon
        );
    }

    /// <summary>
    ///     Compute XP required to level up.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private static float GetLevelUpXpRequirement(float level)
    {
        return Mathf.Pow(level + 1f, 1.5f) * 0.5f + 0.5f;
    }

    /// <summary>
    ///     Patch to apply XP multiplier from this mod.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="factor"></param>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(Skills.Skill.Raise))]
    private static void ActiveSkillXpMultiplier(Skills.Skill __instance, ref float factor)
    {
        Log.LogInfo("Applying active skill XP multiplier", Log.InfoLevel.Medium);

        // modify XP gain rate
        factor *= FortifySkillsRedux.Instance.GetActiveSkillXPMult(__instance);
    }
}
