using LifeLessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class HediffCompProperties_teacher : HediffCompProperties
    {
        public List<ProficiencyDef> proficiencies;
        public ExperienceType experienceType;
        public float learnrate;
        public float SeverityThreshold;
        public HediffCompProperties_teacher()
        {
            compClass = typeof(HediffComp_teacher);
        }
    }
}
