using LifeLessons;
using RimWorld;

namespace LLRoM
{
    [DefOf]
    public static class ProficiencyDefOf
    {
        public static ProficiencyDef Physical_Insight;
        public static ProficiencyDef Magic_Insight;
        public static ProficiencyDef LLRoM_Physical_Conditioning;
        public static ProficiencyDef LLRoM_Endurance;
        public static ProficiencyDef LLRoM_CQC;
        public static ProficiencyDef LLRoM_Ranged;
        public static ProficiencyDef LLRoM_Extreme_Range;
        public static ProficiencyDef LLRoM_Defensive_Fighting;
        static ProficiencyDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ProficiencyDefOf));
        }
    }
}