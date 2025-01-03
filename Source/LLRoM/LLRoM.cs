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
            listingStandard.CheckboxLabeled("ClassRequiresProficiencies".Translate(), ref settings.ClassRequiresProficiencies);
            if (settings.ClassRequiresProficiencies)
            { 
                listingStandard.CheckboxLabeled("StrickMightClassLearning".Translate(), ref settings.StrickMightClassLearning);
                listingStandard.CheckboxLabeled("StrickMagicClassLearning".Translate(), ref settings.StrickMagicClassLearning);
            }
            listingStandard.CheckboxLabeled("AbilityRequiresProficiencies".Translate(), ref settings.AbilityRequiresProficiencies);
            if (settings.AbilityRequiresProficiencies)
            {
                listingStandard.CheckboxLabeled("StrickSkillLearning".Translate(), ref settings.StrickSkillLearning);
                listingStandard.CheckboxLabeled("StrickSpellLearning".Translate(), ref settings.StrickSpellLearning);
            }
            listingStandard.CheckboxLabeled("ObscureCertianProficiencies".Translate(), ref settings.ObscureCertianProficiencies);
            if (settings.ObscureCertianProficiencies)
            {
                listingStandard.CheckboxLabeled("ObscureAllProficiencies".Translate(), ref settings.ObscureAllProficiencies);
            }
            listingStandard.Label("XPMultiplier".Translate());
            listingStandard.Label("ValueXPMultiplier".Translate(settings.XPMultiplier));
            settings.XPMultiplier = listingStandard.Slider(settings.XPMultiplier, 1f, 1000f);
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
