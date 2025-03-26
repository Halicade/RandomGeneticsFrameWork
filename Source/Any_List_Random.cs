using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HALI_RandomGenetics
{
    public class Any_List_Random : DefModExtension
    {

        public List<RandomAny> randomAny;
        public int filler = 0;

        protected internal bool verifyCalculated = false;
        protected internal int totalPossibilities = 0;
        protected internal int totalWeight = 0;

        public IEnumerable<StatDrawEntry> ReturnStatDrawEntries(int priority, int category)
        {
            for (int setNumber = 0; setNumber < randomAny.Count; setNumber++)
            {
                int setNumberDisplay = setNumber + 1;
                int individualChance = GetChanceIndividual(setNumber);
                //Displays sets individual chance
                yield return new StatDrawEntry(
                     category: StatCategoryDefOf.Genetics,
                     label: "HALI_RG_SetNumber".Translate(setNumberDisplay),
                     valueString: (individualChance == 100 ? "" : individualChance.ToString() + "%"),
                     reportText: (individualChance == 100 ? "HALI_RG_SetWillBeUsed".Translate() : "HALI_RG_SetWords".Translate(individualChance)),
                     displayPriorityWithinCategory: priority--
                     //hyperlinks: Dialog_InfoCard.DefsToHyperlinks(GetAllHyperlinks)
                     );

                if (!randomAny[setNumber].randomGenes.NullOrEmpty())
                {
                    //TODO figure out if I need to place an initial list here

                    if (randomAny[setNumber].randomGenes.Count > 1)
                    {

                        yield return new StatDrawEntry(
                            category: StatCategoryDefOf.Genetics,
                            label: "HALI_RG_AnyGenes".Translate(setNumberDisplay),
                            valueString: "",
                            reportText: "HALI_RG_AllGenes".Translate() + randomAny[setNumber].GetAllLabelsAsText(0),
                            displayPriorityWithinCategory: priority--,
                            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(randomAny[setNumber].GetAllRandomGenesAsHyperlinks())
                            );
                    }
                    else
                    {
                        yield return new StatDrawEntry(
                            category: StatCategoryDefOf.Genetics,
                            label: "HALI_RG_AnyGenes".Translate(setNumberDisplay),
                            valueString: "",
                            reportText: "HALI_RG_AllGenes".Translate() + randomAny[setNumber].GetAllLabelsAsText(),
                            displayPriorityWithinCategory: priority--,
                            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(randomAny[setNumber].GetAllRandomGenesAsHyperlinks())
                            );

                        for (int randomGeneListLookingAt = 0; randomGeneListLookingAt < randomAny[setNumber].randomGenes.Count; randomGeneListLookingAt++)
                        {
                            yield return new StatDrawEntry(
                                category: StatCategoryDefOf.Genetics,
                                label: "HALI_RG_AnyGenes".Translate(setNumberDisplay),
                                valueString: "",
                                reportText: "HALI_RG_AllGenes".Translate() + randomAny[setNumber].GetAllLabelsAsText(randomGeneListLookingAt),
                                displayPriorityWithinCategory: priority--,
                                hyperlinks: Dialog_InfoCard.DefsToHyperlinks(randomAny[setNumber].GetRandomGenesAsHyperlinks(randomGeneListLookingAt))
                                );
                        }
                    }
                }

                if (!randomAny[setNumber].filterList.NullOrEmpty())
                {
                    for (int filterLookingAt = 0; filterLookingAt < randomAny[setNumber].filterList.Count; filterLookingAt++)
                    {
                        int filterChance = randomAny[setNumber].FilterChance(filterLookingAt);
                        string filterText = filterChance == 100 ? "HALI_RG_OneGeneFilter".Translate() : "HALI_RG_MightGeneFilter".Translate(filterChance);
                        string repetitionText = randomAny[setNumber].GetFilterRepetitionTimesString(filterLookingAt);
                        string filterListAsText = randomAny[setNumber].GetFiltersAsText(filterLookingAt);

                        yield return new StatDrawEntry(
                            category: StatCategoryDefOf.Genetics,
                            label: "HALI_RG_AnyFilter".Translate(setNumberDisplay, filterLookingAt + 1),
                            valueString: (filterChance == 100 ? "" : "HALI_RG_Chance".Translate(filterChance)),
                            reportText: filterText + repetitionText + filterListAsText,
                            displayPriorityWithinCategory: priority--,
                            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(randomAny[setNumber].GetFilterListAsHyperlinks(filterLookingAt))
                            );
                    }
                }

                if (!randomAny[setNumber].colorFilterList.NullOrEmpty())
                {
                    for (int filterLookingAt = 0; filterLookingAt < randomAny[setNumber].colorFilterList.Count; filterLookingAt++)
                    {
                        int filterChance = randomAny[setNumber].ColorFilterChance(filterLookingAt);

                        string colorFilterText = filterChance == 100 ? "HALI_RG_OneGeneFilter".Translate() : "HALI_RG_MightGeneFilter".Translate(filterChance);
                        string colorRepetitionText = randomAny[setNumber].GetColorRepetitionTimes(filterLookingAt) > 1 ?
                                                        "HALI_RG_TimesAssigned".Translate(randomAny[setNumber].GetColorRepetitionTimes(filterLookingAt)) : "";
                        yield return new StatDrawEntry(
                            category: StatCategoryDefOf.Genetics,
                            label: "HALI_RG_AnyColors".Translate(setNumberDisplay, filterLookingAt + 1),
                            valueString: "",
                            reportText: colorFilterText + colorRepetitionText,
                            displayPriorityWithinCategory: priority--,
                            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(randomAny[setNumber].GetColorFilterListAsHyperlinks(filterLookingAt))
                            );
                    }
                }
            }
        }

        public int GetChanceTotal => (int)(((float)totalWeight / totalPossibilities) * 100);

        public int GetChanceIndividual(int i) => (int)((float)randomAny[i].weight / totalPossibilities * 100);

        public bool VerifyValues()
        {
            if (verifyCalculated)
            {
                return true;
            }
            for (int i = randomAny.Count - 1; i >= 0; i--)
            {

                if (randomAny[i].VerifyValues() == false)
                {
                    totalPossibilities += randomAny[i].weight;
                    randomAny.RemoveAt(i);
                }
                else
                {
                    totalWeight += randomAny[i].weight;
                    totalPossibilities += randomAny[i].weight;
                }
            }
            totalPossibilities += filler;
            verifyCalculated = true;

            return randomAny.Any();
        }

        public bool HasAny => !randomAny.NullOrEmpty();

        public void AssignGenes(Pawn pawn, bool isXenogene)
        {

            int Rvalue = Rand.Range(0, totalPossibilities);

            if (Rvalue >= totalWeight)
            {
                //filler value was reached
                return;
            }

            int searchedweights = 0;
            for (int i = 0; i < randomAny.Count; i++)
            {
                searchedweights += randomAny[i].weight;
                if (Rvalue < searchedweights)
                {
                    randomAny[i].AssignGenes(pawn, isXenogene);
                    break;
                }
            }

            return;
        }
    }

}
