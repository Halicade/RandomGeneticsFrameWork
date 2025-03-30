using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HALI_RandomGenetics
{
    public class ColorFilterList
    {

        public Color colorToCheck;
        public float toleranceLevel = 0.30f;
        public List<GeneDef> excluded;
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
            RGB, Hue, Sat, Val, SatVal, HSV
        }

        protected internal int totalPossibilities = 0;
        protected internal List<GeneDef> matchingColors;
        protected internal bool filterCalculated = false;

        public IEnumerable<DefHyperlink> GetGeneDefHyperlinks()
        {
            for (int i = 0; i < matchingColors.Count; i++)
            {
                yield return new DefHyperlink(matchingColors[i]);
            }
        }

        public string GetMatchingColorsAsText()
        {
            string vals = "\n";
            if (matchingColors.Count > 500)
            {
                for (int i = 0; i < matchingColors.Count; i++)
                {
                    vals += "- " + matchingColors[i].LabelCap + "\n";
                }
            }
            return vals;
        }
        public string ColorChance => "HALI_RG_EachGeneHas".Translate(totalPossibilities);

        public int GetChance => (int)((float)matchingColors.Count / totalPossibilities * 100);


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
                    SimilarColor(g.hairColorOverride.Value, toleranceLevel)
                    && (excluded.NullOrEmpty() ? true : (excluded.Contains(g) == false))

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
                    SimilarColor(g.skinColorOverride.Value, toleranceLevel)

                    && (excluded.NullOrEmpty() ? true : (excluded.Contains(g) == false))
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
                    "\nexcluded = ";// + String.Join(", ", excluded.ToArray());
            }
        }

        private bool SimilarColor(Color geneColor, float toleranceLevel)
        {
            float geneValue;
            float filterValue;

            switch (similarBy)
            {
                case SimilarBy.RGB:
                    return
                Math.Abs(geneColor.r - colorToCheck.r) < toleranceLevel &&
                Math.Abs(geneColor.g - colorToCheck.g) < toleranceLevel &&
                Math.Abs(geneColor.b - colorToCheck.b) < toleranceLevel;
                case SimilarBy.Hue:
                    Color.RGBToHSV(geneColor, out geneValue, out _, out _);
                    Color.RGBToHSV(colorToCheck, out filterValue, out _, out _);
                    return Math.Abs(geneValue - filterValue) < toleranceLevel;
                case SimilarBy.Sat:
                    Color.RGBToHSV(geneColor, out _, out geneValue, out _);
                    Color.RGBToHSV(colorToCheck, out _, out filterValue, out _);
                    return Math.Abs(geneValue - filterValue) < toleranceLevel;
                case SimilarBy.Val:
                    Color.RGBToHSV(geneColor, out _, out _, out geneValue);
                    Color.RGBToHSV(colorToCheck, out _, out _, out filterValue);
                    return Math.Abs(geneValue - filterValue) < toleranceLevel;
                case SimilarBy.SatVal:
                    Color.RGBToHSV(geneColor, out _, out geneValue, out _);
                    Color.RGBToHSV(colorToCheck, out _, out filterValue, out _);
                    if (Math.Abs(geneValue - filterValue) < toleranceLevel)
                    {
                        Color.RGBToHSV(geneColor, out _, out _, out geneValue);
                        Color.RGBToHSV(colorToCheck, out _, out _, out filterValue);
                        return Math.Abs(geneValue - filterValue) < toleranceLevel;
                    }
                    return false;
                case SimilarBy.HSV:
                    //This may not work properly. Look into later
                    Color.RGBToHSV(geneColor, out geneValue, out _, out _);
                    Color.RGBToHSV(colorToCheck, out filterValue, out _, out _);
                    if (Math.Abs(geneValue - filterValue) < toleranceLevel)
                    {
                        Color.RGBToHSV(geneColor, out _, out geneValue, out _);
                        Color.RGBToHSV(colorToCheck, out _, out filterValue, out _);
                        if (Math.Abs(geneValue - filterValue) < toleranceLevel)
                        {
                            Color.RGBToHSV(geneColor, out _, out _, out geneValue);
                            Color.RGBToHSV(colorToCheck, out _, out _, out filterValue);
                            return Math.Abs(geneValue - filterValue) < toleranceLevel;
                        }
                    }
                    return false; 
                        default:
                    Log.Error("You entered the wrong tag for <similarBy> it should be RGB, Hue, Sat, Val, HSV." + ToString());
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
