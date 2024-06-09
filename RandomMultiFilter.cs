using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace HALI_RandomGenetics
{





    public class RandomFilters
    {
        public List<FilterList> filterList;
        public List<ColorFilterList> colorFilterList;
        public List<GeneList> geneList;

        public int weight = 1;


        public bool genValues(Pawn pawn, bool isXenogene)
        {

            if (filterList == null)
            {
                for (int i = 0; i < filterList?.Count; i++)
                {
                    filterList[i].getValue(pawn, isXenogene);
                }
            }

            if (colorFilterList != null)
            {
                for (int i = 0; i < colorFilterList?.Count; i++)
                {
                    colorFilterList[i].GetValue(pawn, isXenogene);
                }
            }


            if (geneList != null)
            {
                for (int i = 0; i < geneList.Count; i++)
                {
                    geneList[i].GenValue(pawn, isXenogene);
                }
            }

            return true;
        }

    }



    public class Gene_MultiFilter : DefModExtension
    {
        public int filler = 0;
        public List<RandomFilters> randomFilters;

        public bool DoAdditions(Pawn pawn, bool isXenogene)
        {




            int Rvalue = Rand.Range(0, randomFilters.Count);
            randomFilters[Rvalue].genValues(pawn, isXenogene);






            return true;
        }

    }


    public class Gene_Multi_Filter : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();

            Gene_MultiFilter multiFilter = def.GetModExtension<Gene_MultiFilter>();

            multiFilter.DoAdditions(pawn, pawn.genes.IsXenogene(this));




            pawn.genes.RemoveGene(this);
            return;



        }
    }
}
