using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{
    public class Gene_Similar_Color : DefModExtension
    {


        public List<ColorFilterList> colorFilterList;

        protected internal bool verifyCalculated = false;
        protected internal int filler = 0;
        protected internal int totalPossibilities = 0;


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
                    totalPossibilities += colorFilterList[i].weight;
                    colorFilterList.RemoveAt(i);
                }
            }
            verifyCalculated = true;
            totalPossibilities += filler;
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
            //Log.Message("Rvalue is " + Rvalue + "Total possibilities is " + genelist.TotalPossibilities);
            int totalweights = 0;
            for (int j = 0; j < colorFilterList.Count; j++)
            {
                totalweights += colorFilterList[j].weight;

                if (Rvalue < totalweights)
                {

                    colorFilterList[j].AssignGenes(pawn, isXenogene);
                    return;

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
            if (filtered == null)
            {
                Log.Error("Unable to find modExtensions \"HALI_RandomGenetics.Gene_Similar_Color\" for " + this.def + " " + this.Label);
                return;
            }

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