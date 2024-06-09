using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{
    public class Random_Single_Gene : DefModExtension
    {
        public List<GeneDef> genes;
        public int filler = 0;
    }
    public class Gene_Random_Single : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            Random_Single_Gene singleGene = def.GetModExtension<Random_Single_Gene>();
            int Rvalue;
            Rvalue = Rand.Range(0, singleGene.genes.Count + singleGene.filler);
            if (Rvalue >= singleGene.genes.Count)
            {
                pawn.genes.RemoveGene(this);
                return;
            }
            pawn.genes.AddGene(singleGene.genes[Rvalue], pawn.genes.IsXenogene(this));
            pawn.genes.RemoveGene(this);
        }
    }
}
