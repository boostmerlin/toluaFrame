using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using LuaFramework;

public class Packager
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();
     ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext)
    {   
        foreach (string e in exts)
        {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    [MenuItem("Build/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource()
    {
        BuildTarget target;
        target = BuildTarget.iOS;
        BuildAssetResource(target);
    }

    [MenuItem("Build/Build Android Resource", false, 101)]
    public static void BuildAndroidResource()
    {
        BuildAssetResource(BuildTarget.Android);
    }

    [MenuItem("Build/Build Windows Resource", false, 102)]
    public static void BuildWindowsResource()
    {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Build/Build Resource(No Bundle)", false, 103)]
    public static void BuildResource()
    {
        BuildAssetResource(BuildTarget.NoTarget, false);
    }

    [MenuItem("Build/Clear StreamingAssets", false, 104)]
    public static void ClearStreamingAssets()
    {
        string resPath = AppDataPath + "/StreamingAssets/";
        Directory.Delete(resPath, true);
    }

    //[MenuItem("Build/Remove Manifest", false, 105)]
    static void RemoveManifest()
    {
        string resPath = AppDataPath + "/StreamingAssets/";
        string[] files = Directory.GetFiles(resPath, "*.manifest", SearchOption.AllDirectories);
        foreach(var f in files)
        {
            File.Delete(f);
        }
    }

    public static void BuildAssetResource(BuildTarget target, bool bundle = true)
    {
        if (Directory.Exists(CSUtil.DataPath))
        {
            Directory.Delete(CSUtil.DataPath, true);
        }
        string streamPath = Application.streamingAssetsPath;
        if (!Directory.Exists(streamPath))
        {
            Directory.CreateDirectory(streamPath);
        }
        if (AppDef.LuaBundleMode)
        {
            HandleLuaBundle();
        }
        else
        {
            HandleLuaFile();
        }
        if (bundle)
        {
            HandleGameAssetsBundle();
            string resPath = "Assets/" + AppDef.AssetDir;
            BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.None, target);
        }
        RemoveManifest();
        BuildFileIndex();
        AssetDatabase.Refresh();
    }

    static void AddBuildMap(string bundleName, string pattern, string path)
    {
        string[] files = Directory.GetFiles(path, pattern);
        if (files.Length == 0) return;

        List<string> bundleFiles = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
                continue;
            bundleFiles.Add(files[i].Replace('\\', '/'));
        }
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = bundleFiles.ToArray();
        maps.Add(build);
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    static void HandleLuaBundle()
    {
        string streamDir = Application.dataPath + "/" + AppDef.LuaTempDir;
        if (!Directory.Exists(streamDir)) Directory.CreateDirectory(streamDir);

        string[] srcDirs = { LuaConst.luaDir, LuaConst.toluaDir };
        for (int i = 0; i < srcDirs.Length; i++)
        {
            if (AppDef.LuaByteMode)
            {
                string sourceDir = srcDirs[i];
                string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                int len = sourceDir.Length;

                if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
                {
                    --len;
                }
                for (int j = 0; j < files.Length; j++)
                {
                    string str = files[j].Remove(0, len);
                    string dest = streamDir + str + ".bytes";
                    string dir = Path.GetDirectoryName(dest);
                    Directory.CreateDirectory(dir);
                    EncodeLuaFile(files[j], dest);
                }
            }
            else
            {
                string sourceDir = srcDirs[i];
                string destDir = streamDir;
                bool appendext = false;
                if (!Directory.Exists(sourceDir))
                {
                    return;
                }

                string[] files = Directory.GetFiles(sourceDir, "*.lua",  SearchOption.AllDirectories);
                int len = sourceDir.Length;

                if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
                {
                    --len;
                }

                for (int j = 0; j < files.Length; j++)
                {
                    string str = files[j].Remove(0, len);
                    string dest = destDir + "/" + str;
                    if (appendext) dest += ".bytes";
                    string dir = Path.GetDirectoryName(dest);
                    Directory.CreateDirectory(dir);
                    File.Copy(files[i], dest, true);
                }
            }
        }
        string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = dirs[i].Replace(streamDir, string.Empty);
            name = name.Replace('\\', '_').Replace('/', '_');
            name = "lua/lua_" + name.ToLower() + AppDef.ExtName;

            string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
            AddBuildMap(name, "*.bytes", path);
        }
        AddBuildMap("lua/lua" + AppDef.ExtName, "*.bytes", "Assets/" + AppDef.LuaTempDir);

        //-------------------------------处理非Lua文件----------------------------------
        string luaPath = AppDataPath + "/StreamingAssets/lua/";
        for (int i = 0; i < srcDirs.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = srcDirs[i].ToLower();
            Recursive(luaDataPath);
            foreach (string f in files)
            {
                if (f.EndsWith(".meta") || f.EndsWith(".lua")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string path = Path.GetDirectoryName(luaPath + newfile);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string destfile = path + "/" + Path.GetFileName(f);
                File.Copy(f, destfile, true);
            }
        }
        AssetDatabase.Refresh();
    }

    static void HandleGameAssetsBundle()
    {
        SetAssetLabers.SetVersionDirAssetName(null);
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile()
    {
        string resPath = AppDataPath + "/StreamingAssets/";
        string luaPath = resPath + "/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath))
        {
            Directory.CreateDirectory(luaPath);
        }
        string[] luaPaths = { LuaConst.luaDir + "/",
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files)
            {
                if (f.EndsWith(".meta") || f.EndsWith(".vs") || f.EndsWith(".vscode")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath))
                {
                    File.Delete(newpath);
                }
                if (AppDef.LuaByteMode)
                {
                    EncodeLuaFile(f, newpath);
                }
                else
                {
                    File.Copy(f, newpath, true);
                }
                UpdateProgress(n++, files.Count, newpath);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static void BuildFileIndex()
    {
        string resPath = AppDataPath + "/StreamingAssets/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = CSUtil.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
    }

    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    public static void EncodeLuaFile(string srcFile, string outFile)
    {
        if (!srcFile.ToLower().EndsWith(".lua"))
        {
            File.Copy(srcFile, outFile, true);
            return;
        }
        bool isWin = true;
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            isWin = false;
            luaexe = "./luajit";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit_mac/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }
}