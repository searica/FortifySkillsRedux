using HarmonyLib;
using static Skills;

namespace FortifySkillsRedux.Patches;

[HarmonyPatch]
internal static class ResetCmdPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.ResetCharacter))]
    internal static void ResetFortifySkills(Player __instance)
    {
        foreach (SkillDef skillDef in __instance.m_skills.m_skills)
        {
            if (FortifySkillData.s_FortifySkills.ContainsKey(skillDef.m_skill))
            {
                FortifySkillData.ResetFortifySkill(skillDef.m_skill);
            }
        }
    }

    /// <summary>
    ///     Patch to reset fortified skill levels when resetskill console command is used to reset skills.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="skillType"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Skills), nameof(Skills.ResetSkill))]
    private static void SkillResetSkill(SkillType skillType)
    {
        FortifySkillData.ResetFortifySkill(skillType);
    }
}
