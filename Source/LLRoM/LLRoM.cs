using Verse;
using HarmonyLib;
using LifeLessons;
using RimWorld;
using UnityEngine;
using System;

namespace LLRoM
{
    public class LLROM : Mod
    {
        internal LLRoMSettings settings;
        public LLROM(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<LLRoMSettings>();
        }
        private static Vector2 scrollPosition = new Vector2(0f, 0f);
        private const float totalContentHeight = 720f;
        private const float ScrollBarWidthMargin = 18f;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            bool scrollBarVisible = totalContentHeight > inRect.height;
            var scrollViewTotal = new Rect(0f, 0f, inRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, scrollViewTotal);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(new Rect(0f, 0f, scrollViewTotal.width, 9999f));
            listingStandard.CheckboxLabeled("ProficienciesMasterOffseter".Translate(), ref settings.ProficienciesMasterOffseter);
            listingStandard.CheckboxLabeled("CostScale".Translate(), ref settings.CostScale);
            if (settings.CostScale)
            {
                string MaxCostScaleFactortext = (settings.MaxCostScaleFactor + 1).ToString("0.00");
                listingStandard.Label("MaxCostScaleFactor".Translate(MaxCostScaleFactortext));
                settings.MaxCostScaleFactor = listingStandard.Slider(settings.MaxCostScaleFactor, .01f, 3f);
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("ClassProLockout".Translate(), ref settings.ClassProLockout);
            if (settings.ClassProLockout)
            {
                listingStandard.CheckboxLabeled("UnlearnProOnClassGain".Translate(), ref settings.UnlearnProOnClassGain);
            }
            else
            {
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("ClassRequiresProficiencies".Translate(), ref settings.ClassRequiresProficiencies);
            if (settings.ClassRequiresProficiencies)
            { 
                listingStandard.CheckboxLabeled("StrictMightClassLearning".Translate(), ref settings.StrictMightClassLearning);
                listingStandard.CheckboxLabeled("StrictMagicClassLearning".Translate(), ref settings.StrictMagicClassLearning);
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("CastProRequirement".Translate(), ref settings.CastProRequirement);
            if (settings.CastProRequirement)
            {
                listingStandard.CheckboxLabeled("strictMightCastReuRequirement".Translate(), ref settings.strictMightCastReuRequirement);
                listingStandard.CheckboxLabeled("strictMagicCastReuRequirement".Translate(), ref settings.strictMagicCastReuRequirement);
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("AbilityRequiresProficiencies".Translate(), ref settings.AbilityRequiresProficiencies);
            if (settings.AbilityRequiresProficiencies)
            {
                listingStandard.CheckboxLabeled("StrictSkillLearning".Translate(), ref settings.StrictSkillLearning);
                listingStandard.CheckboxLabeled("StrictSpellLearning".Translate(), ref settings.StrictSpellLearning);
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("ObscureCertianProficiencies".Translate(), ref settings.ObscureCertianProficiencies);
            if (settings.ObscureCertianProficiencies)
            {
                listingStandard.CheckboxLabeled("ObscureAllProficiencies".Translate(), ref settings.ObscureAllProficiencies);
            }
            else
            {
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("learnBycastingSpells".Translate(), ref settings.learnBycastingSpells);
            if (settings.learnBycastingSpells)
            {
                listingStandard.Label("XPMultiplier".Translate(settings.XPMultiplier));
                settings.XPMultiplier = listingStandard.Slider(settings.XPMultiplier, 1f, 1000f);
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            if (settings.ClassRequiresProficiencies)
            {
                listingStandard.CheckboxLabeled("CanSelfTeachClasses".Translate(), ref settings.CanSelfTeachClasses);
                if (settings.CanSelfTeachClasses)
                {
                    listingStandard.CheckboxLabeled("CanOnlySelfTeachClasses".Translate(), ref settings.CanOnlySelfTeachClasses);
                }
                else
                {
                    listingStandard.Label("");
                }
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            if (settings.AbilityRequiresProficiencies)
            {
                listingStandard.CheckboxLabeled("CanSelfTeachSpells".Translate(), ref settings.CanSelfTeachSpells);
                if (settings.CanSelfTeachSpells)
                {
                    listingStandard.CheckboxLabeled("CanOnlySelfSpells".Translate(), ref settings.CanOnlySelfSpells);
                }
                else
                {
                    listingStandard.Label("");
                }
                listingStandard.CheckboxLabeled("CanSelfTeachSkills".Translate(), ref settings.CanSelfTeachSkills);
                if (settings.CanSelfTeachSkills)
                {
                    listingStandard.CheckboxLabeled("CanOnlySelfSkills".Translate(), ref settings.CanOnlySelfSkills);
                }
                else
                {
                    listingStandard.Label("");
                }
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
                listingStandard.Label("");
                listingStandard.Label("");
            }
            if (settings.CanSelfTeachSkills || settings.CanSelfTeachSpells || settings.CanSelfTeachClasses)
            {
                listingStandard.CheckboxLabeled("CanFailLearn".Translate(), ref settings.CanFailLearn);
            }
            else
            {
                listingStandard.Label("");
            }
            if (settings.CanSelfTeachSkills || settings.CanSelfTeachSpells || settings.CanSelfTeachClasses)
            {
                listingStandard.CheckboxLabeled("LearningDrain".Translate(), ref settings.LearningDrain);
            }
            else
            {
                listingStandard.Label("");
            }
            if (settings.ProficienciesMasterOffseter)
            {
                listingStandard.CheckboxLabeled("Inspiredepiphanies".Translate(), ref settings.Inspiredepiphanies);
            }
            else
            {
                listingStandard.Label("");
            }
            if (listingStandard.ButtonText("LLRoM_RestoreDefaults".Translate()))
            {
                settings.RestoreDefaults();
            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "LLRoM".Translate();
        }
    }
    [StaticConstructorOnStartup]
    public static class LLRoM
    {
        static LLRoM()
        {
            Harmony harmony = new Harmony("albinogod.LLRoM");
            harmony.PatchAll();
        }
    }
}