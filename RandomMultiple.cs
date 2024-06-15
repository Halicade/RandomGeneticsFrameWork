using System.Collections.Generic;
using Verse;
using static HarmonyLib.Code;

namespace HALI_RandomGenetics
{



    public class GeneAndWeight
    {
        public GeneDef gene;
        public int weight = 1;

        public void AssignGene(Pawn pawn, bool isXenogene)
        {
            pawn.genes.AddGene(gene, isXenogene);
            return;

        }

        public bool VerifyValue()
        {
            return DefDatabase<GeneDef>.AllDefsListForReading.Contains(gene);
        }
    }

    public class GeneticList
    {
        public List<GeneAndWeight> geneAndWeight;
        public int filler = 0;


        protected internal int totalPossibilities = 0;
        protected internal bool verifyCalculated = false;

        /// <summary>
        /// Goes through the geneAndWeight list and makes sure each entry is valid.
        /// will then calculate the total possibilities available
        /// </summary>
        /// <returns>True if this was already ran. False if first run and there are no objects in list.</returns>
        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }

            for (int i = geneAndWeight.Count - 1; i >= 0; i--)
            {
                totalPossibilities += geneAndWeight[i].weight;

                if (geneAndWeight[i].VerifyValue() == false)
                {

                    geneAndWeight.RemoveAt(i);

                }
            }
            verifyCalculated = true;
            totalPossibilities += filler;
            if (geneAndWeight.Count == 0)
            {
                return false;
            }

            return true; ;
        }

        /// <summary>
        /// AssignGene assumes we already checked totalPossibilities for it
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="isXenogene"></param>
        public void AssignGene(Pawn pawn, bool isXenogene)
        {

            int Rvalue = Rand.Range(0, totalPossibilities);
            //Log.Message("Rvalue is " + Rvalue + "Total possibilities is " + genelist.TotalPossibilities);
            int totalweights = 0;
            for (int j = 0; j < geneAndWeight.Count; j++)
            {
                totalweights += geneAndWeight[j].weight;

                if (Rvalue < totalweights)
                {

                    geneAndWeight[j].AssignGene(pawn, isXenogene);
                    break;
                }
            }
        }
    }

    public class Multi_Rand_List : DefModExtension
    {
        //public List<List<GeneDef>> genes;
        public List<GeneticList> geneList;
        private bool verifyCalculated = false;



        /// <summary>
        /// Will remove a geneList entry if all lists are blank
        /// </summary>
        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }

            for (int i = geneList.Count - 1; i >= 0; i--)
            {
                geneList[i].VerifyValues();
                if (geneList[i].geneAndWeight.Count == 0)
                {
                    geneList.RemoveAt(i);
                }
            }
            verifyCalculated = true;
            if (geneList.Count == 0)
            {
                return false;
            }

            return true;
        }

        public void AssignGene(Pawn pawn, bool isXenogene)
        {

            for (int i = 0; i < geneList.Count; i++)
            {

                geneList[i].AssignGene(pawn, isXenogene);

            }
        }
    }

    public class Multiple_Random_Lists : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();

            Multi_Rand_List multi = def.GetModExtension<Multi_Rand_List>();
            //Log.Message("Filler is " + multi.geneList[0].filler+ multi.geneList[0].geneAndWeight[0].weight);
            //multi.AssignGene(pawn, pawn.genes.IsXenogene(this));


            if (multi.VerifyValues())
            {
                multi.AssignGene(pawn, pawn.genes.IsXenogene(this));

            }
            else
            {
                Log.Warning("Random Genetics found no genes for the gene " + this.def + " " + this.Label);
            }


            /*
            for (int i = 0; i < multi.geneList.Count; i++)
            {
                
                multi.geneList[i].AssignGene(pawn, pawn.genes.IsXenogene(this));
            }
            */

            pawn.genes.RemoveGene(this);
            return;

        }

    }



    /*
    Unused for now


    [StaticConstructorOnStartup]
    public static class Pregens
    {
        public static IEnumerable<GeneDef> warmTails = DefDatabase<GeneDef>.AllDefsListForReading
            .Where(g => g?.biostatMet == -1 && g?.biostatCpx == 1 && g?.exclusionTags?.Contains("Tail") == true &&
            g?.statOffsets?.StatListContains(StatDefOf.ComfyTemperatureMin) == true
            );

        public static IEnumerable<GeneDef> manipTails = DefDatabase<GeneDef>.AllDefsListForReading
            .Where(g => g?.biostatMet == -1 && g?.biostatCpx == 1 && g?.exclusionTags?.Contains("Tail") == true && g?.capMods?.Where(a => a.capacity == PawnCapacityDefOf.Manipulation) != null);

   
    }




    public class RandomWarmTailGene : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();


            pawn.genes.AddGene(Pregens.warmTails.RandomElement(), pawn.genes.IsXenogene(this));
            pawn.genes.RemoveGene(this);
        }
    }
    public class RandomManipTailGene : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            pawn.genes.AddGene(Pregens.manipTails.RandomElement(), pawn.genes.IsXenogene(this));
            pawn.genes.RemoveGene(this);
        }
    }
 
    */


}
