using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{
    public class GeneticList
    {
        public List<GenesAndWeight> genesAndWeight;
        public int filler = 0;

        protected internal int totalPossibilities = 0;
        protected internal bool verifyCalculated = false;

        public int GetChance(int i) => (int)(((float)genesAndWeight[i].weight / totalPossibilities) * 100);

        public int GetChanceTotal => (int)(((float)(totalPossibilities - filler) / totalPossibilities) * 100);

        public string GetGetAllLabels()
        {
            string labels = "\n";
            char letterpos = 'a';

            for (int i = 0; i < genesAndWeight.Count; i++)
            {
                labels +=
                (GetChance(i) == 100 ? "\n".Translate() : "HALI_RG_Category_NoLetter".Translate() +
                        " " + letterpos++ + ":\n" + "HALI_RG_Chance".Translate(GetChance(i).ToString())) + "\n";
                if (!genesAndWeight[i].genes.NullOrEmpty())
                {
                    labels += genesAndWeight[i].GetAllLabels();
                }
                if (!genesAndWeight[i].filterList.NullOrEmpty())
                {
                    labels += "HALI_RG_HASFilter".Translate(genesAndWeight[i].filterList.Count);
                }
                labels += "\n";
            }
            return labels;
        }

        public IEnumerable<DefHyperlink> GetGetAllHyperlinks()
        {
            IEnumerable<DefHyperlink> hyperlinks = Enumerable.Empty<DefHyperlink>();
            for (int i = 0; i < genesAndWeight.Count; i++)
            {
                if (!genesAndWeight[i].genes.NullOrEmpty())
                {
                    hyperlinks = hyperlinks.Concat((genesAndWeight[i].GetListGeneDefHyperlinks()));
                }
            }
            return hyperlinks;
        }

        public IEnumerable<StatDrawEntry> ReturnStatDrawEntries(int priority, int category)
        {
            yield return new StatDrawEntry(
                  category: StatCategoryDefOf.Genetics,
                  label: "HALI_RG_Category".Translate(category),
                  valueString: GetChanceTotal.ToString() + "%",
                  reportText: (GetChanceTotal == 100 ? "HALI_RG_WillBeChosen".Translate() : "HALI_RG_MightBeChosen".Translate(GetChanceTotal))
                  //Possibly add text here to say if a filter is present
                  + GetGetAllLabels(),
                  displayPriorityWithinCategory: priority--,
                  hyperlinks: Dialog_InfoCard.DefsToHyperlinks(GetGetAllHyperlinks())
                  );

            char letterpos = 'a';
            for (int i = 0; i < genesAndWeight.Count; i++)
            {
                if (!genesAndWeight[i].genes.NullOrEmpty())
                {
                    yield return new StatDrawEntry(
                        category: StatCategoryDefOf.Genetics,
                        label: "HALI_RG_Category".Translate(category) + letterpos,
                        valueString: "HALI_RG_Chance".Translate(GetChance(i)),
                        reportText: "HALI_RG_ChanceAndChosen".Translate(GetChance(i)) + (genesAndWeight[i].skip > 0 ? "HALI_RG_Skip".Translate(genesAndWeight[i].weight) : "".Translate()),
                        displayPriorityWithinCategory: priority--,
                        hyperlinks: (genesAndWeight[i].HasGenes ? Dialog_InfoCard.DefsToHyperlinks(genesAndWeight[i].GetListGeneDefHyperlinks()) : null)
                        );
                }
                if (!genesAndWeight[i].filterList.NullOrEmpty())
                {
                    //Make filters named something like filter1 Filter2

                    for (int filterToLookAt = 0; filterToLookAt < genesAndWeight[i].filterList.Count; filterToLookAt++)
                    {

                        yield return new StatDrawEntry(
                            category: StatCategoryDefOf.Genetics,
                            label: "HALI_RG_CategoryAndFilter".Translate(category, letterpos, filterToLookAt + 1),
                            valueString: "HALI_RG_Chance".Translate(GetChance(i)),
                            reportText: "HALI_RG_FilterChanceAndChosen".Translate(GetChance(i)) +
                            genesAndWeight[i].GetFiltersAsText(filterToLookAt),
                            displayPriorityWithinCategory: priority--,
                            hyperlinks: (genesAndWeight[i].HasGenes ? Dialog_InfoCard.DefsToHyperlinks(genesAndWeight[i].GetFilterGeneDefHyperlinks(filterToLookAt)) : null)
                            );
                    }
                }

                if (!genesAndWeight[i].colorFilterList.NullOrEmpty())
                {
                    //Make filters named something like filter1 Filter2

                    for (int filterToLookAt = 0; filterToLookAt < genesAndWeight[i].filterList.Count; filterToLookAt++)
                    {
                        yield return new StatDrawEntry(
                            category: StatCategoryDefOf.Genetics,
                            label: "HALI_RG_CategoryAndFilter".Translate(category, letterpos, filterToLookAt + 1),
                            valueString: "HALI_RG_Chance".Translate(GetChance(i)),
                            reportText: "HALI_RG_FilterChanceAndChosen".Translate(GetChance(i)) +
                            genesAndWeight[i].GetColorFiltersAsText(filterToLookAt),
                            displayPriorityWithinCategory: priority--,
                            hyperlinks: (genesAndWeight[i].HasGenes ? Dialog_InfoCard.DefsToHyperlinks(genesAndWeight[i].GetColorFilterGeneDefHyperlinks(filterToLookAt)) : null)
                            );
                    }
                }
                letterpos++;
            }
        }

        /// <summary>
        /// Goes through the geneAndWeight list and makes sure each entry is valid.
        /// will then calculate the total possibilities available
        /// </summary>
        /// <returns>True if this was already ran. False if first run and there are no objects in list.</returns>
        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }
            if (genesAndWeight == null)
            {
                verifyCalculated = true;
                return false;
            }

            for (int i = genesAndWeight.Count - 1; i >= 0; i--)
            {
                totalPossibilities += genesAndWeight[i].weight;
                if (genesAndWeight[i].VerifyValues() == false)
                {
                    genesAndWeight.RemoveAt(i);
                }
            }
            verifyCalculated = true;
            totalPossibilities += filler;
            return genesAndWeight.Count > 0;
        }

        /// <summary>
        /// Assigns genes to the pawn from genesAndWeight
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="isXenogene"></param>
        public int AssignGene(Pawn pawn, bool isXenogene)
        {
            int Rvalue = Rand.Range(0, totalPossibilities);
            //Log.Message("Rvalue is " + Rvalue + "Total possibilities is " + genelist.TotalPossibilities);
            int totalweights = 0;
            for (int j = 0; j < genesAndWeight.Count; j++)
            {
                totalweights += genesAndWeight[j].weight;
                if (Rvalue < totalweights)
                {
                    return genesAndWeight[j].AssignGene(pawn, isXenogene);
                }
            }
            return 0;
        }
    }

}
