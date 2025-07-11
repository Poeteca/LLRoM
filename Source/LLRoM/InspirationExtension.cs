using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using LifeLessons;

namespace LLRoM
{
    public class InspirationExtension : DefModExtension
    {
        public HediffDef hediff;
        public List<ProficiencyDef> requiredproficiencies;
        public List<ProficiencyDef> relatedproficiencies;
        public ProficiencyCategoryDef blockingcategory;
        public float relatedCountNeeded = 1f;
        public List<ProficiencyDef> Outcomeproficiencies;
        public bool requiresInspiration = true;
    }
}
