using HALI_RandomGenetics;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomGenetics.Source
{

    [StaticConstructorOnStartup]
    internal static class Harmony_Initializer
    {
        private static readonly Type patchType = typeof(Harmony_Initializer);
        static Harmony_Initializer()
        {
            Harmony harmony = new("Hali.RandomGenetics");
            harmony.Patch(AccessTools.Method(typeof(GeneDef), "SpecialDisplayStats"),
                postfix: new HarmonyMethod(patchType, nameof(SpecialDisplayStatsPostfix)));
        }

        [HarmonyPostfix]
        public static IEnumerable<StatDrawEntry> SpecialDisplayStatsPostfix(IEnumerable<StatDrawEntry> __result, ThingDef __instance)
        {
            foreach (var baseStat in __result)
                yield return baseStat;
            // Category wants to increase. Priority wants to decrease
            int priority = 7878;
            int category = 1;

            if (__instance.HasModExtension<Multi_Rand_List>())
            {
                Multi_Rand_List multi = __instance.GetModExtension<Multi_Rand_List>();
                if (multi.VerifyValues())
                {
                    foreach (var basestat in multi.ReturnStatDrawEntries(priority--, category++))
                    {
                        yield return basestat;
                        priority--;
                    }
                }
                else
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);
                }
            }

            if (__instance.HasModExtension<Gene_Similar_Color>())
            {
                Gene_Similar_Color similar_Color = __instance.GetModExtension<Gene_Similar_Color>();
                if (similar_Color.VerifyValues())
                {
                    IEnumerable<StatDrawEntry> SimColor = similar_Color.ReturnStatDrawEntries(priority--, category++);
                    if (SimColor.Any())
                    {
                        foreach (var basestat in SimColor)
                        {
                            yield return basestat;
                            priority--;
                        }
                    }
                    else
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);

                    }
                }
                else
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);
                }
            }

            if (__instance.HasModExtension<Gene_Filtered>())
            {
                Gene_Filtered filtered = __instance.GetModExtension<Gene_Filtered>();
                if (filtered.VerifyValues())
                {
                    IEnumerable<StatDrawEntry> filtList = filtered.ReturnStatDrawEntries(priority--, category++);
                    if (filtered.HasAny)
                    {
                        foreach (var basestat in filtList)
                        {
                            yield return basestat;
                            priority--;
                        }
                    }
                    else
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);
                    }
                }
                else
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);
                }
            }

            if (__instance.HasModExtension<Any_List_Random>())
            {
                Any_List_Random anyFilter = __instance.GetModExtension<Any_List_Random>();
                if (anyFilter.VerifyValues())
                {
                    IEnumerable<StatDrawEntry> anyList = anyFilter.ReturnStatDrawEntries(priority--, category++);
                    if (anyFilter.HasAny)
                    {

                        foreach (var basestat in anyList)
                        {
                            yield return basestat;
                            priority--;
                        }
                    }
                    else
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);
                    }
                }
                else
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "HALI_RG_NoGenes".Translate(), "", "", priority--);
                }
            }
        }
    }
}
