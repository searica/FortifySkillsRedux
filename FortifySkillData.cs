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
        public SkillDef Info;

        public static bool IsSkillValid(SkillType skillType)
        {
            return Skills.IsSkillValid(skillType) && skillType != SkillType.None && skillType != SkillType.All;
        }

        /// <summary>
        ///     Localized name of the skill this FortifySkill maps to.
        /// </summary>
        public string SkillName => GetLocalizedSkillName(this);

        public FortifySkillData(SkillDef skillDef)
        {
            FortifyAccumulator = 0f;
            FortifyLevel = 0f;
            Info = skillDef;
        }

        public FortifySkillData(SkillDef skillDef, float newLevel, float newAccumulator)
        {
            FortifyAccumulator = newAccumulator;
            FortifyLevel = newLevel;
            Info = skillDef;
        }

        public static void ResetFortifySkill(SkillType skillType)
        {
            if (s_FortifySkillValues.ContainsKey(skillType))
            {
                s_FortifySkillValues[skillType].FortifyLevel = 0;
                s_FortifySkillValues[skillType].FortifyAccumulator = 0;
            }
        }

        /// <summary>
        ///     Reversible operation to map a SkillType to a dummy SkillType
        ///     or map a dummy SkillType to an actual SkillType
        /// </summary>
        /// <param name="skillType"></param>
        /// <returns></returns>
        public static SkillType MapDummySkill(SkillType skillType)
        {
            return (SkillType)(int.MaxValue - (int)skillType);
        }

        public static string GetLocalizedSkillName(Skill skill)
        {
            return $"$skill_{skill.m_info.m_skill.ToString().ToLower()}";
        }

        public static string GetLocalizedSkillName(FortifySkillData fortSkill)
        {
            return $"$skill_{fortSkill.Info.m_skill.ToString().ToLower()}";
        }

        public static string GetLocalizedSkillName(SkillType skillType)
        {
            return $"$skill_{skillType.ToString().ToLower()}";
        }
    }
}