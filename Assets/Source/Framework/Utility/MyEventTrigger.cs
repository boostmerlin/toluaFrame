using Lean.Touch;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 自定义事件
/// </summary>
public class MyEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ICancelHandler
{
    public float durationThreshold = 1.0f;

    public bool isPointerDown = false;
    private bool longPressTriggered = false;
    //private float timePressStarted;
    public delegate void onEventTrigger(GameObject go, Vector2 pos);

    public onEventTrigger onFingerMove;
    public onEventTrigger onFingerUp;

    public onEventTrigger onDrop;
    public onEventTrigger onPointerExit;
    public onEventTrigger onPointerEnter;
    public onEventTrigger onPointerClick;
    public onEventTrigger onPointerDown;
    public onEventTrigger onPointerUp;
    public onEventTrigger onLongPress;
    public onEventTrigger onCancel;

    PointerEventData currentEventData;

    public static MyEventTrigger Get(GameObject go)
    {
        MyEventTrigger myEventTrigger = go.GetComponent<MyEventTrigger>();
        if(myEventTrigger == null)
        {
            myEventTrigger = go.AddComponent<MyEventTrigger>();
        }

        return myEventTrigger;
    }

    public static bool Remove(GameObject go)
    {
        MyEventTrigger myEventTrigger = go.GetComponent<MyEventTrigger>();
        if (myEventTrigger != null)
        {
            Destroy(myEventTrigger);
            return true;
        }
        return false;
    }

    public  void OnPointerClick(PointerEventData eventData)
    {
        if (onPointerClick != null && !longPressTriggered)
        {
            onPointerClick(eventData.pointerPress, eventData.pressPosition);
        }
    }

    public  void OnCancel(BaseEventData eventData)
    {
        if (onCancel != null)
        {
            onCancel(eventData.selectedObject, Vector2.zero);
        }
    }

    public  void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (onPointerUp != null)
        {
            onPointerUp(eventData.pointerPress, eventData.pressPosition);
        }
        if (onLongPress != null && IsInvoking("LongPress"))
        {
            CancelInvoke("LongPress");
        }
    }

    public  void OnPointerDown(PointerEventData eventData)
    {
        //timePressStarted = Time.time;
        isPointerDown = true;
        currentEventData = eventData;
        if (onPointerDown != null)
        {
            onPointerDown(eventData.selectedObject, eventData.pressPosition);
        }
        longPressTriggered = false;
        if (onLongPress != null && durationThreshold >= 0)
        {
            CancelInvoke();
            Invoke("LongPress", durationThreshold);
        }
    }

    void LongPress()
    {
        longPressTriggered = true;
        onLongPress(gameObject, currentEventData.pressPosition);
    }

    public  void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null)
        {
            onPointerEnter(eventData.pointerPress, eventData.position);
        }
    }

    public  void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
        if (onPointerExit != null)
        {
            onPointerExit(eventData.pointerPress, eventData.position);
        }
    }
    public  void OnDrop(PointerEventData eventData)
    {
        if(onDrop != null)
        {
            onDrop(eventData.pointerPress, eventData.position);
        }
    }

    //private void Update()
    //{
    //    if (onLongPress != null && isPointerDown && !longPressTriggered)
    //    {
    //        if (Time.time - timePressStarted > durationThreshold)
    //        {
    //            longPressTriggered = true;
    //            onLongPress(gameObject, Vector2.zero);
    //        }
    //    }
    //}

    private void OnDisable()
    {
        if (onLongPress != null && IsInvoking("LongPress"))
        {
            CancelInvoke();
        }
        LeanTouch.OnFingerSet -= OnFingerSet;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }

    private void OnFingerUp(LeanFinger obj)
    {
        if (longPressTriggered && onFingerUp != null && currentEventData != null)
        {
            onFingerUp(gameObject, currentEventData.position);
        }
        longPressTriggered = false;
    }

    private void OnFingerSet(LeanFinger obj)
    {
        if (longPressTriggered && onFingerMove != null && currentEventData != null)
        {
            onFingerMove(gameObject, currentEventData.position);
        }
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerSet += OnFingerSet;
        LeanTouch.OnFingerUp += OnFingerUp;
    }
}
