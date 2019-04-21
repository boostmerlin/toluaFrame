using System.IO;
using UnityEditor;
using UnityEngine.Events;
using System.Linq;

public class SetAssetLabers
{
    private static void SetVersionDirAssetName(BundleInfo bundleInfo)
    {
        string fullPath = bundleInfo.path;
        if (Directory.Exists(fullPath))
        {
            var dir = new DirectoryInfo(fullPath);
            var files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            string[] assetExts = BundleSetting.Instance.assetExtensions.Split(';');
            for (var i = 0; i < files.Length; ++i)
            {
                var fileInfo = files[i];
                EditorUtility.DisplayProgressBar("Set Asset Label", "Set " + fileInfo.Name + "...", 1f * i / files.Length);
                string extension = fileInfo.Extension.ToLower();
                if (assetExts.Contains(extension))
                {
                    var importer = AssetImporter.GetAtPath(fullPath + "/" + fileInfo.Name);
                    if (importer)
                    {
                        //if (!string.IsNullOrEmpty(importer.assetBundleName))
                        //{
                        //    continue;
                        //}
                        string bundleName = string.Empty;
                        string variantName = null;
                        if (bundleInfo.isBundleIntoOne)
                        {
                            bundleName = dir.Name;
                        }
                        else
                        {
                            bundleName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                        }
                        if (bundleInfo.isFlatternDirectory)
                        {
                        }
                        else
                        {
                            string relaPath = bundleInfo.path.Remove(0, bundleInfo.relativePath.Length);
                            if (relaPath.StartsWith("/"))
                            {
                                relaPath = relaPath.Substring(1);
                            }
                            if (bundleInfo.isBundleIntoOne)
                            {
                                bundleName = relaPath.Replace(bundleName, "");
                            }
                            bundleName = Path.Combine(relaPath, bundleName).Replace("\\", "/");
                        }
                        bundleName = bundleName + BundleSetting.BundleExtension;
                        importer.SetAssetBundleNameAndVariant(bundleName, variantName);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    public static void SetVersionDirAssetName(UnityAction endcall)
    {
        BundleSetting bundleSetting = BundleSetting.Instance;
        foreach (var bi in bundleSetting.bundleInfos)
        {
            SetVersionDirAssetName(bi);
        }
        if (endcall != null) endcall();
    }
}

