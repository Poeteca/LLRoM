using LifeLessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class HediffComp_scaledteacher : HediffComp
    {
        public List<ProficiencyDef> HediffProficiencyDef = new List<ProficiencyDef>();
        public ExperienceType experienceType;
        public float learnrate = 1f;
        public float SeverityThreshold = 0f;
        public HediffCompProperties_scaledteacher Props => (HediffCompProperties_scaledteacher)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            List<ProficiencyDef> proficiencies = Props.proficiencies;
            if (proficiencies != null && proficiencies.Count > 0 && parent.Severity > Props.SeverityThreshold)
            {
                foreach (ProficiencyDef item in proficiencies)
                {
                    HediffProficiencyDef.Clear();
                    ProficiencyComp comp = parent.pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        float xp = parent.Severity * .01f * Props.learnrate;
                        comp.TryGainXp(xp, item, Props.experienceType);
                    }
                }
            }
        }
    }
}
