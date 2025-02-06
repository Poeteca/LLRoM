using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace LLRoM
{
    public class ClassAutoLearnExtension : DefModExtension
    {
        public List<TraitDef> Relatedtraits;
        public List<ProficiencyDef> RequiredProficiencies;
        public List<TraitDef> RequiredTrait;
        public List<HediffDef> BlockingHediffs;
        public List<TraitDef> BlockingTraits;
        public HediffDef appliedHediff;
        public String Prefix = "";
        public bool AllRequiredTraitsNeeded = false;
        public Gender GenderRequirment = Gender.None;
        public bool Strict = false;
        public bool Magic = false;
        public bool Might = false;
        public int failChance = 0;
    }
}
