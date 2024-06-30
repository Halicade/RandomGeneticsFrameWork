using BetterPrerequisites;
using System.Collections.Generic;
using Verse;
using static HarmonyLib.Code;

namespace HALI_RandomGenetics
{



    public class GenesAndWeight
    {
        public List<GeneDef> genes;
        public List<FilterList> filterList;
        public List<ColorFilterList> colorFilterList;
        public int weight = 1;
        public int skip = 0;

        public int AssignGene(Pawn pawn, bool isXenogene)
        {
            if (genes != null)
            {
                for (int i = 0; i < genes.Count; i++)
                {
                    pawn.genes.AddGene(genes[i], isXenogene);
                }
            }

            if (filterList != null)
            {
                for (int i = 0; i < filterList.Count; i++)
                {
                    filterList[i].AssignGenes(pawn, isXenogene);
                }
            }

            if (colorFilterList != null)
            {
                for (int i = 0; i < colorFilterList.Count; i++)
                {
                    colorFilterList[i].AssignGenes(pawn, isXenogene);
                }
            }

            return skip;
        }

        public bool VerifyValue()
        {
            if (genes != null)
            {
                for (int i = genes.Count - 1; i >= 0; i--)
                {
                    if (DefDatabase<GeneDef>.AllDefsListForReading.Contains(genes[i]) == false)
                    {
                        genes.RemoveAt(i);
                    }
                }
            }

            if (filterList != null)
            {
                for (int i = filterList.Count - 1; i >= 0; i--)
                {
                    if (filterList[i].VerifyValues() == false)
                    {
                        filterList.RemoveAt(i);
                    }
                }
            }

            if (colorFilterList != null)
            {
                for (int i = colorFilterList.Count - 1; i >= 0; i--)
                {
                    if (colorFilterList[i].VerifyValues() == false)
                    {
                        colorFilterList.RemoveAt(i);
                    }
                }
            }

            if (genes?.Any() != true)
            {
                if (filterList?.Any() != true)
                {
                    if (colorFilterList?.Any() != true)
                    {
                        //The whole list is empty. Remove this.
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class GeneticList
    {
        public List<GenesAndWeight> genesAndWeight;
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

            if (genesAndWeight == null)
            {
                return false;
            }

            for (int i = genesAndWeight.Count - 1; i >= 0; i--)
            {
                totalPossibilities += genesAndWeight[i].weight;

                if (genesAndWeight[i].VerifyValue() == false)
                {

                    genesAndWeight.RemoveAt(i);

                }
            }
            verifyCalculated = true;
            totalPossibilities += filler;
            if (genesAndWeight.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// AssignGene assumes we already checked totalPossibilities for it
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="isXenogene"></param>
        public int AssignGene(Pawn pawn, bool isXenogene)
        {

            int Rvalue = Rand.Range(0, totalPossibilities);
            //Log.Message("Rvalue is " + Rvalue + "Total possibilities is " + genelist.TotalPossibilities);
            int totalweights = 0;
            for (int j = 0; j < genesAndWeight.Count; j++)
            {
                totalweights += genesAndWeight[j].weight;

                if (Rvalue < totalweights)
                {

                    return genesAndWeight[j].AssignGene(pawn, isXenogene);

                }
            }
            return 0;
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
                if (geneList[i].genesAndWeight.Count == 0)
                {
                    geneList.RemoveAt(i);
                }
            }
            verifyCalculated = true;
            return geneList.Count != 0;

        }

        public void AssignGene(Pawn pawn, bool isXenogene)
        {

            for (int i = 0; i < geneList.Count; i++)
            {

                i += geneList[i].AssignGene(pawn, isXenogene);

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

            if (multi == null)
            {
                Log.Error("Unable to find modExtensions \"HALI_RandomGenetics.Multi_Rand_List\" for " + this.def + " " + this.Label);
                return;
            }

            if (multi.VerifyValues())
            {
                multi.AssignGene(pawn, pawn.genes.IsXenogene(this));

            }
            else
            {
                Log.Warning("Random Genetics found no genes for the gene " + this.def + " " + this.Label);
            }

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
