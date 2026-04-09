using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class ImpressivenessHandler
    {
        public static float ImpressivenessCurve(Room room)
        {
            float num;
            if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().LearningDrain)
            {
                return -10f;
            }
            if (room != null && !room.PsychologicallyOutdoors)
            {
                float impress = room.GetStat(RoomStatDefOf.Impressiveness);
                if (room.Role == RoomRoleDefOf.LLRoM_trainingHall)
                {
                    impress *= 1.25f;
                }
                if (impress > 240f) { num = .25f; }
                else if (impress < 20f) { num = 1.75f; }
                else
                {
                    num = (-1.5f / 220f) * impress + (83f / 44f);
                }
            }
            else
            {
                num = 1.75f;
            }
            return num;
        }
        public static float ImpressivenessFactor(Pawn pawn)
        {
            float num = 1f;
            if (!pawn.Spawned)
            {
                return num;
            }
            Room room = pawn.GetRoom();
            num = ImpressivenessCurve(room);
            return num;
        }
    }
}
