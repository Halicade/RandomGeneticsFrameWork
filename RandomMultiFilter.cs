using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace HALI_RandomGenetics
{





    public class RandomFilters
    {
        public List<FilterList> filterList = new List<FilterList>();
        public List<ColorFilterList> colorFilterList = new List<ColorFilterList>();
        public List<GeneList> geneList = new List<GeneList>();

        public int weight = 1;

        protected internal bool verifyCalculated = false;

        /// <summary>
        /// Checks each list individually to remove any filters that may not be active/empty
        /// </summary>
        /// <returns>
        /// True if at least 1 value list has a value. Otherwise False
        /// </returns>
        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }

            for (int i = geneList.Count - 1; i >= 0; i--)
            {
                if (geneList[i].VerifyValues() == false)
                {
                    geneList.RemoveAt(i);
                }
            }

            for (int i = filterList.Count - 1; i >= 0; i--)
            {
                if (filterList[i].VerifyValues() == false)
                {
                    filterList.RemoveAt(i);
                }
            }

            for (int i = colorFilterList.Count - 1; i >= 0; i--)
            {
                if (colorFilterList[i].VerifyValues() == false)
                {
                    colorFilterList.RemoveAt(i);
                }
            }


            verifyCalculated = true;
            if (geneList.Empty())
            {
                if (filterList.Empty())
                {
                    if (colorFilterList.Empty())
                    {
                        //The whole list is empty. Remove this.
                        return false;
                    }
                }
            }


            return true;

        }


        public bool AssignGenes(Pawn pawn, bool isXenogene)
        {


            for (int i = 0; i < geneList.Count; i++)
            {
                geneList[i].AssignGenes(pawn, isXenogene);
            }



            for (int i = 0; i < filterList.Count; i++)
            {
                filterList[i].AssignGenes(pawn, isXenogene);

            }


            for (int i = 0; i < colorFilterList.Count; i++)
            {
                colorFilterList[i].AssignGenes(pawn, isXenogene);
            }



            return true;
        }

    }



    public class Any_List_Random : DefModExtension
    {

        public List<RandomFilters> randomFilters;
        public int filler = 0;

        protected internal bool verifyCalculated = false;
        protected internal int totalPossibilities = 0;
        protected internal int totalWeight = 0;

        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }


            for (int i = randomFilters.Count - 1; i >= 0; i--)
            {

                if (randomFilters[i].VerifyValues() == false)
                {

                    totalPossibilities += randomFilters[i].weight;
                    randomFilters.RemoveAt(i);

                }
                else
                {
                    totalWeight += randomFilters[i].weight;
                    totalPossibilities += randomFilters[i].weight;
                }

            }
            totalPossibilities += filler;
            verifyCalculated = true;
            if (randomFilters.Count == 0)
            {
                //there were no possibile gene lists to be found
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
                for (int i = 0; i < randomFilters.Count; i++)
                {
                    searchedweights += randomFilters[i].weight;
                    if (Rvalue < searchedweights)
                    {
                        randomFilters[i].AssignGenes(pawn, isXenogene);
                        break;
                    }
                }

            }

            return;
        }

    }


    public class Random_Any_List : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();

            Any_List_Random multiFilter = def.GetModExtension<Any_List_Random>();

            if (multiFilter.VerifyValues())
            {
                multiFilter.AssignGenes(pawn, pawn.genes.IsXenogene(this));
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
