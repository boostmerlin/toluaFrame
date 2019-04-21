using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteList))]
public class SpriteListEditor : Editor
{
    SerializedProperty defaultIndex;
    SerializedProperty sprites;
    int lastIndex;

    void OnEnable()
    {
        defaultIndex = serializedObject.FindProperty("defaultIndex");
        lastIndex = ((SpriteList)target).CurrentIndex;
        sprites = serializedObject.FindProperty("sprites");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var eventType = Event.current.type;
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                if (DragAndDrop.objectReferences.Length > 1)
                {
                    sprites.ClearArray();
                    List<string> list = new List<string>();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (list.Contains(obj.name))
                        {
                            continue;
                        }
                        int j = sprites.arraySize;
                        sprites.InsertArrayElementAtIndex(j);
                        var sp = sprites.GetArrayElementAtIndex(j);
                        if (obj is Texture2D)
                        {
                            sp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(obj));
                        }
                        else
                            sp.objectReferenceValue = obj;
                        list.Add(obj.name);
                    }
                    serializedObject.ApplyModifiedProperties();
                    Event.current.Use();
                }
            }
        }

        EditorGUILayout.PropertyField(defaultIndex, new GUIContent("Default Index"));
        if (sprites.arraySize == 0)
        {
            defaultIndex.intValue = 0;
        }
        else
        {
            defaultIndex.intValue = Mathf.Clamp(defaultIndex.intValue, 0, sprites.arraySize - 1);
        }
        if(lastIndex != defaultIndex.intValue)
        {
            int v = defaultIndex.intValue;
            if (!Application.isPlaying)
            {
                ((SpriteList)target).ChangeSprite(defaultIndex.intValue);
                lastIndex = v;
            }
        }
        if (EditorGUILayout.PropertyField(sprites, new GUIContent("Sprite List")))
        {
            EditorGUI.indentLevel = 1;
            sprites.arraySize = EditorGUILayout.IntField("Size: ", sprites.arraySize);
            EditorGUI.indentLevel = 2;
            for (int i = 0; i < sprites.arraySize; i++)
            {
                var s = sprites.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(s, typeof(Sprite));
                if (GUILayout.Button("up"))
                {
                    int upj = i - 1;
                    if (upj >= 0)
                    {
                        sprites.MoveArrayElement(i, upj);
                    }
                }
                if (GUILayout.Button("down"))
                {
                    int downj = i + 1;
                    if (downj < sprites.arraySize)
                    {
                        sprites.MoveArrayElement(i, downj);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
