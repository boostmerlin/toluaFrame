using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAnim : MonoBehaviour {
	public Sprite[] spriteList;
	private float interval 
	{
		get
		{
			return 1.0f / frame;
		}
	}
	private Image image;
	private float lastChangeTime;
	private int index;
	public bool nativeSize = false;
	public int frame = 10;

	public SpriteAnim Get(GameObject obj)
	{
		SpriteAnim anim = obj.GetComponent<SpriteAnim> ();
		if (anim == null)
			anim = obj.AddComponent<SpriteAnim> ();
		return anim;
	}

	// Use this for initialization
	void Awake () 
	{
		image = gameObject.GetComponent<Image> ();
		index = 0;
		lastChangeTime = Time.realtimeSinceStartup;
		if (spriteList != null && image != null) {
			image.sprite = spriteList [index];
			if (nativeSize)
				image.SetNativeSize ();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (spriteList == null || image == null)
			return;
		float time = Time.realtimeSinceStartup;
		if (time - lastChangeTime >= interval)
		{
			index += 1;
			if (index == spriteList.Length)
				index = 0;
			image.sprite = spriteList [index];
			lastChangeTime = time;
			if (nativeSize)
				image.SetNativeSize ();
		}
	}
}
