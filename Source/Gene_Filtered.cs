using RimWorld;
using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{
    public class Gene_Filtered : DefModExtension
    {
        public List<FilterList> filterList;
        public int filler = 0;
        public GeneLoc geneLoc = 0;

        public enum GeneLoc { Follow, Inverse, Genotype, Xenotype }

        protected internal bool verifyCalculated = false;
        protected internal int totalPossibilities = 0;
        protected internal int totalWeight = 0;

        public int GetChance(int i) => (int)((float)filterList[i].weight / (filterList[i].weight + filterList[i].filler) * 100);
        public int GetChanceTotal => (int)((float)totalWeight / totalPossibilities * 100);

        public string GetChanceTotalString => (GetChanceTotal != 1f ? "HALI_RG_Chance".Translate(GetChanceTotal) : "");

        public int GetChanceInternal(int i) => (GetChanceTotal * GetChance(i)) / 100;

        public bool HasAny => !filterList.NullOrEmpty();

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
            return filterList.Any();
        }

        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            int Rvalue = Rand.Range(0, totalPossibilities);

            if (Rvalue >= totalWeight)
            {
                //filler value was reached
                return;
            }

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
            return;
        }

        public IEnumerable<StatDrawEntry> ReturnStatDrawEntries(int priority, int category)
        {
            yield return new StatDrawEntry(
                category: StatCategoryDefOf.Genetics,
                label: "HALI_RG_Category".Translate(category),
                valueString: GetChanceTotalString,
                reportText: "",
                displayPriorityWithinCategory: priority--);
            char letterpos = 'a';
            for (int i = 0; i < filterList.Count; i++)
            {
                yield return new StatDrawEntry(
                    category: StatCategoryDefOf.Genetics,
                    label: "HALI_RG_Category".Translate(category) + letterpos++,
                    valueString: "HALI_RG_Chance".Translate(GetChanceInternal(i)),
                    reportText: filterList[i].FilterChance + filterList[i].RepetitionTimes + filterList[i].GetPossibleValsAsText,
                    displayPriorityWithinCategory: priority - i,
                    hyperlinks: Dialog_InfoCard.DefsToHyperlinks(filterList[i].GetMyGeneDefHyperlinks())
                    );
            }
        }
    }

}
