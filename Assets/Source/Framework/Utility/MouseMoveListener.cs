using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MouseMoveListener : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
	public static MouseMoveListener Get(GameObject obj)
	{
		if (obj != null) 
		{
			MouseMoveListener listener = obj.GetComponent<MouseMoveListener> ();
			if (listener == null)
				listener = obj.AddComponent<MouseMoveListener> ();
			return listener;
		}
		return null;
	}

	// 鼠标进入事件
	private event UnityAction<GameObject> _OnMouseEnter;
	public void AddMouseEnterEvent(UnityAction<GameObject> mouseEnter)
	{
		_OnMouseEnter += mouseEnter;
	}
	public void SetMouseEnterEvent(UnityAction<GameObject> mouseEnter)
	{
		_OnMouseEnter = mouseEnter;
	}
	public void RemoveMouseEnterEvent(UnityAction<GameObject> mouseEnter)
	{
		if(mouseEnter == null)
			_OnMouseEnter = null;
		else
			_OnMouseEnter -= mouseEnter;
	}

	// 鼠标滑出事件
	private event UnityAction<GameObject> _OnMouseExit;
	public void AddMouseExitEvent(UnityAction<GameObject> mouseExit)
	{
		_OnMouseExit += mouseExit;
	}
	public void SetMouseExitEvent(UnityAction<GameObject> mouseExit)
	{
		_OnMouseExit = mouseExit;
	}
	public void RemoveMouseExitEvent(UnityAction<GameObject> mouseExit)
	{
		if(mouseExit == null)
			_OnMouseExit = null;
		else
			_OnMouseExit -= mouseExit;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_OnMouseEnter != null)
			_OnMouseEnter (gameObject);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_OnMouseExit != null)
			_OnMouseExit (gameObject);
	}

	void OnDestroy()
	{
		_OnMouseExit = null;
		_OnMouseEnter = null;
	}
}
