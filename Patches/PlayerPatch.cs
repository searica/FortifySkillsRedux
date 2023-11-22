using HarmonyLib;

namespace FortifySkillsRedux.Patches
{
    /// <summary>
    ///     Reset Fortify Skills when resetcharacter console command is used
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    internal static class PlayerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.ResetCharacter))]
        internal static void ResetFortifySkills(Player __instance)
        {
            foreach (var skillDef in __instance.m_skills.m_skills)
            {
                if (FortifySkillData.s_FortifySkillValues.ContainsKey(skillDef.m_skill))
                {
                    FortifySkillData.ResetFortifySkill(skillDef.m_skill);
                }
            }
        }
    }
}