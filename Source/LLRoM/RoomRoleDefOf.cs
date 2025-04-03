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
    public static class RoomRoleDefOf
    {
        public static RoomRoleDef LLRoM_trainingHall;
        static RoomRoleDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RoomRoleDefOf));
        }
    }
}
