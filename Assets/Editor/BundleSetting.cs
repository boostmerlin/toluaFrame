using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

#if UNITY_EDITOR
[Serializable]
public class BundleInfo
{
    public string path;
    public string relativePath = "Assets";
    public bool isFlatternDirectory;
    public bool isBundleIntoOne;

    public override bool Equals(object obj)
    {
        return path.Equals(((BundleInfo)obj).path);
    }

    public override int GetHashCode()
    {
        return path.GetHashCode();
    }
}

//[CreateAssetMenu(fileName = "BundleSetting", menuName = "BundleSetting", order = 0)]
public sealed class BundleSetting : ScriptableObject
{
    public const string BundleExtension = ".unity3d";
    private const string AssetName = "BundleSetting";
    private static BundleSetting s_Instance = null;
    private const string assetPath = "Assets/Editor/Custom/" + AssetName + ".asset";

    public List<BundleInfo> bundleInfos = new List<BundleInfo>();
    public string assetExtensions = ".png;.prefab;.otf;.jpg;.spriteatlas";

    public static BundleSetting Instance
    {
        get
        {
            if (!s_Instance)
            {
                s_Instance = AssetDatabase.LoadAssetAtPath<BundleSetting>(assetPath);
            }

            if (!s_Instance)
            {
                s_Instance = CreateSettingsAndSave();
            }
            return s_Instance;
        }
    }

    private static BundleSetting CreateSettingsAndSave()
    {
        var bundleSetting = ScriptableObject.CreateInstance<BundleSetting>();

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += () => SaveAsset(bundleSetting);
        }
        else
        {
            SaveAsset(bundleSetting);
        }
        return bundleSetting;
    }

    private static void SaveAsset(BundleSetting BundleSetting)
    {
        var directoryName = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        var uniqueAssetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
        AssetDatabase.CreateAsset(BundleSetting, uniqueAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(AssetName + " has been created: " + assetPath);
    }
}
#endif
