using BetterPrerequisites;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HALI_RandomGenetics
{

    /// <summary>
    /// Contains filter for filtering genes
    /// </summary>
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
        public List<string> excluded;
        public String defType = "Verse.GeneDef";
        public int weight = 1;
        public int timesToPerform = 1;

        protected internal int cachedTotal = -1;
        protected internal List<GeneDef> possibleVals;
        protected internal bool valsCalculated = false;


        public bool VerifyValues()
        {

            if (valsCalculated)
            {
                return possibleVals.Count != 0;
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

            (excluded == null ? true : excluded?.Contains(g.defName) == false)
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
                        "\ndefType = " + defType +
                        "\nexcluded = ";
            if (excluded != null)
            {
                tostringText += String.Join(", ", excluded.ToArray());
            }
            else
            {
                tostringText += "null";
            }
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
                    if (CheckBetterPrerequisitesEmpty(g))
                    {
                        return true;
                    }
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

            //Log.Message("Checking better prerequisites");

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
                int Rvalue = Rand.Range(0, possibleVals.Count() + filler);
                if (Rvalue < possibleVals.Count)
                {
                    pawn.genes.AddGene(possibleVals[Rvalue], isXenogene);

                }

            }
            return;
        }
    }


    public class ColorFilterList
    {

        public Color colorToCheck;
        public float toleranceLevel = 0.30f;
        public List<string> excluded;
        public int filler = 0;
        public int weight = 1;
        public String defType = "Verse.GeneDef";
        public int timesToPerform = 1;
        public ColorType colorType;
        public SimilarBy similarBy = 0;

        public enum ColorType
        {
            HairColor, SkinColor
        }

        public enum SimilarBy
        {
            RGB, Hue, Sat, Val, SatVal
        }

        protected internal int totalPossibilities = 0;
        protected internal List<GeneDef> matchingColors;
        protected internal bool filterCalculated = false;


        public bool VerifyValues()
        {
            if (filterCalculated)
            {
                return true;
            }

            switch (colorType)
            {
                case ColorType.HairColor:

                    matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g.biostatMet == 0 &&
                    g.biostatCpx == 0 &&
                    g?.exclusionTags?.Contains("HairColor") == true &&
                    g.hairColorOverride.HasValue &&
                    SimilarColor(g.hairColorOverride.Value, colorToCheck, toleranceLevel)
                    && (excluded?.Any() == true ? (excluded.Contains(g.defName) == false) : true) == true

                    ).ToList();
                    filterCalculated = true;
                    if (matchingColors.Count == 0)
                    {
                        Log.Warning("No similar HairColor were found for ColorFilterList with conditions " + ToString());
                        return false;
                    }
                    totalPossibilities = matchingColors.Count + filler;
                    return filterCalculated;

                case ColorType.SkinColor:

                    matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g.biostatMet == 0 &&
                    g.biostatCpx == 0 &&
                    g?.exclusionTags?.Contains("SkinColorOverride") == true &&
                    g.skinColorOverride.HasValue &&
                    SimilarColor(g.skinColorOverride.Value, colorToCheck, toleranceLevel)

                    && (excluded?.Any() == true ? (excluded.Contains(g.defName) == false) : true) == true
                    ).ToList();

                    filterCalculated = true;
                    if (matchingColors.Count == 0)
                    {
                        Log.Warning("No similar SkinColorOverride were found for ColorFilterList with conditions " + ToString());
                        return false;
                    }
                    totalPossibilities = matchingColors.Count + filler;
                    return filterCalculated;
                default:
                    Log.Error("You entered the wrong tag for <colorType> it should be <colorType>SkinColor</colorType> or <colorType>HairColor</colorType>" + ToString());
                    return false;
            }

        }

        public override string ToString()
        {
            if (excluded == null)
            {
                return
                    "\ncolorType = " + colorType.ToString() +
                    "\ncolorToCheck = " + colorToCheck.ToString() +
                    "\nsimilarBy = " + similarBy.ToString() +
                    "\ntoleranceLevel = " + toleranceLevel +
                    "\nfiller = " + filler +
                    "\nweight = " + weight +
                    "\ndefType = " + defType +
                    "\nexcluded = null";

            }
            else
            {
                return
                    "\ncolorType = " + colorType.ToString() +
                    "\ncolorToCheck = " + colorToCheck.ToString() +
                    "\nsimilarBy = " + similarBy.ToString() +
                    "\ntoleranceLevel = " + toleranceLevel +
                    "\nfiller = " + filler +
                    "\nweight = " + weight +
                    "\ndefType = " + defType +
                    "\nexcluded = " + String.Join(", ", excluded.ToArray());
            }

        }

        private bool SimilarColor(Color geneColor, Color filterColor, float toleranceLevel)
        {
            float geneValue;
            float filterValue;

            switch (similarBy)
            {
                case SimilarBy.RGB:
                    return
                Math.Abs(geneColor.r - filterColor.r) < toleranceLevel &&
                Math.Abs(geneColor.g - filterColor.g) < toleranceLevel &&
                Math.Abs(geneColor.b - filterColor.b) < toleranceLevel;
                case SimilarBy.Hue:


                    Color.RGBToHSV(geneColor, out geneValue, out _, out _);
                    Color.RGBToHSV(filterColor, out filterValue, out _, out _);
                    return Math.Abs(geneValue - filterValue) < toleranceLevel;

                case SimilarBy.Sat:
                    Color.RGBToHSV(geneColor, out _, out geneValue, out _);
                    Color.RGBToHSV(filterColor, out _, out filterValue, out _);
                    return Math.Abs(geneValue - filterValue) < toleranceLevel;


                case SimilarBy.Val:
                    Color.RGBToHSV(geneColor, out _, out _, out geneValue);
                    Color.RGBToHSV(filterColor, out _, out _, out filterValue);
                    return Math.Abs(geneValue - filterValue) < toleranceLevel;

                case SimilarBy.SatVal:
                    Color.RGBToHSV(geneColor, out _, out geneValue, out _);
                    Color.RGBToHSV(filterColor, out _, out filterValue, out _);
                    if (Math.Abs(geneValue - filterValue) < toleranceLevel)
                    {
                        Color.RGBToHSV(geneColor, out _, out _, out geneValue);
                        Color.RGBToHSV(filterColor, out _, out _, out filterValue);
                        return Math.Abs(geneValue - filterValue) < toleranceLevel;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    Log.Error("You entered the wrong tag for <similarBy> it should be RGB, Hue, Sat, or Val." + ToString());
                    return false;
            }
        }


        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            for (int i = 0; i < timesToPerform; i++)
            {
                int Rvalue = Rand.Range(0, totalPossibilities);
                if (Rvalue < matchingColors.Count)
                {
                    pawn.genes.AddGene(matchingColors[Rvalue], isXenogene);
                }

            }
            return;
        }
    }
}
