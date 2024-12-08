using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using LifeLessons;

namespace LLRoM
{
    public class ProficiencyDefOf: DefOf
    {
        public static ProficiencyDef Magic_Insight;
        public static ProficiencyDef Physical_Insight;
        static ProficiencyDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ProficiencyDefOf));
        }
    }
}
