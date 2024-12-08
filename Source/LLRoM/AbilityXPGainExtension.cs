using LifeLessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class AbilityXPGainExtension : DefModExtension
    {
        public List<ProficiencyDef> Proficiencies;
        public float LearnRate;
        public ExperienceType experienceType;
    }
}
