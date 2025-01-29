using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class AbilityAutoLearnExtension : DefModExtension
    {
        public int failChance = 0;
        public HediffDef HedifftoApply;
        public ProficiencyTabDef tab;
    }
}
