using BetterPrerequisites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace HALI_RandomGenetics
{
    public class FilterList
    {
        public List<string> exclusionTags;
        public int minMetabolism = -999;
        public int maxMetabolism = 999;
        public int minComplexity = -999;
        public int maxComplexity = 999;
        public int minArchite = 0;
        public int maxArchite = 0;
        public int filler = 0;
        public bool canHavePrerequisite = false;
        public List<string> needsPrerequisite;
        public bool canHaveAbility = true;
        public bool needsAbility = false;
        public List<GeneDef> excluded;
        public String defType = "Verse.GeneDef";
        public int weight = 1;
        public int timesToPerform = 1;

        protected internal int cachedTotal = -1;
        protected internal List<GeneDef> possibleVals;
        protected internal bool valsCalculated = false;
        protected internal int totalPossibilities = 0;

        public IEnumerable<DefHyperlink> MyGeneDefHyperlinks
        {
            get
            {
                if (possibleVals.Count <= 500)
                {
                    for (int i = 0; i < possibleVals.Count; i++)
                    {
                        yield return new DefHyperlink(possibleVals[i]);
                    }
                }
            }
        }

        public string GetPossibleValsAsText
        {
            get
            {
                string vals = "\n";
                if (possibleVals.Count > 500)
                {
                    for (int i = 0; i < possibleVals.Count; i++)
                    {
                        vals += "- " + possibleVals[i].LabelCap + "\n";
                    }
                }
                return vals;
            }
        }

        public string FilterChance => "HALI_RG_EachGeneHas".Translate(totalPossibilities);

        public string RepetitionTimes => timesToPerform > 1 ? "\n" + "HALI_RG_TimesAssigned".Translate(timesToPerform) : "";


        public int GetChance => (int)(((float)possibleVals.Count / totalPossibilities) * 100);

        public bool VerifyValues()
        {

            if (valsCalculated)
            {
                return !possibleVals.Any();
            }

            if (canHaveAbility == false && needsAbility == true)
            {
                Log.Error("Random Genetics Framework encountered an error. You have canHaveAbility = false and needsAbility = true" +
                    "\nThis is not possible. This is found on filter: " + ToString());
                valsCalculated = true;
                return false;
            }
            if (canHavePrerequisite == false && needsPrerequisite != null)
            {
                Log.Error("Random Genetics Framework encountered an error. You have canHavePrerequisite = false and needsPrerequisite is empty" +
                    "\nThis is not possible. This is found on filter: " + ToString());
                valsCalculated = true;
                return false;
            }


            possibleVals = DefDatabase<GeneDef>.AllDefsListForReading
            .Where(g =>
            g?.biostatMet >= minMetabolism &&
            g?.biostatMet <= maxMetabolism &&
            g?.biostatCpx >= minComplexity &&
            g?.biostatCpx <= maxComplexity &&
            g?.biostatArc >= minArchite &&
            g?.biostatArc <= maxArchite &&

            //If the pawn has an ability
            (g?.abilities?.Any() == true ? canHaveAbility : !needsAbility) &&

            CheckPrerequisites(g) &&

            g.GetType().ToString().Equals(defType) &&
            CheckExclusionTags(g) &&
            (excluded == null ? true : excluded.Contains(g) == false)
            ).ToList();

            valsCalculated = true;
            if (possibleVals.Count == 0)
            {

                Log.Warning("Random Genetics Framework encountered an error. There were no genes found for filter:" + ToString());
                exclusionTags = null;
                needsPrerequisite = null;
                excluded = null;

                return false;
            }
            exclusionTags = null;
            needsPrerequisite = null;
            excluded = null;
            totalPossibilities = possibleVals.Count + filler;

            return true;
        }

        public override string ToString()
        {
            string tostringText = "\nexclusionTags = ";

            if (exclusionTags != null)
            {
                tostringText += String.Join(", ", exclusionTags.ToArray());
            }
            else
            {
                tostringText += "null";
            }

            tostringText += "\nminMetabolism = " + minMetabolism +
                        "\nmaxMetabolism = " + maxMetabolism +
                        "\nminComplexity = " + minComplexity +
                        "\nmaxComplexity = " + maxComplexity +
                        "\nminArchite = " + minArchite +
                        "\nmaxArchite = " + maxArchite +
                        "\ncanHavePrerequisite = " + canHavePrerequisite +
                        "\nneedsPrerequisite = ";
            if (needsPrerequisite != null)
            {
                tostringText += String.Join(", ", needsPrerequisite.ToArray());
            }
            else
            {
                tostringText += "null";
            }
            tostringText += "\ncanHaveAbility = " + canHaveAbility +
                        "\nneedsAbility = " + needsAbility +
                        "\ndefType = " + defType 
                        //"\nexcluded = "
                        ;
            /*
            if (excluded != null)
            {
                tostringText += String.Join(", ", excluded.ToArray());
            }
            else
            {
                tostringText += "null";
            }*/
            return tostringText;
        }

        /// <summary>
        /// Checks for exclusion tags in the gene and our filter to see if they match
        /// </summary>
        /// <param name="g"></param>
        /// <returns>true if there is a match or <exclusionTags /> is blank</returns>
        private bool CheckExclusionTags(GeneDef g)
        {
            if (exclusionTags == null)
            {
                return true;
            }
            if (g?.exclusionTags == null)
            {
                return false;
            }
            return (exclusionTags.Intersect(g.exclusionTags).Any());
        }

        private bool CheckPrerequisites(GeneDef g)
        {
            //This is for if the pawn can have a prerequsite only
            if (needsPrerequisite == null)
            {
                if (canHavePrerequisite == true)
                {
                    return true;
                }
            }

            //This is for if the gene can't have a prerequisite and it does
            if (canHavePrerequisite == false)
            {
                if (g?.prerequisite != null)
                {

                    return false;
                }
                if (ModsConfig.IsActive("redmattis.betterprerequisites"))
                {
                    return CheckBetterPrerequisitesEmpty(g);
                    
                }
                return true;
            }

            //At this point, we know needsPrerequisite is not null and we know canHavePrerequisite is true
            //Need to see if gene does not have prerequisites and betterPrerequisites is empty
            if (g?.prerequisite == null)
            {
                if (ModsConfig.IsActive("redmattis.betterprerequisites"))
                {
                    return (CheckBetterPrerequisites(g));
                }
                return false;
            }

            //Now need to see if genes prerequisite matches one of filters
            if (needsPrerequisite.Contains(g.prerequisite.defName))
            {
                if (ModsConfig.IsActive("redmattis.betterprerequisites"))
                {
                    if (CheckBetterPrerequisitesEmpty(g) == false)
                    {
                        return CheckBetterPrerequisites(g);
                    }
                    else
                    {
                        return true;
                    }
                }
                return true;
            }
            else
            {
                if (ModsConfig.IsActive("redmattis.betterprerequisites"))
                {
                    return CheckBetterPrerequisites(g);
                }
            }
            //If no match was found return false
            return false;
        }

        /// <summary>
        /// Checks to see if GenePrerequisites has any prerequisites stored.
        /// </summary>
        /// <param name="g"></param>
        /// <returns>returns true if empty</returns>
        private bool CheckBetterPrerequisitesEmpty(GeneDef g)
        {
            if (!g.HasModExtension<BetterPrerequisites.GenePrerequisites>())
            {
                return true;
            }
            GenePrerequisites betterPrereqs = g.GetModExtension<BetterPrerequisites.GenePrerequisites>();

            return betterPrereqs?.prerequisiteSets == null;
        }

        /// <summary>
        /// Code was stolen with permission from RedMattis' Big and Small - Framework
        /// Will check to see if betterPrerequisites has a gene and if it matches our prerequisite list
        /// </summary>
        /// <param name="g"></param>
        /// <returns>True if prerequisites match</returns>
        private bool CheckBetterPrerequisites(GeneDef g)
        {
            if (!g.HasModExtension<BetterPrerequisites.GenePrerequisites>())
            {
                return false;
            }
            GenePrerequisites betterPrereqs = g.GetModExtension<BetterPrerequisites.GenePrerequisites>();

            if (betterPrereqs?.prerequisiteSets == null)
            {
                //we don't care if it's empty
                return true;
            }

            foreach (var prerequisiteSet in betterPrereqs.prerequisiteSets)
            {
                if (prerequisiteSet.prerequisites != null)
                {
                    bool result = false;
                    switch (prerequisiteSet.type)
                    {
                        //Need to see if the prerequisite is found in any of these cases
                        case PrerequisiteSet.PrerequisiteType.AnyOf:

                            result = prerequisiteSet.prerequisites.Any(geneName => needsPrerequisite.Any(y => y == geneName));
                            break;
                        case PrerequisiteSet.PrerequisiteType.AllOf:
                            result = prerequisiteSet.prerequisites.All(geneName => needsPrerequisite.Any(y => y == geneName));
                            break;
                            /*
                             * We don't want to check for NoneOf
                        case PrerequisiteSet.PrerequisiteType.NoneOf:
                            result = prerequisiteSet.prerequisites.All(geneName => prerequisite.All(y => y != geneName));
                            break;
                            */
                    }
                    if (!result)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            for (int i = 0; i < timesToPerform; i++)
            {
                int chanceValue = Rand.Range(0, weight + filler);
                if (chanceValue < weight)
                {
                    pawn.genes.AddGene(possibleVals.RandomElement(), isXenogene);
                }
            }
            return;
        }
    }
}
