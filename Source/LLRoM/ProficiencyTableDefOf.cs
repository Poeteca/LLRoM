using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using LifeLessons;
using Verse;

namespace LLRoM
{
    [DefOf]
    public static class ProficiencyTableDefOf 
    {
        public static ProficiencyTabDef LLROM_Magic;
        public static ProficiencyTabDef LLROM_Might;
        public static ProficiencyTabDef LLROM_Arcane;
        static ProficiencyTableDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ProficiencyTabDefOf));
        }
    }
}
