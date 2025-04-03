using HarmonyLib;
using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TorannMagic;
using TorannMagic.TMDefs;
using UnityEngine;
using Verse;

namespace LLRoM
{
    public class Utility
    {
        public static float ImpressivenessCurve(Room room)
        {
            float num;
            if (room != null && !room.PsychologicallyOutdoors)
            {
                float impress = room.GetStat(RoomStatDefOf.Impressiveness);
                if (room.Role == RoomRoleDefOf.LLRoM_trainingHall)
                {
                    impress *= 1.25f;
                }
                if (impress > 240f) { num = .25f; }
                else if (impress < 20f) { num = 1.75f; }
                else
                {
                    num = (-1.5f / 220f) * impress + (83f / 44f);
                }
            }
            else
            {
                num = 1.75f;
            }
            return num;
        }
        public static float ImpressivenessFactor(Pawn pawn)
        {
            float num = 1f;
            if (!pawn.Spawned)
            {
                return num;
            }
            Room room = pawn.GetRoom();
            num = ImpressivenessCurve(room);
            return num;
        }
        public static bool CanLearnClass(Pawn p, TraitDef Class)
        {
            bool validClass = true;
            ClassAutoLearnExtension traitextension = Class.GetModExtension<ClassAutoLearnExtension>();
            if (traitextension != null)
            {
                if (traitextension.GenderRequirment != Gender.None && p.gender != traitextension.GenderRequirment)
                {
                    validClass = false;
                }
                if (traitextension.GenderRequirment != Gender.None && p.gender == Gender.None && (Rand.Chance(0.5f)))
                {
                    validClass = false;
                }
                if (traitextension.RequiredProficiencies != null)
                {
                    if (traitextension.RequiredProficiencies.Count == 0)
                    {
                        Log.Warning("Config error: " + Class.defName + " has RequiredProficiencies with none listed.");
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
                        if (!Util.Qualification(p, traitextension.RequiredProficiencies, check).Allowed(check))
                        {
                            validClass = false;
                        }
                    }
                }
                if (traitextension.BlockingTraits != null)
                {
                    if (traitextension.BlockingTraits.Count == 0)
                    {
                        Log.Warning("Config error: " + Class.defName + " has BlockingTraits with none listed.");
                    }
                    else
                    {
                        foreach (TraitDef t in traitextension.BlockingTraits)
                        {
                            if (p.story.traits.HasTrait(t))
                            {
                                validClass = false;
                                break;
                            }
                        }
                    }
                }
                if (traitextension.BlockingHediffs != null)
                {
                    if (traitextension.BlockingHediffs.Count == 0)
                    {
                        Log.Warning("Config error: " + Class.defName + " has BlockingHediffs with none listed.");
                    }
                    else
                    {
                        foreach (HediffDef hediff in traitextension.BlockingHediffs)
                        {
                            if (p.health.hediffSet.HasHediff(hediff))
                            {
                                validClass = false;
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
                        Log.Warning("Config error: " + Class.defName + " has RequiredTrait with none listed.");
                    }
                    else
                    {
                        foreach (TraitDef T in traitextension.RequiredTrait)
                        {
                            if (p.story.traits.HasTrait(T) && !traitextension.AllRequiredTraitsNeeded)
                            {
                                hasanyTrait = true;
                                break;
                            }
                            if (p.story.traits.HasTrait(T) && traitextension.AllRequiredTraitsNeeded)
                            {
                                hasanyTrait = false;
                                break;
                            }
                        }
                    }
                    if (validClass)
                    {
                        validClass = hasanyTrait;
                    }
                }
            }
            return validClass;
        }
        public static List<ProficiencyCategoryDef> GetMissingAbilityRequirements(TMAbilityDef ability, Pawn p)
        {
            List<ProficiencyCategoryDef> temp = new List<ProficiencyCategoryDef>();
            AbilityXPGainExtension extension = ability.GetModExtension<AbilityXPGainExtension>();
            ProficiencyComp comp = p.TryGetComp<ProficiencyComp>();
            List<ProficiencyDef> needed = extension.Relatedproficiencies;
            if (extension != null && comp != null)
            {
                List<ProficiencyDef> compareTo = comp.CompletedProficiencies;
                bool notStrict = true;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMagicClassLearning || LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMightClassLearning)
                {
                    notStrict = false;
                }
                if (notStrict)
                {
                    compareTo.AddRange(comp.AllLearnableProficiencies);
                }
                foreach (ProficiencyDef pro in needed)
                {
                    if (!compareTo.Contains(pro))
                    {
                        temp.Add(pro.category);
                    }
                }
            }
            List<ProficiencyCategoryDef> outList = temp.Distinct().ToList();
            return outList;
        }
        public static List<ProficiencyCategoryDef> GetMissingClassRequirements(TraitDef Class, Pawn p)
        {
            List<ProficiencyCategoryDef> temp = new List<ProficiencyCategoryDef>();
            ClassAutoLearnExtension extension = Class.GetModExtension<ClassAutoLearnExtension>();
            ProficiencyComp comp = p.TryGetComp<ProficiencyComp>();
            List<ProficiencyDef> needed = extension.RequiredProficiencies;
            if (extension != null && comp != null)
            {
                List<ProficiencyDef> compareTo = comp.CompletedProficiencies;
                bool notStrict = true;
                if (LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMagicClassLearning || LoadedModManager.GetMod<LLROM>().GetSettings<LLRoMSettings>().StrictMightClassLearning)
                {
                    notStrict = false;
                }
                if (notStrict)
                {
                    compareTo.AddRange(comp.AllLearnableProficiencies);
                }
                foreach (ProficiencyDef pro in needed)
                {
                    if (!compareTo.Contains(pro))
                    {
                        temp.Add(pro.category);
                    }
                }
            }
            List<ProficiencyCategoryDef> outList = temp.Distinct().ToList();
            return outList;
        }
        public static bool LearnableSkillCheck(Pawn p, ThingDef S)
        {
            List<Trait> traits = p.story.traits.allTraits;
            List<TraitDef> traitdefs = new List<TraitDef>();
            CompAbilityUserTMBase data = p.GetComp<CompAbilityUserTMBase>();
            TM_CustomClass customclass = data.customClass;
            foreach (Trait t in traits)
            {
                traitdefs.Add(t.def);
            }
            if (TM_Data.RestrictedAbilities.Contains(S) && customclass != null && !customclass.learnableSkills.Contains(S))
            {
                return false;
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
                return true;
            }
            if ((S.defName == "SkillOf_Legion") && (!traitdefs.Contains(TorannMagicDefOf.Faceless) || p.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent))
            {
                return true;
            }
            if ((S.defName == "SkillOf_Sprint") && !traitdefs.Contains(TorannMagicDefOf.Gladiator))
            {
                return true;
            }
            if (TM_Data.RestrictedAbilities.Contains(S))
            {
                return false;
            }
            return true;
        }
        public static bool LearnableSpellCheck(Pawn p, ThingDef S)
        {
            List<Trait> traits = p.story.traits.allTraits;
            List<TraitDef> traitdefs = new List<TraitDef>();
            CompAbilityUserTMBase data = p.GetComp<CompAbilityUserTMBase>();
            TM_CustomClass customclass = data.customClass;
            foreach (Trait t in traits)
            {
                traitdefs.Add(t.def);
            }
            if (TM_Data.RestrictedAbilities.Contains(S) && customclass != null && !customclass.learnableSpells.Contains(S))
            {
                return false;
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
                        break;
                    }
                }
                if (!check) { return false; }
            }
            if (S == TorannMagicDefOf.SpellOf_LivingWall && !customclass.classAbilities.Contains(TorannMagicDefOf.TM_LivingWall))
            {
                return false;
            }
            if ((S == TorannMagicDefOf.SpellOf_DryGround || S == TorannMagicDefOf.SpellOf_Firestorm || S == TorannMagicDefOf.SpellOf_HeatShield || S == TorannMagicDefOf.SpellOf_CauterizeWound) && traitdefs.Contains(TorannMagicDefOf.InnerFire))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_Blizzard || S == TorannMagicDefOf.SpellOf_WetGround) && traitdefs.Contains(TorannMagicDefOf.HeartOfFrost))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_ChargeBattery || S == TorannMagicDefOf.SpellOf_EyeOfTheStorm) && traitdefs.Contains(TorannMagicDefOf.StormBorn))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_FoldReality) && traitdefs.Contains(TorannMagicDefOf.Arcanist))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_Resurrection) && traitdefs.Contains(TorannMagicDefOf.Priest))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_BattleHymn) && traitdefs.Contains(TorannMagicDefOf.TM_Bard))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_HolyWrath) && traitdefs.Contains(TorannMagicDefOf.Paladin))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_LichForm) && traitdefs.Contains(TorannMagicDefOf.Necromancer))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_SummonPoppi) && traitdefs.Contains(TorannMagicDefOf.Summoner))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_Scorn) && traitdefs.Contains(TorannMagicDefOf.Succubus))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_PsychicShock) && traitdefs.Contains(TorannMagicDefOf.Warlock))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_Meteor) && traitdefs.Contains(TorannMagicDefOf.Geomancer))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_Recall) && traitdefs.Contains(TorannMagicDefOf.Chronomancer))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_RegrowLimb || S == TorannMagicDefOf.SpellOf_BriarPatch || S == TorannMagicDefOf.SpellOf_FertileLands) && traitdefs.Contains(TorannMagicDefOf.Druid))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_OrbitalStrike || S == TorannMagicDefOf.SpellOf_MechaniteReprogramming || S == TorannMagicDefOf.SpellOf_Overdrive || S == TorannMagicDefOf.SpellOf_Sabotage || S == TorannMagicDefOf.SpellOf_TechnoShield) && traitdefs.Contains(TorannMagicDefOf.Technomancer))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_BlankMind || S == TorannMagicDefOf.SpellOf_Shapeshift) && traitdefs.Contains(TorannMagicDefOf.Enchanter))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_BloodMoon) && traitdefs.Contains(TorannMagicDefOf.BloodMage))
            {
                return true;
            }
            if ((S == TorannMagicDefOf.SpellOf_LightningTrap || S == TorannMagicDefOf.SpellOf_ArcaneBolt) && p.story.DisabledWorkTagsBackstoryAndTraits != WorkTags.Violent)
            {
                return true;
            }
            if (TM_Data.RestrictedAbilities.Contains(S))
            {
                return false;
            }
            return true;
        }
    }
}
