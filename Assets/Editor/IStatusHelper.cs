using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IStatus), true)]
public class IStatusHelper : Editor 
{
	private SerializedObject istatus;
	private SerializedProperty uitype, sprites, contents,textures,gameObjects,status,nativeSize,shaders,includeChildren;

	void OnEnable() 
	{
		istatus = new SerializedObject(target);
		uitype = istatus.FindProperty("uitype");
		sprites = istatus.FindProperty("sprites");
		contents = istatus.FindProperty("contents");
		textures = istatus.FindProperty("textures");
		gameObjects = istatus.FindProperty("gameObjects");
		status = istatus.FindProperty("_status");
		nativeSize =  istatus.FindProperty("nativeSize");
		shaders = istatus.FindProperty("shaders");
		includeChildren = istatus.FindProperty("includeChildren");
	}
	public override void OnInspectorGUI()
	{
		istatus.Update();//更新test
		EditorGUILayout.PropertyField(uitype);
		EditorGUILayout.PropertyField(status,new GUIContent("status","当前显示的状态"));

		if (uitype.enumValueIndex == 1) {
			EditorGUILayout.PropertyField (contents, new GUIContent ("contents", "各个状态的文本内容"), true);
		} else if (uitype.enumValueIndex == 2) {
			EditorGUILayout.PropertyField (nativeSize);
			EditorGUILayout.PropertyField (sprites, new GUIContent ("sprites", "各个状态显示的sprite"), true);
		} else if (uitype.enumValueIndex == 3) {
			EditorGUILayout.PropertyField (nativeSize);
			EditorGUILayout.PropertyField (textures, new GUIContent ("textures", "各个状态显示的texture"), true);
		} else if (uitype.enumValueIndex == 4)
			EditorGUILayout.PropertyField (sprites, new GUIContent ("sprites", "各个状态显示的sprite"), true);
		else if (uitype.enumValueIndex == 5) 
		{
			EditorGUILayout.PropertyField (includeChildren);
			EditorGUILayout.PropertyField (shaders, new GUIContent ("shaders", "各个状态显示的shader名字"), true);
		}
		else
			EditorGUILayout.PropertyField (gameObjects,new GUIContent("gameObjects","各个状态显示的对象"),true);
		
		istatus.ApplyModifiedProperties();//应用
	}
}
