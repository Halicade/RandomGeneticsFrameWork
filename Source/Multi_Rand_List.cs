using RimWorld;
using System.Collections.Generic;
using Verse;
using static HALI_RandomGenetics.Any_List_Random;

namespace HALI_RandomGenetics
{
    public class Multi_Rand_List : DefModExtension
    {
        public List<GeneticList> geneList;
        private bool verifyCalculated = false;
        public GeneLoc geneLoc = 0;

        public enum GeneLoc { Follow, Inverse, Genotype, Xenotype }

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
            return !geneList.NullOrEmpty();
        }

        public void AssignGene(Pawn pawn, bool isXenogene)
        {

            switch (geneLoc)
            {
                case GeneLoc.Inverse:
                    isXenogene = !isXenogene;
                    break;
                case GeneLoc.Genotype:
                    isXenogene = false;
                    break;
                case GeneLoc.Xenotype:
                    isXenogene = true;
                    break;
            }

            for (int i = 0; i < geneList.Count; i++)
            {

                i += geneList[i].AssignGene(pawn, isXenogene);

            }
        }

        public IEnumerable<StatDrawEntry> ReturnStatDrawEntries(int priority, int category)
        {
            for (int i = 0; i < geneList.Count; i++)
            {
                var asdf = geneList[i].ReturnStatDrawEntries(priority--, category++);
                foreach (var entry in asdf)
                {
                    yield return entry;
                    priority--;
                }
            }
        }
    }
}
