using RimWorld;
using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{
    public class RandomAny
    {
        public List<FilterList> filterList;
        public List<ColorFilterList> colorFilterList;
        public List<RandomGene> randomGenes;

        public int weight = 1;

        protected internal bool verifyCalculated = false;

        public IEnumerable<DefHyperlink> GetRandomGenesAsHyperlinks(int i)
        {
            foreach (DefHyperlink randomGeneHyperlink in randomGenes[i].GetListGeneDefHyperlinks())
            {
                yield return randomGeneHyperlink;
            }
        }

        public IEnumerable<DefHyperlink> GetAllRandomGenesAsHyperlinks()
        {
            foreach (RandomGene randomGeneList in randomGenes)
            {
                foreach (DefHyperlink randomGene in randomGeneList.GetListGeneDefHyperlinks())
                    yield return randomGene;
            }
        }

        public IEnumerable<DefHyperlink> GetFilterListAsHyperlinks(int i)
        {
            foreach (DefHyperlink Filters in filterList[i].GetMyGeneDefHyperlinks())
            {
                yield return Filters;
            }
        }

        public string GetFiltersAsText(int i) => filterList[i].GetPossibleValsAsText;

        public IEnumerable<DefHyperlink> GetColorFilterListAsHyperlinks(int i)
        {
            foreach (DefHyperlink colorFilters in colorFilterList[i].GetGeneDefHyperlinks())
            {
                yield return colorFilters;
            }
        }

        public string GetAllLabelsAsText(int i)
        {
            return randomGenes[i].GetAllLabelsAsText();
        }

        public string GetAllLabelsAsText()
        {
            string geneText = "";
            for (int geneLoc = 0; geneLoc < randomGenes.Count; geneLoc++)
            {
                geneText += "HALI_RG_Category".Translate((geneLoc + 1)) + "\n"
                + randomGenes[geneLoc].GetAllLabelsAsText();
            }
            return geneText;
        }

        /// <summary>
        /// Checks each list individually to remove any filters that may not be active/empty
        /// </summary>
        /// <returns>
        /// True if at least 1 value list has a value. Otherwise False
        /// </returns>
        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }

            if (!randomGenes.NullOrEmpty())
            {
                for (int i = randomGenes.Count - 1; i >= 0; i--)
                {
                    if (randomGenes[i].VerifyValues() == false)
                    {
                        randomGenes.RemoveAt(i);
                    }
                }
            }
            if (!filterList.NullOrEmpty())
            {
                for (int i = filterList.Count - 1; i >= 0; i--)
                {
                    if (filterList[i].VerifyValues() == false)
                    {
                        filterList.RemoveAt(i);
                    }
                }
            }
            if (!colorFilterList.NullOrEmpty())
            {
                for (int i = colorFilterList.Count - 1; i >= 0; i--)
                {
                    if (colorFilterList[i].VerifyValues() == false)
                    {
                        colorFilterList.RemoveAt(i);
                    }
                }
            }
            verifyCalculated = true;
            if (randomGenes.NullOrEmpty() && filterList.NullOrEmpty() && colorFilterList.NullOrEmpty())
            {
                //The whole list is empty. Remove this.
                return false;
            }
            return true;
        }


        public void AssignGenes(Pawn pawn, bool isXenogene)
        {
            if (!randomGenes.NullOrEmpty())
            {
                randomGenes.RandomElement().AssignGene(pawn, isXenogene);
            }
            if (!filterList.NullOrEmpty())
            {
                for (int i = 0; i < filterList.Count; i++)
                {
                    filterList[i].AssignGenes(pawn, isXenogene);
                }
            }
            if (!colorFilterList.NullOrEmpty())
            {
                for (int i = 0; i < colorFilterList.Count; i++)
                {
                    colorFilterList[i].AssignGenes(pawn, isXenogene);
                }
            }
            return;
        }

        public int FilterChance(int filterLookingAt)
        {
            if (filterList.NullOrEmpty())
            {
                return 100;
            }
            return filterList[filterLookingAt].GetChance;
        }

        public int ColorFilterChance(int filterLookingAt)
        {
            if (colorFilterList.NullOrEmpty())
            {
                return 100;
            }
            return colorFilterList[filterLookingAt].GetChance;
        }

        public int GetFilterRepetitionTimes(int filterLookingAt) => filterList.NullOrEmpty() ? 0 : filterList[filterLookingAt].timesToPerform;

        public string GetFilterRepetitionTimesString(int filterLookingAt) =>
            GetFilterRepetitionTimes(filterLookingAt) > 1
            ? "HALI_RG_TimesAssigned".Translate(GetFilterRepetitionTimes(filterLookingAt))
            : "";

        public int GetColorRepetitionTimes(int filterLookingAt) => colorFilterList.NullOrEmpty() ? 0 : colorFilterList[filterLookingAt].timesToPerform;
    }

}
