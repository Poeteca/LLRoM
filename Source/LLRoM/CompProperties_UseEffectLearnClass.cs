using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLRoM
{
    public class CompProperties_UseEffectLearnClass : CompProperties_UseEffect
    {
        public CompProperties_UseEffectLearnClass()
        {
            compClass = typeof(CompUseEffect_LearnClass);
        }
    }
}
