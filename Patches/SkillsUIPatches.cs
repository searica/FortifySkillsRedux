using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using Logging;

namespace FortifySkillsRedux.Patches;

[HarmonyPatch(typeof(SkillsDialog))]
public static class SkillsUIPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SkillsDialog.Setup))]
    private static void SkillsDialogSetupPostfix(SkillsDialog __instance, Player player)
    {
        Log.LogInfo("SkillsDialog.Setup.Postfix()", Log.InfoLevel.Medium);

        List<Skills.Skill> skillList = player.GetSkills().GetSkillList();

        foreach (UnityEngine.GameObject element in __instance.m_elements)
        {
            string description = element.GetComponentInChildren<UITooltip>().m_text;
            foreach (Skills.Skill skill in skillList)
            {
                if (skill.m_info.m_description == description)
                {
                    // Check for a corresponding FortifySkill
                    if (FortifySkillData.s_FortifySkills.TryGetValue(skill.m_info.m_skill, out FortifySkillData fortifySkill))
                    {
                        // Modify SkillsDialog level
                        string fortLevelText = $" ({(int)fortifySkill.FortifyLevel})";
                        TMP_Text levelText = Utils.FindChild(element.transform, "leveltext").GetComponent<TMP_Text>();
                        levelText.text += fortLevelText;
                    }
                    else
                    {
                        Log.LogInfo($"No Fortified skill for: {skill.m_info.m_skill}", Log.InfoLevel.Medium);
                    }

                    break;
                }
            }
        }
    }
}
