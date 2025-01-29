using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class LLRoMSettings : ModSettings
    {
        public bool StrickMightClassLearning = false;
        public bool StrickMagicClassLearning = false;
        public bool StrickSpellLearning = false;
        public bool StrickSkillLearning = false;
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
        public override void ExposeData()
        {
            Scribe_Values.Look(ref StrickMightClassLearning, "StrickMightClassLearning");
            Scribe_Values.Look(ref StrickMagicClassLearning, "StrickMagicClassLearning");
            Scribe_Values.Look(ref StrickSpellLearning, "StrickSpellClassLearning");
            Scribe_Values.Look(ref StrickSkillLearning, "StrickSkillClassLearning");
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
            base.ExposeData();
        }
    }
}
