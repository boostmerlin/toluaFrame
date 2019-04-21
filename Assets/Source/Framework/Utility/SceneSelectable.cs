using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LuaInterface;

[RequireComponent(typeof(BoxCollider2D))]

public class SceneSelectable : MonoBehaviour {
    public class SceneSelectEvent : UnityEvent<LeanFinger, GameObject>{}

    public SceneSelectEvent onTaped;

    public LuaFunction luaCallBack;


    void Awake () {
        onTaped = new SceneSelectEvent();
    }

    public void DoSelect()
    {
        if (onTaped != null)
        {
            onTaped.Invoke(null, gameObject);
        }
    }

    private void OnDestroy()
    {
        if(luaCallBack != null)
        {
            luaCallBack.Dispose();
            luaCallBack = null;
        }
    }
}
