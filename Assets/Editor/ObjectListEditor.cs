using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectList), true)]
public class ObjectListEditor : Editor
{
    private SerializedProperty objType;
    private SerializedProperty nativeSize;
    private SerializedProperty includeChildren;
    private SerializedProperty enableAnimation;
    private SerializedProperty loopAnimation;
    private SerializedProperty frames;
    private SerializedProperty index;
    private SerializedProperty objects;
    private SerializedProperty autoApplyObject;

    void OnEnable()
	{
        objType = serializedObject.FindProperty("_objectType");
        index = serializedObject.FindProperty("_index");
		nativeSize =  serializedObject.FindProperty("nativeSize");
		includeChildren = serializedObject.FindProperty("includeChildren");

        frames = serializedObject.FindProperty("frames");
        loopAnimation = serializedObject.FindProperty("loopAnimation");
        enableAnimation = serializedObject.FindProperty("_enableAnimation");
        autoApplyObject = serializedObject.FindProperty("autoApplyObject");
        objects = serializedObject.FindProperty("objects");
    }

    private bool IsObjectTypeConsistent()
    {
        Type lastType= null;
        foreach (var obj in DragAndDrop.objectReferences)
        {
            if (lastType == null) lastType = obj.GetType();
            if (lastType != obj.GetType())
            {
                return false;
            }
        }
        var objectType = GetCurrentType().ToString();
        var name = lastType.Name;
        return name.Contains(objectType) || (name.Contains("Texture") && objectType == "Sprite");
    }

    private void PerformDrag(SerializedProperty currentObjectList)
    {
        //only support multiple here
        if (DragAndDrop.objectReferences.Length <= 1)
        {
            return;
        }
        var eventType = Event.current.type;
        if (eventType != EventType.DragUpdated && eventType != EventType.DragPerform) return;
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (eventType != EventType.DragPerform) return;
        if (!IsObjectTypeConsistent())
        {
            Debug.LogWarning("Object Type Not consistent!");
            return;
        }
        DragAndDrop.AcceptDrag();
        currentObjectList.ClearArray();
        foreach (var obj in DragAndDrop.objectReferences)
        {
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
        }
        Event.current.Use();
    }

    private void DrawObjects(SerializedProperty currentObjectList)
    {
        if (EditorGUILayout.PropertyField(currentObjectList))
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
    }

    private void DrawGeneralProperty()
    {
        EditorGUILayout.PropertyField(objType, new GUIContent("Object Type"));
        EditorGUILayout.PropertyField(index, new GUIContent("Index", "当前索引"));
        EditorGUILayout.PropertyField(autoApplyObject, new GUIContent("Apply Default Usage"));
        EditorGUILayout.PropertyField(enableAnimation, new GUIContent("Enable Animation", "是否动画"));
        if (enableAnimation.boolValue)
        {
            EditorGUILayout.PropertyField(frames, new GUIContent("Frames", "动画帧率"));
            EditorGUILayout.PropertyField(loopAnimation, new GUIContent("Loop Animation", "动画循环"));
        }
    }

    private void DrawTypeSpecificProperty(ObjectList.ObjectType objectType)
    {
        switch (objectType)
        {
            case ObjectList.ObjectType.Sprite:
                EditorGUILayout.PropertyField(nativeSize);
                break;
            case ObjectList.ObjectType.Shader:
            case ObjectList.ObjectType.Material:
                EditorGUILayout.PropertyField(includeChildren, new GUIContent("Include Children", "设置孩子"));
                break;
        }
    }

    private ObjectList.ObjectType GetCurrentType()
    {
        int intType = objType.enumValueIndex;
        var currentType = (ObjectList.ObjectType) intType;
        return currentType;
    }

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
        var currentType = GetCurrentType();
        var currentObjectList = objects.FindPropertyRelative(currentType.ToString());
        PerformDrag(currentObjectList);
        DrawGeneralProperty();
        DrawTypeSpecificProperty(currentType);
        DrawObjects(currentObjectList);
        serializedObject.ApplyModifiedProperties();
    }
}
