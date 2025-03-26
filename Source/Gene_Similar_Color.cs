using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{
    public class Gene_Similar_Color : DefModExtension
    {
        public List<ColorFilterList> colorFilterList;
        public int filler = 0;

        protected internal bool verifyCalculated = false;
        protected internal int totalWeight = 0;
        protected internal int totalPossibilities = 0;
        public int GetChance => (int)((float)totalWeight / totalPossibilities * 100);

        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }
            for (int i = colorFilterList.Count - 1; i >= 0; i--)
            {
                if (colorFilterList[i].VerifyValues() == false)
                {
                    filler += colorFilterList[i].weight;
                    colorFilterList.RemoveAt(i);
                }
                else
                {
                    totalWeight += colorFilterList[i].weight;
                }
            }
            verifyCalculated = true;
            totalPossibilities = filler + totalWeight;
            return colorFilterList.Any();
        }

        /// <summary>
        /// Generate random value.
        /// If the Value is greater than the total weight, return.
        /// If not go through for loop to find where the value is that reaches that weight.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="isXenogene"></param>
        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            int Rvalue = Rand.Range(0, totalPossibilities);
            int totalweights = 0;
            for (int i = 0; i < colorFilterList.Count; i++)
            {
                totalweights += colorFilterList[i].weight;
                if (Rvalue < totalweights)
                {
                    colorFilterList[i].AssignGenes(pawn, isXenogene);
                    break;
                }
            }
            return;
        }

        public IEnumerable<StatDrawEntry> ReturnStatDrawEntries(int priority, int category)
        {

            yield return new StatDrawEntry(
                category: StatCategoryDefOf.Genetics,
                label: "HALI_RG_Category".Translate(category),
                valueString: "HALI_RG_Chance".Translate(GetChance),
                reportText: "",
                displayPriorityWithinCategory: priority--);

            char letterpos = 'a';
            for (int i = 0; i < colorFilterList.Count; i++)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Genetics,
                    "HALI_RG_Category".Translate(category) + letterpos++,
                    "HALI_RG_Chance".Translate(colorFilterList[i].GetChance),
                    colorFilterList[i].ColorChance,
                    priority--,
                    hyperlinks: Dialog_InfoCard.DefsToHyperlinks(colorFilterList[i].GetGeneDefHyperlinks())
                    );
            }
        }

    }
}