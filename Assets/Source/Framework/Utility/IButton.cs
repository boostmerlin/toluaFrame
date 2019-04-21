using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(IButtonHelper))]
public class IButton : Button 
{
	private static Material  disabledMaterial = null;
	public bool disabledIncludChilren = false;
	public bool disabledBlackWhite = false;

	private Material GetInteractableMaterial()
	{
		if (this.interactable)
			return null;
		if (disabledMaterial == null) 
		{
			IButtonHelper helper = transform.GetComponent<IButtonHelper> ();
			if (helper != null)
				disabledMaterial = helper.grayImage;
			else
				disabledMaterial = new Material (Shader.Find ("Sprites/GrayImage"));
		}
		return disabledMaterial;
	}

	private void SetMaskableGraphicMaterial(Transform tran)
	{
        Text text = tran.GetComponent<Text>();
        if (text == null || !(text.color == Color.white || text.color == Color.black))
        {
            MaskableGraphic graphic = tran.GetComponent<MaskableGraphic>();
            if (graphic != null)
                graphic.material = GetInteractableMaterial();
        }
		
		if (disabledIncludChilren) 
		{
			for (int i = 0; i < tran.childCount; ++i)
				SetMaskableGraphicMaterial (tran.GetChild(i));
		}
	}
		

	new public bool interactable
	{
		get
		{ 
			return base.interactable;
		}
		set
		{
			base.interactable = value;
			if (disabledBlackWhite) 
				SetMaskableGraphicMaterial (this.transform);
		}
	}

	protected override void Awake ()
	{
		base.Awake ();
		if (disabledBlackWhite) 
		{
			if (this.transition == Transition.ColorTint) {
				ColorBlock cb = this.colors;
				cb.disabledColor = Color.white;
				this.colors = cb;
			}
		}
	}

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (disabledBlackWhite)
            SetMaskableGraphicMaterial(this.transform);
    }

    protected override void Reset()
    {
        base.Reset();
        if (disabledBlackWhite)
        {
            if (this.transition == Transition.ColorTint)
            {
                ColorBlock cb = this.colors;
                cb.disabledColor = Color.white;
                this.colors = cb;
            }
        }
    }
#endif
}
