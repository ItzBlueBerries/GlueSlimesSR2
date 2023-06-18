using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization;
using static GlueSlimes.HarmonyPatches;

internal class Utility
{
    public static T Get<T>(string name) where T : UnityEngine.Object
    {
        return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault((T found) => found.name.Equals(name));
    }

    public static Texture2D LoadImage(string filename)
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + "." + filename + ".png");
        byte[] array = new byte[manifestResourceStream.Length];
        manifestResourceStream.Read(array, 0, array.Length);
        Texture2D texture2D = new Texture2D(1, 1);
        ImageConversion.LoadImage(texture2D, array);
        texture2D.filterMode = FilterMode.Bilinear;
        return texture2D;
    }

    public static Sprite CreateSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
    }

    public static class PrefabUtils
    {
        static PrefabUtils()
        {
            DisabledParent.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(DisabledParent.gameObject);
            DisabledParent.gameObject.hideFlags |= HideFlags.HideAndDontSave;
        }

        public static GameObject CopyPrefab(GameObject prefab)
        {
            return UnityEngine.Object.Instantiate(prefab, DisabledParent);
        }

        //public static UnityEngine.Object DeepCopyObject(UnityEngine.Object ob)
        //{
        //    Dictionary<UnityEngine.Object, UnityEngine.Object> refToRef = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
        //    UnityEngine.Object DeepCopyObject_Internal(UnityEngine.Object obj)
        //    {
        //        if (!obj) return obj;
        //        if (refToRef.TryGetValue(obj, out var old)) return old;
        //        if (refToRef.Values.Contains(obj)) return obj;
        //        var newObj = UnityEngine.Object.Instantiate(obj);
        //        if (!typeof(ScriptableObject).IsAssignableFrom(newObj.GetType())) return newObj;
        //        refToRef[obj] = newObj;
        //        System.Object ProcessObject(System.Object theObj)
        //        {
        //            if (theObj == null) return null;

        //            if (typeof(Component).IsAssignableFrom(theObj.GetType()) || typeof(GameObject).IsAssignableFrom(theObj.GetType()))
        //            {
        //                return theObj;
        //            }
        //            else
        //            if (typeof(UnityEngine.Object).IsAssignableFrom(theObj.GetType()))
        //            {
        //                return DeepCopyObject_Internal((UnityEngine.Object)theObj);
        //            }
        //            else if (theObj.GetType().IsArray)
        //            {
        //                var g = theObj as Array;
        //                for (int i = 0; i < g.Length; i++)
        //                {
        //                    g.SetValue(ProcessObject(g.GetValue(i)), i);
        //                }

        //            }
        //            else if (!theObj.GetType().IsPrimitive && !typeof(String).IsAssignableFrom(theObj.GetType()) && !theObj.GetType().IsEnum)
        //            {
        //                foreach (var v in theObj.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
        //                {
        //                    v.SetValue(theObj, ProcessObject(v.GetValue(theObj)));
        //                }
        //            }

        //            return theObj;
        //        }

        //        foreach (var v in newObj.GetType().GetFields())
        //        {
        //            v.SetValue(newObj, ProcessObject(v.GetValue(newObj)));
        //        }
        //        return newObj;
        //    }
        //    return DeepCopyObject_Internal(ob);
        //}

        public static Transform DisabledParent = new GameObject("DeactivedObject").transform;
    }

    public static class Spawner
    {
        public static void ToSpawn(string name)
        {
            SRBehaviour.InstantiateActor(Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault((GameObject x) => x.name == name), SRSingleton<SceneContext>.Instance.RegionRegistry.CurrentSceneGroup, SRSingleton<SceneContext>.Instance.Player.transform.position, Quaternion.identity, false, SlimeAppearance.AppearanceSaveSet.NONE, SlimeAppearance.AppearanceSaveSet.NONE);
        }
    }

    public static class Pedia
    {
        internal static HashSet<PediaEntry> addedPedias = new HashSet<PediaEntry>();

        public static void AddSlimepediaPage(string pediaEntryName, int pageNumber, string pediaText = "Placeholder Text. (Please set)", bool isRisks = false, bool isPlortonomics = false)
        {
            IdentifiablePediaEntry identifiablePediaEntry = Get<IdentifiablePediaEntry>(pediaEntryName);

            string CreatePageKey(string prefix)
            { return "m." + prefix + "." + identifiablePediaEntry.identifiableType.localizationSuffix + ".page." + pageNumber.ToString(); }

            if (isRisks && !isPlortonomics)
                LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", CreatePageKey("risks"), pediaText);
            else if (!isRisks && isPlortonomics)
                LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", CreatePageKey("plortonomics"), pediaText);
            else if (!isRisks && !isPlortonomics)
                LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", CreatePageKey("slimeology"), pediaText);
        }

        public static PediaEntry AddSlimepedia(IdentifiableType identifiableType, string pediaEntryName, string pediaIntro, string pediaSlimeology, string pediaRisks, string pediaPlortonomics, bool unlockedInitially = false)
        {
            if (Get<IdentifiablePediaEntry>(pediaEntryName))
                return null;

            PediaEntryCategory pediaEntryCategory = SRSingleton<SceneContext>.Instance.PediaDirector.entryCategories.items.ToArray().First(x => x.name == "Slimes");
            PediaEntryCategory basePediaEntryCategory = SRSingleton<SceneContext>.Instance.PediaDirector.entryCategories.items.ToArray().First(x => x.name == "Slimes");
            PediaEntry pediaEntry = basePediaEntryCategory.items.ToArray().First();
            IdentifiablePediaEntry identifiablePediaEntry = ScriptableObject.CreateInstance<IdentifiablePediaEntry>();

            string CreateKey(string prefix)
            { return "m." + prefix + "." + identifiableType.localizationSuffix; }

            string CreatePageKey(string prefix)
            { return "m." + prefix + "." + identifiableType.localizationSuffix + ".page." + 1.ToString(); }

            LocalizedString intro = LocalizationDirectorLoadTablePatch.AddTranslation("Pedia", CreateKey("intro"), pediaIntro);
            LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", CreatePageKey("slimeology"), pediaSlimeology);
            LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", CreatePageKey("risks"), pediaRisks);
            LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", CreatePageKey("plortonomics"), pediaPlortonomics);

            identifiablePediaEntry.hideFlags |= HideFlags.HideAndDontSave;
            identifiablePediaEntry.name = pediaEntryName;
            identifiablePediaEntry.identifiableType = identifiableType;
            identifiablePediaEntry.template = pediaEntry.template;
            identifiablePediaEntry.title = identifiableType.localizedName;
            identifiablePediaEntry.description = intro;
            identifiablePediaEntry.isUnlockedInitially = unlockedInitially;
            identifiablePediaEntry.actionButtonLabel = pediaEntry.actionButtonLabel;
            identifiablePediaEntry.infoButtonLabel = pediaEntry.infoButtonLabel;

            if (!pediaEntryCategory.items.Contains(identifiablePediaEntry))
                pediaEntryCategory.items.Add(identifiablePediaEntry);
            if (!addedPedias.Contains(identifiablePediaEntry))
                addedPedias.Add(identifiablePediaEntry);

            return identifiablePediaEntry;
        }
    }
}
