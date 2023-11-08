using System.Collections.Generic;
using static Skills;

namespace FortifySkillsRedux
{
    internal class FortifySkillData
    {
        public static Dictionary<SkillType, FortifySkillData> s_FortifySkillValues = new();

        public static Player s_AssociatedPlayer;

        public float FortifyLevel;
        public float FortifyAccumulator;
        public SkillDef SkillInfo;

        public FortifySkillData(SkillDef newSkillDef)
        {
            FortifyAccumulator = 0f;
            FortifyLevel = 0f;
            SkillInfo = newSkillDef;
        }

        public FortifySkillData(SkillDef newSkillDef, float newLevel, float newAccumulator)
        {
            FortifyAccumulator = newAccumulator;
            FortifyLevel = newLevel;
            SkillInfo = newSkillDef;
        }

        public static void ResetFortifySkill(SkillType skillType)
        {
            if (s_FortifySkillValues.ContainsKey(skillType))
            {
                s_FortifySkillValues[skillType].FortifyLevel = 0;
                s_FortifySkillValues[skillType].FortifyAccumulator = 0;
            }
        }
    }
}