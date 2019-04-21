using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DragListener : MonoBehaviour,IBeginDragHandler,IEndDragHandler,IDragHandler   
{
	public static DragListener Get(GameObject obj)
	{
		if (obj != null) 
		{
			DragListener listener = obj.GetComponent<DragListener> ();
			if (listener == null)
				listener = obj.AddComponent<DragListener> ();
			return listener;
		}
		return null;
	}

	// 拖拽
	private event UnityAction<GameObject,Vector2> _OnDrag;
	public void AddOnDragEvent(UnityAction<GameObject,Vector2> onDrag)
	{
		_OnDrag += onDrag;
	}
	public void SetOnDragEvent(UnityAction<GameObject,Vector2> onDrag)
	{
		_OnDrag = onDrag;
	}
	public void RemoveOnDragEvent(UnityAction<GameObject,Vector2> onDrag)
	{
		if(onDrag == null)
			_OnDrag = null;
		else
			_OnDrag -= onDrag;
	}

	// 开始拖拽
	private event UnityAction<GameObject,Vector2> _OnDragBegin;
	public void AddOnDragBeginEvent(UnityAction<GameObject,Vector2> onDragBegin)
	{
		_OnDragBegin += onDragBegin;
	}
	public void SetOnDragBeginEvent(UnityAction<GameObject,Vector2> onDragBegin)
	{
		_OnDragBegin = onDragBegin;
	}
	public void RemoveOnDragBeginEvent(UnityAction<GameObject,Vector2> onDragBegin)
	{
		if(onDragBegin == null)
			_OnDragBegin = null;
		else
			_OnDragBegin -= onDragBegin;
	}		

	// 拖拽结束
	private event UnityAction<GameObject,Vector2> _OnDragEnd;
	public void AddOnDragEndEvent(UnityAction<GameObject,Vector2> onDragEnd)
	{
		_OnDragEnd += onDragEnd;
	}
	public void SetOnDragEndEvent(UnityAction<GameObject,Vector2> onDragEnd)
	{
		_OnDragEnd = onDragEnd;
	}
	public void RemoveOnDragEndEvent(UnityAction<GameObject,Vector2> onDragEnd)
	{
		if(onDragEnd == null)
			_OnDragEnd = null;
		else
			_OnDragEnd -= onDragEnd;
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (_OnDragBegin != null)
			_OnDragBegin (gameObject,eventData.delta);
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		if (_OnDragEnd != null)
			_OnDragEnd (gameObject,eventData.delta);
	}

	public void OnDrag (PointerEventData eventData)
	{
		if (_OnDrag != null)
			_OnDrag(gameObject,eventData.delta);
	}

	void OnDestroy()
	{
		_OnDrag = null;
		_OnDragBegin = null;
		_OnDragEnd = null;
	}
}