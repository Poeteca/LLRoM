using HarmonyLib;
using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TorannMagic;
using TorannMagic.ModOptions;
using Verse;

namespace LLRoM
{
    [HarmonyPatch]
    public static class TraitProficiencGenPatch
    {
        [HarmonyPatch(typeof(ProficiencyResolver), nameof(ProficiencyResolver.ResolveForPawn))]
        public static class TraitProficiencGenPatchPostfix
        {
            public static void Postfix(Pawn pawn, ProficiencyComp comp)
            {
                List<Trait> traits = pawn.story.traits.TraitsSorted;
                foreach (Trait trait in traits)
                {
                    TraitDef Def = trait.def;
                    BackstoryProficiencyExtension extension = Def.GetModExtension<BackstoryProficiencyExtension>();
                    if (extension != null)
                    {
                        if (!extension.templates.NullOrEmpty())
                        {
                            ProficiencyResolver.ResolveTemplates(pawn, comp, extension.templates, skipTests: true);
                        }
                        if (!extension.package.proficiencies.NullOrEmpty())
                        {
                            foreach (ProficiencyDef def in extension.package.proficiencies)
                            {
                                comp.TryGainProficiency(def, force: true);
                            }
                        }
                        if (!extension.package.conditionalProficiencies.NullOrEmpty())
                        {
                            ProficiencyResolver.ResolveConditionalRolls(extension.package.conditionalProficiencies, pawn, comp);
                        }
                        if (!extension.package.conditionalTemplates.NullOrEmpty())
                        {
                            ProficiencyResolver.ResolveConditionalRolls(extension.package.conditionalTemplates, pawn, comp);
                        }
                    }
                    ProficiencyResolver.ResolveTemplates(pawn, comp, DefDatabase<BackstoryProficiencyTemplateDef>.AllDefs.ToList());
                }
            }
        }
    }
    public static class HediffLockoutPatch
    {
        [HarmonyPatch(typeof(ProficiencyComp), nameof(ProficiencyComp.CanLearn))]
        public class HediffLockoutPrefix
        {
            public static bool Prefix(ProficiencyDef def, ProficiencyComp __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout)
                {
                    LockoutExtension extension = def.GetModExtension<LockoutExtension>();
                    Pawn pawn = ((ProficiencyComp)(object)__instance).parent as Pawn;
                    if (pawn != null && extension != null && !pawn.health.hediffSet.HasHediff(extension.withouHediff) && pawn.health.hediffSet.HasHediff(extension.hasHediff))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }      
    public static class Hide_Proficiency_Patch_DrawRelations
    {
        [HarmonyPatch(typeof(ProficiencyViewerWindow), "DrawRelations")]
        public class Hide_Proficiency_DrawRelations_PreFix
        {
            public static bool Prefix(ProficiencyDef def, ProficiencyViewerWindow __instance)
            {
                HideIfCantLearnExtension extension = def.GetModExtension<HideIfCantLearnExtension>();
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && extension != null && pawn != null && extension.HiddenIfCantLearn == true && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null && !Util.IsQualified(pawn, def))
                    {
                        return comp.CanLearn(def);
                    }
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureAllProficiencies && pawn != null && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null && !Util.IsQualified(pawn, def))
                    {
                        return comp.CanLearn(def);
                    }
                }
                return true;
            }
        }
    }
    public static class Hide_Proficiency_Patch_DrawGhostPrerequisites
    {
        [HarmonyPatch(typeof(ProficiencyViewerWindow), nameof(ProficiencyViewerWindow.DrawGhostPrerequisites))]
        public class Hide_Proficiency_DrawGhostPrerequisites_PreFix
        {
            public static bool Prefix(ProficiencyDef def, ProficiencyViewerWindow __instance)
            {

                HideIfCantLearnExtension extension = def.GetModExtension<HideIfCantLearnExtension>();
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && extension != null && pawn != null && extension.HiddenIfCantLearn == true && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyTabDef selectedTab = Traverse.Create(__instance).Field("selectedTab").GetValue() as ProficiencyTabDef;
                    List<ProficiencyDef> nonghostPrereqs = def.prerequisites.Where((ProficiencyDef p) => p.tab == selectedTab).ToList();
                    int hintflag = 0;
                    if (nonghostPrereqs.Count > 0)
                    {
                        foreach (ProficiencyDef item in nonghostPrereqs)
                        {
                            if (Util.IsQualified(pawn, item) || pawn.TryGetComp<ProficiencyComp>().CanLearn(item))
                            {
                                hintflag = 1;
                            }
                        }
                    }
                    if (hintflag == 0)
                    {
                        return false;
                    }
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureAllProficiencies && pawn != null && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyTabDef selectedTab = Traverse.Create(__instance).Field("selectedTab").GetValue() as ProficiencyTabDef;
                    List<ProficiencyDef> nonghostPrereqs = def.prerequisites.Where((ProficiencyDef p) => p.tab == selectedTab).ToList();
                    List<ProficiencyDef> ghostPrereqs = def.prerequisites.Where((ProficiencyDef p) => p.tab != selectedTab).ToList();
                    int hintflag = 0;
                    if (nonghostPrereqs.Count > 0)
                    {
                        foreach (ProficiencyDef item in nonghostPrereqs)
                        {
                            if (Util.IsQualified(pawn, item) || pawn.TryGetComp<ProficiencyComp>().CanLearn(item))
                            {
                                hintflag = 1;
                            }
                        }
                    }
                    if (ghostPrereqs.Count > 0)
                    {
                        foreach (ProficiencyDef item in ghostPrereqs)
                        {
                            if (Util.IsQualified(pawn, item) || pawn.TryGetComp<ProficiencyComp>().CanLearn(item))
                            {
                                hintflag = 1;
                            }
                        }
                    }
                    if (hintflag == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
    public static class Hide_Proficiency_Patch
    {
        [HarmonyPatch(typeof(ProficiencyViewerWindow), nameof(ProficiencyViewerWindow.DrawProficiencyCard))]
        public class Hide_Proficiency_PreFix
        {
            public static bool Prefix(ProficiencyDef def, ProficiencyViewerWindow __instance)
            {
                HideIfCantLearnExtension extension = def.GetModExtension<HideIfCantLearnExtension>();
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && extension != null && pawn != null && extension.HiddenIfCantLearn == true && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null && !Util.IsQualified(pawn, def))
                    {
                        return comp.CanLearn(def);
                    }
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureAllProficiencies && pawn != null && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null && !Util.IsQualified(pawn, def))
                    {
                        return comp.CanLearn(def);
                    }
                }
                return true;
            }
        }
    }
    public static class Skill_Proficiency_XP_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.MightAbility), nameof(TorannMagic.MightAbility.PostAbilityAttempt))]
        public class PostAbilityAttempt_Postfix
        {
            public static void Postfix(MightAbility __instance)
            {
                AbilityXPGainExtension extension = __instance.Def.GetModExtension<AbilityXPGainExtension>();
                if (extension != null)
                {
                    List<ProficiencyDef> proficiencies = new List<ProficiencyDef>();
                    proficiencies = extension.Proficiencies;
                    if (proficiencies != null && proficiencies.Count > 0)
                    {
                        foreach (ProficiencyDef item in proficiencies)
                        {
                            ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                            if (comp != null)
                            {
                                CompAbilityUserMight Stamina = __instance.Pawn.TryGetComp<CompAbilityUserMight>();
                                if (Stamina != null)
                                {
                                    float LearnreateMod = .01f * LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().XPMultiplier;
                                    float xp = LearnreateMod * extension.LearnRate * 10 * (__instance.mightDef.staminaCost + __instance.mightDef.upkeepEnergyCost + __instance.mightDef.upkeepRegenCost);
                                    if (xp > 0)
                                    {
                                        comp.TryGainXp(xp, item, extension.experienceType);
                                    }
                                    else
                                    {
                                        xp = LearnreateMod * extension.LearnRate;
                                        comp.TryGainXp(xp, item, extension.experienceType);
                                    }
                                }
                            }
                        }
                    }
                }
                return;
            }
        }
    }
    public static class Spell_Proficiency_XP_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.MagicAbility), nameof(TorannMagic.MagicAbility.PostAbilityAttempt))]
        public class PostAbilityAttempt_Postfix
        {
            public static void Postfix(MagicAbility __instance)
            {
                AbilityXPGainExtension extension = __instance.Def.GetModExtension<AbilityXPGainExtension>();
                if (extension != null)
                {
                    List<ProficiencyDef> proficiencies = new List<ProficiencyDef>();
                    proficiencies = extension.Proficiencies;
                    if (proficiencies != null && proficiencies.Count > 0)
                    {
                        foreach (ProficiencyDef item in proficiencies)
                        {
                            ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                            if (comp != null)
                            {
                                CompAbilityUserMagic mana = __instance.Pawn.TryGetComp<CompAbilityUserMagic>();
                                if (mana != null)
                                {
                                    float LearnreateMod = .01f * LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().XPMultiplier;
                                    float xp = LearnreateMod * extension.LearnRate * 10 * (__instance.magicDef.manaCost + __instance.magicDef.upkeepEnergyCost + __instance.magicDef.upkeepRegenCost);
                                    if (xp > 0)
                                    {
                                        comp.TryGainXp(xp, item, extension.experienceType);
                                    }
                                    else
                                    {
                                        xp = LearnreateMod * extension.LearnRate;
                                        comp.TryGainXp(xp, item, extension.experienceType);
                                    }
                                }
                            }
                        }
                    }
                }
                return;
            }
        }
    }
    public static class CompUseEffect_GemOfInsight_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_GemOfInsight), nameof(TorannMagic.CompUseEffect_GemOfInsight.DoEffect))]
        public class DoEffect_PretFix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfTeachClasses && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.SituationResolved);
                    return false;
                }
                CompAbilityUserMight compAbilityUserMight = user.GetCompAbilityUserMight();
                CompAbilityUserMagic compAbilityUserMagic = user.GetCompAbilityUserMagic();
                ProficiencyComp comp = user.TryGetComp<ProficiencyComp>();
                if (!compAbilityUserMagic.IsMagicUser && !compAbilityUserMight.IsMightUser && !user.story.traits.HasTrait(TorannMagicDefOf.TM_Gifted) && !user.story.traits.HasTrait(TorannMagicDefOf.PhysicalProdigy) && !user.story.traits.HasTrait(TorannMagicDefOf.Undead) && !user.IsShambler && !user.IsGhoul)
                {
                    GemOfInsightProficiencyExtension extension = ((ThingComp)(object)__instance).parent.def.GetModExtension<GemOfInsightProficiencyExtension>();
                    if (extension != null)
                    {
                        comp.TryGainProficiency(extension.Proficiency, true);
                        Messages.Message("LLAROM_GemOfInsight".Translate(user.LabelShort, extension.Proficiency.label, user.Named("USER")), user, MessageTypeDefOf.PositiveEvent);
                    }
                }
                return true;
            }
        }
    }
    public static class Can_Larn_Magic_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnMagic), nameof(TorannMagic.CompUseEffect_LearnMagic.DoEffect))]
        public class DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfTeachClasses && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.SituationResolved);
                    return false;
                }
                BillProficiencyExtension extension = ((ThingComp)(object)__instance).parent.def.GetModExtension<BillProficiencyExtension>();
                if (extension != null && extension.AnyRequirements() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
                {
                    List<ProficiencyDef> resolvedRequirements = extension.ResolvedRequirements();
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickMagicClassLearning).Allowed(false))
                    {
                        ProficiencyComp comp = user.TryGetComp<ProficiencyComp>();
                        Messages.Message("LLARoM_LearnMagicMissingProficiencies".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                        return false;
                    }
                }
                return true;
            }
        }
    }
    public static class CompUseEffect_LearnMight_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnMight), nameof(TorannMagic.CompUseEffect_LearnMight.DoEffect))]
        public class DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfTeachClasses && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.SituationResolved);
                    return false;
                }
                BillProficiencyExtension extension = ((ThingComp)(object)__instance).parent.def.GetModExtension<BillProficiencyExtension>();
                if (extension != null && extension.AnyRequirements() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
                {
                    List<ProficiencyDef> resolvedRequirements = extension.ResolvedRequirements();
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickMightClassLearning).Allowed(false))
                    {
                        ProficiencyComp comp = user.TryGetComp<ProficiencyComp>();
                        Messages.Message("LLARoM_LearnMightMissingProficiencies".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                        return false;
                    }
                }
                return true;
            }
        }
    }
    public static class CompUseEffect_LearnSpell_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnSpell), nameof(TorannMagic.CompUseEffect_LearnSpell.DoEffect))]
        public class DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfSpells && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachSpells)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.SituationResolved);
                    return false;
                }
                BillProficiencyExtension extension = ((ThingComp)(object)__instance).parent.def.GetModExtension<BillProficiencyExtension>();
                if (extension != null && extension.AnyRequirements() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
                {
                    List<ProficiencyDef> resolvedRequirements = extension.ResolvedRequirements();
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickSpellLearning).Allowed(false))
                    {
                        ProficiencyComp comp = user.TryGetComp<ProficiencyComp>();
                        Messages.Message("LLARoM_LearnSpellMissingProficiencies".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                        return false;
                    }
                }
                return true;
            }
        }
    }
    public static class CompUseEffect_LearnSkill_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnSkill), nameof(TorannMagic.CompUseEffect_LearnSkill.DoEffect))]
        public class DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachSkills && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfSkills)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.SituationResolved);
                    return false;
                }
                BillProficiencyExtension extension = ((ThingComp)(object)__instance).parent.def.GetModExtension<BillProficiencyExtension>();
                if (extension != null && extension.AnyRequirements() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
                {
                    List<ProficiencyDef> resolvedRequirements = extension.ResolvedRequirements();
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickSkillLearning).Allowed(false))
                    {
                        ProficiencyComp comp = user.TryGetComp<ProficiencyComp>();
                        Messages.Message("LLARoM_LearnMightMissingProficiencies".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
