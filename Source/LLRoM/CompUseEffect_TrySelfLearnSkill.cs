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
                    float imfactor = Utility.ImpressivenessFactor(usedBy) * Parentextension.drainBase;
                    if (usedBy.health.hediffSet.HasHediff(Parentextension.HedifftoApply) && Parentextension.HedifftoApply != null)
                    {
                        HealthUtility.AdjustSeverity(usedBy, Parentextension.HedifftoApply, imfactor);
                    }
                    else if (Parentextension.HedifftoApply != null)
                    {
                        usedBy.health.AddHediff(Parentextension.HedifftoApply);
                        HealthUtility.AdjustSeverity(usedBy, Parentextension.HedifftoApply, imfactor);
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
                                    Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
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
                                        Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
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
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_GearRepair" && !compAbilityUserMight.skill_GearRepair)
                            {
                                compAbilityUserMight.skill_GearRepair = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_GearRepair).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_GearRepair, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_InnerHealing" && !compAbilityUserMight.skill_InnerHealing)
                            {
                                compAbilityUserMight.skill_InnerHealing = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_InnerHealing).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_InnerHealing, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_StrongBack" && !compAbilityUserMight.skill_StrongBack)
                            {
                                compAbilityUserMight.skill_StrongBack = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_StrongBack).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_StrongBack, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_HeavyBlow" && !compAbilityUserMight.skill_HeavyBlow)
                            {
                                compAbilityUserMight.skill_HeavyBlow = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_HeavyBlow).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_HeavyBlow, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_ThickSkin" && !compAbilityUserMight.skill_ThickSkin)
                            {
                                compAbilityUserMight.skill_ThickSkin = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ThickSkin).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_ThickSkin, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll.defName == "SkillOf_FightersFocus" && !compAbilityUserMight.skill_FightersFocus)
                            {
                                compAbilityUserMight.skill_FightersFocus = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_FightersFocus).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_FightersFocus, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_ThrowingKnife && !compAbilityUserMight.skill_ThrowingKnife && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_ThrowingKnife = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_ThrowingKnife).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_BurningFury && !compAbilityUserMight.skill_BurningFury && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_BurningFury = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_BurningFury).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_PommelStrike && !compAbilityUserMight.skill_PommelStrike && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_PommelStrike = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_PommelStrike).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_Legion && !compAbilityUserMight.skill_Legion && !usedBy.story.traits.HasTrait(TorannMagicDefOf.Faceless) && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_Legion = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_Legion).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SkillOf_TempestStrike && !compAbilityUserMight.skill_TempestStrike && ((CompAbilityUser)compAbilityUserMight).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMight.skill_TempestStrike = true;
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(TorannMagicDefOf.TM_TempestStrike).learned = true;
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
                                return;
                            }
                            else if (tMAbilityDef2 != null)
                            {
                                compAbilityUserMight.MightData.ReturnMatchingMightPower(tMAbilityDef2).learned = true;
                                ((CompAbilityUser)compAbilityUserMight).AddPawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef2, true, -1f);
                                compAbilityUserMight.InitializeSkill();
                                parent.SplitOff(1).Destroy();
                                Messages.Message("LearnedAbility".Translate(usedBy.LabelShortCap, TargetScroll.label), MessageTypeDefOf.PositiveEvent);
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
        public override TaggedString ConfirmMessage(Pawn p)
        {
            AbilityAutoLearnExtension Parentextension = parent.def.GetModExtension<AbilityAutoLearnExtension>();
            if (Parentextension == null)
            {
                return null;
            }
            bool flagTH = true;
            bool flagI = true;
            bool flagD = true;
            bool flagQ = true;
            Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.LLRoM_Drained);
            Room room = parent.GetRoom();
            float factor = Utility.ImpressivenessCurve(room) * Parentextension.drainBase;
            float sevfactor = 0f;
            if (firstHediffOfDef != null)
            {
                float severity = firstHediffOfDef.Severity;
                if (severity + factor > .5f)
                {
                    flagD = false;
                    sevfactor = factor + severity;
                }
            }
            else if ((factor + .1f) > .5f)
            {
                flagD = false;
                sevfactor = factor + .1f;
            }
            if (sevfactor > 1f) { sevfactor = 1f; }
            string duration = (sevfactor / .4f).ToString("#.#");
            if (flagD) { return null; }
            int tipcount = 0;
            if (room == null || room.PsychologicallyOutdoors || room.Role != RoomRoleDefOf.LLRoM_trainingHall) { flagTH = false; tipcount++; }
            if (room != null && !room.PsychologicallyOutdoors && room.GetStat(RoomStatDefOf.Impressiveness) < 240) { flagI = false; tipcount++; }
            if (Parentextension.drainBase > .3f) { flagQ = false; tipcount++; }
            string outstring = "LLRoM_DangeriousDrainWarning".Translate(duration, p.LabelShort);
            if (tipcount > 0)
            {
                outstring += "LLRoM_DurationHintStart".Translate();
                if (!flagTH)
                {
                    outstring += "LLRoM_WrongRoomHint".Translate();
                    if (tipcount == 2) { outstring += "LLRoM_Or".Translate(); }
                    else if (tipcount == 3) { outstring += "LLRoM_Comma".Translate(); }
                }
                if (!flagI)
                {
                    outstring += "LLRoM_ImpressivenessHint".Translate();
                    if (tipcount == 2 && flagTH) { outstring += "LLRoM_Or".Translate(); }
                    else if (tipcount == 3) { outstring += "LLRoM_OxOr".Translate(); }
                }
                if (!flagQ) { outstring += "LLRoM_ItemQualityHint".Translate(); }
                outstring += "LLRoM_Period".Translate();
            }
            return outstring;
        }
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!p.GetComp<ProficiencyComp>().CanRead())
            {
                return "LLRoMMustBeAbleToRead".Translate(p.LabelShort);
            }
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
    }
}