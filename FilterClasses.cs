using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

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
        public bool hasAbility = false;
        public List<string> excluded = new List<string>();
        public String defType = "Verse.GeneDef";


        protected internal int cachedTotal = 0;
        protected internal int TotalPossibilities
        {
            get
            {
                if (cachedTotal == 0)
                {
                    cachedTotal = possibleVals.Count() + filler;
                }
                return cachedTotal;
            }
        }
        protected internal List<GeneDef> possibleVals;
        protected internal bool valsCalculated = false;

        private bool genValue()
        {
            if (valsCalculated == false)
            {
                possibleVals = DefDatabase<GeneDef>.AllDefsListForReading
                .Where(g =>
                //g.geneClass.GetType()==GeneDef &&
                g?.biostatMet >= minMetabolism &&
                g?.biostatMet <= maxMetabolism &&
                g?.biostatCpx >= minComplexity &&
                g?.biostatCpx <= maxComplexity &&
                g?.biostatArc >= minArchite &&
                g?.biostatArc <= maxArchite &&
                (hasAbility ? g?.abilities?.Any() == true : true) &&
                (hasPrerequisite ? g?.prerequisite?.defName.Equals(prerequisite) == true : true) &&
                g.GetType().ToString().Equals(defType) &&
                (exclusionTags.Empty() ? true : (g?.exclusionTags?.Any() == true ? g.exclusionTags.Intersect(exclusionTags).Any() == true : false)) &&

                excluded?.Contains(g.defName) == false
                ).ToList();
                
                
                valsCalculated = true;
                if (possibleVals.Count == 0)
                {
                    Log.WarningOnce("Random Genetics Framework encountered an error. There were no genes found for filter: " +
                        "\nexclusionTags = " + String.Join(", ", exclusionTags.ToArray()) +
                        "\nminMetabolism = " + minMetabolism +
                        "\nmaxMetabolism = " + maxMetabolism +
                        "\nminComplexity = " + minComplexity +
                        "\nmaxComplexity = " + maxComplexity +
                        "\nminArchite = " + minArchite +
                        "\nmaxArchite = " + maxArchite +
                        "\nhasPrerequisite = " + hasPrerequisite +
                        "\nprerequisite = " + prerequisite +
                        "\nhasAbility = " + hasAbility +
                        "\ndefType = " + defType +
                        "\nexcluded =" + String.Join(", ", excluded.ToArray())
                        , this.GetHashCode()
                        );

                    return false;
                }

            }
            return true;
        }
        public void getValue(Pawn pawn, bool isXenogene)
        {
            if (genValue() == false)
            {
                return;
            }

            //this is for fillers within the filter gene itself.
            //used if 
            int Rvalue = Rand.Range(0, TotalPossibilities);
            if (Rvalue < possibleVals.Count)
            {
                //Log.Message("Adding the random gene " + possibleVals[Rvalue]);

                pawn.genes.AddGene(possibleVals[Rvalue], isXenogene);

                //pawn.genes.RemoveGene(this);
                return;
            }
            else
            {
                //this reached a filler
                //pawn.genes.RemoveGene(this);
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


        protected internal List<GeneDef> matchingColors;
        protected internal bool filterCalculated = false;

        public bool GenValue()
        {
            if (filterCalculated == false)
            {
                if (hairColor == skinColor)
                {

                    Log.Error("error with ColorFilterList, <hairColor> or <skinColor> needs to be selected");

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
                    filterCalculated = true;

                    if (matchingColors.Count == 0)
                    {
                        Log.Warning("No similar HairColor were found for ColorFilterList with conditions ");
                    }

                }
                else
                {

                    matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g?.exclusionTags?.Contains("SkinColorOverride") == true &&
                    SimilarColor(g.skinColorOverride.Value, colorToCheck, toleranceLevel)

                    && (excluded?.Any() == true ? (excluded.Contains(g.defName) == false) : true) == true
                    ).ToList();

                    filterCalculated = true;
                    if (matchingColors.Count == 0)
                    {
                        Log.Warning("No similar SkinColorOverride were found for ColorFilterList with conditions ");
                    }
                }


            }
            return filterCalculated;

        }
        public bool SimilarColor(Color geneColor, Color filterColor, float toleranceLevel)
        {

            return
                Math.Abs(geneColor.r - filterColor.r) < toleranceLevel &&
                Math.Abs(geneColor.g - filterColor.g) < toleranceLevel &&
                Math.Abs(geneColor.b - filterColor.b) < toleranceLevel;
        }


        public void GetValue(Pawn pawn, bool isXenogene)
        {
            if (GenValue() == false)
            {
                return;
            }

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


        protected internal bool? confirmedValid;

        public void GenValue(Pawn pawn, bool isXenogene)
        {
            if (confirmedValid == null)
            {
                confirmedValid = DefDatabase<GeneDef>.AllDefsListForReading.Contains(gene);
                if (confirmedValid == false)
                {
                    Log.Warning("Unable to load gene with genedef " + gene.defName + " it was likely removed through cherrypicker or other means");
                }
            }
            if (confirmedValid == true)
            {
                pawn.genes.AddGene(gene, isXenogene);

                return;
            }
        }
    }


}
