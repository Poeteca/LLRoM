using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using TorannMagic;
using UnityEngine;
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
                                    if (traitextension.Magic && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMagicClassLearning)
                                    {
                                        check = true;
                                    }
                                    if (traitextension.Might && LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMightClassLearning)
                                    {
                                        check = true;
                                    }
                                    if (traitextension.Strict)
                                    {
                                        check = true;
                                    }
                                    if (!Util.Qualification(usedBy, traitextension.RequiredProficiencies, check).Allowed(check))
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
                string classname = null;
                int degree = 0;
                foreach (TraitDegreeData data in Class.degreeDatas)
                {
                    classname = data.label;
                    degree = data.degree;
                    break;
                }
                if (classname == null)
                {
                    Log.Warning("Could not find Class name");
                    classname = "";
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
                    if (Classextension.RequiredTrait == null && usedBy.story.traits.allTraits.Count > 7)
                    {
                        int toremove = Rand.RangeInclusive(0, 6);
                        RemovecertainTrait(usedBy.story.traits.allTraits, toremove);
                    }
                    usedBy.story.traits.GainTrait(new Trait(Class, degree));
                    Messages.Message("LLRoM_AutoLearnedClass".Translate(usedBy.LabelShort, classname, Classextension.Prefix), MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("LLRoM_AutoLearnClassFail".Translate(usedBy.LabelShort, classname, Classextension.Prefix), MessageTypeDefOf.RejectInput);
                }
                float factor = Utility.ImpressivenessFactor(usedBy) * Parentextension.drainBase;
                if (usedBy.health.hediffSet.HasHediff(Classextension.appliedHediff))
                {
                    HealthUtility.AdjustSeverity(usedBy, Classextension.appliedHediff, factor);
                }
                else
                {
                    usedBy.health.AddHediff(Classextension.appliedHediff);
                    HealthUtility.AdjustSeverity(usedBy, Classextension.appliedHediff, factor);
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
        public override TaggedString ConfirmMessage(Pawn p)
        {
            ClassAutoLearnExtension Parentextension = parent.def.GetModExtension<ClassAutoLearnExtension>();
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
            else if ((factor +.1f) > .5f)
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
            if (parent.def.defName.Contains("Unfinished") && !p.GetComp<ProficiencyComp>().CanRead())
            {
                return "LLRoMMustBeAbleToRead".Translate(p.LabelShort);
            }
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
            if(!p.story.traits.HasTrait(TorannMagicDefOf.TM_Gifted) && !p.story.traits.HasTrait(TorannMagicDefOf.PhysicalProdigy) && parent.def.defName.Contains("Unfinished"))
            {
                return "LLRoM_NoClasses".Translate();
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