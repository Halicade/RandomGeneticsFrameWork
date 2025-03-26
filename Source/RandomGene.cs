using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{
    /// <summary>
    /// Unlike GenesAndWeight this is strictly a random list of genes. No weight involved.
    /// </summary>
    public class RandomGene
    {
        public List<GeneDef> genes;

        public bool VerifyValues()
        {
            if (!genes.NullOrEmpty())
            {
                List<GeneDef> listOfGenes = DefDatabase<GeneDef>.AllDefsListForReading;
                for (int i = genes.Count - 1; i >= 0; i--)
                {
                    if (listOfGenes.Contains(genes[i]) == false)
                    {
                        genes.RemoveAt(i);
                    }
                }
            }
            return !genes.NullOrEmpty();
        }

        public string GetAllLabelsAsText()
        {
            string labels = "";
            for (int i = 0; i < genes.Count; i++)
            {
                labels += "- " + genes[i].LabelCap + "\n";
            }
            return labels;
        }

        public IEnumerable<DefHyperlink> GetListGeneDefHyperlinks()
        {
            for (int i = 0; i < genes.Count; i++)
            {
                yield return new DefHyperlink(genes[i]);
            }
        }

        public void AssignGene(Pawn pawn, bool isXenogene)
        {
            if (!genes.NullOrEmpty())
            {
                for (int i = 0; i < genes.Count; i++)
                {
                    pawn.genes.AddGene(genes[i], isXenogene);
                }
            }
        }

    }
}
