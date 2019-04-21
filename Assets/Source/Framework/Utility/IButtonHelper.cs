using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IButtonHelper : MonoBehaviour 
{
	private IButton ibutton = null;
	[SerializeField]
	private bool disabledIncludChilren = false;
	[SerializeField]
	private bool disabledBlackWhite = false;
	public Material grayImage;

	void Awake()
	{
		UpdateValue ();
	}

	private void UpdateValue()
	{
		if (ibutton == null)
			ibutton = gameObject.GetComponent<IButton> ();
		if (ibutton != null) {
			ibutton.disabledIncludChilren = disabledIncludChilren;
			ibutton.disabledBlackWhite = disabledBlackWhite;
		}
	}

	#if UNITY_EDITOR
	void OnValidate()
	{
		if (grayImage == null)
			grayImage = AssetDatabase.LoadAssetAtPath<Material>("Assets/Res/Materials/GrayImage.mat");
		UpdateValue ();
	}
	#endif
}