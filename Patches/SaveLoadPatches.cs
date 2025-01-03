using HarmonyLib;
using System.Collections.Generic;
using static Skills;
using Logging;

namespace FortifySkillsRedux.Patches;

[HarmonyPatch(typeof(Skills))]
public static class SaveLoadPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Skills.Load))]
    private static void LoadPrefix(Skills __instance, ZPackage pkg)
    {
        Log.LogInfo("Skills.Load.Prefix()", Log.InfoLevel.Medium);

        int currentPos = pkg.GetPos();

        FortifySkillData.s_FortifySkills.Clear();
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

                // Skill is missing info or already has a FortifySkill
                if (skill?.m_info == null || FortifySkillData.s_FortifySkills.ContainsKey(skillType))
                {
                    continue;
                }

                // Init a Fortify skill and set it to 95% of current skill if one does not already exist.
                // Data will be overridden by stored values from character save data if said data was saved.
                Log.LogInfo($"Fortify Skill initialized for: {FortifySkillData.LocalizeSkillName(skillType)} a.k.a {skill.m_info?.m_description}", Log.InfoLevel.Medium);
                FortifySkillData.s_FortifySkills[skillType] = new FortifySkillData(skill.m_info, level * 0.95f, 0f);
            }
            else // Should be dummy skill added by this mod to store FortifySkill data on save.
            {
                // Get the skill type that the dummy skill maps to.
                SkillType activeSkillType = FortifySkillData.MapDummySkill(skillType);
                if (!FortifySkillData.IsSkillValid(activeSkillType))
                {
                    Log.LogInfo("Unrecognized Fortify skill!", Log.InfoLevel.Medium);
                    continue;
                }

                Skill skill = __instance.GetSkill(activeSkillType);
                if (skill?.m_info == null)
                {
                    continue;
                }

                if (Log.VerbosityLevel >= Log.InfoLevel.Medium)
                {
                    string skillName = FortifySkillData.LocalizeSkillName(activeSkillType);
                    Log.LogInfo($"Fortify Skill mapped to: {skillName} a.k.a {skill.m_info?.m_description} @: {level}");
                }
                FortifySkillData.s_FortifySkills[activeSkillType] = new FortifySkillData(skill.m_info, level, accumulator);
            }
        }
        pkg.SetPos(currentPos); // reset pkg to position prior to reading data
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyPatch(nameof(Skills.Save))]
    private static void SavePrefix(Skills __instance, out Dictionary<SkillType, Skill> __state)
    {
        Log.LogInfo("Skills.Save.Prefix()", Log.InfoLevel.Medium);

        // Early return if not the right player
        if (FortifySkillData.s_AssociatedPlayer != __instance.m_player)
        {
            __state = null;
            Log.LogInfo("New character: skip saving Fortified Skill data", Log.InfoLevel.Medium);
            return;
        }

        // Store a copy off m_skill_data as it currently is.
        __state = new Dictionary<SkillType, Skill>();
        foreach (KeyValuePair<SkillType, Skill> pair in __instance.m_skillData)
        {
            if (pair.Value?.m_info == null) { continue; }

            Log.LogInfo($"Copying {FortifySkillData.LocalizeSkillName(pair.Key)} a.k.a {pair.Value.m_info.m_description}", Log.InfoLevel.High);

            __state[pair.Key] = pair.Value;
        }

        // Add dummy skills before saving to allow storing fortified skill data.
        foreach (KeyValuePair<SkillType, FortifySkillData> pair in FortifySkillData.s_FortifySkills)
        {
            if (pair.Value?.Info?.m_skill == null) { continue; }

            Log.LogInfo($"Making dummy skill for {FortifySkillData.LocalizeSkillName(pair.Key)} a.k.a {pair.Value.Info.m_description}", Log.InfoLevel.High);

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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Skills.Save))]
    private static void SavePostfix(
        Skills __instance,
        Dictionary<SkillType, Skill> __state
    )
    {
        if (__state == null)
        {
            Log.LogInfo("No fortified skill data, skip removing dummy skills.", Log.InfoLevel.Medium);
            return;
        }

        Log.LogInfo("Reseting m_skillData to remove dummy skills.", Log.InfoLevel.Medium);

        // Reset m_skillData to remove dummy skills.
        // Copy back the original values that were stored in __state.
        __instance.m_skillData.Clear();
        foreach (KeyValuePair<SkillType, Skill> pair in __state)
        {
            if (Log.VerbosityLevel >= Log.InfoLevel.High)
            {
                string skillName = FortifySkillData.LocalizeSkillName(pair.Key);
                Log.LogInfo($"Copying {skillName} a.k.a {pair.Value.m_info.m_description}");
            }
            __instance.m_skillData[pair.Key] = pair.Value;
        }
    }
}
