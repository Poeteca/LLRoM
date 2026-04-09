using LifeLessons;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLRoM
{
    public class InspiratinHandler
    {
        public static ProficiencyDef GetRandomInspirationProficiency(Pawn pawn, InspirationDef inspiration)
        {
            List<ProficiencyDef> possibleResults = new List<ProficiencyDef>();
            InspirationExtension extension = inspiration.GetModExtension<InspirationExtension>();
            if (extension != null)
            {
                foreach (ProficiencyDef pro in extension.Outcomeproficiencies)
                {
                    if (extension != null && Util.Qualification(pawn, pro.prerequisites).Allowed(true))
                    {
                        possibleResults.Add(pro);
                    }
                }
            }
            if (possibleResults.Count == 0)
            {
                Log.Error("ERROR: No Possible Inspiration Proficiencies");
                return null;
            }
            ProficiencyDef outProficiency = possibleResults.RandomElement();
            if (outProficiency == null) { Log.Error("ERROR: Random Inspiration Proficiency is null"); }
            return outProficiency;
        }
        public static List<ProficiencyDef> KnownInCatagory(Pawn pawn, ProficiencyCategoryDef category)
        {
            List<ProficiencyDef> foundProficiencies = new List<ProficiencyDef>();
            foreach (ProficiencyDef pro in DefDatabase<ProficiencyDef>.AllDefs)
            {
                if (pro.category == category && Util.IsQualified(pawn, pro))
                {
                    foundProficiencies.Add(pro);
                }
            }
            return foundProficiencies;
        }
    }
}
