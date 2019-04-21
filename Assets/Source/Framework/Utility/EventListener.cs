using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class EventListener : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IPointerUpHandler
{
	public static EventListener Get(GameObject obj)
	{
		if (obj != null) 
		{
			EventListener listener = obj.GetComponent<EventListener> ();
			if (listener == null)
				listener = obj.AddComponent<EventListener> ();
			return listener;
		}
		return null;
	}

	// 鼠标点击事件
	private event UnityAction<GameObject> _OnClick; 
	public void AddOnClickEvent(UnityAction<GameObject> onClick)
	{
		_OnClick += onClick;
	}

	public void SetOnClickEvent(UnityAction<GameObject> onClick)
	{
		_OnClick = onClick;
	}

	public void RemoveOnClickEvent(UnityAction<GameObject> onClick)
	{
		if(onClick == null)
			_OnClick = null;
		else
			_OnClick -= onClick;
	}

	private Dictionary<UnityAction<GameObject,object>,object> _onClickParameters = new Dictionary<UnityAction<GameObject,object>, object> ();

	public void AddOnClickParameterEvent(UnityAction<GameObject,object> onClick,object parameter)
	{
		if (_onClickParameters.ContainsKey (onClick))
			_onClickParameters [onClick] = parameter;
		else
			_onClickParameters.Add (onClick, parameter);
	}

	public void SetOnClickParameterEvent(UnityAction<GameObject,object> onClick,object parameter)
	{
		_onClickParameters.Clear ();
		AddOnClickParameterEvent (onClick, parameter);
	}

		
	public void RemoveOnClickParameterEvent(UnityAction<GameObject,object> onClick)
	{
		if (_onClickParameters.ContainsKey (onClick))
			_onClickParameters.Remove (onClick);
	}
		
	// 鼠标按下/抬起
	private event UnityAction<GameObject,bool> _OnPress;
	public void AddOnPressEvent(UnityAction<GameObject,bool> onPress)
	{
		_OnPress += onPress;
	}

	public void SetOnPressEvent(UnityAction<GameObject,bool> onPress)
	{
		_OnPress = onPress;
	}

	public void RemoveOnPressEvent(UnityAction<GameObject,bool> onPress)
	{
		if(onPress == null)
			_OnPress = null;
		else
			_OnPress -= onPress;
	}

	// 鼠标进入事件
	public void AddMouseEnterEvent(UnityAction<GameObject> mouseEnter)
	{
		MouseMoveListener.Get (gameObject).AddMouseEnterEvent (mouseEnter);
	}
	public void SetMouseEnterEvent(UnityAction<GameObject> mouseEnter)
	{
		MouseMoveListener.Get (gameObject).SetMouseEnterEvent (mouseEnter);

	}
	public void RemoveMouseEnterEvent(UnityAction<GameObject> mouseEnter)
	{
		MouseMoveListener.Get (gameObject).RemoveMouseEnterEvent (mouseEnter);
	}

	// 鼠标滑出事件
	public void AddMouseExitEvent(UnityAction<GameObject> mouseExit)
	{
		MouseMoveListener.Get (gameObject).AddMouseExitEvent (mouseExit);
	}
	public void SetMouseExitEvent(UnityAction<GameObject> mouseExit)
	{
		MouseMoveListener.Get (gameObject).SetMouseExitEvent (mouseExit);
	}
	public void RemoveMouseExitEvent(UnityAction<GameObject> mouseExit)
	{
		MouseMoveListener.Get (gameObject).RemoveMouseExitEvent (mouseExit);
	}

	// 拖拽
	public void AddOnDragEvent(UnityAction<GameObject,Vector2> onDrag)
	{
		DragListener.Get (gameObject).AddOnDragEvent (onDrag);
	}
	public void SetOnDragEvent(UnityAction<GameObject,Vector2> onDrag)
	{
		DragListener.Get (gameObject).SetOnDragEvent (onDrag);
	}
	public void RemoveOnDragEvent(UnityAction<GameObject,Vector2> onDrag)
	{
		DragListener.Get (gameObject).RemoveOnDragEvent (onDrag);
	}

	// 开始拖拽
	public void AddOnDragBeginEvent(UnityAction<GameObject,Vector2> onDragBegin)
	{
		DragListener.Get (gameObject).AddOnDragBeginEvent (onDragBegin);
	}
	public void SetOnDragBeginEvent(UnityAction<GameObject,Vector2> onDragBegin)
	{
		DragListener.Get (gameObject).SetOnDragBeginEvent (onDragBegin);
	}
	public void RemoveOnDragBeginEvent(UnityAction<GameObject,Vector2> onDragBegin)
	{
		DragListener.Get (gameObject).RemoveOnDragBeginEvent (onDragBegin);
	}		

	// 拖拽结束
	public void AddOnDragEndEvent(UnityAction<GameObject,Vector2> onDragEnd)
	{
		DragListener.Get (gameObject).AddOnDragEndEvent (onDragEnd);
	}
	public void SetOnDragEndEvent(UnityAction<GameObject,Vector2> onDragEnd)
	{
		DragListener.Get (gameObject).SetOnDragEndEvent (onDragEnd);
	}
	public void RemoveOnDragEndEvent(UnityAction<GameObject,Vector2> onDragEnd)
	{
		DragListener.Get (gameObject).RemoveOnDragEndEvent (onDragEnd);
	}


	private float pressTime = 0;
	private event UnityAction<GameObject> _KeepPress;
	public void AddKeepPress(UnityAction<GameObject> keepPress,float second = 0.5f)
	{
		pressTime = second;
		_KeepPress += keepPress;
	}

	public void SetKeepPress(UnityAction<GameObject> keepPress,float second = 0.5f)
	{
		pressTime = second;
		_KeepPress = keepPress;
	}

	public void RemoveKeepPress(UnityAction<GameObject> keepPress)
	{
		pressTime = 0;
		if(keepPress == null)
			_KeepPress = null;
		else
			_KeepPress -= keepPress;
	}

	public void AddToggleEvent(UnityAction<bool> onValueChanged)
	{
		Toggle toggle = gameObject.GetComponent<Toggle> ();
		if (toggle != null) 
		{
			toggle.onValueChanged.AddListener (onValueChanged);
		}
	}

	public void SetToggleEvent(UnityAction<bool> onValueChanged)
	{
		Toggle toggle = gameObject.GetComponent<Toggle> ();
		if (toggle != null) 
		{
			toggle.onValueChanged.RemoveAllListeners ();
			toggle.onValueChanged.AddListener (onValueChanged);
		}
	}

	public void AddSliderEvent(UnityAction<float> onValueChanged)
	{
		Slider slider = gameObject.GetComponent<Slider> ();
		if (slider != null) 
		{
			slider.onValueChanged.AddListener (onValueChanged);
		}
	}

	public void SetSliderEvent(UnityAction<float> onValueChanged)
	{
		Slider slider = gameObject.GetComponent<Slider> ();
		if (slider != null) 
		{
			slider.onValueChanged.RemoveAllListeners ();
			slider.onValueChanged.AddListener (onValueChanged);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!Interactable)
			return;
		
		if (hasKeepPress)
			return;
		
		if (_OnClick != null)
			_OnClick (gameObject);

		UnityAction<GameObject,object>[] keys = new UnityAction<GameObject, object>[_onClickParameters.Count];
		_onClickParameters.Keys.CopyTo (keys, 0);
		for (int i = 0; i < keys.Length; ++i)
			keys [i] (gameObject,_onClickParameters[keys[i]]);
	}

	bool hasKeepPress = false;
	IEnumerator PressTimer()
	{
		hasKeepPress = false;
		yield return new WaitForSeconds (pressTime);
		if (_KeepPress != null) 
		{
			hasKeepPress = true;
			_KeepPress (gameObject);
		}
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		if (!Interactable)
			return;
		
		if (_OnPress != null)
			_OnPress (gameObject,true);

		if (pressTime > 0 && _KeepPress != null) 
			StartCoroutine ("PressTimer");
	}

	public void OnPointerUp (PointerEventData eventData)
	{
		if (!Interactable)
			return;
		
		StopCoroutine ("PressTimer");
		if (_OnPress != null)
			_OnPress (gameObject,false);
	}

	private bool Interactable
	{
		get {
			Selectable sel = gameObject.GetComponent<Selectable> ();
			if (sel != null)
				return sel.interactable;	
			return true;
		}
	}

	private event UnityAction<GameObject> _OnEnable; 
	public void AddOnEnableEvent(UnityAction<GameObject> onEnable)
	{
		_OnEnable += onEnable;
	}

	public void SetOnEnableEvent(UnityAction<GameObject> onEnable)
	{
		_OnEnable = onEnable;
	}

	public void RemoveOnEnableEvent(UnityAction<GameObject> onEnable)
	{
		if(_OnEnable == null)
			_OnClick = null;
		else
			_OnEnable -= onEnable;
	}
		
	void OnEnable()
	{
		if (_OnEnable != null)
			_OnEnable (gameObject);
	}
		
	void OnDestroy()
	{
		StopCoroutine ("PressTimer");
		_onClickParameters.Clear ();
		_OnClick = null;
		_OnPress = null;
		_KeepPress = null;
	}
}
