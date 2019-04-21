using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IStatus : MonoBehaviour 
{
	public enum UIType 
	{
		GameObject = 0,
		Text = 1,
		Image = 2,
		RawImage = 3,
		Sprite2D = 4,
		Shader = 5
	}

	[SerializeField]
	private UIType uitype;
	[SerializeField]
	private Sprite[] sprites; 
	[SerializeField]
	private Texture[] textures;
	[SerializeField]
	private string[] contents;
	[SerializeField]
	private string[] shaders;
	[SerializeField]
	private GameObject[] gameObjects;
	[SerializeField]
	private int _status;
	[SerializeField]
	private bool nativeSize = false;
	[SerializeField]
	private bool includeChildren = false;
	[SerializeField]
	private Dictionary<string,Material> materials = null;

	public UnityAction<GameObject,int,int> onStatusChanged;

	public int status
	{
		get
		{
			return _status;
		}
		set
		{
			if (value == _status)
				return;
			UpdateStatus (value);
		}
	}

	private bool UpdateStatus(int value)
	{
		if (uitype == UIType.Text) {
			if (contents.Length == 0 || value >= contents.Length || value < 0)
				return false;
			
			Text text = gameObject.GetComponent<Text> ();
			if (text == null)
				text = gameObject.AddComponent<Text> ();
			text.text = contents [value];
		} else if (uitype == UIType.Image) {
			if (sprites.Length == 0 || value >= sprites.Length || value < 0)
				return false;

			Image image = gameObject.GetComponent<Image> ();
			if (image == null)
				image = gameObject.AddComponent<Image> ();
			image.sprite = sprites [value];
			image.enabled = sprites [value] != null;
			if (nativeSize)
				image.SetNativeSize ();
		} else if (uitype == UIType.RawImage) {
			if (textures.Length == 0 || value >= textures.Length || value < 0)
				return false;

			RawImage image = gameObject.GetComponent<RawImage> ();
			if (image == null)
				image = gameObject.AddComponent<RawImage> ();
			image.texture = textures [value];
			image.enabled = textures [value] != null;
			if (nativeSize)
				image.SetNativeSize ();
		} else if (uitype == UIType.Sprite2D) {
			if (sprites.Length == 0 || value >= sprites.Length || value < 0)
				return false;
			SpriteRenderer render = gameObject.GetComponent<SpriteRenderer> ();
			if (render == null)
				render = gameObject.AddComponent<SpriteRenderer> ();
			render.sprite = sprites [value];
			render.enabled = sprites [value] != null;
		} 
		else if (uitype == UIType.Shader) 
		{
			if (shaders.Length == 0 || value >= shaders.Length || value < 0)
				return false;
			SetMaskableGraphicMaterial (transform, shaders [value]);
		}
		else
		{
			if (gameObjects == null || gameObjects.Length == 0 || value >= gameObjects.Length || value < 0)
				return false;
			for (int i = 0; i < gameObjects.Length; ++i) 
			{
				if (i != value && gameObjects[i] != null && gameObjects[i].activeSelf)
					gameObjects [i].SetActive (false);
			}
			if (gameObjects [value] != null)
				gameObjects [value].SetActive (true);
		}

		if (_status != value) 
		{
			if (onStatusChanged != null)
				onStatusChanged (gameObject,_status,value);
			_status = value;
		}
		return true;
			
	}
		
	public void AddItem(GameObject obj)
	{
		if (uitype != UIType.GameObject)
			return;
		if (gameObjects == null)
			gameObjects = new GameObject[]{ obj };
		else 
		{
			int len = gameObjects.Length;
			GameObject[] list = new GameObject[len + 1];
			gameObjects.CopyTo (list, 0);
			list [len] = obj;
			gameObjects = list;
		}
	}

	public void AddItem(Sprite obj)
	{
		if (uitype != UIType.Sprite2D && uitype != UIType.Image)
			return;
		if (sprites == null)
			sprites = new Sprite[]{ obj };
		else 
		{
			int len = sprites.Length;
			Sprite[] list = new Sprite[len + 1];
			sprites.CopyTo (list, 0);
			list [len] = obj;
			sprites = list;
		}
	}

	public void AddItem(Texture obj)
	{
		if (uitype != UIType.RawImage)
			return;
		if (textures == null)
			textures = new Texture[]{ obj };
		else 
		{
			int len = textures.Length;
			Texture[] list = new Texture[len + 1];
			textures.CopyTo (list, 0);
			list [len] = obj;
			textures = list;
		}
	}

	public void AddItem(string obj)
	{
		if (uitype == UIType.Text) 
		{
			if (contents == null)
				contents = new string[]{ obj };
			else {
				int len = contents.Length;
				string[] list = new string[len + 1];
				contents.CopyTo (list, 0);
				list [len] = obj;
				contents = list;
			}
		}
		else if (uitype == UIType.Shader) 
		{
			if (shaders == null)
				shaders = new string[]{ obj };
			else {
				int len = shaders.Length;
				string[] list = new string[len + 1];
				shaders.CopyTo (list, 0);
				list [len] = obj;
				shaders = list;
			}
		}
	}

	// Use this for initialization
	void Awake () 
	{
		UpdateStatus (_status);
	}

    private Material GetInteractableMaterial(string shaderPath)
    {
        if (string.IsNullOrEmpty(shaderPath))
            return null;
        if (materials == null)
            materials = new Dictionary<string, Material>();
        if (!materials.ContainsKey(shaderPath))
			#if UNITY_EDITOR
			materials[shaderPath] = AssetDatabase.LoadAssetAtPath<Material>(
				"Assets/Res/Materials/"+ shaderPath.Substring(shaderPath.LastIndexOf("/")+1) +".mat");
			#else
			materials[shaderPath] = new Material(Shader.Find(shaderPath));
			#endif	
        return materials[shaderPath];
    }

    private void SetMaskableGraphicMaterial(Transform tran, string shaderPath)
    {
        MaskableGraphic graphic = tran.GetComponent<MaskableGraphic>();
        if (graphic != null)
            graphic.material = GetInteractableMaterial(shaderPath);
        if (includeChildren)
        {
            for (int i = 0; i < tran.childCount; ++i)
                SetMaskableGraphicMaterial(tran.GetChild(i), shaderPath);
        }
    }

#if UNITY_EDITOR
    private void AddComponent()
	{
		if (uitype == UIType.Text) {
			if (gameObject.GetComponent<Text> () == null)
				gameObject.AddComponent<Text> ();
		} else if (uitype == UIType.Image) {
			if (gameObject.GetComponent<Image> () == null)
				gameObject.AddComponent<Image> ();
		} else if (uitype == UIType.RawImage) {
			if (gameObject.GetComponent<RawImage> () == null)
				gameObject.AddComponent<RawImage> ();
		} else if (uitype == UIType.Sprite2D) {
			if (gameObject.GetComponent<SpriteRenderer> () == null)
				gameObject.AddComponent<SpriteRenderer> ();
		} else if (uitype == UIType.Shader) 
		{
			
		}
		if (!UpdateStatus (_status))
			_status = 0;
	}

	private void RemoveComponent<T>()
	{
		T comp = gameObject.GetComponent<T> ();
		if (comp != null) 
		{
			UnityEditor.EditorApplication.delayCall+=()=>
			{
				Component.DestroyImmediate(comp as Component);
			};
		}
	}
		
	void OnValidate()
	{
		if (uitype == UIType.Text) {
			RemoveComponent<Image> ();
			RemoveComponent<RawImage> ();
			RemoveComponent<SpriteRenderer> ();
		} else if (uitype == UIType.Image) {
			RemoveComponent<Text> ();
			RemoveComponent<RawImage> ();
			RemoveComponent<SpriteRenderer> ();
		} else if (uitype == UIType.RawImage) {
			RemoveComponent<Text> ();
			RemoveComponent<Image> ();
			RemoveComponent<SpriteRenderer> ();
		}
		else if (uitype == UIType.Sprite2D) {
			RemoveComponent<Text> ();
			RemoveComponent<Image> ();
			RemoveComponent<RawImage> ();
		} 

		this.Invoke ("AddComponent", 0.1f);
	}

	void Reset()
	{
		if (gameObject.GetComponent<Text>() != null)
			uitype = UIType.Text;
		else if (gameObject.GetComponent<Image>() != null)
			uitype = UIType.Image;
		else if (gameObject.GetComponent<RawImage>() != null)
			uitype = UIType.RawImage;
		else if (gameObject.GetComponent<SpriteRenderer>() != null)
			uitype = UIType.Sprite2D;
		else
			uitype = UIType.GameObject;
	}
	#endif
}
