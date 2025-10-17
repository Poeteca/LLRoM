using AbilityUser;
using HarmonyLib;
using LifeLessons;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using TorannMagic;
using TorannMagic.ModOptions;
using TorannMagic.TMDefs;
using TorannMagic.Utils;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static HarmonyLib.Code;
using static UnityEngine.Scripting.GarbageCollector;

namespace LLRoM
{
    [HarmonyPatch]
    public static class ConstructDetectionPatch
    {
        [HarmonyPatch(typeof(Util), nameof(Util.MechanicalLike))]
        public static void Postfix(ThingDef def, ref bool __result)
        {
            if (!__result && def.race.thinkTreeMain.defName == "TM_GolemMain" || def.race.thinkTreeMain.defName == "TM_GolemConstant")
            {
                __result = true;
            }
        }
    }
    public static class RandomClassPatch
    {
        [HarmonyPatch(typeof(TM_Calc), nameof(TM_Calc.GetRandomAcceptableMagicClassIndex))]
        public static class RandomPostChanger
        {
            public static void Postfix(Pawn p, ref int __result)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
                {
                    if (Utility.CanLearnClass(p, TM_Data.EnabledMagicTraits[__result]))
                    {
                        return;
                    }
                    List<TraitDef> classes = new List<TraitDef>();
                    foreach (TraitDef td in DefDatabase<TraitDef>.AllDefs.Where((TraitDef t) => Utility.CanLearnClass(p, t) && TM_Data.EnabledMagicTraits.Contains(t)))
                    {
                        classes.Add(td);
                    }
                    if (classes.Count == 0) { return; }
                    TraitDef ranClass = classes.RandomElement();
                    int outIndex = TM_Data.EnabledMagicTraits.IndexOf(ranClass);
                    if (outIndex == -1) { return; }
                    __result = outIndex;
                }
            }
        }
    }
    public static class AdjsutCommonality
    {
        [HarmonyPatch(typeof(InspirationWorker), nameof(InspirationWorker.CommonalityFor))]
        public static class ExtenderInspirationChecher
        {
            public static void Postfix(Pawn pawn, ref float __result, InspirationWorker __instance)
            {
                float adjuster = 1f;
                InspirationExtension extension = __instance.def.GetModExtension<InspirationExtension>();
                if (__instance.def == TorannMagicDefOf.ID_ArcanePathways && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
                {
                    List<TraitDef> classes = new List<TraitDef>();
                    foreach (TraitDef td in DefDatabase<TraitDef>.AllDefs.Where((TraitDef t) => Utility.CanLearnClass(pawn, t) && TM_Data.EnabledMagicTraits.Contains(t)))
                    {
                        classes.Add(td);
                    }
                    if (classes.Count == 0)
                    {
                        adjuster = 0f;
                    }
                }
                if (extension != null)
                {
                    if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().Inspiredepiphanies || !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ProficienciesMasterOffseter)
                    {
                        adjuster = 0f;
                    }
                    if (extension.hediff != null)
                    {
                        if (!pawn.health.hediffSet.HasHediff(extension.hediff))
                        {
                            adjuster = 0f;
                        }
                    }
                    if (extension.requiredproficiencies != null && extension.requiredproficiencies.Count > 0)
                    {
                        foreach (ProficiencyDef pro in extension.requiredproficiencies)
                        {
                            if (!Util.IsQualified(pawn, pro))
                            {
                                adjuster = 0f;
                            }
                        }
                    }
                    if (extension.relatedproficiencies != null)
                    {
                        float knownRelated = 0f;
                        foreach (ProficiencyDef related in extension.relatedproficiencies)
                        {
                            if (Util.IsQualified(pawn, related))
                            {
                                knownRelated++;
                            }
                        }
                        if (knownRelated < extension.relatedCountNeeded)
                        {
                            adjuster = 0f;
                        }
                    }
                    if (extension.blockingcategory != null)
                    {
                        if (Utility.KnownInCatagory(pawn, extension.blockingcategory).Count > 0)
                        {
                            adjuster = 0f;
                        }
                    }
                }
                __result *= adjuster;
            }
        }
    }
    public static class ConsumNeed
    {
        [HarmonyPatch(typeof(Util), nameof(Util.LearnFromActivity))]
        public static class ConsumeNeededNeed
        {
            public static void Postfix(LearningActivity activity, Pawn pawn)
            {
                NeedActivityConsumerExtension extension = activity.def.GetModExtension<NeedActivityConsumerExtension>();
                if (extension != null)
                {
                    if (extension.magicPro && TM_Calc.IsMagicUser(pawn))
                    {
                        CompAbilityUserMagic compAbilityUserMagic = pawn.GetCompAbilityUserMagic();
                        compAbilityUserMagic.Mana.UseMagicPower(extension.amount);
                        compAbilityUserMagic.MagicUserXP += Mathf.Clamp((int)(extension.amount * 150f * compAbilityUserMagic.xpGain * Settings.Instance.xpMultiplier), 0, 9999);
                    }
                    else if (extension.mightPro && TM_Calc.IsMightUser(pawn))
                    {
                        CompAbilityUserMight compAbilityUseright = pawn.GetCompAbilityUserMight();
                        compAbilityUseright.Stamina.UseMightPower(extension.amount);
                        compAbilityUseright.MightUserXP += Mathf.Clamp((int)(extension.amount * 150f * compAbilityUseright.xpGain * Settings.Instance.xpMultiplier), 0, 9999);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(LearningActivity), nameof(LearningActivity.CanStartNow))]
        public static class HasNeededNeed
        {
            public static void Postfix(ref bool __result, Pawn pawn, LearningActivity __instance)
            {
                if (!__result) { return; }
                NeedActivityConsumerExtension extension = __instance.def.GetModExtension<NeedActivityConsumerExtension>();
                if (extension != null)
                {
                    if (extension.magicPro && TM_Calc.IsMagicUser(pawn))
                    {
                        CompAbilityUserMagic compAbilityUserMagic = pawn.GetCompAbilityUserMagic();
                        if (extension.amount > compAbilityUserMagic.Mana.CurLevel)
                        {
                            __result = false;
                        }
                    }
                    else if (extension.mightPro && TM_Calc.IsMightUser(pawn))
                    {
                        CompAbilityUserMight compAbilityUseright = pawn.GetCompAbilityUserMight();
                        if (extension.amount > compAbilityUseright.Stamina.CurLevel)
                        {
                            __result = false;
                        }
                    }
                    else if ((extension.mightPro && !TM_Calc.IsMagicUser(pawn)) || (extension.mightPro && !TM_Calc.IsMightUser(pawn)))
                    {
                        __result = false;
                    }
                }
            }
        }
    }
    public static class MagicBillCheck
    {
        [HarmonyPatch(typeof(Building_TMMagicCircleBase), nameof(Building_TMMagicCircleBase.CanEverDoBill))]
        public static class MagicbBillProficiencyCheck
        {
            public static void Postfix(Bill bill, ref List<Pawn> pawnsAble, MagicRecipeDef mrDefIn, ref bool __result)
            {
                if (!__result) { return; }
                MagicRecipeDef magicRecipeDef = null;
                if (bill != null)
                {
                    magicRecipeDef = bill.recipe as MagicRecipeDef;
                }
                if (mrDefIn != null)
                {
                    magicRecipeDef = mrDefIn;
                }
                List<Pawn> pawnscapable = new List<Pawn>();
                foreach (Pawn pawn in pawnsAble)
                {
                    BillProficiencyExtension extension = Util.GetExtensionFromRecipeDef(bill.recipe);
                    if (extension != null && extension.AnyRequirements())
                    {
                        if (bill.recipe.productHasIngredientStuff && !extension.StuffRequirements.NullOrEmpty())
                        {
                            List<StuffCategoryDef> stuffCats = Util.GetStuffCategoriesEnabled(bill);
                            foreach (StuffRequirements req in extension.StuffRequirements.Where((StuffRequirements sr) => stuffCats.Contains(sr.stuffCategory)))
                            {
                                if (!Util.Qualification(pawn, req.requirements, extension.IsHardRequirement).Allowed(extension.IsHardRequirement))
                                {
                                    BillContext.LastMissingStuffRequirements.Add(req);
                                }
                            }
                        }
                        List<ProficiencyDef> resolvedRequirements = extension.ResolvedRequirements();
                        if (!Util.Qualification(pawn, resolvedRequirements, extension.IsHardRequirement).Allowed(extension.IsHardRequirement))
                        {
                            ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                            JobFailReason.Is(comp.UnqualifiedJobMessage(resolvedRequirements, extension.IsHardRequirement), bill.Label);
                            continue;
                        }
                        else { pawnscapable.Add(pawn); }
                    }
                }
                if (pawnscapable.Count < magicRecipeDef.mageCount) { __result = false; }
            }
        }
    }
    public static class EnableChecks
    {
        [HarmonyPatch(typeof(CompUseEffect_LearnMagic),nameof(CompUseEffect_LearnMagic.DoEffect))]
        public static class BlockClassCheck
        {
            public static bool Prefix(CompUseEffect_LearnMagic __instance)
            {
                ThingDef book = ((ThingComp)(object)__instance).parent.def;
                bool Out = Utility.EnabledBook(book);
                if (!Out) { Messages.Message("LLRoMDisableInOptions".Translate(), MessageTypeDefOf.RejectInput); }
                return Out;
            }
        }
    }
    public static class RemoveClassPatch
    {
        [HarmonyPatch(typeof(TM_DebugTools), nameof(TM_DebugTools.RemoveClass))]
        public static class RemoveProficienciesPatch
        {
            public static void Postfix(Pawn pawn)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
                {
                    pawn.GetComp<ProficiencyComp>().RemoveProficiency(ProficiencyDefOf.Physical_Insight, true);
                    pawn.GetComp<ProficiencyComp>().RemoveProficiency(ProficiencyDefOf.Magic_Insight, true);
                }
            }
        }
    }
    public static class PowerUpgradeCheckPatch
    {
        [HarmonyPatch(typeof(CompAbilityUserMight), nameof(CompAbilityUserMight.LevelUpPower))]
        public static class MightPowerLevelUpPrefixPatch
        {
            public static bool Prefix(MightPower power, CompAbilityUserMight __instance)
            {
                int indexToCheck = power.level + 1;
                AbilityUser.AbilityDef abilityToCheck = power.GetAbilityDef(indexToCheck);
                AbilityXPGainExtension extension = abilityToCheck.GetModExtension<AbilityXPGainExtension>();
                if (extension != null && __instance.Pawn != null && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
                {
                    ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        List<ProficiencyDef> proficienciesToCompare = comp.CompletedProficiencies;
                        if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictSkillLearning)
                        {
                            proficienciesToCompare.AddRange(comp.AllLearnableProficiencies);
                        }
                        foreach (ProficiencyDef pro in extension.Proficiencies)
                        {
                            if (!proficienciesToCompare.Contains(pro))
                            {
                                Messages.Message("CantLevelPower".Translate(__instance.Pawn.LabelShort), MessageTypeDefOf.RejectInput);
                                __instance.MightData.MightAbilityPoints += power.costToLevel;
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(CompAbilityUserMagic), nameof(CompAbilityUserMagic.LevelUpPower))]
        public static class MagicPowerLevelUpPrefixPatch
        {
            public static bool Prefix(MagicPower power, CompAbilityUserMagic __instance)
            {
                int indexToCheck = power.level + 1;
                AbilityUser.AbilityDef abilityToCheck = power.GetAbilityDef(indexToCheck);
                AbilityXPGainExtension extension = abilityToCheck.GetModExtension<AbilityXPGainExtension>();
                if (extension != null && __instance.Pawn != null && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
                {
                    ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        List<ProficiencyDef> proficienciesToCompare = comp.CompletedProficiencies;
                        if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictSpellLearning)
                        {
                            proficienciesToCompare.AddRange(comp.AllLearnableProficiencies);
                        }
                        foreach (ProficiencyDef pro in extension.Proficiencies)
                        {
                            if (!proficienciesToCompare.Contains(pro))
                            {
                                Messages.Message("CantLevelPower".Translate(__instance.Pawn.LabelShort), MessageTypeDefOf.RejectInput);
                                __instance.MagicData.MagicAbilityPoints += power.costToLevel;
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
    public static class CastRequirementsPatch
    {
        [HarmonyPatch(typeof(MightAbility), nameof(MightAbility.CanCastPowerCheck))]
        public static class CastRequirementsMightPatchPostFix
        {
            public static void Postfix(ref bool __result, ref string reason, MightAbility __instance)
            {
                if (__result && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CastProRequirement)
                {
                    ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        List<ProficiencyDef> pro = comp.CompletedProficiencies;
                        if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().strictMightCastReuRequirement)
                        {
                            pro.AddRange(comp.AllLearnableProficiencies);
                        }
                        AbilityXPGainExtension extension = __instance.Def.GetModExtension<AbilityXPGainExtension>();
                        if (extension != null)
                        {
                            List<ProficiencyDef> missingpro = new List<ProficiencyDef>();
                            foreach (ProficiencyDef def in extension.Proficiencies)
                            {
                                if (!pro.Contains(def))
                                {
                                    missingpro.Add(def);
                                    __result = false;
                                }
                            }
                            if (missingpro.Count > 0)
                            {
                                StringBuilder Reason = new StringBuilder();
                                Reason.AppendLine("MissingPro".Translate());
                                foreach (ProficiencyDef def in missingpro)
                                {
                                    Reason.AppendLine(def.defName);
                                }
                                reason = Reason.ToString();
                            }
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(MagicAbility), nameof(MagicAbility.CanCastPowerCheck))]
        public static class CastRequirementsMagicPatchPostFix
        {
            public static void Postfix(ref bool __result, ref string reason, MagicAbility __instance)
            {
                if (__result && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CastProRequirement)
                {
                    ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        List<ProficiencyDef> pro = comp.CompletedProficiencies;
                        if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().strictMagicCastReuRequirement)
                        {
                            pro.AddRange(comp.AllLearnableProficiencies);
                        }
                        AbilityXPGainExtension extension = __instance.Def.GetModExtension<AbilityXPGainExtension>();
                        if (extension != null)
                        {
                            List<ProficiencyDef> missingpro = new List<ProficiencyDef>();
                            foreach (ProficiencyDef def in extension.Proficiencies)
                            {
                                if (!pro.Contains(def))
                                {
                                    missingpro.Add(def);
                                    __result = false;
                                }
                            }
                            if (missingpro.Count > 0)
                            {
                                StringBuilder Reason = new StringBuilder();
                                Reason.AppendLine("MissingPro".Translate());
                                foreach (ProficiencyDef def in missingpro)
                                {
                                    Reason.AppendLine(def.defName);
                                }
                                reason = Reason.ToString();
                            }
                        }
                    }
                }
            }
        }
    }
    public static class DrawAbilitiesInWIndowPatch
    {
        public static bool ShouldShw(Pawn p, TMAbilityDef ability)
        {
            CompAbilityUserMagic pMagic = p.GetCompAbilityUserMagic();
            CompAbilityUserMight pMight = p.GetCompAbilityUserMight();
            TM_CustomClass customclass = null;
            if (pMagic.customClass != null) { customclass = pMagic.customClass; }
            else if (pMight.customClass != null) { customclass = pMight.customClass; }
            if (p == null || ability == null) { return false; }
            if (Utility.FullyKnowsAbility(p, ability)) { return false; }
            if (ability == TorannMagicDefOf.TM_LivingWall)
            {
                AbilityXPGainExtension Livingextention = TorannMagicDefOf.TM_LivingWall.GetModExtension<AbilityXPGainExtension>();
                if (customclass == null)
                {
                    foreach (TraitDef trait in Livingextention.Classes)
                    {
                        if (p.story.traits.HasTrait(trait)) { return true; }
                    }
                    return false;
                }
            }
            if (ability.learnItem != null && customclass != null)
            {
                if (ability.learnItem.defName.Contains("SpellOf") && customclass.isMage && Utility.LearnableSpellCheck(p, ability.learnItem) && TM_Calc.IsMagicUser(p))
                {
                    return true;
                }
                else if (ability.learnItem.defName.Contains("SkillOf") && customclass.isFighter && Utility.LearnableSkillCheck(p, ability.learnItem) && TM_Calc.IsMightUser(p))
                {
                    return true;
                }
            }
            else if (ability.learnItem != null)
            {
                if (pMagic.MagicData != null && pMagic.IsMagicUser && ability.learnItem.defName.Contains("SpellOf") && Utility.LearnableSpellCheck(p, ability.learnItem) && TM_Calc.IsMagicUser(p))
                {
                    return true;
                }
                else if (pMight.MightData != null && pMight.IsMightUser && ability.learnItem.defName.Contains("SkillOf") && Utility.LearnableSkillCheck(p, ability.learnItem) && TM_Calc.IsMightUser(p))
                {
                    return true;
                }
            }
            else
            {
                if (customclass != null)
                {
                    if (customclass.classAbilities.Contains(ability))
                    {
                        return true;
                    }
                }
                AbilityXPGainExtension extension = ability.GetModExtension<AbilityXPGainExtension>();
                if (extension != null && extension.Classes != null && extension.Classes.Count > 0)
                {
                    foreach (TraitDef Class in extension.Classes)
                    {
                        if (p.story.traits.HasTrait(Class))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        [HarmonyPatch(nameof(ProficiencyViewerWindow), "DrawUsedBy")]
        public static class DrawUsedByPostfix
        {
            public static void Postfix(ref float __result, Rect displayRect, ProficiencyDef ___selectedProficiency, ProficiencyViewerWindow __instance)
            {
                bool flag = true;
                if (___selectedProficiency.tab != ProficiencyTableDefOf.LLROM_Might && ___selectedProficiency.tab != ProficiencyTableDefOf.LLROM_Magic) { flag = false; }
                if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CastProRequirement && !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().learnBycastingSpells) { flag = false; }
                float currentY = __result;
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                if (___selectedProficiency.tab == ProficiencyTableDefOf.LLROM_Magic && !pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MagicUserHD) && (!DebugSettings.godMode || Current.Game.InitData != null)) { flag = false; }
                if (___selectedProficiency.tab == ProficiencyTableDefOf.LLROM_Might && !pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MightUserHD) && (!DebugSettings.godMode || Current.Game.InitData != null)) { flag = false; }
                int iconWidth = 34;
                int iconHeight = 34;
                int maxRowIconCount = ((int)displayRect.width - 10) / iconWidth;
                List<TMAbilityDef> abilities = new List<TMAbilityDef>();
                foreach (TMAbilityDef ability in DefDatabase<TMAbilityDef>.AllDefs)
                {
                    AbilityXPGainExtension extension = ability.GetModExtension<AbilityXPGainExtension>();
                    if (extension != null && extension.Proficiencies.Contains(___selectedProficiency) && ShouldShw(pawn, ability) && extension.LearnRate != 0 && ability.uiIcon != null)
                    {
                        abilities.Add(ability);
                    }
                }
                if (flag && abilities != null && abilities.Count > 0)
                {
                    Rect requiredByabilityRect = new Rect(displayRect.x, displayRect.y + currentY, displayRect.width, 0f);
                    Widgets.LabelCacheHeight(ref requiredByabilityRect, "RelatedSpells".Translate());
                    currentY += requiredByabilityRect.height;
                    GenUI.ResetLabelAlign();
                    int i = 0;
                    int j = 0;
                    for (int abilityCounter = 0; abilityCounter < abilities.Count; abilityCounter++)
                    {
                        TMAbilityDef ability = abilities[abilityCounter];
                        if (i != 0 && i % maxRowIconCount == 0)
                        {
                            j++;
                            i = 0;
                        }
                        if (i == 0)
                        {
                            currentY += (float)iconHeight;
                        }
                        Rect abilityRect = new Rect(requiredByabilityRect.x + (float)(i % maxRowIconCount * iconWidth), requiredByabilityRect.y + requiredByabilityRect.height + (float)(j * iconHeight), iconWidth, iconHeight);
                        string label = ability.LabelCap;
                        List<ProficiencyCategoryDef> missingTypes = Utility.GetMissingAbilityRequirements(ability, pawn);
                        if (missingTypes.Count > 0)
                        {
                            label += "MissingCat".Translate();
                            int total = missingTypes.Count;
                            for (int smallCount = 0; smallCount < total; smallCount++)
                            {
                                ProficiencyCategoryDef type = missingTypes[smallCount];
                                if (smallCount + 1 == total)
                                {
                                    label += type.LabelCap;
                                }
                                else
                                {
                                    label += type.LabelCap + ", ";
                                }
                            }
                        }
                        TooltipHandler.TipRegion(abilityRect, label);
                        Widgets.DrawTextureFitted(abilityRect, ability.uiIcon, 1);
                        i++;
                    }
                }
                ClassAutoLearnExtension extension2 = ___selectedProficiency.GetModExtension<ClassAutoLearnExtension>();
                List<TraitDef> Classes = new List<TraitDef>();
                bool flag2 = true;
                if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses || !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
                {
                    flag2 = false;
                }
                if (extension2 != null && flag2)
                {
                    foreach (TraitDef possibleClass in extension2.Relatedtraits)
                    {
                        if (possibleClass == TorannMagicDefOf.TM_Gifted || possibleClass == TorannMagicDefOf.PhysicalProdigy)
                        {
                            continue;
                        }
                        if (!Utility.EnabledClass(possibleClass))
                        {
                            continue;
                        }
                        ClassAutoLearnExtension extension3 = possibleClass.GetModExtension<ClassAutoLearnExtension>();
                        if (extension3 != null)
                        {
                            if (pawn.story.traits.HasTrait(TorannMagicDefOf.TM_Gifted) && extension3.Magic == true)
                            {
                                if (extension3.GenderRequirment != Gender.None)
                                {
                                    if (pawn.gender == extension3.GenderRequirment)
                                    {
                                        Classes.Add(possibleClass);
                                    }
                                    if (pawn.gender == Gender.None)
                                    {
                                        Classes.Add(possibleClass);
                                    }
                                }
                                if (extension3.GenderRequirment == Gender.None)
                                {
                                    Classes.Add(possibleClass);
                                }
                            }
                            if (pawn.story.traits.HasTrait(TorannMagicDefOf.PhysicalProdigy) && extension3.Might == true)
                            {
                                if (extension3.GenderRequirment != Gender.None)
                                {
                                    if (pawn.gender == extension3.GenderRequirment)
                                    {
                                        Classes.Add(possibleClass);
                                    }
                                    if (pawn.gender == Gender.None)
                                    {
                                        Classes.Add(possibleClass);
                                    }
                                }
                                if (extension3.GenderRequirment == Gender.None)
                                {
                                    Classes.Add(possibleClass);
                                }
                            }
                        }
                    }
                    if (Classes.Count > 0)
                    {
                        Rect requiredByclassRect = new Rect(displayRect.x, displayRect.y + currentY, displayRect.width, 0f);
                        Widgets.LabelCacheHeight(ref requiredByclassRect, "RelatedClasses".Translate());
                        currentY += requiredByclassRect.height;
                        GenUI.ResetLabelAlign();
                        int i = 0;
                        int j = 0;
                        for (int classCounter = 0; classCounter < Classes.Count; classCounter++)
                        {
                            TraitDef Class = Classes[classCounter];
                            if (i != 0 && i % maxRowIconCount == 0)
                            {
                                j++;
                                i = 0;
                            }
                            if (i == 0)
                            {
                                currentY += (float)iconHeight;
                            }
                            TraitIconMap.TraitIconValue orCreate = TraitIconMap.Get(Class);
                            if ((Texture2D)(object)orCreate.IconTexture == null)
                            {
                                i++;
                                continue;
                            }
                            Rect classRect = new Rect(requiredByclassRect.x + (float)(i % maxRowIconCount * iconWidth), requiredByclassRect.y + requiredByclassRect.height + (float)(j * iconHeight), iconWidth, iconHeight);
                            string label = null;
                            foreach (TraitDegreeData data in Class.degreeDatas)
                            {
                                label = data.label;
                                break;
                            }
                            List<ProficiencyCategoryDef> missingTypes = Utility.GetMissingClassRequirements(Class, pawn);
                            if (missingTypes.Count > 0)
                            {
                                label += "MissingCat".Translate();
                                int total = missingTypes.Count;
                                for (int smallCount = 0; smallCount < total; smallCount++)
                                {
                                    ProficiencyCategoryDef type = missingTypes[smallCount];
                                    if (smallCount + 1 == total)
                                    {
                                        label += type.LabelCap;
                                    }
                                    else
                                    {
                                        label += type.LabelCap + ", ";
                                    }
                                }
                            }
                            TooltipHandler.TipRegion(classRect, label);
                            Widgets.DrawTextureFitted(classRect, (Texture2D)(object)orCreate.IconTexture, 1);
                            i++;
                        }
                    }
                }
                __result += currentY;
            }
        }
    }
    public static class CastCostPatch
    {
        [HarmonyPatch(typeof(CompAbilityUserMagic), nameof(CompAbilityUserMagic.ActualManaCost))]
        public static class ManaCostPatchPostFix
        {
            public static void Postfix(ref float __result, TMAbilityDef magicDef, CompAbilityUserMagic __instance)
            {
                if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CostScale) { return; }
                ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                AbilityXPGainExtension extension = magicDef.GetModExtension<AbilityXPGainExtension>();
                if (extension != null && comp != null)
                {
                    List<ProficiencyDef> relatedproficiencies = extension.Relatedproficiencies;
                    float count = relatedproficiencies.Count;
                    foreach (ProficiencyDef pro in relatedproficiencies)
                    {
                        if (comp.CompletedProficiencies.Contains(pro))
                        {
                            count -= 1;
                        }
                    }
                    float ratio = (count / relatedproficiencies.Count);
                    __result *= 1 + (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().MaxCostScaleFactor * ratio);
                }
            }
        }
        [HarmonyPatch(typeof(CompAbilityUserMight), nameof(CompAbilityUserMight.ActualStaminaCost))]
        public static class StaminaCostPatchPostFix
        {
            public static void Postfix(ref float __result, TMAbilityDef mightDef, CompAbilityUserMight __instance)
            {
                if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CostScale) { return; }
                ProficiencyComp comp = __instance.Pawn.TryGetComp<ProficiencyComp>();
                AbilityXPGainExtension extension = mightDef.GetModExtension<AbilityXPGainExtension>();
                if (extension != null && comp != null)
                {
                    List<ProficiencyDef> relatedproficiencies = extension.Relatedproficiencies;
                    float count = relatedproficiencies.Count;
                    foreach (ProficiencyDef pro in relatedproficiencies)
                    {
                        if (comp.CompletedProficiencies.Contains(pro))
                        {
                            count -= 1;
                        }
                    }
                    float ratio = (count / relatedproficiencies.Count);
                    __result *= 1 + (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().MaxCostScaleFactor * ratio);
                }
            }
        }
    }
    public static class DrawStatExplainPatch
    {
        [HarmonyPatch(nameof(StatWorker), "GetExplanationUnfinalized")]
        public static class DrawStatExplainPatchPostFix
        {
            public static void Postfix(StatRequest req, ref string __result, StatDef ___stat)
            {
                StringBuilder statbuilder = new StringBuilder(__result);
                bool flag = false;
                if (req.Thing is Pawn pawn)
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        List<ProficiencyDef> relevant = new List<ProficiencyDef>();
                        foreach (ProficiencyDef prof in comp.CompletedProficiencies)
                        {
                            if (prof.HasModExtension<StatOffsetExtension>())
                            {
                                relevant.Add(prof);
                            }
                        }
                        if (relevant.Count > 0)
                        {
                            statbuilder.AppendLine("ProStatOffset".Translate());
                            foreach (ProficiencyDef pro in relevant)
                            {
                                StatOffsetExtension extension = pro.GetModExtension<StatOffsetExtension>();
                                if (extension != null)
                                {
                                    if (extension.statOffseters != null)
                                    {
                                        foreach (StatOffseter offset in extension.statOffseters)
                                        {
                                            if (offset.Stat == ___stat)
                                            {
                                                statbuilder.AppendLine("    " + pro.label + ": " + offset.GetString());
                                                flag = true;
                                            }
                                        }
                                    }
                                    if (extension.statModifiers != null)
                                    {
                                        foreach (StatModifier modifier in extension.statModifiers)
                                        {
                                            if (modifier.Stat == ___stat)
                                            {
                                                statbuilder.AppendLine("    " + pro.label + ": " + modifier.GetString());
                                                flag = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ProficienciesMasterOffseter)
                {
                    __result = statbuilder.ToString();
                }
            }
        }
    }
    public static class DrawFailPatch
    {
        [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats))]
        public static class DrawThingFailPatchPostfix
        {
            public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingDef __instance)
            {
                ClassAutoLearnExtension extension1 = __instance.GetModExtension<ClassAutoLearnExtension>();
                AbilityAutoLearnExtension extension2 = __instance.GetModExtension<AbilityAutoLearnExtension>();
                if (extension1 != null && extension1.failChance != 0 && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanFailLearn)
                {
                    string failchance = extension1.failChance.ToString() + "%";
                    yield return new StatDrawEntry((StatCategoryDef)LLStatCategoryDefOf.ProficiencyInfo, (string)"FailChance".Translate(), (string)failchance, (string)"FailChanceReport".Translate(), (int)500);
                }
                if (extension2 != null && extension2.failChance != 0 && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanFailLearn)
                {
                    string failchance = extension2.failChance.ToString() + "%";
                    yield return new StatDrawEntry((StatCategoryDef)LLStatCategoryDefOf.ProficiencyInfo, (string)"FailChance".Translate(), (string)failchance, (string)"FailChanceReport".Translate(), (int)500);
                }
            }
        }
        [HarmonyPatch(typeof(BuildingProperties), nameof(BuildingProperties.SpecialDisplayStats))]
        public static class DrawFailBuildingPatchPostfix
        {
            public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingDef parentDef)
            {
                ClassAutoLearnExtension extension = parentDef.GetModExtension<ClassAutoLearnExtension>();
                if (extension.failChance != 0 && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanFailLearn)
                {
                    string failchance = extension.failChance.ToString() + "%";
                    yield return new StatDrawEntry((StatCategoryDef)LLStatCategoryDefOf.ProficiencyInfo, (string)"FailChance".Translate(), (string)failchance, (string)"FailChanceReport".Translate(), (int)500);
                }
            }
        }
    }
    public static class StatFactorsPatch
    {
        [HarmonyPatch(typeof(ProficiencyViewerWindow), "DrawStatModifiers")]
        public static class StatOffsetPatchPostFix
        {
            public static void Postfix(Rect displayRect, ref float __result, ProficiencyDef ___selectedProficiency, ProficiencyViewerWindow __instance)
            {
                if (___selectedProficiency.HasModExtension<StatOffsetExtension>() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ProficienciesMasterOffseter)
                {
                    StatOffsetExtension extension = ___selectedProficiency.GetModExtension<StatOffsetExtension>();
                    bool flag = true;
                    Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                    if (___selectedProficiency.tab == ProficiencyTableDefOf.LLROM_Magic && !pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MagicUserHD))
                    {
                        flag = false;
                    }
                    if (___selectedProficiency.tab == ProficiencyTableDefOf.LLROM_Might && !pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MightUserHD))
                    {
                        flag = false;
                    }
                    if (extension != null && flag)
                    {
                        float CurrentY = __result;
                        if (extension.statOffseters != null && extension.statOffseters.Count > 0)
                        {
                            Rect statModLabelRect = new Rect(displayRect.x, displayRect.y + CurrentY, displayRect.width, 0f);
                            Widgets.LabelCacheHeight(ref statModLabelRect, "StatOffseterLable".Translate());
                            CurrentY += statModLabelRect.height;
                            TooltipHandler.TipRegion(statModLabelRect, "StatOffseterLableDesc".Translate());
                            foreach (StatOffseter Offset in extension.statOffseters)
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
                            if (pro.tab == ProficiencyTableDefOf.LLROM_Magic && !pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MagicUserHD))
                            {
                                continue;
                            }
                            if (pro.tab == ProficiencyTableDefOf.LLROM_Might && !pawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_MightUserHD))
                            {
                                continue;
                            }
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
        public static class SkillGainPatchPostFix
        {
            public static void Postfix(SkillDef sDef, float xp, Pawn_SkillTracker __instance)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                if (sDef == SkillDefOf.Shooting && pawn != null && xp > 0)
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (pawn != null && comp != null)
                    {
                        SkillRecord Shooting = __instance.GetSkill(SkillDefOf.Shooting);
                        comp.TryGainXp(xp * .001f, ProficiencyDefOf.LLRoM_Ranged, ExperienceType.PracticalHalfTheoretical);
                        if (Shooting.levelInt >= 7)
                        {
                            comp.TryGainXp(xp * .001f, ProficiencyDefOf.LLRoM_Extreme_Range, ExperienceType.PracticalHalfTheoretical);
                        }
                    }
                }
                if (sDef == SkillDefOf.Melee && pawn != null && xp > 0)
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
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        public static class PostDamageLearnPatchPostFix
        {
            public static void Postfix(float totalDamageDealt, Pawn __instance)
            {
                if (!__instance.Dead && __instance.HasComp<ProficiencyComp>())
                {
                    __instance.TryGetComp<ProficiencyComp>().TryGainXp(totalDamageDealt * .1f, ProficiencyDefOf.LLRoM_Physical_Conditioning, ExperienceType.PracticalHalfTheoretical);
                    __instance.TryGetComp<ProficiencyComp>().TryGainXp(totalDamageDealt * .1f, ProficiencyDefOf.LLRoM_Endurance, ExperienceType.PracticalHalfTheoretical);
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
    public static class ProficiencyLockoutPatch
    {
        [HarmonyPatch(typeof(ProficiencyComp), nameof(ProficiencyComp.CanLearn))]
        public class ProficiencyLockoutPrefix
        {
            public static bool Prefix(ProficiencyDef def, ProficiencyComp __instance)
            {
                Pawn pawn = ((ProficiencyComp)(object)__instance).parent as Pawn;
                bool canlearn = true;
                InspirationExtension Inspirationextension = def.GetModExtension<InspirationExtension>();
                if (Inspirationextension != null)
                {
                    canlearn = !Inspirationextension.requiresInspiration;
                }
                if (TM_Calc.IsUndead(pawn))
                {
                    canlearn = false;
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout && !TM_Calc.IsWanderer(pawn) && !TM_Calc.IsWayfarer(pawn))
                {
                    LockoutExtension extension = def.GetModExtension<LockoutExtension>();
                    if (pawn != null && extension != null && !pawn.health.hediffSet.HasHediff(extension.withouHediff) && pawn.health.hediffSet.HasHediff(extension.hasHediff))
                    {
                        canlearn = false;
                    }
                }
                return canlearn;
            }
        }
    }
    public static class Hide_Proficiency_Patch
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
                    if (comp != null)
                    {
                        return comp.CanLearn(def) || Util.IsQualified(pawn, def);
                    }
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureAllProficiencies && pawn != null && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        return comp.CanLearn(def) || Util.IsQualified(pawn, def);
                    }
                }
                return true;
            }
        }
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
                    if (comp != null)
                    {
                        return comp.CanLearn(def) || Util.IsQualified(pawn, def);
                    }
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureCertianProficiencies && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ObscureAllProficiencies && pawn != null && (!DebugSettings.godMode || Current.Game.InitData != null))
                {
                    ProficiencyComp comp = pawn.TryGetComp<ProficiencyComp>();
                    if (comp != null)
                    {
                        return comp.CanLearn(def) || Util.IsQualified(pawn, def);
                    }
                }
                return true;
            }
        }
    }
    public static class Ability_Proficiency_XP_Patch
    {
        [HarmonyPatch(typeof(TorannMagic.MightAbility), nameof(TorannMagic.MightAbility.PostAbilityAttempt))]
        public class MightAbility_PostAbilityAttempt_Postfix
        {
            public static void Postfix(MightAbility __instance)
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
        [HarmonyPatch(typeof(TorannMagic.MagicAbility), nameof(TorannMagic.MagicAbility.PostAbilityAttempt))]
        public class MagicAbility_PostAbilityAttempt_Postfix
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
                                            if (item == ProficiencyDefOf.Physical_Insight || item == ProficiencyDefOf.Magic_Insight)
                                            {
                                                xp *= 10;
                                            }
                                            comp.TryGainXp(xp, item, extension.experienceType);
                                        }
                                        else
                                        {
                                            xp = LearnreateMod * extension.LearnRate;
                                            if (item == ProficiencyDefOf.Physical_Insight || item == ProficiencyDefOf.Magic_Insight)
                                            {
                                                xp *= 10;
                                            }
                                            comp.TryGainXp(xp, item, extension.experienceType);
                                        }
                                    }
                                    else
                                    {
                                        foreach (ProficiencyDef pro in item.AllPrequisites)
                                        {
                                            if (pro.category.defName == "ProficiencyCategory_MixedMagic" || pro.tab != ProficiencyTableDefOf.LLROM_Magic)
                                            {
                                                continue;
                                            }
                                            if (xp > 0)
                                            {
                                                if (item == ProficiencyDefOf.Physical_Insight || item == ProficiencyDefOf.Magic_Insight)
                                                {
                                                    xp *= 10;
                                                }
                                                comp.TryGainXp(xp, pro, extension.experienceType);
                                            }
                                            else
                                            {
                                                xp = LearnreateMod * extension.LearnRate;
                                                if (item == ProficiencyDefOf.Physical_Insight || item == ProficiencyDefOf.Magic_Insight)
                                                {
                                                    xp *= 10;
                                                }
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
                ProficiencyComp comp = user.TryGetComp<ProficiencyComp>();
                if (user.mindState.inspirationHandler.Inspired)
                {
                    InspirationExtension inspirationextension = user.mindState.inspirationHandler.CurStateDef.GetModExtension<InspirationExtension>();
                    if (inspirationextension != null)
                    {
                        if (((ThingComp)(object)__instance).parent.def.defName == "GemstoneOfInsight_Magic" && inspirationextension.hediff != null && inspirationextension.hediff == TorannMagicDefOf.TM_MagicUserHD)
                        {
                            ProficiencyDef randomPro = Utility.GetRandomInspirationProficiency(user, user.mindState.inspirationHandler.CurStateDef);
                            if (randomPro != null)
                            {
                                comp.TryGainProficiency(randomPro, true);
                                ((ThingComp)(object)__instance).parent.SplitOff(1).Destroy();
                                user.mindState.inspirationHandler.EndInspiration(user.mindState.inspirationHandler.CurState);
                                return false;
                            }
                        }
                        if (((ThingComp)(object)__instance).parent.def.defName == "GemstoneOfInsight_Might" && inspirationextension.hediff != null && inspirationextension.hediff == TorannMagicDefOf.TM_MightUserHD)
                        {
                            ProficiencyDef randomPro = Utility.GetRandomInspirationProficiency(user, user.mindState.inspirationHandler.CurStateDef);
                            if (randomPro != null)
                            {
                                comp.TryGainProficiency(randomPro, true);
                                ((ThingComp)(object)__instance).parent.SplitOff(1).Destroy();
                                user.mindState.inspirationHandler.EndInspiration(user.mindState.inspirationHandler.CurState);
                                return false;
                            }
                        }
                    }
                }
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfTeachClasses && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.SituationResolved);
                    return false;
                }
                CompAbilityUserMight compAbilityUserMight = user.GetCompAbilityUserMight();
                CompAbilityUserMagic compAbilityUserMagic = user.GetCompAbilityUserMagic();
                if (!compAbilityUserMagic.IsMagicUser && !compAbilityUserMight.IsMightUser && !user.story.traits.HasTrait(TorannMagicDefOf.TM_Gifted) && !user.story.traits.HasTrait(TorannMagicDefOf.PhysicalProdigy) && !user.story.traits.HasTrait(TorannMagicDefOf.Undead) && !user.IsShambler && !user.IsGhoul)
                {
                    GemOfInsightProficiencyExtension extension = ((ThingComp)(object)__instance).parent.def.GetModExtension<GemOfInsightProficiencyExtension>();
                    if (extension != null && !comp.CompletedProficiencies.Contains(extension.Proficiency))
                    {
                        comp.TryGainProficiency(extension.Proficiency, true);
                        Messages.Message("LLAROM_GemOfInsight".Translate(user.LabelShort, extension.Proficiency.label, user.Named("USER")), user, MessageTypeDefOf.PositiveEvent);
                    }
                }
                return true;
            }
        }
    }
    public static class CompUseEffect_ClassPatch
    {
        [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
        public class CompUseEffect_LearnMagic_DoEffectk_Postfix
        {
            public static void Postfix(Trait trait, TraitSet __instance)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
                ClassAutoLearnExtension extension = trait.def.GetModExtension<ClassAutoLearnExtension>();
                if (extension != null)
                {
                    if (extension.Magic && !extension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().UnlearnProOnClassGain && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout)
                    {
                        pawn.GetComp<ProficiencyComp>().RemoveProficiency(ProficiencyDefOf.Physical_Insight, true);
                    }
                    if (!extension.Magic && extension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().UnlearnProOnClassGain && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout)
                    {
                        pawn.GetComp<ProficiencyComp>().RemoveProficiency(ProficiencyDefOf.Magic_Insight, true);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnMagic), nameof(TorannMagic.CompUseEffect_LearnMagic.DoEffect))]
        public class CompUseEffect_LearnMagic_DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfTeachClasses && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.RejectInput);
                    return false;
                }
                if (!user.GetComp<ProficiencyComp>().CanRead())
                {
                    Messages.Message("LLRoMMustBeAbleToRead".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
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
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnMight), nameof(TorannMagic.CompUseEffect_LearnMight.DoEffect))]
        public class CompUseEffect_LearnMight_DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, object __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfTeachClasses && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.RejectInput);
                    return false;
                }
                if (!user.GetComp<ProficiencyComp>().CanRead())
                {
                    Messages.Message("LLRoMMustBeAbleToRead".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
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
    public static class CompUseEffect_LearnAbilityPatch
    {
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnSpell), nameof(TorannMagic.CompUseEffect_LearnSpell.DoEffect))]
        public class CompUseEffect_LearnSpell_DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, ThingComp __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfSpells && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachSpells)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.RejectInput);
                    return false;
                }
                if (!user.GetComp<ProficiencyComp>().CanRead())
                {
                    Messages.Message("LLRoMMustBeAbleToRead".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                    return false;
                }
                BillProficiencyExtension extension = __instance.parent.def.GetModExtension<BillProficiencyExtension>();
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
        [HarmonyPatch(typeof(TorannMagic.CompUseEffect_LearnSkill), nameof(TorannMagic.CompUseEffect_LearnSkill.DoEffect))]
        public class CompUseEffect_LearnSkill_DoEffectk_Prefix
        {
            public static bool Prefix(Pawn user, ThingComp __instance)
            {
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachSkills && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanOnlySelfSkills)
                {
                    Messages.Message("LLAROM_OptionDisabled".Translate(user.LabelShort, user.Named("USER")), user, MessageTypeDefOf.RejectInput);
                    return false;
                }
                if (!user.GetComp<ProficiencyComp>().CanRead())
                {
                    Messages.Message("LLRoMMustBeAbleToRead".Translate(user.LabelShort), MessageTypeDefOf.RejectInput);
                    return false;
                }
                BillProficiencyExtension extension = __instance.parent.def.GetModExtension<BillProficiencyExtension>();
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