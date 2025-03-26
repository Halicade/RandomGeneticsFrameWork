using RimWorld;
using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{
    public class Multi_Rand_List : DefModExtension
    {
        public List<GeneticList> geneList;
        private bool verifyCalculated = false;

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
