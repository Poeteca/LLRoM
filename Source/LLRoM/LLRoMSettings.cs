﻿using Verse;

namespace LLRoM
{
    public class LLRoMSettings : ModSettings
    {
        public bool StrictMightClassLearning = false;
        public bool StrictMagicClassLearning = false;
        public bool StrictSpellLearning = false;
        public bool StrictSkillLearning = false;
        public bool ObscureCertianProficiencies = true;
        public bool ObscureAllProficiencies = false;
        public bool ClassRequiresProficiencies = true;
        public bool AbilityRequiresProficiencies = true;
        public float XPMultiplier = 100f;
        public bool CanSelfTeachClasses = true;
        public bool CanOnlySelfTeachClasses = false;
        public bool CanFailLearn = true;
        public bool CanSelfTeachSpells = true;
        public bool CanOnlySelfSpells = false;
        public bool CanSelfTeachSkills = true;
        public bool CanOnlySelfSkills = false;
        public bool ClassProLockout = true;
        public bool UnlearnProOnClassGain = true;
        public bool ProficienciesMasterOffseter = true;
        public bool CostScale = true;
        public bool CastProRequirement = false;
        public bool strictMightCastReuRequirement = false;
        public bool strictMagicCastReuRequirement = false;
        public bool learnBycastingSpells = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref learnBycastingSpells, "learnBycastingSpells");
            Scribe_Values.Look(ref CostScale, "CostScale");
            Scribe_Values.Look(ref CastProRequirement, "CastProRequirement");
            Scribe_Values.Look(ref strictMagicCastReuRequirement, "strictMagicCastReuRequirement");
            Scribe_Values.Look(ref strictMagicCastReuRequirement, "strictMagicCastReuRequirement");
            Scribe_Values.Look(ref StrictMightClassLearning, "StrictMightClassLearning");
            Scribe_Values.Look(ref StrictMagicClassLearning, "StrictMagicClassLearning");
            Scribe_Values.Look(ref StrictMightClassLearning, "StrictMightClassLearning");
            Scribe_Values.Look(ref StrictMightClassLearning, "StrictMightClassLearning");
            Scribe_Values.Look(ref ObscureCertianProficiencies, "ObscureCertianProficiencies");
            Scribe_Values.Look(ref ObscureAllProficiencies, "ObscureAllProficiencies");
            Scribe_Values.Look(ref ClassRequiresProficiencies, "ClassRequiresProficiencies");
            Scribe_Values.Look(ref AbilityRequiresProficiencies, "AbilityRequiresProficiencies");
            Scribe_Values.Look(ref XPMultiplier, "XPMultiplier", 100f);
            Scribe_Values.Look(ref CanFailLearn, "CanFailLearn");
            Scribe_Values.Look(ref CanSelfTeachClasses, "CanSelfTeachClasses");
            Scribe_Values.Look(ref CanSelfTeachSpells, "CanSelfTeachSpells");
            Scribe_Values.Look(ref CanOnlySelfSpells, "CanOnlySelfSpells");
            Scribe_Values.Look(ref CanSelfTeachSkills, "CanSelfTeachSkills");
            Scribe_Values.Look(ref CanOnlySelfSkills, "CanOnlySelfSkills");
            Scribe_Values.Look(ref ClassProLockout, "ClassProLockout");
            Scribe_Values.Look(ref UnlearnProOnClassGain, "UnlearnProOnClassGain");
            Scribe_Values.Look(ref ProficienciesMasterOffseter, "ProficienciesMasterOffseter");
            base.ExposeData();
        }
    }
}