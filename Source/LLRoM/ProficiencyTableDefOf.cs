using RimWorld;
using LifeLessons;

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