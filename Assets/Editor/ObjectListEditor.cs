using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectList), true)]
public class ObjectListEditor : Editor 
{
    private SerializedProperty objType;
    private SerializedProperty currentObjectList;
    private SerializedProperty nativeSize;
    private SerializedProperty includeChildren;
    private SerializedProperty enableAnamtion;
    private SerializedProperty loopAnimation;
    private SerializedProperty frames;
    private SerializedProperty index;


    void OnEnable() 
	{
        objType = serializedObject.FindProperty("objectType");
        index = serializedObject.FindProperty("_index");
		nativeSize =  serializedObject.FindProperty("nativeSize");
		includeChildren = serializedObject.FindProperty("includeChildren");

        frames = serializedObject.FindProperty("frames");
        loopAnimation = serializedObject.FindProperty("loopAnimation");
        enableAnamtion = serializedObject.FindProperty("_enableAnimation");
    }

    private void PerformDrag()
    {
        int intType = objType.enumValueIndex;
        if (intType == 1
            || intType == 0)
        {
            return;
        }
        var eventType = Event.current.type;
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                if (DragAndDrop.objectReferences.Length > 1)
                {
                    currentObjectList.ClearArray();
                    List<string> list = new List<string>();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (list.Contains(obj.name))
                        {
                            continue;
                        }
                        int j = currentObjectList.arraySize;
                        currentObjectList.InsertArrayElementAtIndex(j);
                        var sp = currentObjectList.GetArrayElementAtIndex(j);
                        if (obj is Texture2D)
                        {
                            sp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(obj));
                        }
                        else
                        {
                            sp.objectReferenceValue = obj;
                        }
                        list.Add(obj.name);
                    }
                    serializedObject.ApplyModifiedProperties();
                    Event.current.Use();
                }
            }
        }
    }

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
        currentObjectList = serializedObject.FindProperty(ObjectList.propertyName[objType.enumValueIndex]);
		EditorGUILayout.PropertyField(objType, new GUIContent("Object Type"));
		EditorGUILayout.PropertyField(index, new GUIContent("index", "当前索引"));
        EditorGUILayout.PropertyField(enableAnamtion, new GUIContent("enableAnamtion", "是否动画"));

        if (enableAnamtion.boolValue)
        {
            EditorGUILayout.PropertyField(frames, new GUIContent("frames", "动画帧率"));
            EditorGUILayout.PropertyField(loopAnimation, new GUIContent("loopAnimation", "动画循环"));
        }

        int intType = objType.enumValueIndex;
        switch ((ObjectList.ObjectType)intType)
        {
            case ObjectList.ObjectType.Image:
            case ObjectList.ObjectType.RawImage:
                EditorGUILayout.PropertyField(nativeSize);
                break;
            case ObjectList.ObjectType.Shader:
                EditorGUILayout.PropertyField(includeChildren);
                break;
        }
        if (EditorGUILayout.PropertyField(currentObjectList, new GUIContent(ObjectList.propertyName[intType])))
        {
            EditorGUI.indentLevel = 1;
            currentObjectList.arraySize = EditorGUILayout.IntField("Size: ", currentObjectList.arraySize);
            EditorGUI.indentLevel = 2;
            for (int i = 0; i < currentObjectList.arraySize; i++)
            {
                var s = currentObjectList.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(s);
                if (GUILayout.Button("up", GUILayout.Width(45)))
                {
                    int upj = i - 1;
                    if (upj >= 0)
                    {
                        currentObjectList.MoveArrayElement(i, upj);
                    }
                }
                if (GUILayout.Button("down", GUILayout.Width(45)))
                {
                    int downj = i + 1;
                    if (downj < currentObjectList.arraySize)
                    {
                        currentObjectList.MoveArrayElement(i, downj);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
