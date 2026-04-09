using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorannMagic.ModOptions;
using TorannMagic.TMDefs;
using TorannMagic;
using Verse;

namespace LLRoM
{
    public class ClassLearnabilityHandler
    {
        private static List<TraitDef> _EnabledMightTraits = null;
        public static List<TraitDef> EnabledMightTraits
        {
            get
            {
                if (_EnabledMightTraits == null)
                {
                    List<TraitDef> list = new List<TraitDef>();
                    list.Clear();
                    if (Settings.Instance.Apothecary)
                    {
                        list.Add(TorannMagicDefOf.TM_Apothecary);
                    }
                    if (Settings.Instance.Sniper)
                    {
                        list.Add(TorannMagicDefOf.TM_Sniper);
                    }
                    if (Settings.Instance.Bladedancer)
                    {
                        list.Add(TorannMagicDefOf.Bladedancer);
                    }
                    if (Settings.Instance.Ranger)
                    {
                        list.Add(TorannMagicDefOf.Ranger);
                    }
                    if (Settings.Instance.Faceless)
                    {
                        list.Add(TorannMagicDefOf.Faceless);
                    }
                    if (Settings.Instance.Psionic)
                    {
                        list.Add(TorannMagicDefOf.TM_Psionic);
                    }
                    if (Settings.Instance.DeathKnight)
                    {
                        list.Add(TorannMagicDefOf.DeathKnight);
                    }
                    if (Settings.Instance.Monk)
                    {
                        list.Add(TorannMagicDefOf.TM_Monk);
                    }
                    if (Settings.Instance.SuperSoldier)
                    {
                        list.Add(TorannMagicDefOf.TM_SuperSoldier);
                    }
                    if (Settings.Instance.Commander)
                    {
                        list.Add(TorannMagicDefOf.TM_Commander);
                    }
                    foreach (TM_CustomClass customClass in TM_ClassUtility.CustomClasses)
                    {
                        if (customClass.isFighter && !list.Contains(customClass.classTrait) && Settings.Instance.CustomClass[customClass.classTrait.ToString()])
                        {
                            list.Add(customClass.classTrait);
                        }
                    }
                    _EnabledMightTraits = list;
                }
                return _EnabledMightTraits;
            }
        }
        public static bool EnabledBook(ThingDef def)
        {
            TraitDef trait = null;
            foreach (TM_CustomClass customClass in TM_ClassUtility.CustomClasses)
            {
                if ((customClass.tornScript == def && customClass.tornScript != null) || (customClass.fullScript == def && customClass.fullScript != null))
                {
                    trait = customClass.classTrait;
                }
            }
            if (def == TorannMagicDefOf.Torn_BookOfInnerFire || def == TorannMagicDefOf.BookOfInnerFire)
            {
                trait = TorannMagicDefOf.InnerFire;
            }
            if (def == TorannMagicDefOf.Torn_BookOfHeartOfFrost || def == TorannMagicDefOf.BookOfHeartOfFrost)
            {
                trait = TorannMagicDefOf.HeartOfFrost;
            }
            if (def == TorannMagicDefOf.Torn_BookOfStormBorn || def == TorannMagicDefOf.BookOfStormBorn)
            {
                trait = TorannMagicDefOf.StormBorn;
            }
            if (def == TorannMagicDefOf.Torn_BookOfArcanist || def == TorannMagicDefOf.BookOfArcanist)
            {
                trait = TorannMagicDefOf.Arcanist;
            }
            if (def == TorannMagicDefOf.Torn_BookOfValiant || def == TorannMagicDefOf.BookOfValiant)
            {
                trait = TorannMagicDefOf.Paladin;
            }
            if (def == TorannMagicDefOf.Torn_BookOfSummoner || def == TorannMagicDefOf.BookOfSummoner)
            {
                trait = TorannMagicDefOf.Summoner;
            }
            if (def == TorannMagicDefOf.Torn_BookOfNature || def == TorannMagicDefOf.BookOfDruid)
            {
                trait = TorannMagicDefOf.Druid;
            }
            if (def == TorannMagicDefOf.Torn_BookOfUndead || def == TorannMagicDefOf.BookOfNecromancer)
            {
                trait = TorannMagicDefOf.Necromancer;
            }
            if (def == TorannMagicDefOf.Torn_BookOfPriest || def == TorannMagicDefOf.BookOfPriest)
            {
                trait = TorannMagicDefOf.Priest;
            }
            if (def == TorannMagicDefOf.Torn_BookOfBard || def == TorannMagicDefOf.BookOfBard)
            {
                trait = TorannMagicDefOf.TM_Bard;
            }
            if (def == TorannMagicDefOf.Torn_BookOfDemons || def == TorannMagicDefOf.BookOfDemons)
            {
                trait = TorannMagicDefOf.Succubus;
            }
            if (def == TorannMagicDefOf.Torn_BookOfEarth || def == TorannMagicDefOf.BookOfEarth)
            {
                trait = TorannMagicDefOf.Geomancer;
            }
            if (def == TorannMagicDefOf.Torn_BookOfMagitech || def == TorannMagicDefOf.BookOfMagitech)
            {
                trait = TorannMagicDefOf.Technomancer;
            }
            if (def == TorannMagicDefOf.Torn_BookOfHemomancy || def == TorannMagicDefOf.BookOfHemomancy)
            {
                trait = TorannMagicDefOf.BloodMage;
            }
            if (def == TorannMagicDefOf.Torn_BookOfEnchanter || def == TorannMagicDefOf.BookOfEnchanter)
            {
                trait = TorannMagicDefOf.Enchanter;
            }
            if (def == TorannMagicDefOf.Torn_BookOfChronomancer || def == TorannMagicDefOf.BookOfChronomancer)
            {
                trait = TorannMagicDefOf.Chronomancer;
            }
            if (def == TorannMagicDefOf.Torn_BookOfChaos || def == TorannMagicDefOf.BookOfChaos)
            {
                trait = TorannMagicDefOf.ChaosMage;
            }
            if (def == TorannMagicDefOf.Torn_BookOfTheSun || def == TorannMagicDefOf.BookOfTheSun)
            {
                trait = TorannMagicDefOf.TM_Brightmage;
            }
            if (def == TorannMagicDefOf.Torn_BookOfShamanism || def == TorannMagicDefOf.BookOfShamanism)
            {
                trait = TorannMagicDefOf.TM_Shaman;
            }
            if (def == TorannMagicDefOf.Torn_BookOfGolemancy || def == TorannMagicDefOf.BookOfGolemancy)
            {
                trait = TorannMagicDefOf.TM_Golemancer;
            }
            if (def == TorannMagicDefOf.BookOfShadow)
            {
                trait = TorannMagicDefOf.TM_TheShadow;
            }
            if (def == TorannMagicDefOf.BookOfGladiator)
            {
                trait = TorannMagicDefOf.Gladiator;
            }
            if (def == TorannMagicDefOf.BookOfPsionic)
            {
                trait = TorannMagicDefOf.TM_Psionic;
            }
            if (def == TorannMagicDefOf.BookOfFaceless)
            {
                trait = TorannMagicDefOf.Faceless;
            }
            if (def == TorannMagicDefOf.BookOfSniper)
            {
                trait = TorannMagicDefOf.TM_Sniper;
            }
            if (def == TorannMagicDefOf.BookOfBladedancer)
            {
                trait = TorannMagicDefOf.Bladedancer;
            }
            if (def == TorannMagicDefOf.BookOfRanger)
            {
                trait = TorannMagicDefOf.Ranger;
            }
            if (def == TorannMagicDefOf.BookOfDeathKnight)
            {
                trait = TorannMagicDefOf.DeathKnight;
            }
            if (def == TorannMagicDefOf.BookOfMonk)
            {
                trait = TorannMagicDefOf.TM_Monk;
            }
            if (def == TorannMagicDefOf.BookOfCommander)
            {
                trait = TorannMagicDefOf.TM_Commander;
            }
            if (def == TorannMagicDefOf.BookOfSuperSoldier)
            {
                trait = TorannMagicDefOf.TM_SuperSoldier;
            }
            if (trait != null)
            {
                return EnabledClass(trait);
            }
            return true;
        }
        public static bool EnabledClass(TraitDef trait)
        {
            if (!EnabledMightTraits.Contains(trait) && !TM_Data.EnabledMagicTraits.Contains(trait))
            {
                return false;
            }
            return true;
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
        public static bool CanLearnClass(Pawn p, TraitDef Class)
        {
            bool validClass = true;
            if (!EnabledClass(Class))
            {
                return false;
            }
            ClassAutoLearnExtension traitextension = Class.GetModExtension<ClassAutoLearnExtension>();
            if (traitextension == null) { return false; }
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
    }
}
