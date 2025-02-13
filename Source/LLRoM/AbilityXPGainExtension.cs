﻿using LifeLessons;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LLRoM
{
    public class AbilityXPGainExtension : DefModExtension
    {
        public List<ProficiencyDef> Proficiencies;
        public float LearnRate;
        public ExperienceType experienceType;
        public List<TraitDef> Classes;
    }
}