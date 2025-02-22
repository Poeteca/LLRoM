using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
