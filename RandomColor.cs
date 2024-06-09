using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HALI_RandomGenetics
{
    public class Gene_Similar_Color : DefModExtension
    {
        public bool hairColor = false;
        public bool skinColor = false;
        public Color colorToCheck;
        public float toleranceLevel = 0.30f;
        public List<string> excluded;


        protected internal List<GeneDef> matchingColors;
        protected internal bool filterCalculated = false;

    }


    public class Gene_Random_Color : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            Gene_Similar_Color filtered = def.GetModExtension<Gene_Similar_Color>();

            if (filtered.filterCalculated == false)
            {
                if (filtered.hairColor == filtered.skinColor)
                {
                    Log.Error("error with " + this.def + ", " + this.Label + "<hairColor> or <skinColor> needs to be selected");
                    pawn.genes.RemoveGene(this);
                    return;
                }

                if (filtered.hairColor)
                {

                    filtered.matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g?.exclusionTags?.Contains("HairColor") == true &&
                    SimilarColor(g.hairColorOverride.Value, filtered.colorToCheck, filtered.toleranceLevel)

                    && (filtered?.excluded?.Any() == true ? (filtered.excluded.Contains(g.defName) == false) : true) == true

                    ).ToList();
                    //Log.Error("excluded is used? " + (filtered?.excluded?.Any() == true));
                    filtered.filterCalculated = true;

                }
                else
                {

                    filtered.matchingColors = DefDatabase<GeneDef>.AllDefsListForReading
                    .Where(g =>
                    g?.exclusionTags?.Contains("SkinColorOverride") == true &&
                    SimilarColor(g.skinColorOverride.Value, filtered.colorToCheck, filtered.toleranceLevel)

                    && (filtered?.excluded?.Any() == true ? (filtered.excluded.Contains(g.defName) == false) : true) == true
                    ).ToList();

                    filtered.filterCalculated = true;
                }


            }

            if (filtered.matchingColors.Any())
            {
                pawn.genes.AddGene(filtered.matchingColors.RandomElement(), pawn.genes.IsXenogene(this));
                pawn.genes.RemoveGene(this);
                return;
            }
            else
            {
                Log.Error("No compatible colors were found for " + this.def + ", " + this.Label);
                pawn.genes.RemoveGene(this);
                return;
            }
        }

        public bool SimilarColor(Color geneColor, Color filterColor, float toleranceLevel)
        {

            return
                Math.Abs(geneColor.r - filterColor.r) < toleranceLevel &&
                Math.Abs(geneColor.g - filterColor.g) < toleranceLevel &&
                Math.Abs(geneColor.b - filterColor.b) < toleranceLevel;
        }
    }
}