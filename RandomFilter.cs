using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{

    public class Gene_Filtered : DefModExtension
    {
        public List<FilterList> filterList;
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


            for (int i = filterList.Count - 1; i >= 0; i--)
            {
                if (filterList[i].VerifyValues() == false)
                {
                    totalPossibilities += filterList[i].weight;
                    filterList.RemoveAt(i);
                }
                else
                {
                    totalWeight += filterList[i].weight;
                    totalPossibilities += filterList[i].weight;
                }
            }
            totalPossibilities += filler;
            verifyCalculated = true;
            if (filterList.Count == 0)
            {
                return false;
            }



            return true; ;
        }

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
                for (int i = 0; i < filterList.Count; i++)
                {
                    searchedweights += filterList[i].weight;
                    if (Rvalue < searchedweights)
                    {
                        filterList[i].AssignGenes(pawn, isXenogene);
                        break;
                    }
                }

            }

        }

    }



    public class Random_Filter_List : Gene
    {
        private bool ListVerified = false;

        public override void PostAdd()
        {
            base.PostAdd();
            Gene_Filtered filtered = def.GetModExtension<Gene_Filtered>();


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
