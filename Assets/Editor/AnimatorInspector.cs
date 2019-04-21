using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

[CustomEditor(typeof(Animator))]
public class AnimatorInspector : Editor
{
    const string STATE_SHOW = "Show";
    const string STATE_HIDE = "Hide";

    UIAnimation UIAnimation;
    bool validate = true;
    private void Awake()
    {
        Animator animator = target as Animator;
        validate = animator.parameters.All((p)=> {
            if(p.name == "Play" && p.type == AnimatorControllerParameterType.Trigger
            || p.name == "IsShow" && p.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
            return false;
        });

        UIAnimation = new UIAnimation(animator);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            EditorHelper.DrawButton(true, STATE_SHOW, () =>
            {
                UIAnimation.Show();
            });
            EditorHelper.DrawButton(true, STATE_HIDE, () =>
            {
                UIAnimation.Hide();
            });
            EditorGUILayout.EndHorizontal();
        }
    }
}

