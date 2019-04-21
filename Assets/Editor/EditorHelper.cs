
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class EditorHelper{
    public const string Prefix_FrameToolkit = "FrameTool/";

    public struct CmdResult
    {
        public int code;
        public string msg;
    }
    public static CmdResult RunCmd(string cmdExe, string args)
    {
        int code = -121;
        string result = string.Empty;
        try
        {
            using (System.Diagnostics.Process myPro = new System.Diagnostics.Process())
            {
                myPro.StartInfo.FileName = cmdExe;
                myPro.StartInfo.Arguments = args;
                myPro.StartInfo.UseShellExecute = false;
                myPro.StartInfo.CreateNoWindow = true;
                myPro.StartInfo.RedirectStandardOutput = true;
                myPro.StartInfo.RedirectStandardError = true;
                myPro.Start();
                result = myPro.StandardError.ReadToEnd();
                if (string.IsNullOrEmpty(result))
                {
                    result = myPro.StandardOutput.ReadToEnd();
                }
                myPro.WaitForExit();
                code = myPro.ExitCode;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("some error on {0} {1} width exeception: {2}", cmdExe, args, e);
        }
        return new CmdResult() { code = code, msg = result };
    }

    public static string GetSelectPath()
    {
        string root = "Assets";
        string[] guids = Selection.assetGUIDs;
        if (guids.Length > 0)
        {
            root = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return root;
    }

    public static List<T> LoadAllAsset<T>(this Object obj) where T : Object
    {
        List<T> assets = new List<T>();

        Object[] subs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj));
        foreach (Object sub in subs)
        {
            T asset = sub as T;
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }

    public static void DrawButton(bool enable, string name, System.Action onClick)
    {
        GUI.enabled = enable;
        if (GUILayout.Button(name))
        {
            onClick();
        }
        GUI.enabled = true;
        GUILayout.Space(EditorGUI.indentLevel * 15);
    }

    public static void RemoveInvalidFile(string dir, bool delete)
    {
        System.IO.DirectoryInfo rootDirInfo = new System.IO.DirectoryInfo(dir);
        foreach (var fi in rootDirInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories))
        {
            if (fi.Name.EndsWith(".meta"))
            {
                continue;
            }
            if (!EditorHelper.IsEnglishFileName(fi.Name) && BundleSetting.Instance.assetExtensions.Contains(fi.Extension))
            {
                Debug.LogWarning("File is not English name: " + fi.Name);
                if (delete)
                {
                    fi.Delete();
                }
            }
        }
    }

    public static bool IsEnglishFileName(string name)
    {
        foreach (char c in name)
        {
            if ((int)c > 127)
            {
                return false;
            }
        }
        return true;
    }

    public static bool SaveRenderTextureToPNG(RenderTexture rt, string contents, string pngName, bool open)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        string filePath = contents + "/" + pngName + ".png";
        FileStream file = File.Open(filePath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        png = null;
        RenderTexture.active = prev;
        if (open)
            Application.OpenURL(filePath);
        int index = filePath.IndexOf("Assets/");
        if (index == -1)
            return false;

        TextureImporter textureImporter = AssetImporter.GetAtPath(filePath.Substring(index).Replace("\\", "/")) as TextureImporter;
        if (textureImporter == null)
            return false;
        TextureImporterSettings settings = new TextureImporterSettings();
        textureImporter.ReadTextureSettings(settings);
        textureImporter.maxTextureSize = 4096;
        textureImporter.SetTextureSettings(settings);
        AssetDatabase.SaveAssets();
        return true;
    }
}
