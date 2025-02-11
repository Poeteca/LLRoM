using AbilityUser;
using LifeLessons;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using TorannMagic;
using Verse;

namespace LLRoM
{
    public class CompUseEffect_TrySelfLearnSkill : CompUseEffect
    {
        public CompProperties_UseEffectTrySelfLearnSpell Props => (CompProperties_UseEffectTrySelfLearnSpell)props;
        public override void DoEffect(Pawn usedBy)
        {
            AbilityAutoLearnExtension Parentextension = parent.def.GetModExtension<AbilityAutoLearnExtension>();
            List<ProficiencyDef> completedProficiencies = usedBy.GetComp<ProficiencyComp>().CompletedProficiencies;
            List<ProficiencyDef> learnableProficiencies = usedBy.GetComp<ProficiencyComp>().AllLearnableProficiencies;
            List<ProficiencyDef> possibleProficiencies = new List<ProficiencyDef>();
            CompAbilityUserMight compAbilityUserMight = usedBy.GetCompAbilityUserMight();
            if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictSkillLearning)
            {
                possibleProficiencies = completedProficiencies;
            }
            else
            {
                possibleProficiencies = learnableProficiencies;
                foreach (ProficiencyDef def in completedProficiencies)
                {
                    possibleProficiencies.Add(def);
                }
            }
            List<ThingDef> possibleAbilities = new List<ThingDef>();
            List<ThingDef> AllScrolls = new List<ThingDef>();
            List<ProficiencyDef> ValidAbilities = new List<ProficiencyDef>();
            if (Parentextension.tab != null)
            {
                foreach (ProficiencyDef temp in possibleProficiencies)
                {
                    if (temp.tab == ProficiencyTableDefOf.LLROM_Might)
                    {
                        ValidAbilities.Add(temp);
                    }
                }
            }
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => t.defName.Contains("SkillOf_")))
            {
                AllScrolls.Add(thingDef);
            }
            foreach (ProficiencyDef proficiencyDef in ValidAbilities)
            {
                foreach (ThingDef Scroll in AllScrolls)
                {
                    if (possibleAbilities.Contains(Scroll))
                    {
                        continue;
                    }
                    AbilityAutoLearnExtension isspellscrollextension = Scroll.GetModExtension<AbilityAutoLearnExtension>();
                    BillProficiencyExtension scrollextension = Scroll.GetModExtension<BillProficiencyExtension>();
                    if (scrollextension != null && scrollextension.AnyRequirements() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
                    {
                        bool canGain = true;
                        List<ProficiencyDef> resolvedRequirements = scrollextension.ResolvedRequirements();
                        foreach (ProficiencyDef def in resolvedRequirements)
                        {
                            if (!ValidAbilities.Contains(def)) { canGain = false; }
                        }
                        if (canGain && Utility.LearnableSkillCheck(usedBy, Scroll))
                        {
                            possibleAbilities.Add(Scroll);
                        }
                    }
                }
            }
            for (int i = 0; i < compAbilityUserMight.MightData.AllMightPowers.Count; i++)
            {
                TMAbilityDef tMAbilityDef = (TMAbilityDef)(object)compAbilityUserMight.MightData.AllMightPowers[i].abilityDef;
                if (!possibleAbilities.Contains(tMAbilityDef.learnItem))
                {
                    continue;
                }
                if (possibleAbilities.Contains(tMAbilityDef.learnItem) && compAbilityUserMight.MightData.AllMightPowers[i].learned == true)
                {
                    possibleAbilities.Remove(tMAbilityDef.learnItem);
                }
            }
            if (possibleAbilities.Count == 0)
            {
                Messages.Message("LLROMNoPossibleSkill".Translate(usedBy.LabelShort), MessageTypeDefOf.RejectInput);
                return;
            }
            if (possibleAbilities.Count > 0)
            {
                ThingDef TargetScroll = possibleAbilities.RandomElement();
                int chance = 100;
                int compare = 1;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanFailLearn)
                {
                    chance = Rand.RangeInclusive(1, 100);
                    compare = Parentextension.failChance;
                }
                if (chance > compare)
                {
                    if (usedBy.health.hediffSet.HasHediff(Parentextension.HedifftoApply) && Parentextension.HedifftoApply != null)
                    {
                        HealthUtility.AdjustSeverity(usedBy, Parentextension.HedifftoApply, .2f);
                    }
                    else if (Parentextension.HedifftoApply != null)
                    {
                        usedBy.health.AddHediff(Parentextension.HedifftoApply);
                    }
                    if (TargetScroll != null && (TM_Calc.IsMightUser(usedBy) || TM_Calc.IsWayfarer(usedBy)))
                    {
                        List<TraitDef> list = null;
                        if (TargetScroll.HasModExtension<DefModExtension_LearnAbilityRequiredTraits>())
                        {
                            list = new List<TraitDef>();
                            list.Clear();
                            list = TargetScroll.GetModExtension<DefModExtension_LearnAbilityRequiredTraits>().traits;
                        }
                        bool flag = true;
                        if (compAbilityUserMight.customClass != null && !compAbilityUserMight.customClass.canLearnKnacks)
                        {
                            flag = false;
                        }
                        if (list != null && list.Count > 0)
                        {
                            flag = false;
                            foreach (TraitDef item in list)
                            {
                                if (((CompAbilityUser)compAbilityUserMight).Pawn.story.traits.HasTrait(item))
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (compAbilityUserMight.customClass != null)
                        {
                            bool flag2 = false;
                            for (int i = 0; i < compAbilityUserMight.MightData.AllMightPowers.Count; i++)
                            {
                                TMAbilityDef tMAbilityDef = (TMAbilityDef)(object)compAbilityUserMight.MightData.AllMightPowers[i].abilityDef;
                                if (tMAbilityDef.learnItem != TargetScroll)
                                {
                                    continue;
                                }
                                if (!TM_Data.RestrictedAbilities.Contains(TargetScroll) && !compAbilityUserMight.MightData.AllMightPowers[i].learned && flag)
                                {
                                    flag2 = true;
                                    compAbilityUserMight.MightData.AllMightPowers[i].learned = true;
                                    compAbilityUserMight.InitializeSkill();
                                    parent.SplitOff(1).Destroy();
                                    continue;
                                }
                                if ((TM_Data.RestrictedAbilities.Contains(TargetScroll) || flag) && !compAbilityUserMight.MightData.AllMightPowers[i].learned)
                                {
                                    if (compAbilityUserMight.customClass.learnableSkills.Contains(TargetScroll))
                                    {
                                        flag2 = true;
                                        compAbilityUserMight.MightData.AllMightPowers[i].learned = true;
                                        compAbilityUserMight.InitializeSkill();
                                        parent.SplitOff(1).Destroy();
                                        continue;
                                    }
                                    Messages.Message("CannotLearnSkill".Translate(), MessageTypeDefOf.RejectInput);
                                    return;
                                }
                                if (!flag)
                                {
                                    Messages.Message("CannotLearnSkill".Translate(), MessageTypeDefOf.RejectInput);
                                }
                                else
                                {
                                    Messages.Message("TM_AlreadyLearnedAbility".Translate(usedBy.LabelShort, ((Def)(object)tMAbilityDef).label), MessageTypeDefOf.RejectInput);
                                }
                                return;
                            }
                            if (!flag2)
                            {
                                Messages.Message("CannotLearnSkill".Translate(), MessageTypeDefOf.RejectInput);
                            }
                            return;
                        }
                        TMAbilityDef tMAbilityDef2 = null;
                        for (int j = 0; j < compAbilityUserMight.MightData.MightPowersCustomAll.Count; j++)
                        {
                            TMAbilityDef tMAbilityDef3 = (TMAbilityDef)(object)compAbilityUserMight.MightData.MightPowersCustomAll[j].abilityDef;
                            if (tMAbilityDef3.learnItem != null && tMAbilityDef3.learnItem == TargetScroll && !compAbilityUserMight.MightData.MightPowersCustomAll[j].learned)
                            {
                                tMAbilityDef2 = tMAbilityDef3;
                                break;
                            }
                        }
                        if (flag)
                        {
                            if (TargetScroll.defName == "SkillOf_Sprint" && !compAbilityUserMight.skill_Sprint && !usedBy.story.traits.HasTrait(TorannMagicDefOf.Gladiator))
                            {
                                compAbilityUserMight.skill_Sprint = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Sprint).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Sprint, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_GearRepair" && !compAbilityUserMight.skill_GearRepair)
                            {
                                compAbilityUserMight.skill_GearRepair = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_GearRepair).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_GearRepair, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_InnerHealing" && !compAbilityUserMight.skill_InnerHealing)
                            {
                                compAbilityUserMight.skill_InnerHealing = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_InnerHealing).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_InnerHealing, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_StrongBack" && !compAbilityUserMight.skill_StrongBack)
                            {
                                compAbilityUserMight.skill_StrongBack = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_StrongBack).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_StrongBack, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_HeavyBlow" && !compAbilityUserMight.skill_HeavyBlow)
                            {
                                compAbilityUserMight.skill_HeavyBlow = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_HeavyBlow).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_HeavyBlow, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_ThickSkin" && !compAbilityUserMight.skill_ThickSkin)
                            {
                                compAbilityUserMight.skill_ThickSkin = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ThickSkin).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_ThickSkin, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_FightersFocus" && !compAbilityUserMight.skill_FightersFocus)
                            {
                                compAbilityUserMight.skill_FightersFocus = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_FightersFocus).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_FightersFocus, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_ThrowingKnife && !compAbilityUserMight.skill_ThrowingKnife && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_ThrowingKnife = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ThrowingKnife).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_BurningFury && !compAbilityUserMight.skill_BurningFury && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_BurningFury = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_BurningFury).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_PommelStrike && !compAbilityUserMight.skill_PommelStrike && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_PommelStrike = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PommelStrike).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_Legion && !compAbilityUserMight.skill_Legion && !usedBy.story.traits.HasTrait(TorannMagicDefOf.Faceless) && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_Legion = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Legion).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_TempestStrike && !compAbilityUserMight.skill_TempestStrike && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_TempestStrike = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_TempestStrike).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (tMAbilityDef2 != null)
                            {
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(tMAbilityDef2).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef2, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else
                            {
                                Messages.Message("CannotLearnSkill".Translate(), MessageTypeDefOf.RejectInput);
                                return;
                            }
                        }
                        else
                        {
                            Messages.Message("CannotLearnSkill".Translate(), MessageTypeDefOf.RejectInput);
                            parent.SplitOff(1).Destroy();
                            return;
                        }
                    }
                }
                else
                {
                    Messages.Message("LLROMNoFailedtoLearnSkill".Translate(usedBy.LabelShort), MessageTypeDefOf.RejectInput);
                    parent.SplitOff(1).Destroy();
                    return;
                }
            }
        }
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.IsShambler || p.IsGhoul || p.story.traits.HasTrait(TorannMagicDefOf.Undead))
            {
                return "LLRoMMustNotBeUndead".Translate();
            }
            if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachSkills || !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
            {
                return "LLRoMDisableInOptions".Translate();
            }
            if (!TM_Calc.IsMightUser(p) && !TM_Calc.IsWayfarer(p))
            {
                return "LLRoMNotMighty".Translate();
            }
            return base.CanBeUsedBy(p);
        }
        private bool LearnableCheck(Pawn p, ThingDef S)
        {
            List<Trait> traits = p.story.traits.allTraits;
            List<TraitDef> traitdefs = new List<TraitDef>();
            foreach (Trait t in traits)
            {
                traitdefs.Add(t.def);
            }
            DefModExtension_LearnAbilityRequiredTraits list = S.GetModExtension<DefModExtension_LearnAbilityRequiredTraits>();
            if (list != null)
            {
                bool check = false;
                foreach (TraitDef T in list.traits)
                {
                    if (traitdefs.Contains(T))
                    {
                        check = true;
                    }
                }
                if (!check) { return false; }
            }
            if ((S.defName == "SkillOf_ThrowingKnife" || S.defName == "SkillOf_PommelStrike" || S.defName == "SkillOf_TempestStrike" || S.defName == "spell_ArcaneBolt") && p.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
            {
                return false;
            }
            if ((S.defName == "SkillOf_Legion") && (traitdefs.Contains(TorannMagicDefOf.Faceless) || p.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent))
            {
                return false;
            }
            if ((S.defName == "SkillOf_Sprint") && traitdefs.Contains(TorannMagicDefOf.Gladiator))
            {
                return false;
            }
            return true;
        }
    }
}