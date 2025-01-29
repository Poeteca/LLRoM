using AbilityUser;
using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorannMagic;
using Verse;

namespace LLRoM
{
    public class CompUseEffect_TrySelfLearnSpell : CompUseEffect
    {
        public CompProperties_UseEffectTrySelfLearnSpell Props => (CompProperties_UseEffectTrySelfLearnSpell)props;
        public override void DoEffect(Pawn usedBy)
        {
            AbilityAutoLearnExtension Parentextension = parent.def.GetModExtension<AbilityAutoLearnExtension>();
            List<ProficiencyDef> completedProficiencies = usedBy.GetComp<ProficiencyComp>().CompletedProficiencies;
            List<ProficiencyDef> learnableProficiencies = usedBy.GetComp<ProficiencyComp>().AllLearnableProficiencies;
            List<ProficiencyDef> possibleProficiencies = new List<ProficiencyDef>();
            if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickSpellLearning)
            {
                possibleProficiencies = completedProficiencies;
            }
            else
            {
                possibleProficiencies = learnableProficiencies;
            }
            List<ThingDef> possibleAbilities = new List<ThingDef>();
            List<ThingDef> AllScrolls = new List<ThingDef>();
            List<ProficiencyDef> ValidAbilities = new List<ProficiencyDef>();
            if (Parentextension.tab != null)
            {
                foreach (ProficiencyDef temp in possibleProficiencies)
                {
                    if (temp.tab == ProficiencyTableDefOf.LLROM_Magic)
                    {
                        ValidAbilities.Add(temp);
                    }
                }
            }
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => t.defName.Contains("SpellOf_")))
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
                    BillProficiencyExtension scrollextension = Scroll.GetModExtension<BillProficiencyExtension>();
                    if (scrollextension != null && scrollextension.AnyRequirements() && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
                    {
                        List<ProficiencyDef> resolvedRequirements = scrollextension.ResolvedRequirements();
                        if (Util.Qualification(usedBy, resolvedRequirements, LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickSpellLearning).Allowed(false))
                        {
                            possibleAbilities.Add(Scroll);
                        }
                    }
                }
            }
            if (possibleAbilities.Count == 0)
            {
                Messages.Message("LLROMNoPossibleSpell".Translate(usedBy.LabelShort), MessageTypeDefOf.RejectInput);
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
                    CompAbilityUserMagic compAbilityUserMagic = usedBy.GetCompAbilityUserMagic();
                    if (TargetScroll != null && (TM_Calc.IsMagicUser(usedBy) || TM_Calc.IsWanderer(usedBy)))
                    {
                        List<TraitDef> list = null;
                        if (TargetScroll.HasModExtension<DefModExtension_LearnAbilityRequiredTraits>())
                        {
                            list = new List<TraitDef>();
                            list.Clear();
                            list = TargetScroll.GetModExtension<DefModExtension_LearnAbilityRequiredTraits>().traits;
                        }
                        bool flag = true;
                        if (compAbilityUserMagic.customClass != null && !compAbilityUserMagic.customClass.canLearnCantrips)
                        {
                            flag = false;
                        }
                        if (list != null && list.Count > 0)
                        {
                            flag = false;
                            foreach (TraitDef item in list)
                            {
                                if (((CompAbilityUser)compAbilityUserMagic).Pawn.story.traits.HasTrait(item))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (compAbilityUserMagic.customClass != null)
                        {
                            bool flag2 = false;
                            for (int i = 0; i < compAbilityUserMagic.MagicData.AllMagicPowers.Count; i++)
                            {
                                TMAbilityDef tMAbilityDef = (TMAbilityDef)(object)compAbilityUserMagic.MagicData.AllMagicPowers[i].abilityDef;
                                if (tMAbilityDef.learnItem != TargetScroll)
                                {
                                    continue;
                                }
                                if (!TM_Data.RestrictedAbilities.Contains(TargetScroll) && !compAbilityUserMagic.MagicData.AllMagicPowers[i].learned && flag)
                                {
                                    flag2 = true;
                                    compAbilityUserMagic.MagicData.AllMagicPowers[i].learned = true;
                                    if (tMAbilityDef.shouldInitialize)
                                    {
                                        ((CompAbilityUser)compAbilityUserMagic).RemovePawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef);
                                        ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef, true, -1f);
                                    }
                                    compAbilityUserMagic.InitializeSpell();
                                    parent.SplitOff(1).Destroy();
                                    break;
                                }
                                if ((TM_Data.RestrictedAbilities.Contains(TargetScroll) || flag) && !compAbilityUserMagic.MagicData.AllMagicPowers[i].learned)
                                {
                                    if (compAbilityUserMagic.customClass.learnableSpells.Contains(TargetScroll))
                                    {
                                        flag2 = true;
                                        compAbilityUserMagic.MagicData.AllMagicPowers[i].learned = true;
                                        if (tMAbilityDef.shouldInitialize)
                                        {
                                            ((CompAbilityUser)compAbilityUserMagic).RemovePawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef);
                                            ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef, true, -1f);
                                        }
                                        compAbilityUserMagic.InitializeSpell();
                                        parent.SplitOff(1).Destroy();
                                    }
                                    else
                                    {
                                        Messages.Message("CannotLearnSpell".Translate(), MessageTypeDefOf.RejectInput);
                                        parent.SplitOff(1).Destroy();
                                    }
                                    break;
                                }
                                if (!flag)
                                {
                                    Messages.Message("CannotLearnSpell".Translate(), MessageTypeDefOf.RejectInput);
                                }
                                else
                                {
                                    Messages.Message("TM_AlreadyLearnedAbility".Translate(usedBy.LabelShort, ((Def)(object)tMAbilityDef).label), MessageTypeDefOf.RejectInput);
                                }
                                return;
                            }
                            if (!flag2)
                            {
                                Messages.Message("CannotLearnSpell".Translate(), MessageTypeDefOf.RejectInput);
                            }
                            return;
                        }
                        TMAbilityDef tMAbilityDef2 = null;
                        for (int j = 0; j < compAbilityUserMagic.MagicData.MagicPowersCustomAll.Count; j++)
                        {
                            TMAbilityDef tMAbilityDef3 = (TMAbilityDef)(object)compAbilityUserMagic.MagicData.MagicPowersCustomAll[j].abilityDef;
                            if (tMAbilityDef3.learnItem != null && tMAbilityDef3.learnItem == TargetScroll && !compAbilityUserMagic.MagicData.MagicPowersCustomAll[j].learned)
                            {
                                tMAbilityDef2 = tMAbilityDef3;
                                break;
                            }
                        }
                        if (flag)
                        {
                            if (TargetScroll.defName == "SpellOf_Rain" && !compAbilityUserMagic.spell_Rain)
                            {
                                compAbilityUserMagic.spell_Rain = true;
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersHoF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Rainmaker);
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Rainmaker, true, -1f);
                                magicPower.learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Blink" && !compAbilityUserMagic.spell_Blink)
                            {
                                compAbilityUserMagic.spell_Blink = true;
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Blink);
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Blink, true, -1f);
                                if (!usedBy.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                                {
                                    magicPower.learned = true;
                                }
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Teleport" && !compAbilityUserMagic.spell_Teleport)
                            {
                                compAbilityUserMagic.spell_Teleport = true;
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Teleport);
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Teleport, true, -1f);
                                if (!usedBy.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                                {
                                    magicPower.learned = true;
                                }
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Heal" && !compAbilityUserMagic.spell_Heal)
                            {
                                compAbilityUserMagic.spell_Heal = true;
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersP.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Heal);
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Heal, true, -1f);
                                if (!usedBy.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                                {
                                    magicPower.learned = true;
                                }
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Heater" && !compAbilityUserMagic.spell_Heater)
                            {
                                compAbilityUserMagic.spell_Heater = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Heater).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Cooler" && !compAbilityUserMagic.spell_Cooler)
                            {
                                compAbilityUserMagic.spell_Cooler = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Cooler).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_PowerNode" && !compAbilityUserMagic.spell_PowerNode)
                            {
                                compAbilityUserMagic.spell_PowerNode = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_PowerNode).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Sunlight" && !compAbilityUserMagic.spell_Sunlight)
                            {
                                compAbilityUserMagic.spell_Sunlight = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_PowerNode).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_DryGround" && !compAbilityUserMagic.spell_DryGround && usedBy.story.traits.HasTrait(TorannMagicDefOf.InnerFire))
                            {
                                compAbilityUserMagic.spell_DryGround = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_DryGround).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Firestorm" && !compAbilityUserMagic.spell_Firestorm && usedBy.story.traits.HasTrait(TorannMagicDefOf.InnerFire))
                            {
                                compAbilityUserMagic.spell_Firestorm = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Firestorm).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_WetGround" && !compAbilityUserMagic.spell_WetGround && usedBy.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost))
                            {
                                compAbilityUserMagic.spell_WetGround = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_WetGround).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Blizzard" && !compAbilityUserMagic.spell_Blizzard && usedBy.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost))
                            {
                                compAbilityUserMagic.spell_Blizzard = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Blizzard).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_ChargeBattery" && !compAbilityUserMagic.spell_ChargeBattery && usedBy.story.traits.HasTrait(TorannMagicDefOf.StormBorn))
                            {
                                compAbilityUserMagic.spell_ChargeBattery = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_ChargeBattery).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_SmokeCloud" && !compAbilityUserMagic.spell_SmokeCloud)
                            {
                                compAbilityUserMagic.spell_SmokeCloud = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_SmokeCloud).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Extinguish" && !compAbilityUserMagic.spell_Extinguish)
                            {
                                compAbilityUserMagic.spell_Extinguish = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_EMP" && !compAbilityUserMagic.spell_EMP)
                            {
                                compAbilityUserMagic.spell_EMP = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_EMP).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_SummonMinion" && !compAbilityUserMagic.spell_SummonMinion)
                            {
                                compAbilityUserMagic.spell_SummonMinion = true;
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersS.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonMinion);
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_SummonMinion, true, -1f);
                                if (!usedBy.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                                {
                                    magicPower.learned = true;
                                }
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_TransferMana" && !compAbilityUserMagic.spell_TransferMana)
                            {
                                compAbilityUserMagic.spell_TransferMana = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_TransferMana).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_SiphonMana" && !compAbilityUserMagic.spell_SiphonMana)
                            {
                                compAbilityUserMagic.spell_SiphonMana = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_SiphonMana).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_RegrowLimb" && !compAbilityUserMagic.spell_RegrowLimb && usedBy.story.traits.HasTrait(TorannMagicDefOf.Druid))
                            {
                                compAbilityUserMagic.spell_RegrowLimb = true;
                                compAbilityUserMagic.InitializeSpell();
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersD.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RegrowLimb);
                                magicPower.learned = true;
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_EyeOfTheStorm" && !compAbilityUserMagic.spell_EyeOfTheStorm && usedBy.story.traits.HasTrait(TorannMagicDefOf.StormBorn))
                            {
                                compAbilityUserMagic.spell_EyeOfTheStorm = true;
                                MagicPower magicPower = compAbilityUserMagic.MagicData.MagicPowersSB.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EyeOfTheStorm);
                                magicPower.learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_ManaShield" && !compAbilityUserMagic.spell_ManaShield)
                            {
                                compAbilityUserMagic.spell_ManaShield = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_ManaShield).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_FoldReality" && !compAbilityUserMagic.spell_FoldReality && usedBy.story.traits.HasTrait(TorannMagicDefOf.Arcanist))
                            {
                                compAbilityUserMagic.spell_FoldReality = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_FoldReality).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Resurrection" && !compAbilityUserMagic.spell_Resurrection && usedBy.story.traits.HasTrait(TorannMagicDefOf.Priest))
                            {
                                compAbilityUserMagic.spell_Resurrection = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Resurrection).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_BattleHymn" && !compAbilityUserMagic.spell_BattleHymn && usedBy.story.traits.HasTrait(TorannMagicDefOf.TM_Bard))
                            {
                                compAbilityUserMagic.spell_BattleHymn = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_BattleHymn).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_HolyWrath" && !compAbilityUserMagic.spell_HolyWrath && usedBy.story.traits.HasTrait(TorannMagicDefOf.Paladin))
                            {
                                compAbilityUserMagic.spell_HolyWrath = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_HolyWrath).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_LichForm" && !compAbilityUserMagic.spell_LichForm && usedBy.story.traits.HasTrait(TorannMagicDefOf.Necromancer))
                            {
                                compAbilityUserMagic.spell_LichForm = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_LichForm).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_SummonPoppi" && !compAbilityUserMagic.spell_SummonPoppi && usedBy.story.traits.HasTrait(TorannMagicDefOf.Summoner))
                            {
                                compAbilityUserMagic.spell_SummonPoppi = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_SummonPoppi).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Scorn" && !compAbilityUserMagic.spell_Scorn && usedBy.story.traits.HasTrait(TorannMagicDefOf.Succubus))
                            {
                                compAbilityUserMagic.spell_Scorn = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Scorn).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_PsychicShock" && !compAbilityUserMagic.spell_PsychicShock && usedBy.story.traits.HasTrait(TorannMagicDefOf.Warlock))
                            {
                                compAbilityUserMagic.spell_PsychicShock = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_PsychicShock).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Meteor" && !compAbilityUserMagic.spell_Meteor && usedBy.story.traits.HasTrait(TorannMagicDefOf.Geomancer))
                            {
                                compAbilityUserMagic.spell_Meteor = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Meteor).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_OrbitalStrike" && !compAbilityUserMagic.spell_OrbitalStrike && usedBy.story.traits.HasTrait(TorannMagicDefOf.Technomancer))
                            {
                                compAbilityUserMagic.spell_OrbitalStrike = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_OrbitalStrike).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_CauterizeWound" && !compAbilityUserMagic.spell_CauterizeWound && usedBy.story.traits.HasTrait(TorannMagicDefOf.InnerFire))
                            {
                                compAbilityUserMagic.spell_CauterizeWound = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_CauterizeWound).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_FertileLands" && !compAbilityUserMagic.spell_FertileLands && usedBy.story.traits.HasTrait(TorannMagicDefOf.Druid))
                            {
                                compAbilityUserMagic.spell_FertileLands = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_FertileLands).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_SpellMending" && !compAbilityUserMagic.spell_SpellMending)
                            {
                                compAbilityUserMagic.spell_SpellMending = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_SpellMending).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_TechnoShield" && !compAbilityUserMagic.MagicData.MagicPowersT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoShield).learned && usedBy.story.traits.HasTrait(TorannMagicDefOf.Technomancer))
                            {
                                compAbilityUserMagic.MagicData.MagicPowersT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoShield).learned = true;
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_TechnoShield, true, -1f);
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Sabotage" && !compAbilityUserMagic.MagicData.MagicPowersT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Sabotage).learned && usedBy.story.traits.HasTrait(TorannMagicDefOf.Technomancer))
                            {
                                compAbilityUserMagic.MagicData.MagicPowersT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Sabotage).learned = true;
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Sabotage, true, -1f);
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_Overdrive" && !compAbilityUserMagic.MagicData.MagicPowersT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Overdrive).learned && usedBy.story.traits.HasTrait(TorannMagicDefOf.Technomancer))
                            {
                                compAbilityUserMagic.MagicData.MagicPowersT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Overdrive).learned = true;
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)TorannMagicDefOf.TM_Overdrive, true, -1f);
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll.defName == "SpellOf_BloodMoon" && !compAbilityUserMagic.spell_BloodMoon && usedBy.story.traits.HasTrait(TorannMagicDefOf.BloodMage))
                            {
                                compAbilityUserMagic.spell_BloodMoon = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_BloodMoon).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_Shapeshift && !compAbilityUserMagic.spell_Shapeshift && usedBy.story.traits.HasTrait(TorannMagicDefOf.Enchanter))
                            {
                                compAbilityUserMagic.spell_Shapeshift = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Shapeshift).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_Blur && !compAbilityUserMagic.spell_Blur)
                            {
                                compAbilityUserMagic.spell_Blur = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Blur).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_BlankMind && !compAbilityUserMagic.spell_BlankMind && usedBy.story.traits.HasTrait(TorannMagicDefOf.Enchanter))
                            {
                                compAbilityUserMagic.spell_BlankMind = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_BlankMind).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_DirtDevil && !compAbilityUserMagic.spell_DirtDevil)
                            {
                                compAbilityUserMagic.spell_DirtDevil = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_DirtDevil).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_MechaniteReprogramming && !compAbilityUserMagic.spell_MechaniteReprogramming && usedBy.story.traits.HasTrait(TorannMagicDefOf.Technomancer))
                            {
                                compAbilityUserMagic.spell_MechaniteReprogramming = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_MechaniteReprogramming).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_ArcaneBolt && !compAbilityUserMagic.spell_ArcaneBolt && ((CompAbilityUser)compAbilityUserMagic).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMagic.spell_ArcaneBolt = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_ArcaneBolt).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_LightningTrap && !compAbilityUserMagic.spell_LightningTrap && ((CompAbilityUser)compAbilityUserMagic).Pawn.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
                            {
                                compAbilityUserMagic.spell_LightningTrap = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_LightningTrap).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_Invisibility && !compAbilityUserMagic.spell_Invisibility)
                            {
                                compAbilityUserMagic.spell_Invisibility = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Invisibility).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_BriarPatch && !compAbilityUserMagic.spell_BriarPatch && usedBy.story.traits.HasTrait(TorannMagicDefOf.Druid))
                            {
                                compAbilityUserMagic.spell_BriarPatch = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_BriarPatch).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_Recall && !compAbilityUserMagic.spell_Recall && usedBy.story.traits.HasTrait(TorannMagicDefOf.Chronomancer))
                            {
                                compAbilityUserMagic.spell_Recall = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Recall).learned = true;
                                compAbilityUserMagic.MagicData.MagicPowersStandalone.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TimeMark).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_HeatShield && !compAbilityUserMagic.spell_HeatShield && usedBy.story.traits.HasTrait(TorannMagicDefOf.InnerFire))
                            {
                                compAbilityUserMagic.spell_HeatShield = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_HeatShield).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_MageLight && !compAbilityUserMagic.spell_MageLight)
                            {
                                compAbilityUserMagic.spell_MageLight = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_MageLight).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_Ignite && !compAbilityUserMagic.spell_Ignite)
                            {
                                compAbilityUserMagic.spell_Ignite = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Ignite).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (TargetScroll == TorannMagicDefOf.SpellOf_SnapFreeze && !compAbilityUserMagic.spell_SnapFreeze)
                            {
                                compAbilityUserMagic.spell_SnapFreeze = true;
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_SnapFreeze).learned = true;
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else if (tMAbilityDef2 != null)
                            {
                                compAbilityUserMagic.MagicData.ReturnMatchingMagicPower(tMAbilityDef2).learned = true;
                                ((CompAbilityUser)compAbilityUserMagic).AddPawnAbility((AbilityUser.AbilityDef)(object)tMAbilityDef2, true, -1f);
                                compAbilityUserMagic.InitializeSpell();
                                parent.SplitOff(1).Destroy();
                                return;
                            }
                            else
                            {
                                Messages.Message("CannotLearnSpell".Translate(), MessageTypeDefOf.RejectInput);
                                return;
                            }
                        }
                        else
                        {
                            Messages.Message("CannotLearnSpell".Translate(), MessageTypeDefOf.RejectInput);
                            return;
                        }
                    }
                }
                else
                {
                    Messages.Message("LLROMNoFailedtoLearnSpell".Translate(usedBy.LabelShort), MessageTypeDefOf.RejectInput);
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
            if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachSpells || !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().AbilityRequiresProficiencies)
            {
                return "LLRoMDisableInOptions".Translate();
            }
            if (!TM_Calc.IsMagicUser(p) && !TM_Calc.IsWanderer(p))
            {
                return "LLRoMNotMagical".Translate();
            }
            return base.CanBeUsedBy(p);
        }
    }
}
