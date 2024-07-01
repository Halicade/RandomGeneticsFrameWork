using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{
    public class RandomAny
    {
        public List<FilterList> filterList;
        public List<ColorFilterList> colorFilterList;
        public List<GeneDef> genes;

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

            verifyCalculated = true;
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


        public void AssignGenes(Pawn pawn, bool isXenogene)
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
            return;
        }
    }



    public class Any_List_Random : DefModExtension
    {

        public List<RandomAny> randomAny;
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

            for (int i = randomAny.Count - 1; i >= 0; i--)
            {

                if (randomAny[i].VerifyValues() == false)
                {
                    totalPossibilities += randomAny[i].weight;
                    randomAny.RemoveAt(i);
                }
                else
                {
                    totalWeight += randomAny[i].weight;
                    totalPossibilities += randomAny[i].weight;
                }
            }
            totalPossibilities += filler;
            verifyCalculated = true;

            return randomAny.Any();
        }

        public void AssignGenes(Pawn pawn, bool isXenogene)
        {

            int Rvalue = Rand.Range(0, totalPossibilities);

            if (Rvalue >= totalWeight)
            {
                //filler value was reached
                return;
            }

            int searchedweights = 0;
            for (int i = 0; i < randomAny.Count; i++)
            {
                searchedweights += randomAny[i].weight;
                if (Rvalue < searchedweights)
                {
                    randomAny[i].AssignGenes(pawn, isXenogene);
                    break;
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
            if (multiFilter == null)
            {
                Log.Error("Unable to find modExtensions \"HALI_RandomGenetics.Any_List_Random\" for " + this.def + " " + this.Label);
                return;
            }

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
