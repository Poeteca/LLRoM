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
    public class CompProperties_UseEffectTrySelfLearnSpell : CompProperties_UseEffect
    {
        public CompProperties_UseEffectTrySelfLearnSpell()
        {
            compClass = typeof(CompUseEffect_TrySelfLearnSpell);
        }
    }
}
