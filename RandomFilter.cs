using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{

    public class Gene_Filtered : DefModExtension
    {
        public List<FilterList> filterList;

        public bool VerifyValues()
        {

            for (int i = filterList.Count - 1; i >= 0; i--)
            {
                if (filterList[i].VerifyValues() == false)
                {
                    filterList.RemoveAt(i);
                }
            }
            if (filterList.Count == 0)
            {
                return false;
            }
            return true;
        }

        public void genValues(Pawn pawn, bool isXenogene)
        {


            for (int i = 0; i < filterList.Count; i++)
            {
                filterList[i].GetValue(pawn, isXenogene);
            }

        }

    }



    public class Gene_Random_Filtered : Gene
    {
        private bool ListVerified = false;

        public override void PostAdd()
        {
            base.PostAdd();
            Gene_Filtered filtered = def.GetModExtension<Gene_Filtered>();

            if (ListVerified == false)
            {
                ListVerified = filtered.VerifyValues();
            }
            if (ListVerified)
            {
                filtered.genValues(pawn, pawn.genes.IsXenogene(this));
            }
            pawn.genes.RemoveGene(this);
            return;


        }

    }
}
