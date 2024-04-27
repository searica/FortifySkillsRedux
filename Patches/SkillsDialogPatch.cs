using HarmonyLib;
using System.Collections.Generic;
using TMPro;

namespace FortifySkillsRedux
{
    [HarmonyPatch(typeof(SkillsDialog))]
    public static class SkillsDialogPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SkillsDialog.Setup))]
        private static void SkillsDialogSetupPostfix(SkillsDialog __instance, Player player)
        {
            Log.LogInfo("SkillsDialog.Setup.Postfix()", LogLevel.Medium);

            List<Skills.Skill> skillList = player.GetSkills().GetSkillList();

            foreach (var element in __instance.m_elements)
            {
                string description = element.GetComponentInChildren<UITooltip>().m_text;
                foreach (var skill in skillList)
                {
                    if (skill.m_info.m_description == description)
                    {
                        // Check for a corresponding FortifySkill
                        if (FortifySkillData.s_FortifySkillValues.ContainsKey(skill.m_info.m_skill))
                        {
                            // Modify SkillsDialog level
                            string fortLevelText = $" ({(int)FortifySkillData.s_FortifySkillValues[skill.m_info.m_skill].FortifyLevel})";
                            var levelText = Utils.FindChild(element.transform, "leveltext").GetComponent<TMP_Text>();
                            levelText.text += fortLevelText;
                        }
                        else
                        {
                            Log.LogInfo($"No Fortified skill for: {skill.m_info.m_skill}", LogLevel.Medium);
                        }

                        break;
                    }
                }
            }
        }
    }
}