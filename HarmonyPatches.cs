using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace GlueSlimes
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(MarketUI), "Start")]
        public static class PatchMarketUIStart
        {
            public static void Prefix(MarketUI __instance)
            {
                __instance.plorts = (from x in __instance.plorts
                                     where !GlueEntry.plortsToPatch.Exists((MarketUI.PlortEntry y) => y == x)
                                     select x).ToArray();
                __instance.plorts = __instance.plorts.ToArray().AddRangeToArray(GlueEntry.plortsToPatch.ToArray());
            }
        }

        [HarmonyPatch(typeof(EconomyDirector), "InitModel")]
        public static class PatchEconomyDirectorInitModel
        {
            public static void Prefix(EconomyDirector __instance)
            {
                __instance.baseValueMap = __instance.baseValueMap.ToArray().AddRangeToArray(GlueEntry.valueMapsToPatch.ToArray());
            }
        }

        [HarmonyPatch(typeof(AnalyticsUtil), "ReportPerIdentifiableData")]
        public static class PatchAnalyticsUtilReportPerIdentifiableData
        {
            public static bool Prefix(System.Collections.Generic.IEnumerable<IdentifiableType> ids)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(AutoSaveDirector), "Awake")]
        public static class PatchAutoSaveDirectorAwake
        {
            public static void Prefix(AutoSaveDirector __instance)
            {
                Utility.Get<IdentifiableTypeGroup>("PlortGroup").memberTypes.Add(GlueEntry.gluePlortType);
                Utility.Get<IdentifiableTypeGroup>("BaseSlimeGroup").memberTypes.Add(GlueEntry.glueDefinition);
                Utility.Get<IdentifiableTypeGroup>("VaccableBaseSlimeGroup").memberTypes.Add(GlueEntry.glueDefinition);
                Utility.Get<IdentifiableTypeGroup>("SlimesGroup").memberTypes.Add(GlueEntry.glueDefinition);
                __instance.identifiableTypes.memberTypes.Add(GlueEntry.gluePlortType);
                __instance.identifiableTypes.memberTypes.Add(GlueEntry.glueDefinition);
            }
        }

        [HarmonyPatch(typeof(LocalizationDirector), "LoadTables")]
        internal static class LocalizationDirectorLoadTablePatch
        {
            public static void Postfix(LocalizationDirector __instance)
            {
                MelonCoroutines.Start(LoadTable(__instance));
            }

            private static IEnumerator LoadTable(LocalizationDirector director)
            {
                WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.01f);
                yield return waitForSecondsRealtime;
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, StringTable> keyValuePair in director.Tables)
                {
                    if (addedTranslations.TryGetValue(keyValuePair.Key, out var dictionary))
                    {
                        foreach (System.Collections.Generic.KeyValuePair<string, string> keyValuePair2 in dictionary)
                        {
                            keyValuePair.Value.AddEntry(keyValuePair2.Key, keyValuePair2.Value);
                        }
                    }
                }
                yield break;
            }

            public static LocalizedString AddTranslation(string table, string key, string localized)
            {
                System.Collections.Generic.Dictionary<string, string> dictionary;
                if (!addedTranslations.TryGetValue(table, out dictionary))
                {
                    dictionary = new System.Collections.Generic.Dictionary<string, string>(); ;
                    addedTranslations.Add(table, dictionary);
                }
                dictionary.Add(key, localized);
                StringTable table2 = LocalizationUtil.GetTable(table);
                StringTableEntry stringTableEntry = table2.AddEntry(key, localized);
                return new LocalizedString(table2.SharedData.TableCollectionName, stringTableEntry.SharedEntry.Id);
            }

            public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> addedTranslations = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
        }
    }
}
