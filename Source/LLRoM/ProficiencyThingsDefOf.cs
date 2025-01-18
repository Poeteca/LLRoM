using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using LifeLessons;

namespace LLRoM
{
    public class ProficiencyThingsDefOf : DefOf
    {
        public static ProficiencyDef Magic_Insight;
        public static ProficiencyDef Physical_Insight;
        public static ProficiencyTabDef LLROM_Magic;
        public static ProficiencyTabDef LLROM_Might;
        public static LearningActivityDef LLRoM_UseScroll;
        static ProficiencyThingsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ProficiencyDefOf));
            DefOfHelper.EnsureInitializedInCtor(typeof(ProficiencyTabDef));
            DefOfHelper.EnsureInitializedInCtor(typeof(LearningActivityDef));
        }
    }
}
