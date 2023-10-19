using HarmonyLib;
using System.Collections.Generic;

namespace FortifySkillsRedux
{
    [HarmonyPatch(typeof(Skills))]
    public static class SkillsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Skills.Load))]
        private static void LoadPrefix(Skills __instance, ZPackage pkg)
        {
            if (PluginConfig.IsVerbosityMedium)
            {
                Log.LogInfo("Skills.Load.Prefix()");
            }

            int currentPos = pkg.GetPos();

            FortifySkillData.s_FortifySkillValues.Clear();
            FortifySkillData.s_AssociatedPlayer = __instance.m_player;

            int num = pkg.ReadInt();
            int num2 = pkg.ReadInt();

            for (int i = 0; i < num2; i++)
            {
                Skills.SkillType skillType = (Skills.SkillType)pkg.ReadInt();
                float level = pkg.ReadSingle();
                float accumulator = num >= 2 ? pkg.ReadSingle() : 0f;

                // Is an existing skill in the base game (or a skill added by a mod)
                if (Skills.IsSkillValid(skillType))
                {
                    Skills.Skill skill = __instance.GetSkill(skillType);

                    // Init a Fortify skill and set it to 95% of current skill if one does not already exist.
                    // Data will be overridden by stored values from character save data if said data was saved.
                    if (!FortifySkillData.s_FortifySkillValues.ContainsKey(skillType))
                    {
                        FortifySkillData.s_FortifySkillValues[skillType] = new FortifySkillData(skill.m_info, level * 0.95f, 0f);
                    }
                }
                else // Should be dummy skill added by this mod to store FortifySkill data on save.
                {
                    // Compute the skill that the FortifySkill maps to.
                    Skills.SkillType activeSkillType = (Skills.SkillType)(int.MaxValue - (int)skillType);
                    if (Skills.IsSkillValid(activeSkillType))
                    {
                        if (PluginConfig.IsVerbosityMedium)
                        {
                            Log.LogInfo($"Fortify Skill mapped to: {activeSkillType} @: {level}");
                        }
                        Skills.Skill skill = __instance.GetSkill(activeSkillType);
                        FortifySkillData.s_FortifySkillValues[activeSkillType] = new FortifySkillData(skill.m_info, level, accumulator);
                    }
                    else
                    {
                        if (PluginConfig.IsVerbosityMedium)
                        {
                            Log.LogInfo("Unrecognised Fortify skill!");
                        }
                    }
                }
            }
            pkg.SetPos(currentPos); // reset pkg to position prior to reading data
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Skills.Save))]
        private static void SavePrefix(
            Skills __instance,
            out Dictionary<Skills.SkillType, Skills.Skill> __state
        )
        {
            if (PluginConfig.IsVerbosityMedium)
            {
                Log.LogInfo("Skills.Save.Prefix()");
            }
            if (FortifySkillData.s_AssociatedPlayer == __instance.m_player)
            {
                __state = new Dictionary<Skills.SkillType, Skills.Skill>();

                // Create a copy of m_skill_data as it currently is.
                foreach (KeyValuePair<Skills.SkillType, Skills.Skill> pair in __instance.m_skillData)
                {
                    if (PluginConfig.IsVerbosityMedium)
                    {
                        Log.LogInfo($"Copying {pair.Value.m_info.m_skill} a.k.a {pair.Value.m_info.m_description}");
                    }
                    __state[pair.Key] = pair.Value;
                }

                // Add dummy skills before saving to allow storing fortified skill data.
                foreach (KeyValuePair<Skills.SkillType, FortifySkillData> pair in FortifySkillData.s_FortifySkillValues)
                {
                    if (PluginConfig.IsVerbosityMedium)
                    {
                        Log.LogInfo($"Making dummy skill for {pair.Value.SkillInfo.m_skill}");
                    }
                    if (pair.Value.SkillInfo == null)
                    {
                        continue;
                    }

                    Skills.SkillDef dummySkillInfo = new()
                    {
                        m_skill = (Skills.SkillType)(int.MaxValue - (int)pair.Value.SkillInfo.m_skill)
                    };

                    Skills.Skill dummySkill = new(dummySkillInfo)
                    {
                        m_accumulator = pair.Value.FortifyAccumulator,
                        m_level = pair.Value.FortifyLevel
                    };

                    __instance.m_skillData[dummySkillInfo.m_skill] = dummySkill;
                }
            }
            else
            {
                __state = null;
                if (PluginConfig.IsVerbosityMedium)
                {
                    Log.LogInfo("New character: skip saving Fortified Skill data");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Skills.Save))]
        private static void SavePostfix(
            Skills __instance,
            Dictionary<Skills.SkillType, Skills.Skill> __state
        )
        {
            if (PluginConfig.IsVerbosityMedium)
            {
                Log.LogInfo("Skills.Save.Postfix()");
            }
            if (__state == null)
            {
                if (PluginConfig.IsVerbosityMedium)
                {
                    Log.LogInfo("__state is null, skip removing dummy skills.");
                }
                return;
            }
            if (PluginConfig.IsVerbosityMedium)
            {
                Log.LogInfo("Removing dummy skills.");
            }
            // Reset m_skillData to remove dummy skills.
            // Copy back the original values that were stored in __state.
            __instance.m_skillData.Clear();
            foreach (KeyValuePair<Skills.SkillType, Skills.Skill> pair in __state)
            {
                if (PluginConfig.IsVerbosityMedium)
                {
                    Log.LogInfo($"Copying {pair.Value.m_info.m_skill}");
                }
                __instance.m_skillData[pair.Key] = pair.Value;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Skills.OnDeath))]
        private static void SkillsOnDeathPostfix(Skills __instance)
        {
            if (PluginConfig.IsVerbosityMedium)
            {
                Log.LogInfo("Skills.OnDeath.Postfix()");
            }
            foreach (KeyValuePair<Skills.SkillType, Skills.Skill> pair in __instance.m_skillData)
            {
                if (FortifySkillData.s_FortifySkillValues.ContainsKey(pair.Key))
                {
                    FortifySkillData fortify = FortifySkillData.s_FortifySkillValues[pair.Key];
                    if (PluginConfig.IsVerbosityMedium)
                    {
                        Log.LogInfo($"Setting {pair.Key} to fortify level: {fortify.FortifyLevel}");
                    }
                    pair.Value.m_level = fortify.FortifyLevel;
                    pair.Value.m_accumulator = 0f;
                }
            }
        }
    }
}