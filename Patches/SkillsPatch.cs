using FortifySkillsRedux.Configs;
using HarmonyLib;
using System.Collections.Generic;
using static Skills;

namespace FortifySkillsRedux
{
    [HarmonyPatch(typeof(Skills))]
    public static class SkillsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Skills.Load))]
        private static void LoadPrefix(Skills __instance, ZPackage pkg)
        {
            if (ConfigManager.IsVerbosityMedium)
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
                SkillType skillType = (SkillType)pkg.ReadInt();
                float level = pkg.ReadSingle();
                float accumulator = num >= 2 ? pkg.ReadSingle() : 0f;

                // Is an existing skill in the base game (or a skill added by a mod)
                if (FortifySkillData.IsSkillValid(skillType))
                {
                    Skill skill = __instance.GetSkill(skillType);

                    // Init a Fortify skill and set it to 95% of current skill if one does not already exist.
                    // Data will be overridden by stored values from character save data if said data was saved.
                    if (skill?.m_info != null && !FortifySkillData.s_FortifySkillValues.ContainsKey(skillType))
                    {
                        if (ConfigManager.IsVerbosityMedium)
                        {
                            var skillName = FortifySkillData.GetLocalizedSkillName(skillType);
                            Log.LogInfo($"Fortify Skill initialized for: {skillName} a.k.a {skill.m_info?.m_description}");
                        }
                        FortifySkillData.s_FortifySkillValues[skillType] = new FortifySkillData(skill.m_info, level * 0.95f, 0f);
                    }
                }
                else // Should be dummy skill added by this mod to store FortifySkill data on save.
                {
                    // Compute the skill that the dummy skill maps to.
                    SkillType activeSkillType = FortifySkillData.MapDummySkill(skillType);
                    if (FortifySkillData.IsSkillValid(activeSkillType))
                    {
                        Skill skill = __instance.GetSkill(activeSkillType);
                        if (skill?.m_info != null)
                        {
                            if (ConfigManager.IsVerbosityMedium)
                            {
                                var skillName = FortifySkillData.GetLocalizedSkillName(activeSkillType);
                                Log.LogInfo($"Fortify Skill mapped to: {skillName} a.k.a {skill.m_info?.m_description} @: {level}");
                            }
                            FortifySkillData.s_FortifySkillValues[activeSkillType] = new FortifySkillData(skill.m_info, level, accumulator);
                        }
                    }
                    else if (ConfigManager.IsVerbosityMedium)
                    {
                        Log.LogInfo("Unrecognized Fortify skill!");
                    }
                }
            }
            pkg.SetPos(currentPos); // reset pkg to position prior to reading data
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(nameof(Skills.Save))]
        private static void SavePrefix(Skills __instance, out Dictionary<SkillType, Skill> __state)
        {
            if (ConfigManager.IsVerbosityMedium) { Log.LogInfo("Skills.Save.Prefix()"); }

            if (FortifySkillData.s_AssociatedPlayer == __instance.m_player)
            {
                __state = new Dictionary<SkillType, Skill>();

                // Store a copy off m_skill_data as it currently is.
                foreach (KeyValuePair<SkillType, Skill> pair in __instance.m_skillData)
                {
                    if (pair.Value?.m_info == null) { continue; }

                    if (ConfigManager.IsVerbosityHigh)
                    {
                        var skillName = FortifySkillData.GetLocalizedSkillName(pair.Key);
                        Log.LogInfo($"Copying {skillName} a.k.a {pair.Value.m_info.m_description}");
                    }
                    __state[pair.Key] = pair.Value;
                }

                // Add dummy skills before saving to allow storing fortified skill data.
                foreach (KeyValuePair<SkillType, FortifySkillData> pair in FortifySkillData.s_FortifySkillValues)
                {
                    if (pair.Value?.Info?.m_skill == null) { continue; }
                    if (ConfigManager.IsVerbosityHigh)
                    {
                        var skillName = FortifySkillData.GetLocalizedSkillName(pair.Key);
                        Log.LogInfo($"Making dummy skill for {skillName} a.k.a {pair.Value.Info.m_description}");
                    }

                    SkillDef dummySkillInfo = new()
                    {
                        m_skill = FortifySkillData.MapDummySkill(pair.Value.Info.m_skill),
                        m_description = pair.Value.Info?.m_description,
                    };

                    Skill dummySkill = new(dummySkillInfo)
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
                if (ConfigManager.IsVerbosityMedium)
                {
                    Log.LogInfo("New character: skip saving Fortified Skill data");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Skills.Save))]
        private static void SavePostfix(
            Skills __instance,
            Dictionary<SkillType, Skill> __state
        )
        {
            if (__state == null)
            {
                if (ConfigManager.IsVerbosityMedium)
                {
                    Log.LogInfo("No fortified skill data, skip removing dummy skills.");
                }
                return;
            }
            if (ConfigManager.IsVerbosityMedium)
            {
                Log.LogInfo("Reseting m_skillData to remove dummy skills.");
            }
            // Reset m_skillData to remove dummy skills.
            // Copy back the original values that were stored in __state.
            __instance.m_skillData.Clear();
            foreach (KeyValuePair<SkillType, Skill> pair in __state)
            {
                if (ConfigManager.IsVerbosityHigh)
                {
                    var skillName = FortifySkillData.GetLocalizedSkillName(pair.Key);
                    Log.LogInfo($"Copying {skillName} a.k.a {pair.Value.m_info.m_description}");
                }
                __instance.m_skillData[pair.Key] = pair.Value;
            }
        }

        [HarmonyFinalizer]
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(nameof(Skills.OnDeath))]
        private static void SkillsOnDeathFinalizer(Skills __instance)
        {
            if (ConfigManager.IsVerbosityMedium)
            {
                Log.LogInfo("Finalizing skills on death");
            }
            foreach (KeyValuePair<SkillType, Skill> pair in __instance.m_skillData)
            {
                if (FortifySkillData.s_FortifySkillValues.ContainsKey(pair.Key))
                {
                    FortifySkillData fortify = FortifySkillData.s_FortifySkillValues[pair.Key];

                    if (ConfigManager.IsVerbosityMedium)
                    {
                        Log.LogInfo($"Setting {fortify.SkillName} to fortify level: {fortify.FortifyLevel}");
                    }
                    pair.Value.m_level = fortify.FortifyLevel;
                    pair.Value.m_accumulator = 0f;
                }
            }
        }

        /// <summary>
        ///     Patch to reset fortified skill levels when resetskill console command is used to reset skills.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="skillType"></param>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Skills.ResetSkill))]
        private static void SkillResetSkill(SkillType skillType)
        {
            FortifySkillData.ResetFortifySkill(skillType);
        }
    }
}