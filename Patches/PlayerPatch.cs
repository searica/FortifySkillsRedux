using HarmonyLib;
using static Skills;
using System.Collections.Generic;

namespace FortifySkillsRedux.Patches {
    /// <summary>
    ///     Reset Fortify Skills when resetcharacter console command is used
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    internal static class PlayerPatch {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.ResetCharacter))]
        internal static void ResetFortifySkills(Player __instance) {
            foreach (var skillDef in __instance.m_skills.m_skills) {
                if (FortifySkillData.s_FortifySkillValues.ContainsKey(skillDef.m_skill)) {
                    FortifySkillData.ResetFortifySkill(skillDef.m_skill);
                }
            }
        }

        [HarmonyFinalizer]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(nameof(Player.OnDeath))]
        private static void OnDeathFinalizer(Player __instance) {
            Log.LogInfo("Finalizing skills on death", LogLevel.Medium);
            if (!__instance || !__instance.m_skills) {
                return;
            }
            var skills = __instance.m_skills;
            foreach (KeyValuePair<SkillType, Skill> pair in skills.m_skillData) {
                if (FortifySkillData.s_FortifySkillValues.ContainsKey(pair.Key)) {
                    FortifySkillData fortify = FortifySkillData.s_FortifySkillValues[pair.Key];

                    Log.LogInfo($"Setting {fortify.SkillName} to fortify level: {fortify.FortifyLevel}", LogLevel.Medium);

                    pair.Value.m_level = fortify.FortifyLevel;
                    pair.Value.m_accumulator = 0f;
                }
            }
        }
    }
}