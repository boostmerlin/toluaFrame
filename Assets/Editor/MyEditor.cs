using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Animations;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEditor.ProjectWindowCallback;
using UnityEngine.UI;

public class MyEditor : EditorWindow
{
    [MenuItem("GameObject/UI/IButton")]
    public static void CreateIButton()
    {
        Transform parent = Selection.activeTransform;
        GameObject obj = new GameObject("IButton", typeof(Image), typeof(IButton));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(200, 50);
        }
    }

    [MenuItem("GameObject/UI/IButton(with text)")]
    public static void CreateIButtonWithText()
    {
        Transform parent = Selection.activeTransform;
        GameObject obj = new GameObject("IButton", typeof(Image), typeof(IButton));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(200, 50);
        }

        GameObject text = new GameObject("text", typeof(Text));
        text.transform.SetParent(obj.transform, false);
        RectTransform rect1 = text.GetComponent<RectTransform>();
        rect1.anchorMin = Vector2.zero;
        rect1.anchorMax = Vector2.one;
        rect1.offsetMin = Vector2.one;
        rect1.offsetMax = Vector2.one * -1;
        Text txt = text.GetComponent<Text>();
        txt.text = "IButton";
        txt.color = Color.black;
        txt.fontSize = 22;
        txt.alignment = TextAnchor.MiddleCenter;
    }

    [MenuItem(EditorHelper.Prefix_FrameToolkit + "Generate Config lua File", priority = 1)]
    public static void BuildConfigLuaFile()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "xls2lua");
        string protoc = Path.Combine(dir, "xls2lua.bat");

        runBat(protoc, dir);
    }

    static void runBat(string cmd, string dir)
    {
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = cmd;
        info.UseShellExecute = true;
        info.WorkingDirectory = dir;
        info.ErrorDialog = true;
        Debug.Log(info.FileName + " " + info.Arguments);
        Process pro = Process.Start(info);
        pro.WaitForExit();
    }

    [MenuItem(EditorHelper.Prefix_FrameToolkit + "Generate Protobuf lua File", priority = 1)]
    public static void BuildProtobufFile()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "protoc-gen-lua");
        string protoc = Path.Combine(dir, "gen_pblua.bat");
        runBat(protoc, dir);

        string dest = Path.Combine(LuaConst.luaDir, "pblua");
        if (Directory.Exists(dest))
        {
            FileUtil.DeleteFileOrDirectory(dest);
        }

        FileUtil.CopyFileOrDirectory(Path.Combine(dir, "lua"), dest);

        AssetDatabase.Refresh();
    }




    [MenuItem("Assets/Create/Create Lua View")]
    public static void CreateViewScript()
    {
        string path = EditorHelper.GetSelectPath() + "/NewView.lua";
        CreateScriptAssetAction action = ScriptableObject.CreateInstance<CreateScriptAssetAction>();
        action.content = "local NewView = class(\"NewView\", UIView)\n\n" +
            "function NewView:ctor(data)\n\tNewView.super.ctor(self)\n\t--body\nend\n\n" +
            "function NewView:onCreate()\n\tNewView.super.onCreate(self)\n\t--body\nend\n\n" +
            "function NewView:onShow()\n\tNewView.super.onShow(self)\n\t--body\nend\n\n" +
            "function NewView:onHided()\n\t--body\n\tNewView.super.onHided(self)\nend\n\n" +
            "function NewView:onDispose()\n\t-- body\n\tNewView.super.onDispose(self)\nend\n\n" +
            "return NewView";
        action.replaceName = "NewView";
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, path, null, null);
    }
}

class CreateScriptAssetAction : EndNameEditAction
{
    public string content = "";
    public string replaceName = "";

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        //创建资源
        UnityEngine.Object obj = CreateAssetFromTemplate(pathName, content, replaceName);
        //高亮显示该资源
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }

    internal static UnityEngine.Object CreateAssetFromTemplate(string pathName, string content, string replaceName)
    {
        //获取要创建的资源的绝对路径
        string fullName = Path.GetFullPath(pathName);

        //获取资源的文件名
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        if (!string.IsNullOrEmpty(replaceName))
            content = content.Replace(replaceName, fileName);

        StreamWriter writer = new StreamWriter(fullName, false);
        writer.Write(content);
        writer.Close();

        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();
        Debug.Log(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }
}

