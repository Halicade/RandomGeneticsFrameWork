using System.Collections.Generic;
using Verse;

namespace HALI_RandomGenetics
{

    public class GenesAndWeight
    {
        public List<GeneDef> genes;
        public List<FilterList> filterList;
        public List<ColorFilterList> colorFilterList;
        public int weight = 1;
        public int skip = 0;

        public int AssignGene(Pawn pawn, bool isXenogene)
        {
            if (!genes.NullOrEmpty())
            {
                for (int i = 0; i < genes.Count; i++)
                {
                    pawn.genes.AddGene(genes[i], isXenogene);
                }
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

            return skip;
        }

        public bool HasGenes { get { return !genes.NullOrEmpty() || !filterList.NullOrEmpty() || !colorFilterList.NullOrEmpty(); } }

        public IEnumerable<DefHyperlink> GetListGeneDefHyperlinks()
        {
            for (int i = 0; i < genes.Count; i++)
            {
                yield return new DefHyperlink(genes[i]);
            }
        }

        public IEnumerable<DefHyperlink> GetFilterGeneDefHyperlinks(int filterToLookAt)
        {
            foreach (DefHyperlink filterdGenes in filterList[filterToLookAt].MyGeneDefHyperlinks)
            {
                yield return filterdGenes;
            }
        }

        public IEnumerable<DefHyperlink> GetColorFilterGeneDefHyperlinks(int filterToLookAt)
        {
            foreach (DefHyperlink colorFilterdGenes in colorFilterList[filterToLookAt].GetGeneDefHyperlinks())
            {
                yield return colorFilterdGenes;
            }
        }

        public string GetFiltersAsText(int i) => filterList[i].GetPossibleValsAsText;

        public string GetColorFiltersAsText(int i) => colorFilterList[i].GetMatchingColorsAsText();

        public string GetAllLabels()
        {
            string labels = "";
            for (int i = 0; i < genes.Count; i++)
            {
                labels += genes[i].LabelCap + "\n";
            }
            return labels;
        }

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
            return !genes.NullOrEmpty() || !filterList.NullOrEmpty() || !colorFilterList.NullOrEmpty();
        }
    }
}
