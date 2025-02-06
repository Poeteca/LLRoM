using HarmonyLib;
using LifeLessons;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using TorannMagic;
using UnityEngine;
using Verse;

namespace LLRoM
{
    [HarmonyPatch]
    public static class DrawnStatOffseterPatch
    {
        [HarmonyPatch(typeof(ProficiencyViewerWindow), "DrawStatModifiers")]
        public static class StatOffsetPatchPostFix
        {
            public static void Postfix(Rect displayRect, ref float __result, ProficiencyDef ___selectedProficiency)
            {
                if (___selectedProficiency.HasModExtension<StatOffsetExtension>() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ProficienciesMasterOffseter)
                {
                    StatOffsetExtension extension = ___selectedProficiency.GetModExtension<StatOffsetExtension>();
                    if (extension != null)
                    {
                        float CurrentY = __result;
                        if (extension.statOffseters != null && extension.statOffseters.Count > 0)
                        {
                            Rect statModLabelRect = new Rect(displayRect.x, displayRect.y + CurrentY, displayRect.width, 0f);
                            Widgets.LabelCacheHeight(ref statModLabelRect, "StatOffseterLable".Translate());
                            CurrentY += statModLabelRect.height;
                            TooltipHandler.TipRegion(statModLabelRect, "StatOffseterLableDesc".Translate());
                            foreach (StatOffseter Offset in  extension.statOffseters)
                            {
                                Rect statModRect = new Rect(statModLabelRect.x, statModLabelRect.y + statModLabelRect.height + (float)extension.statOffseters.IndexOf(Offset) * statModLabelRect.height, displayRect.width, statModLabelRect.height);
                                Widgets.Label(statModRect, Offset.Stat.LabelForFullStatListCap + " " + Offset.GetString());
                                CurrentY += statModRect.height;
                            }
                            CurrentY += 20f;
                        }
                        if (extension.statModifiers != null && extension.statModifiers.Count > 0)
                        {
                            Rect statModLabelRect = new Rect(displayRect.x, displayRect.y + CurrentY, displayRect.width, 0f);
                            Widgets.LabelCacheHeight(ref statModLabelRect, "StatModiferLable".Translate());
                            CurrentY += statModLabelRect.height;
                            TooltipHandler.TipRegion(statModLabelRect, "StatModiferLableDesc".Translate());
                            foreach (StatModifier modifier in extension.statModifiers)
                            {
                                Rect statModRect = new Rect(statModLabelRect.x, statModLabelRect.y + statModLabelRect.height + extension.statModifiers.IndexOf(modifier) * statModLabelRect.height, displayRect.width, statModLabelRect.height);
                                Widgets.Label(statModRect, modifier.Stat.LabelForFullStatListCap + " " + modifier.GetString());
                                CurrentY += statModRect.height;
                            }
                        }
                        __result = CurrentY;
                    }
                }
            }
        }
    }
    public static class SatModifierPatch
    {
        [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
        public static class StatModifierPatchPostFix
        {
            public static void Postfix(StatRequest req, ref float __result, StatDef ___stat)
            {
                if (req.Thing is Pawn pawn && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ProficienciesMasterOffseter)
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        foreach (ProficiencyDef pro in comp.CompletedProficiencies.Where((ProficiencyDef p) => p.HasModExtension<StatOffsetExtension>()))
                        {
                            StatOffsetExtension extension = pro.GetModExtension<StatOffsetExtension>();
                            if (extension != null && extension.statModifiers != null && extension.statModifiers.Count > 0)
                            {
                                foreach (StatModifier offseter in extension.statModifiers.Where((StatModifier s) => s.Stat == ___stat))
                                {
                                    __result *= offseter.value;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public static class StatOffseterPatch
    {
        [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetBaseValueFor))]
        public static class StatOffseterPatchPostFix
        {
            public static void Postfix(StatRequest request, ref float __result, StatDef ___stat)
            {
                if (request.Thing is Pawn pawn && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ProficienciesMasterOffseter)
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        foreach (ProficiencyDef pro in comp.CompletedProficiencies.Where((ProficiencyDef p) => p.HasModExtension<StatOffsetExtension>()))
                        {
                            StatOffsetExtension extension = pro.GetModExtension<StatOffsetExtension>();
                            if (extension != null && extension.statOffseters != null && extension.statOffseters.Count > 0)
                            {
                                foreach (StatOffseter offseter in extension.statOffseters.Where((StatOffseter s) => s.Stat == ___stat))
                                {
                                    __result += offseter.value;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public static class XPCombatGainPatch
    {
        [HarmonyPatch(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.Learn))]
        public static class XPCombatGainPatchPostFix
        {
            public static void Postfix(SkillDef sDef, float xp, Pawn_SkillTracker __instance)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                if (sDef == SkillDefOf.Shooting && pawn != null)
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (pawn != null && comp != null)
                    {
                        SkillRecord Shooting =__instance.GetSkill(SkillDefOf.Shooting);
                        comp.TryGainXp(xp*.001f, ProficiencyDefOf.LLRoM_Ranged, ExperienceType.PracticalHalfTheoretical);
                        if (Shooting.levelInt >= 7)
                        {
                            comp.TryGainXp(xp * .001f, ProficiencyDefOf.LLRoM_Extreme_Range, ExperienceType.PracticalHalfTheoretical);
                        }
                    }
                }
                if (sDef == SkillDefOf.Melee && pawn != null)
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    SkillRecord Melee = __instance.GetSkill(SkillDefOf.Melee);
                    if (pawn != null && comp != null)
                    {
                        comp.TryGainXp(xp * .001f, ProficiencyDefOf.LLRoM_CQC, ExperienceType.PracticalHalfTheoretical);
                        if (Melee.levelInt >= 7)
                        {
                            comp.TryGainXp(xp * .001f, ProficiencyDefOf.LLRoM_Defensive_Fighting, ExperienceType.PracticalHalfTheoretical);
                        }
                    }
                }
            }
        }
    }
    public static class PostDamageLearnPatch
    {
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        public static class PostDamageLearnPatchPostFix
        {
            public static void Postfix(float totalDamageDealt, Pawn __instance)
            {
                if (!__instance.Dead && __instance.HasComp<ProficiencyComp>())
                {
                    __instance.TryGetComp<ProficiencyComp>().TryGainXp(totalDamageDealt*.1f, ProficiencyDefOf.LLRoM_Physical_Conditioning, ExperienceType.PracticalHalfTheoretical);
                    __instance.TryGetComp<ProficiencyComp>().TryGainXp(totalDamageDealt*.1f, ProficiencyDefOf.LLRoM_Endurance, ExperienceType.PracticalHalfTheoretical);
                }
            }
        }
    }
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
                Pawn pawn = ((ProficiencyComp)(object)__instance).parent as Pawn;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout && !TM_Calc.IsWanderer(pawn) && !TM_Calc.IsWayfarer(pawn))
                {
                    LockoutExtension extension = def.GetModExtension<LockoutExtension>();
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
                    List<ProficiencyDef>  proficiencies = extension.Proficiencies;
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
                                    if (comp.AllLearnableProficiencies.Contains(item))
                                    {
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
                                    else
                                    {
                                        foreach (ProficiencyDef pro in item.AllPrequisites)
                                        {
                                            if (pro.tab != ProficiencyTableDefOf.LLROM_Might)
                                            {
                                                continue;
                                            }
                                            if (xp > 0)
                                            {
                                                comp.TryGainXp(xp, pro, extension.experienceType);
                                            }
                                            else
                                            {
                                                xp = LearnreateMod * extension.LearnRate;
                                                comp.TryGainXp(xp, pro, extension.experienceType);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (extension != null && proficiencies.Count == 0)
                    {
                        Log.Warning(__instance.Def.defName + " missing proficiencies");
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
                    List<ProficiencyDef> proficiencies = extension.Proficiencies;
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
                                    if (comp.AllLearnableProficiencies.Contains(item))
                                    {
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
                                    else
                                    {
                                        foreach (ProficiencyDef pro in item.AllPrequisites)
                                        {
                                            if (pro.category == ProficiencyCategoryDefOf.ProficiencyCategory_MixedMagic || pro.tab != ProficiencyTableDefOf.LLROM_Magic)
                                            {
                                                continue;
                                            }
                                            if (xp > 0)
                                            {
                                                comp.TryGainXp(xp, pro, extension.experienceType);
                                            }
                                            else
                                            {
                                                xp = LearnreateMod * extension.LearnRate;
                                                comp.TryGainXp(xp, pro, extension.experienceType);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (extension != null && proficiencies.Count == 0)
                    {
                        Log.Warning(__instance.Def.defName + " missing proficiencies");
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
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMagicClassLearning).Allowed(false))
                    {
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
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMightClassLearning).Allowed(false))
                    {
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
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictSpellLearning).Allowed(false))
                    {
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
                    if (!Util.Qualification(user, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictSkillLearning).Allowed(false))
                    {
                        Messages.Message("LLARoM_LearnMightMissingProficiencies".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                        return false;
                    }
                }
                return true;
            }
        }
    }
}