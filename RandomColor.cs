using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HALI_RandomGenetics
{
    public class Gene_Similar_Color : DefModExtension
    {


        public List<ColorFilterList> colorFilterList;
        
        protected internal bool verifyCalculated = false;
        protected internal int filler=0;
        protected internal int totalPossibilities = 0;
        protected internal int totalWeight = 0;


        public bool VerifyValues()
        {

            if (verifyCalculated)
            {
                return true;
            }
            for (int i = colorFilterList.Count - 1; i >= 0; i--)
            {
                if (colorFilterList[i].VerifyValues() == false)
                {
                    colorFilterList.RemoveAt(i);
                }
            }
            verifyCalculated = true;
            if (colorFilterList.Count == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Generate random value.
        /// If the Value is greater than the total weight, return.
        /// If not go through for loop to find where the value is that reaches that weight.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="isXenogene"></param>
        public void AssignGenes(Pawn pawn, bool isXenogene)
        {


            int Rvalue = Rand.Range(0, totalPossibilities);

            if (Rvalue >= totalWeight)
            {
                //filler value was reached
                return;
            }
            else
            {

                int searchedweights = 0;
                for (int i = 0; i < colorFilterList.Count; i++)
                {
                    searchedweights += colorFilterList[i].weight;
                    if (Rvalue < searchedweights)
                    {
                        colorFilterList[i].AssignGenes(pawn, isXenogene);
                        break;
                    }
                }
            }

            return;
        }

    }


    public class Random_Color_List : Gene
    {
        public bool ListVerified = false;
        public override void PostAdd()
        {
            base.PostAdd();
            Gene_Similar_Color filtered = def.GetModExtension<Gene_Similar_Color>();

            if (filtered.VerifyValues())
            {
                filtered.AssignGenes(pawn, pawn.genes.IsXenogene(this));
            }
            else
            {
                Log.Warning("Random Genetics found no genes for the gene " + this.def + " " + this.Label);
            }


            pawn.genes.RemoveGene(this);
            return;
        }
    }
}