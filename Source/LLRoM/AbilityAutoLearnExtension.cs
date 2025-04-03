using LifeLessons;
using Verse;

namespace LLRoM
{
    public class AbilityAutoLearnExtension : DefModExtension
    {
        public int failChance = 0;
        public HediffDef HedifftoApply;
        public ProficiencyTabDef tab;
        public float drainBase = .1f;
    }
}