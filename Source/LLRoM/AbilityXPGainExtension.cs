using LifeLessons;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LLRoM
{
    public class AbilityXPGainExtension : DefModExtension
    {
        public List<ProficiencyDef> Proficiencies;
        public float LearnRate;
        public ExperienceType experienceType;
        public List<TraitDef> Classes;
        public List<ProficiencyDef> Relatedproficiencies
        {
            get
            {
                List<ProficiencyDef> temp = new List<ProficiencyDef>();
                foreach (ProficiencyDef pro in Proficiencies)
                {
                    temp.AddRange(pro.AllPrequisites);
                }
                temp.AddRange(Proficiencies);
                List<ProficiencyDef> outList = temp.Distinct().ToList();
                return outList;
            }
        }
    }
}