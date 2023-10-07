using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortifySkillsRedux
{
    class FortifySkillData
    {

        public static Dictionary<Skills.SkillType,FortifySkillData> s_FortifySkillValues = new();
        
        public static Player s_AssociatedPlayer;

        public float FortifyLevel;
        public float FortifyAccumulator;
        public Skills.SkillDef SkillInfo;

        public FortifySkillData(Skills.SkillDef newSkillDef)
        {
            FortifyAccumulator = 0f;
            FortifyLevel = 0f;
            SkillInfo = newSkillDef;
        }
        public FortifySkillData(Skills.SkillDef newSkillDef, float newLevel, float newAccumulator)
        {
            FortifyAccumulator = newAccumulator;
            FortifyLevel = newLevel;
            SkillInfo = newSkillDef;
        }

    }

}
