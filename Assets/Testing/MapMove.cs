using UnityEngine;
using Lean.Touch;
using System.Collections.Generic;
using LuaInterface;

public class MapMove : CameraScrollZoom, CameraScrollZoom.IScrollZoomEvent
{
    LuaFunction luaSnapMoveEndCallBack;
    LuaFunction luaSnapZoomEndCallBack;
    List<LuaFunction> moveRollCallBacks = new List<LuaFunction>();
    LuaFunction mapMoveEvent;
    LuaFunction mapZoomEvent;

    void Start()
    {
        SetScrollZoomEvent(this);
    }
    public void SnapToPostion(Vector3 pos, LuaFunction func, bool transition)
    {
        if (luaSnapMoveEndCallBack != func && luaSnapMoveEndCallBack!= null)
        {
            luaSnapMoveEndCallBack.Dispose();
            luaSnapMoveEndCallBack = null;
        }
        luaSnapMoveEndCallBack = func;
        SnapToPostion(pos, transition);
    }

    public void ReleaseSnapCallBack()
    {
        if (luaSnapZoomEndCallBack != null)
        {
            luaSnapZoomEndCallBack.Dispose();
            luaSnapZoomEndCallBack = null;
        }

        if (luaSnapMoveEndCallBack != null)
        {
            luaSnapMoveEndCallBack.Dispose();
            luaSnapMoveEndCallBack = null;
        }
    }

    public void SnapToZoom(float v, LuaFunction func = null, bool transition = true)
    {
        if (luaSnapZoomEndCallBack != func && luaSnapZoomEndCallBack != null)
        {
            luaSnapZoomEndCallBack.Dispose();
            luaSnapZoomEndCallBack = null;
        }
        luaSnapZoomEndCallBack = func;
        base.SnapToZoom(v, transition);
    }

    public void SnapToGameObject(GameObject go, LuaFunction func, bool transition = true)
    {
        Vector3 pos = go.transform.position;
        this.SnapToPostion(pos, func, transition);
    }

    public void SetMapEvent(LuaFunction func)
    {
        if (mapMoveEvent != func && mapMoveEvent != null)
        {
            mapMoveEvent.Dispose();
            mapMoveEvent = null;
        }
        mapMoveEvent = func;
    }

    public void SetMapZoomEvent(LuaFunction func)
    {
        if (mapZoomEvent != func && mapZoomEvent != null)
        {
            mapZoomEvent.Dispose();
            mapZoomEvent = null;
        }
        mapZoomEvent = func;
    }

    public void RemoveMoveRollEvent(LuaFunction func)
    {
        moveRollCallBacks.Remove(func);
        func.Dispose();
    }

    public void AddMoveRollEvent(LuaFunction func)
    {
        if (!moveRollCallBacks.Contains(func))
        {
            moveRollCallBacks.Add(func);
        }
    }

    public void ClearMoveRollCallBack()
    {
        foreach (var func in moveRollCallBacks)
        {
            func.Dispose();
        }
        moveRollCallBacks.Clear();
    }

    private void OnDestroy()
    {
        ClearMoveRollCallBack();
        if (mapMoveEvent != null)
        {
            mapMoveEvent.Dispose();
            mapMoveEvent = null;
        }
        if (mapZoomEvent != null)
        {
            mapZoomEvent.Dispose();
            mapZoomEvent = null;
        }

        if (luaSnapMoveEndCallBack != null)
        {
            luaSnapMoveEndCallBack.Dispose();
            luaSnapMoveEndCallBack = null;
        }
        if (luaSnapZoomEndCallBack != null)
        {
            luaSnapZoomEndCallBack.Dispose();
            luaSnapZoomEndCallBack = null;
        }
    }

    void IScrollZoomEvent.OnMoveEvent(int state, bool isSnap)
    {
        if (mapMoveEvent != null)
        {
            mapMoveEvent.Call(state, isSnap);
        }
        if (state == END && isSnap)
        {
            if (luaSnapMoveEndCallBack != null)
            {
                luaSnapMoveEndCallBack.Call();
                luaSnapMoveEndCallBack.Dispose();
                luaSnapMoveEndCallBack = null;
            }
        }
    }

    void IScrollZoomEvent.OnMapZoom(int state, bool isSnap)
    {
        if(mapZoomEvent != null)
        {
            mapZoomEvent.Call(state, isSnap);
        }
        if (state == END && isSnap)
        {
            if (luaSnapZoomEndCallBack != null)
            {
                luaSnapZoomEndCallBack.Call();
                luaSnapZoomEndCallBack.Dispose();
                luaSnapZoomEndCallBack = null;
            }
        }
    }

    void IScrollZoomEvent.OnViewRectChange()
    {
        foreach(var callBack in moveRollCallBacks)
        {
            callBack.Call(ViewRect.x, ViewRect.y, ViewRect.width, ViewRect.height, actualMoveDelta.x, actualMoveDelta.y);
        }
    }

    public Vector2 SetMapSize(GameObject mapObj)
    {
        if (mapObj)
        {
            var spriteRenderer = mapObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer && spriteRenderer.sprite)
            {
                Vector3 size = spriteRenderer.sprite.bounds.size * 0.5f;
                size.x *= mapObj.transform.localScale.x;
                size.y *= mapObj.transform.localScale.y;
                maxRangeRect = new Rect(mapObj.transform.position, size);
            }
        }
        return new Vector2(maxRangeRect.width, maxRangeRect.height);
    }

    public Vector2 SetMapSize(Vector2 position, Vector2 halfSize)
    {
        return SetSize(position, halfSize);
    }
}
