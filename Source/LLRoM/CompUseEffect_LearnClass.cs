using LifeLessons;
using RimWorld;
using System.Collections.Generic;
using TorannMagic;
using Verse;

namespace LLRoM
{
    public class CompUseEffect_LearnClass : CompUseEffect
    {
        public CompProperties_UseEffectLearnClass Props => (CompProperties_UseEffectLearnClass)props;
        public override void DoEffect(Pawn usedBy)
        {
            ClassAutoLearnExtension Parentextension = parent.def.GetModExtension<ClassAutoLearnExtension>();
            List<ProficiencyDef> completedProficiencies = usedBy.GetComp<ProficiencyComp>().CompletedProficiencies;
            List<ProficiencyDef> learnableProficiencies = usedBy.GetComp<ProficiencyComp>().AllLearnableProficiencies;
            List<TraitDef> possibleclasses = new List<TraitDef>();
            List<ProficiencyDef> DefsToChec = completedProficiencies;
            if (!LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMagicClassLearning || !LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMightClassLearning)
            {
                foreach (ProficiencyDef def in learnableProficiencies)
                {
                    DefsToChec.Add(def);
                }
            }
            foreach (ProficiencyDef proficiencyDef in DefsToChec)
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
                                continue;
                            }
                            if (traitextension.GenderRequirment != Gender.None && usedBy.gender == Gender.None && (Rand.Chance(0.5f)))
                            {
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
                                    bool canGain = true;
                                    if (traitextension.Magic && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMagicClassLearning)
                                    {
                                        check = true;
                                    }
                                    if  (traitextension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMightClassLearning)
                                    {
                                        check = true;
                                    }
                                    if (traitextension.Strict)
                                    {
                                        check = true;
                                    }
                                    if (check)
                                    {
                                        foreach(ProficiencyDef p in traitextension.RequiredProficiencies)
                                        {
                                            if (!completedProficiencies.Contains(p))
                                            {
                                                canGain = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (ProficiencyDef p in traitextension.RequiredProficiencies)
                                        {
                                            if (!learnableProficiencies.Contains(p))
                                            {
                                                canGain = false;
                                            }
                                        }
                                    }
                                    if (!canGain)
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
                                        if (Parentextension.Magic && !traitextension.Magic) { break; }
                                        if (Parentextension.Might && !traitextension.Might) { break; }
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
                string classname = Class.DataAtDegree(4).label;
                if (Class.DataAtDegree(0).label != null)
                {
                    classname = Class.DataAtDegree(0).label;
                }
                int chance = 100;
                int compare = 1;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().CanFailLearn)
                {
                    chance = Rand.RangeInclusive(1, 100);
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
                                Removerequirements(T, usedBy.story.traits.allTraits);
                            }
                        }
                    }
                    if (Classextension.Magic && !Classextension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().UnlearnProOnClassGain && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout)
                    {
                        usedBy.GetComp<ProficiencyComp>().RemoveProficiency(ProficiencyDefOf.Physical_Insight, true);
                    }
                    if (!Classextension.Magic && Classextension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().UnlearnProOnClassGain && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().ClassProLockout)
                    {
                        usedBy.GetComp<ProficiencyComp>().RemoveProficiency(ProficiencyDefOf.Magic_Insight, true);
                    }
                    if (Classextension.RequiredTrait == null && usedBy.story.traits.allTraits.Count > 7)
                    {
                        int toremove = Rand.RangeInclusive(0, 6);
                        RemovecertainTrait(usedBy.story.traits.allTraits, toremove);
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
                if (parent.def.defName.Contains("Unfinished"))
                {
                    parent.SplitOff(1).Destroy();
                }
                return;
            }
            if (possibleclasses.Count == 0)
            {
                if (parent.def.defName.Contains("Unfinished"))
                {
                    parent.SplitOff(1).Destroy();
                }
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
        private void Removerequirements(TraitDef trait, List<Trait> traitlist)
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
        private void RemovecertainTrait(List<Trait> traitlist, int target)
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