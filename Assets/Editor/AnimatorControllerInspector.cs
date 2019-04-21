using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

[CustomEditor(typeof(AnimatorController))]
public class AnimatorControllerInspector : Editor
{
    public const string Show_Hide = "Show_Hide";
    public const string Show_Showing_Hide = "Show_Showing_Hide";

    public bool quick_setup_enable = true;

    string assetPath;
    private void Awake()
    {
        assetPath = AssetDatabase.GetAssetPath(target);
        if(assetPath.Contains(Show_Hide) 
           || assetPath.Contains(Show_Showing_Hide))
        {
            quick_setup_enable = false;
        }
        AnimatorController ac = target as AnimatorController;
        var states = ac.LoadAllAsset<AnimatorState>();
        if (quick_setup_enable)
        {
            quick_setup_enable = !states.Any((obj) =>
            {
                return obj.motion != null;
            });
        }
    }

    string findSrcAsset(string name)
    {
        var assets = AssetDatabase.FindAssets(name + " t:" + typeof(AnimatorController).Name);
        if(assets.Length == 0)
        {
            return null;
        }
        return AssetDatabase.GUIDToAssetPath(assets[0]);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PrefixLabel("Quick Setup");
        EditorHelper.DrawButton(quick_setup_enable, Show_Hide, () =>
        {
            string asset = findSrcAsset(Show_Hide);
            if(asset == null)
            {
                Debug.Log(Show_Hide + " source animator not exist!");
                return;
            }
            AssetDatabase.CopyAsset(asset, assetPath);
        });
        EditorHelper.DrawButton(quick_setup_enable, Show_Showing_Hide, () =>
        {
            string asset = findSrcAsset(Show_Showing_Hide);
            if (asset == null)
            {
                Debug.Log(Show_Showing_Hide + " source animator not exist!");
                return;
            }
            AssetDatabase.CopyAsset(asset, assetPath);
        });
    }
}

