using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    [DefOf]
    public static class Thing_BuildingDefOf
    {
        public static ThingDef LLRoM_practicespot;
        static Thing_BuildingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Thing_BuildingDefOf));
        }
    }
}
