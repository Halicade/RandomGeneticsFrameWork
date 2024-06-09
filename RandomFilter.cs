using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{

    public class Gene_Filtered : DefModExtension
    {
        List<FilterList> filterList;
        
        public void genValues(Pawn pawn, bool isXenogene)
        {
            if (filterList != null)
            {
                for (int i = 0; i < filterList?.Count; i++)
                {
                    filterList[i].getValue(pawn, isXenogene);
                }
            }
        }
       
    }



    public class Gene_Random_Filtered : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            Gene_Filtered filtered = def.GetModExtension<Gene_Filtered>();
            /*
            Log.Error("printing list of filteres exclusion tags");
            for(int i = 0; i < filtered.exclusionTags.Count; i++)
            {
                Log.Message(filtered.exclusionTags[i]);
            }
            */
            
            filtered.genValues(pawn, pawn.genes.IsXenogene(this));

            pawn.genes.RemoveGene(this);
            return;
            /*
            Log.Error("showing possible values for filter Total possibilities is "+filtered.TotalPossibilities);
            for(int i = 0; i < filtered.possibleVals.Count(); i++)
            {
                Log.Message(filtered.possibleVals.ElementAt(i));
            }*/

            /*
            if (filtered.possibleVals.Any()==false)
            {
                Log.Warning("The list for the RandomFiltered gene " + this.def + ", " + this.Label + " was empty.");
                pawn.genes.RemoveGene(this);
                return;
            }

            int Rvalue = Rand.Range(0, filtered.TotalPossibilities);
            if (Rvalue < filtered.possibleVals.Count)
            {

                pawn.genes.AddGene(filtered.possibleVals[Rvalue], pawn.genes.IsXenogene(this));
                pawn.genes.RemoveGene(this);
                return;
            }
            else
            {
                pawn.genes.RemoveGene(this);
                return;
            }*/

        }

    }
}
