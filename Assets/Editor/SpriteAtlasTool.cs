using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.U2D;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public class SpriteAtlasTool
{
    const int ATLAS_MAX_SIZE = 4096;

    private static string spriteAtlasDir = AppDef.GameResDir + "SpriteAtlas";
    private static string spriteSrcRoot = AppDef.ArtsDir;
    const string kCreateAtlasMenu = EditorHelper.Prefix_FrameToolkit + "Create Sprite Atlas";
    const string kRemoveInvalidFilesMenu = EditorHelper.Prefix_FrameToolkit + "Delete Invalid Files";

    [MenuItem(kRemoveInvalidFilesMenu)]
    public static void RemoveFilesOfInavlidName()
    {
        EditorHelper.RemoveInvalidFile(spriteSrcRoot, true);
    }

    [MenuItem(kCreateAtlasMenu)]
    public static void CreateAtlasByFolders()
    {
        DirectoryInfo rootDirInfo = new DirectoryInfo(spriteSrcRoot);
        List<Object> folders = new List<Object>();
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            folders.Clear();
            if (dirInfo.Name.ToLower() == "atlasless")
            {
                continue;
            }
            string assetPath = dirInfo.FullName.Substring(dirInfo.FullName.IndexOf("Assets"));
            var o = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
            if (IsPackable(o))
            {
                folders.Add(o);
            }
            string atlasName = dirInfo.Name + ".spriteatlas";
            string atlaspath = CreateAtlas2017(atlasName);
            if (atlaspath != null)
            {
                SpriteAtlas sptAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlaspath);
                Debug.Log("Create Atlas OK:" + sptAtlas.tag);
                AddPackAtlas(sptAtlas, folders.ToArray());
            }
        }
    }

    static bool IsPackable(Object o)
    {
        return o != null && (o.GetType() == typeof(Sprite) || o.GetType() == typeof(Texture2D) || (o.GetType() == typeof(DefaultAsset) && ProjectWindowUtil.IsFolder(o.GetInstanceID())));
    }

    static void AddPackAtlas(SpriteAtlas atlas, Object[] sprites)
    {
        MethodInfo methodInfo = System.Type.GetType("UnityEditor.U2D.SpriteAtlasExtensions, UnityEditor")
             .GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
        if (methodInfo != null)
            methodInfo.Invoke(null, new object[] { atlas, sprites });
        else
            Debug.Log("methodInfo is null, not support maybe.");
    }

    public static string CreateAtlas2017(string atlasName)
    {
        //copy from template...haha
        string yaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!687078895 &4343727234628468602
SpriteAtlas:
  m_ObjectHideFlags: 0
  m_PrefabParentObject: {fileID: 0}
  m_PrefabInternal: {fileID: 0}
  m_Name: New Sprite Atlas
  m_EditorData:
    textureSettings:
      serializedVersion: 2
      anisoLevel: 1
      compressionQuality: 50
      maxTextureSize: 8192
      textureCompression: 0
      filterMode: 1
      generateMipMaps: 0
      readable: 0
      crunchedCompression: 0
      sRGB: 1
    platformSettings: []
    packingParameters:
      serializedVersion: 2
      padding: 4
      blockOffset: 1
      allowAlphaSplitting: 0
      enableRotation: 0
      enableTightPacking: 0
    variantMultiplier: 1
    packables: []
    bindAsDefault: 0
  m_MasterAtlas: {fileID: 0}
  m_PackedSprites: []
  m_PackedSpriteNamesToIndex: []
  m_Tag: New Sprite Atlas
  m_IsVariant: 0
";
        if (!Directory.Exists(spriteAtlasDir))
        {
            Directory.CreateDirectory(spriteAtlasDir);
        }
        string filePath = spriteAtlasDir + "/" + atlasName;
        if (File.Exists(filePath))
        {
            Debug.LogWarning("Atlas already exist: " + atlasName);
            return null;
        }
        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        yaml = yaml.Replace("New Sprite Atlas", atlasName).Replace("maxTextureSize: 8192", "maxTextureSize: " + ATLAS_MAX_SIZE);
        byte[] bytes = new UTF8Encoding().GetBytes(yaml);
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();
        AssetDatabase.Refresh();

        return filePath;
    }
}
