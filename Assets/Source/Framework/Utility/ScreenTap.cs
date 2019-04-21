using Lean.Touch;
using LuaFramework;
using UnityEngine;

public class ScreenTap : MonoBehaviour
{
    public bool IgnoreStartedOverGui = true;
    public bool IgnoreIsOverGui = true;
    public Camera Camera;
    public LayerMask LayerMask;

    public enum SearchType
    {
        GetComponent,
        GetComponentInParent,
        GetComponentInChildren
    }

    public enum TapType
    {
     //   Raycast3D,
        Overlap2D,
    }

    public SearchType Search = SearchType.GetComponent;
    public TapType tapType = TapType.Overlap2D;

    protected virtual void OnEnable()
    {
        LeanTouch.OnFingerTap += FingerTap;
        if (LayerMask.value == 0)
        {
            LayerMask = LayerMask.GetMask("Default", "UI");
        }
    }

    protected virtual void OnDisable()
    {
        LeanTouch.OnFingerTap -= FingerTap;
    }

    private void FingerTap(LeanFinger finger)
    {
        if (finger.StartedOverGui == true && IgnoreStartedOverGui)
        {
            return;
        }

        if (finger.IsOverGui == true && IgnoreIsOverGui)
        {
            return;
        }
        SelectScreenPosition(finger, finger.ScreenPosition);
    }

    void SelectScreenPosition(LeanFinger finger, Vector2 screenPosition)
    {
        var component = default(Component);
        var camera = LeanTouch.GetCamera(Camera, gameObject);
        Camera = camera;
        if (camera != null)
        {
            switch (tapType)
            {
                //case TapType.Raycast3D:
                //    var ray = camera.ScreenPointToRay(screenPosition);
                //    var hit = default(RaycastHit);
                //    if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
                //    {
                //        component = hit.collider;
                //    }
                //    break;
                case TapType.Overlap2D:
                    var point = camera.ScreenToWorldPoint(screenPosition);
                    component = Physics2D.OverlapPoint(point, LayerMask);
                    break;
            }
        }
        else
        {
            Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
        }
        Select(finger, component);
    }

    void Select(LeanFinger finger, Component component)
    {
        var selectable = default(SceneSelectable);
        if (component != null)
        {
            switch (Search)
            {
                case SearchType.GetComponent: selectable = component.GetComponent<SceneSelectable>(); break;
                case SearchType.GetComponentInParent: selectable = component.GetComponentInParent<SceneSelectable>(); break;
                case SearchType.GetComponentInChildren: selectable = component.GetComponentInChildren<SceneSelectable>(); break;
            }
        }

        DoSelect(finger, selectable);
    }

    void DoSelect(LeanFinger finger, SceneSelectable selectable)
    {
        if (selectable == null)
        {
            LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
            if (luaMgr == null)
            {
                return;
            }
            luaMgr.CallFunction("ScreenTap", finger);
        }
        else
        {
            if (selectable.onTaped != null)
            {
                selectable.onTaped.Invoke(finger, selectable.gameObject);
            }
        }
    }
}