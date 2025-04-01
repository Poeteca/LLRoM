using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorannMagic;
using Verse;

namespace LLRoM
{
    public class RoomRoleWorker_TrainingHall : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            float num = 0f;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Thing thing = containedAndAdjacentThings[i];
                if (thing.def == TorannMagicDefOf.TableMagicCircle)
                {
                    num += 1f;
                }
                if (thing.def == TorannMagicDefOf.TableSmallMagicCircle || thing.def == Thing_BuildingDefOf.LLRoM_practicespot)
                {
                    num += .5f;
                }
            }
            return num * 30f;
        }
    }
}
