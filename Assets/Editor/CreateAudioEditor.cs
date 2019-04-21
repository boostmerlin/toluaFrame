using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAudioEditor : Editor
{
    const string kCreateAudioMenu = EditorHelper.Prefix_FrameToolkit + "Create Audio Prefab";
    private static string audiosDir = "Assets/Other/Music";
    private static string prefabDir = "Assets/Other/Music";
    const string soundDefPath = "Assets/LuaFramework/lua/Config/SoundDef.lua";

    [MenuItem(kCreateAudioMenu)]
    static void CreateAudioPrefab()
    {
        string[] _patterns = new string[] { "*.mp3", "*.wav", "*.ogg" };
        List<string> _allFilePaths = new List<string>();
        if (!Directory.Exists(prefabDir))
        {
            Directory.CreateDirectory(prefabDir);
        }
        foreach (var item in _patterns)
        {
            string[] _temp = Directory.GetFiles(audiosDir, item, SearchOption.AllDirectories);
            _allFilePaths.AddRange(_temp);
        }

        //StreamWriter soundDef = new StreamWriter(soundDefPath);
        //soundDef.WriteLine("--use like SoundManager.Play(SoundDef.xxx)");
        //soundDef.WriteLine("SoundDef = {");
        foreach (var item in _allFilePaths)
        {
            FileInfo _fi = new System.IO.FileInfo(item);
            var _tempName = _fi.Name.Replace(_fi.Extension, "");
            AudioClip _clip = AssetDatabase.LoadAssetAtPath<AudioClip>(item);
            string path = string.Format("{0}/{1}.prefab", prefabDir, _tempName);
            if (null != _clip && !File.Exists(path))
            {
                //soundDef.WriteLine(string.Format("    {0}=\"{1}\",", _tempName, _tempName));
                GameObject _go = new GameObject();
                _go.name = _tempName;
                AudioSource _as = _go.AddComponent<AudioSource>();
                _as.playOnAwake = false;
                SoundData _data = _go.AddComponent<SoundData>();
                _data.audio = _as;
                _data.audio.clip = _clip;
                var temp = PrefabUtility.CreatePrefab(path, _go);

                DestroyImmediate(_go);
                EditorUtility.SetDirty(temp);
                Resources.UnloadAsset(_clip);
            }
        }
        //soundDef.WriteLine("}");
        //soundDef.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
