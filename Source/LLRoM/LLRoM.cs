using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using HarmonyLib;
using TorannMagic;
using AbilityUser;
using LifeLessons;
using TorannMagic.Ideology;
using TorannMagic.ModOptions;
using RimWorld;
using Verse.AI;
using LifeLessons.Patches;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using static HarmonyLib.Code;
using System.Security.Policy;
using UnityEngine;

namespace LLRoM
{
    public class LLROM : Mod
    {
        LLRoMSettings settings;
        public LLROM(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<LLRoMSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("ClassProLockout".Translate(), ref settings.ClassProLockout);
            listingStandard.CheckboxLabeled("ClassRequiresProficiencies".Translate(), ref settings.ClassRequiresProficiencies);
            if (settings.ClassRequiresProficiencies)
            { 
                listingStandard.CheckboxLabeled("StrickMightClassLearning".Translate(), ref settings.StrickMightClassLearning);
                listingStandard.CheckboxLabeled("StrickMagicClassLearning".Translate(), ref settings.StrickMagicClassLearning);
            }
            else
            {
                listingStandard.Label("");
                listingStandard.Label("");
            }
            listingStandard.CheckboxLabeled("AbilityRequiresProficiencies".Translate(), ref settings.AbilityRequiresProficiencies);
            if (settings.AbilityRequiresProficiencies)
            {
                listingStandard.CheckboxLabeled("StrickSkillLearning".Translate(), ref settings.StrickSkillLearning);
                listingStandard.CheckboxLabeled("StrickSpellLearning".Translate(), ref settings.StrickSpellLearning);
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
            listingStandard.Label("XPMultiplier".Translate());
            listingStandard.Label("ValueXPMultiplier".Translate(settings.XPMultiplier));
            settings.XPMultiplier = listingStandard.Slider(settings.XPMultiplier, 1f, 1000f);
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
            listingStandard.End();
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
