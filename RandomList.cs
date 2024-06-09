using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{

    public class MultiList
    {
        public List<GeneDef> multiList;
        public int weight = 1;
    }


    public class Gene_List : DefModExtension
    {
        public List<MultiList> geneList;
        public int filler=0;

        protected internal int cachedValue = 0;
        protected internal int TotalValue
        {
            get
            {
                if (cachedValue == 0)
                {
                    cachedValue = filler;
                    for (int i = 0; i < geneList.Count; i++)
                    {
                        cachedValue += geneList[i].weight;
                    }
                } 
                return cachedValue;
            }
        }

    }



    public class Gene_Random_List: Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            Gene_List geneList = def.GetModExtension<Gene_List>();



            //Log.Error("checking randomList total value "+ geneList.TotalValue);
            if(geneList.TotalValue == 0)
            {
                Log.Warning("The list for the RandomList gene " + this.def + ", " + this.Label + " was empty.");
                pawn.genes.RemoveGene(this);
                return;
            }

            int totalCounted = 0;
            int Rvalue = Rand.Range(0, geneList.TotalValue);
            for(int i = 0;i < geneList.geneList.Count;i++)
            {
                totalCounted += geneList.geneList[i].weight;
                if (Rvalue < totalCounted)
                {
                    for(int j = 0; j < geneList.geneList[i].multiList.Count;j++)
                    {
                        pawn.genes.AddGene(geneList.geneList[i].multiList[j], pawn.genes.IsXenogene(this));

                    }

                    break;
                }
            }



            
            pawn.genes.RemoveGene(this);
        }
    }


}
