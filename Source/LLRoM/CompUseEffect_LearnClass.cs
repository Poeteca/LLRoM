using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TorannMagic;
using TorannMagic.ModOptions;
using Verse;

namespace LLRoM
{
    public class CompUseEffect_LearnClass : CompUseEffect
    {
        public CompProperties_UseEffectLearnClass Props => (CompProperties_UseEffectLearnClass)props;
        public override void DoEffect(Pawn usedBy)
        {
            List<ProficiencyDef> completedProficiencies = usedBy.GetComp<ProficiencyComp>().CompletedProficiencies;
            List<TraitDef> possibleclasses = new List<TraitDef>();
            foreach (ProficiencyDef proficiencyDef in completedProficiencies)
            {
                ClassAutoLearnExtension proextension = proficiencyDef.GetModExtension<ClassAutoLearnExtension>();
                if (proextension != null)
                {
                    if (proextension.Relatedtraits.Count == 0)
                    {
                        Log.Error("Config error: " + proficiencyDef.defName + " has mod extension LLRoM.CompUseEffect_LearnClass, but is missing related traits.");
                        continue;
                    }
                    foreach (TraitDef trait in proextension.Relatedtraits)
                    {
                        if (possibleclasses.Contains(trait))
                        {
                            continue;
                        }
                        bool addTrait = true;
                        ClassAutoLearnExtension traitextension = trait.GetModExtension<ClassAutoLearnExtension>();
                        if (traitextension == null)
                        {
                            Log.Warning("Config error: Skipping " + trait.defName + ", it lacks LLRoM.CompUseEffect_LearnClass.");
                            continue;
                        }
                        if (traitextension != null)
                        {
                            if (traitextension.GenderRequirment != Gender.None && usedBy.gender != traitextension.GenderRequirment)
                            {
                                addTrait = false;
                                continue;
                            }
                            if (traitextension.GenderRequirment != Gender.None && usedBy.gender == Gender.None && (Rand.Chance(0.5f)))
                            {
                                addTrait = false;
                                continue;
                            }
                            if (traitextension.RequiredProficiencies != null)
                            {
                                if (traitextension.RequiredProficiencies.Count == 0)
                                {
                                    Log.Warning("Config error: " + trait.defName + " has RequiredProficiencies with none listed.");
                                }
                                else
                                {
                                    bool check = false;
                                    if (traitextension.Strict || (traitextension.Magic && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickMagicClassLearning) || (traitextension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrickMightClassLearning))
                                    {
                                        check = true;
                                    }
                                    if (!Util.Qualification(usedBy, traitextension.RequiredProficiencies, check).Allowed(false))
                                    {
                                        addTrait = false;
                                    }
                                }
                            }
                            if (traitextension.BlockingTraits != null)
                            {
                                if (traitextension.BlockingTraits.Count == 0)
                                {
                                    Log.Warning("Config error: " + trait.defName + " has BlockingTraits with none listed.");
                                }
                                else
                                {
                                    foreach (TraitDef t in traitextension.BlockingTraits)
                                    {
                                        if (usedBy.story.traits.HasTrait(t))
                                        {
                                            addTrait = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (traitextension.BlockingHediffs != null)
                            {
                                if (traitextension.BlockingHediffs.Count == 0)
                                {
                                    Log.Warning("Config error: " + trait.defName + " has BlockingHediffs with none listed.");
                                }
                                else
                                {
                                    foreach (HediffDef hediff in traitextension.BlockingHediffs)
                                    {
                                        if (usedBy.health.hediffSet.HasHediff(hediff))
                                        {
                                            addTrait = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (traitextension.RequiredTrait != null)
                            {
                                bool hasanyTrait = traitextension.AllRequiredTraitsNeeded;
                                if (traitextension.RequiredTrait.Count == 0)
                                {
                                    Log.Warning("Config error: " + trait.defName + " has RequiredTrait with none listed.");
                                }
                                else
                                {
                                    foreach (TraitDef T in traitextension.RequiredTrait)
                                    {
                                        if (usedBy.story.traits.HasTrait(T) && !traitextension.AllRequiredTraitsNeeded)
                                        {
                                            hasanyTrait = true;
                                            break;
                                        }
                                        if (usedBy.story.traits.HasTrait(T) && traitextension.AllRequiredTraitsNeeded)
                                        {
                                            hasanyTrait = false;
                                            break;
                                        }
                                    }
                                }
                                if (addTrait)
                                {
                                    addTrait = hasanyTrait;
                                }
                            }
                        }
                        if (addTrait)
                        {
                            possibleclasses.Add(trait);
                        }
                    }
                }
            }
            if (possibleclasses.Count > 0)
            {
                TraitDef Class = possibleclasses.RandomElement();
                ClassAutoLearnExtension Classextension = Class.GetModExtension<ClassAutoLearnExtension>();
                string classname = Class.DataAtDegree(0).label;
                int chance = 100;
                int compare = 1;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanFailLearn)
                {
                    chance = Rand.RangeInclusive(1, 100);
                    ClassAutoLearnExtension Parentextension = parent.def.GetModExtension<ClassAutoLearnExtension>();
                    compare = Parentextension.failChance;
                }
                if (chance > compare)
                {
                    if (Classextension.RequiredTrait != null)
                    {
                        if (Classextension.RequiredTrait.Count == 0)
                        {
                            Log.Warning("Config error: " + Class.defName + " has RequiredTrait with none listed.");
                        }
                        else
                        {
                            foreach (TraitDef T in Classextension.RequiredTrait)
                            {
                                removerequirements(T, usedBy.story.traits.allTraits);
                            }
                        }
                    }
                    if (Classextension.RequiredTrait == null && usedBy.story.traits.allTraits.Count > 7)
                    {
                        int toremove = Rand.RangeInclusive(0, 6);
                        removecertainTrait(usedBy.story.traits.allTraits, toremove);
                    }
                    usedBy.story.traits.GainTrait(new Trait(Class));
                    Messages.Message("LLRoM_AutoLearnedClass".Translate(usedBy.LabelShort, classname, Classextension.Prefix), MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("LLRoM_AutoLearnClassFail".Translate(usedBy.LabelShort, classname, Classextension.Prefix), MessageTypeDefOf.RejectInput);
                }
                if (usedBy.health.hediffSet.HasHediff(Classextension.appliedHediff))
                {
                    HealthUtility.AdjustSeverity(usedBy, Classextension.appliedHediff, .5f);
                }
                else
                {
                    usedBy.health.AddHediff(Classextension.appliedHediff);
                    HealthUtility.AdjustSeverity(usedBy, Classextension.appliedHediff, .5f);
                }
                return;
            }
            if (possibleclasses.Count == 0)
            {
                Messages.Message("LLRoM_FailAutoLearnedClassNoClasses".Translate(usedBy.LabelShort), MessageTypeDefOf.RejectInput);
                return;
            }
        }
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.IsShambler || p.IsGhoul || p.story.traits.HasTrait(TorannMagicDefOf.Undead))
            {
                return "LLRoMMustNotBeUndead".Translate();
            }
            if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanSelfTeachClasses || !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassRequiresProficiencies)
            {
                return "LLRoMDisableInOptions".Translate();
            }
            if (TM_Calc.IsMagicUser(p) || TM_Calc.IsMightUser(p))
            {
                return "LLRoMNothingToLearn".Translate();
            }
            return base.CanBeUsedBy(p);
        }
        private void removerequirements(TraitDef trait, List<Trait> traitlist)
        {
            int target = -1;
            foreach (Trait T in traitlist)
            {
                if (T.def.defName == trait.defName)
                {
                    target = traitlist.IndexOf(T);
                    break;
                }
            }
            if (target != -1) 
            { 
                traitlist.RemoveAt(target); 
            }
        }
        private void removecertainTrait(List<Trait> traitlist, int target)
        {
            if (target != -1 && target < (traitlist.Count - 1))
            { 
                traitlist.RemoveAt(target); 
            }
            else 
            { 
                Log.Warning("Trait remover index out of range"); 
            }
        }
    }
}
