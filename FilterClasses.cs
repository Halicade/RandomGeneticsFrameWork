using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static RimWorld.FleshTypeDef;
using Verse.Noise;

namespace HALI_RandomGenetics
{

    public class FilterList
    {
        public List<string> exclusionTags = new List<string>();
        public int minMetabolism = -999;
        public int maxMetabolism = 999;
        public int minComplexity = -999;
        public int maxComplexity = 999;
        public int minArchite = 0;
        public int maxArchite = 0;
        public int filler = 0;
        public bool hasPrerequisite = false;
        public String prerequisite = "";
        public bool canHaveAbility = true;
        public bool needsAbility = false;
        public List<string> excluded = new List<string>();
        public String defType = "Verse.GeneDef";
        public int weight=1;


        protected internal int cachedTotal = -1;
        protected internal int TotalPossibilities
        {
            get
            {
                if (cachedTotal == -1)
                {
                    cachedTotal = possibleVals.Count() + filler;
                }
                return cachedTotal;
            }
        }
        protected internal List<GeneDef> possibleVals;
        protected internal bool valsCalculated = false;




        public bool VerifyValues()
        {
            if (valsCalculated)
            {
                return possibleVals.Count != 0;
            }
            else
            {

                if (canHaveAbility == false && needsAbility == true)
                {
                    Log.Error("Random Genetics Framework encountered an error. You have canHaveAbility == false and needsAbility == true" +
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

                (!hasPrerequisite || g?.prerequisite?.defName.Equals(prerequisite) == true) &&
                g.GetType().ToString().Equals(defType) &&
                (exclusionTags.Empty() ? true : (g?.exclusionTags?.Any() == true ? g.exclusionTags.Intersect(exclusionTags).Any() == true : false)) &&

                excluded?.Contains(g.defName) == false
                ).ToList();


                valsCalculated = true;
                if (possibleVals.Count == 0)
                {
                    Log.Warning("Random Genetics Framework encountered an error. There were no genes found for filter:" + ToString());
                    return false;
                }

            }
            return true;
        }

        public override string ToString()
        {
            return "\nexclusionTags = " + String.Join(", ", exclusionTags.ToArray()) +
                        "\nminMetabolism = " + minMetabolism +
                        "\nmaxMetabolism = " + maxMetabolism +
                        "\nminComplexity = " + minComplexity +
                        "\nmaxComplexity = " + maxComplexity +
                        "\nminArchite = " + minArchite +
                        "\nmaxArchite = " + maxArchite +
                        "\nhasPrerequisite = " + hasPrerequisite +
                        "\nprerequisite = " + prerequisite +
                        "\ncanHaveAbility = " + canHaveAbility +
                        "\nneedsAbility = " + needsAbility +
                        "\ndefType = " + defType +
                        "\nexcluded =" + String.Join(", ", excluded.ToArray());
        }

        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            if (VerifyValues() == false)
            {
                Log.Error("Something happened with Random Genetics and getValue was ran before VerifyValues. Please let the auther know " + ToString());
                return;
            }


            int Rvalue = Rand.Range(0, TotalPossibilities);
            if (Rvalue < possibleVals.Count)
            {
                
                pawn.genes.AddGene(possibleVals[Rvalue], isXenogene);

                return;
            }
            else
            {
                //this reached a filler
                return;
            }
        }
    }


    public class ColorFilterList
    {
        public bool hairColor = false;
        public bool skinColor = false;
        public Color colorToCheck;
        public float toleranceLevel = 0.30f;
        public List<string> excluded;
        public int filler = 0;
        public int weight = 1;

        private int cachedTotal = -1;
        protected internal int TotalPossibilities
        {
            get
            {
                if (cachedTotal == -1)
                {
                    cachedTotal = matchingColors.Count() + filler;
                }
                return cachedTotal;
            }
        }
        protected internal List<GeneDef> matchingColors;
        protected internal bool filterCalculated = false;

        /// <summary>
        /// Will generate to see how many values are created.
        /// </summary>
        /// <returns>false if there are no values in matchingColors</returns>
        public bool VerifyValues()
        {
            if (filterCalculated == false)
            {
                if (hairColor == skinColor)
                {

                    Log.Error("error with ColorFilterList, <hairColor> or <skinColor> needs to be selected");
                    filterCalculated = true;
                    return false;
                }

                if (hairColor)
                {
                    //DefDatabase<GeneDef>;
                    matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g?.exclusionTags?.Contains("HairColor") == true &&
                    SimilarColor(g.hairColorOverride.Value, colorToCheck, toleranceLevel)

                    && (excluded?.Any() == true ? (excluded.Contains(g.defName) == false) : true) == true

                    ).ToList();
                    //Log.Error("excluded is used? " + (filtered?.excluded?.Any() == true

                    if (matchingColors.Count == 0)
                    {
                        Log.Warning("No similar HairColor were found for ColorFilterList with conditions ");
                        return false;
                    }
                    filterCalculated = true;


                }
                else
                {

                    matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g?.exclusionTags?.Contains("SkinColorOverride") == true &&
                    SimilarColor(g.skinColorOverride.Value, colorToCheck, toleranceLevel)

                    && (excluded?.Any() == true ? (excluded.Contains(g.defName) == false) : true) == true
                    ).ToList();


                    if (matchingColors.Count == 0)
                    {
                        Log.Warning("No similar SkinColorOverride were found for ColorFilterList with conditions ");
                        return false;
                    }
                    filterCalculated = true;
                }


            }
            return filterCalculated;

        }
        private bool SimilarColor(Color geneColor, Color filterColor, float toleranceLevel)
        {

            return
                Math.Abs(geneColor.r - filterColor.r) < toleranceLevel &&
                Math.Abs(geneColor.g - filterColor.g) < toleranceLevel &&
                Math.Abs(geneColor.b - filterColor.b) < toleranceLevel;
        }


        public void AssignGenes(Pawn pawn, bool isXenogene)
        {

            //Needs to be rewritten to add filter
            if (matchingColors.Any())
            {
                pawn.genes.AddGene(matchingColors.RandomElement(), isXenogene);

                return;
            }
            else
            {
                Log.Error("No compatible colors were found for INSERT TOSTRING HERE");

                return;

            }
        }
    }


    public class GeneList
    {
        public GeneDef gene;


        protected internal bool confirmedValid;

        public bool VerifyValues()
        {
            if (confirmedValid == false)
            {
                confirmedValid = DefDatabase<GeneDef>.AllDefsListForReading.Contains(gene);
                if (confirmedValid == false)
                {
                    Log.Warning("Unable to load gene with genedef " + gene.defName + " it was likely removed through cherrypicker or other means");
                    return false;
                }
            }
            return true;

        }

        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            if (confirmedValid)
            {
                pawn.genes.AddGene(gene, isXenogene);

                return;
            }
        }
    }


}
