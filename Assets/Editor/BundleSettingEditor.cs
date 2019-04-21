using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

//[CreateAssetMenu(fileName = "BundleSetting", menuName = "BundleSetting", order = 0)]
[CustomEditor(typeof(BundleSetting))]
public sealed class BundleSettingEditor : Editor
{
    ReorderableList reorderableList;
    private void OnEnable()
    {
        SerializedProperty prop = serializedObject.FindProperty("bundleInfos");
        reorderableList = new ReorderableList(serializedObject, prop, false, true, true, true);
        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "打包路径");
        };
        var h = EditorGUIUtility.singleLineHeight;
        reorderableList.elementHeight = 3 * (h + 5);
        reorderableList.elementHeightCallback = (index) =>
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty isFlatternDirectory = element.FindPropertyRelative("isFlatternDirectory");
            if (!isFlatternDirectory.boolValue)
            {
                return reorderableList.elementHeight + h + 5;
            }
            else
            {
                return reorderableList.elementHeight;
            }
        };
        reorderableList.drawElementCallback =
            (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty path = element.FindPropertyRelative("path");
                Rect r = new Rect(rect.x, rect.y + 4, rect.width, h);
                if ((Event.current.type == EventType.DragUpdated
                  || Event.current.type == EventType.DragPerform)
                  && r.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        if (Directory.Exists(DragAndDrop.paths[0]))
                        {
                            path.stringValue = DragAndDrop.paths[0];
                        }
                    }
                    Event.current.Use();
                }
                path.stringValue = EditorGUI.TextField(r, "打包路径:", path.stringValue);
                SerializedProperty isFlatternDirectory = element.FindPropertyRelative("isFlatternDirectory");
                r = new Rect(rect.x, r.y + h + 4, rect.width, h);
                isFlatternDirectory.boolValue = EditorGUI.Toggle(r, "是否忽略文件夹结构:", isFlatternDirectory.boolValue);
                if (!isFlatternDirectory.boolValue)
                {
                    SerializedProperty relativePath = element.FindPropertyRelative("relativePath");
                    r = new Rect(rect.x, r.y + h + 4, rect.width, h);
                    relativePath.stringValue = EditorGUI.TextField(r, "资源相对路径:", relativePath.stringValue);
                }
                SerializedProperty isBundleIntoOne = element.FindPropertyRelative("isBundleIntoOne");
                r = new Rect(rect.x, r.y + h + 4, rect.width, h);
                isBundleIntoOne.boolValue = EditorGUI.Toggle(r, "是否打包成单一Bundle:", isBundleIntoOne.boolValue);
            };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BundleSetting bundleSetting = ((BundleSetting)target);
        bundleSetting.assetExtensions = EditorGUILayout.TextField("待打包资源扩展名: ", bundleSetting.assetExtensions);
        if (GUILayout.Button("移除重复路径"))
        {
            var bundleInfos = bundleSetting.bundleInfos;
            HashSet<BundleInfo> unique = new HashSet<BundleInfo>(bundleInfos);
            bundleInfos.Clear();
            bundleInfos.AddRange(unique);
        }
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
