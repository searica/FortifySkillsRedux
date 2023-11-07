using FortifySkillsRedux.Configs;
using HarmonyLib;
using UnityEngine;

namespace FortifySkillsRedux.Patches
{
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
            if (Config.IsVerbosityMedium)
            {
                Log.LogInfo("Raising XP for Fortified skill");
            }

            // calculate XP gained for the skill (used for getting fortified skill level XP)
            float baseXP = __instance.m_info.m_increseStep * factor;

            FortifySkillData fortSkill;
            if (FortifySkillData.s_FortifySkillValues.ContainsKey(__instance.m_info.m_skill))
            {
                fortSkill = FortifySkillData.s_FortifySkillValues[__instance.m_info.m_skill];
            }
            else
            {
                fortSkill = new FortifySkillData(__instance.m_info, __instance.m_level, 0f);
                FortifySkillData.s_FortifySkillValues[__instance.m_info.m_skill] = fortSkill;
            }

            if (fortSkill.FortifyLevel < 100f)
            {
                fortSkill.FortifyAccumulator += baseXP * Mathf.Clamp(
                        (__instance.m_level - fortSkill.FortifyLevel) * Config.FortifyLevelRate.Value,
                        0.0f,
                        Config.FortifyXPRateMax.Value
                    );
                if (Config.IsVerbosityMedium)
                {
                    Log.LogInfo("Fortify XP:" + fortSkill.FortifyAccumulator);
                }
                if (fortSkill.FortifyAccumulator >= GetLevelUpXpRequirement(fortSkill.FortifyLevel))
                {
                    // Level up Fortify skill
                    fortSkill.FortifyLevel = Mathf.Clamp(fortSkill.FortifyLevel + 1f, 0f, 100f);
                    fortSkill.FortifyAccumulator = 0f;

                    if (Config.IsVerbosityMedium)
                    {
                        Debug.Log("Fortify level:" + fortSkill.FortifyLevel);
                    }
                    // Display level up effect
                    var player = Player.m_localPlayer;
                    GameObject vfx_prefab = ZNetScene.instance.GetPrefab("vfx_ColdBall_launch");
                    GameObject sfx_prefab = player.m_skillLevelupEffects.m_effectPrefabs[1].m_prefab;

                    Object.Instantiate(vfx_prefab, player.GetHeadPoint(), Quaternion.Euler(-90f, 0, 0));
                    Object.Instantiate(sfx_prefab, player.GetHeadPoint(), Quaternion.identity);

                    // Display level up message
                    MessageHud.MessageType type = (int)fortSkill.FortifyLevel == 0 ? MessageHud.MessageType.Center : MessageHud.MessageType.TopLeft;

                    player.Message(
                        type,
                        $"Fortified skill improved $skill_{fortSkill.SkillInfo.m_skill.ToString().ToLower()}: {(int)fortSkill.FortifyLevel}",
                        0,
                        fortSkill.SkillInfo.m_icon
                    );
                }
            }
        }

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
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(nameof(Skills.Skill.Raise))]
        private static void ActiveSkillXpMultiplier(Skills.Skill __instance, ref float factor)
        {
            if (Config.IsVerbosityMedium)
            {
                Log.LogInfo("Applying active skill XP multiplier");
            }

            // modify XP gain rate
            if (Config.EnableIndividualSettings.Value)
            {
                if (Config.SkillConfigEntries.ContainsKey(__instance.m_info.m_skill.ToString()))
                {
                    factor *= Config.SkillConfigEntries[__instance.m_info.m_skill.ToString()].Value;
                }
                else
                {
                    factor *= Config.ModdedSkillXPMult.Value;
                }
            }
            else
            {
                factor *= Config.XPMult.Value;
            }
        }
    }
}