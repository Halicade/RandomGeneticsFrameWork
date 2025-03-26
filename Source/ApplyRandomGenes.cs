using Verse;

namespace HALI_RandomGenetics
{

    public class ApplyRandomGenes : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (def.HasModExtension<Multi_Rand_List>())
            {
                Multi_Rand_List multi = def.GetModExtension<Multi_Rand_List>();
                if (multi.VerifyValues())
                {
                    multi.AssignGene(pawn, pawn.genes.IsXenogene(this));
                }
                else
                {
                    Log.Warning("Random Genetics found no genes for the gene " + this.def + " " + this.Label);
                }
            }
            if (def.HasModExtension<Gene_Filtered>())
            {
                Gene_Filtered filtered = def.GetModExtension<Gene_Filtered>();
                if (filtered.VerifyValues())
                {
                    filtered.AssignGenes(pawn, pawn.genes.IsXenogene(this));
                }
                else
                {
                    Log.Warning("Random Genetics found no genes for the gene " + this.def + " " + this.Label);
                }
            }
            if (def.HasModExtension<Any_List_Random>())
            {
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
            }
            if (def.HasModExtension<Gene_Similar_Color>())
            {
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
            }

            pawn.genes.RemoveGene(this);
            return;
        }
    }
}
