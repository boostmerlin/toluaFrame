using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

[ExecuteInEditMode]
public class ObjectList : MonoBehaviour
{
    public enum ObjectType
    {
        GameObject = 0,
        Text,
        Image,
        RawImage,
        Sprite2D,
        Shader = 5,
    }

    [SerializeField]
    private Sprite[] sprites;
    [SerializeField]
    private Texture[] textures;
    [SerializeField]
    private string[] texts;
    [SerializeField]
    private Shader[] shaders;
    [SerializeField]
    private GameObject[] gameObjects;

    [SerializeField]
    private ObjectType objectType = ObjectType.GameObject;
    [SerializeField]
    private int _index;

    private Dictionary<string, Material> materials = null;

    public UnityAction<GameObject, int, int> onIndexChanged;

    public bool nativeSize = false;
    public bool includeChildren = false;
    [SerializeField]
    private bool _enableAnimation = false;
    public bool enableAnimation
    {
        get
        {
            return _enableAnimation;
        }
        set
        {
            if (value)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
            _enableAnimation = value;
        }
    }
    public bool loopAnimation;
    public int frames = 5;

    public int index
    {
        get
        {
            return _index;
        }
        set
        {
            if (value == _index)
                return;
            UpdateIndex(value);
        }
    }

    void Awake()
    {
        UpdateIndex(_index);
    }

    private void OnEnable()
    {
        if (enableAnimation)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void StopAnimation()
    {
        CancelInvoke("AnimationUpdate");
    }
    private void StartAnimation()
    {
        if (IsInvoking("AnimationUpdate"))
        {
            StopAnimation();
        }
        float t = 1.0f / frames;
        InvokeRepeating("AnimationUpdate", t, t);
    }

    private void AnimationUpdate()
    {
        int index = _index + 1;
        if (!this.UpdateIndex(index))
        {
            StopAnimation();
        }
    }

    private bool CheckBounds(object[] objs, ref int value)
    {
        if (objs == null || objs.Length == 0)
        {
            return false;
        }
        if (value >= objs.Length || value < 0)
        {
            if (!loopAnimation)
                return false;
            else
                value = 0;
        }
        return true;
    }

    private bool UpdateIndex(int value)
    {
        if (objectType == ObjectType.Text)
        {
            if (!CheckBounds(texts, ref value))
                return false;

            Text text = GetComponent<Text>();
            if (text != null)
                text.text = texts[value];
        }
        else if (objectType == ObjectType.Image)
        {
            if (!CheckBounds(sprites, ref value))
                return false;

            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprites[value];
                image.enabled = sprites[value] != null;
                if (nativeSize)
                    image.SetNativeSize();
            }
        }
        else if (objectType == ObjectType.RawImage)
        {
            if (!CheckBounds(textures, ref value))
                return false;
            RawImage image = GetComponent<RawImage>();
            if (image != null)
            {
                image.texture = textures[value];
                image.enabled = textures[value] != null;
                if (nativeSize)
                    image.SetNativeSize();
            }
        }
        else if (objectType == ObjectType.Sprite2D)
        {
            if (!CheckBounds(sprites, ref value))
                return false;
            SpriteRenderer render = GetComponent<SpriteRenderer>();
            if (render != null)
            {
                render.sprite = sprites[value];
                render.enabled = sprites[value] != null;
            }
        }
        else if (objectType == ObjectType.Shader)
        {
            if (!CheckBounds(shaders, ref value))
                return false;
            SetMaskableGraphicMaterial(transform, shaders[value]);
        }
        else
        {
            if (!CheckBounds(gameObjects, ref value))
                return false;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                if (i != value && gameObjects[i] != null && gameObjects[i].activeSelf)
                    gameObjects[i].SetActive(false);
            }
            if (gameObjects[value] != null)
                gameObjects[value].SetActive(true);
        }


        if (_index != value)
        {
            if (onIndexChanged != null)
                onIndexChanged(gameObject, _index, value);
            _index = value;
        }
        return true;
    }

    public void AddObject(GameObject obj)
    {
        if (objectType != ObjectType.GameObject)
            return;
        if (gameObjects == null)
            gameObjects = new GameObject[] { obj };
        else
        {
            int len = gameObjects.Length;
            GameObject[] list = new GameObject[len + 1];
            gameObjects.CopyTo(list, 0);
            list[len] = obj;
            gameObjects = list;
        }
    }

    public void AddObject(Sprite obj)
    {
        if (objectType != ObjectType.Sprite2D && objectType != ObjectType.Image)
            return;
        if (sprites == null)
            sprites = new Sprite[] { obj };
        else
        {
            int len = sprites.Length;
            Sprite[] list = new Sprite[len + 1];
            sprites.CopyTo(list, 0);
            list[len] = obj;
            sprites = list;
        }
    }

    public void AddObject(Texture obj)
    {
        if (objectType != ObjectType.RawImage)
            return;
        if (textures == null)
            textures = new Texture[] { obj };
        else
        {
            int len = textures.Length;
            Texture[] list = new Texture[len + 1];
            textures.CopyTo(list, 0);
            list[len] = obj;
            textures = list;
        }
    }
    public void AddObject(Shader obj)
    {
        if (shaders == null)
            shaders = new Shader[] { obj };
        else
        {
            int len = shaders.Length;
            Shader[] list = new Shader[len + 1];
            shaders.CopyTo(list, 0);
            list[len] = obj;
            shaders = list;
        }
    }
    public void AddObject(string obj)
    {
        if (texts == null)
            texts = new string[] { obj };
        else
        {
            int len = texts.Length;
            string[] list = new string[len + 1];
            texts.CopyTo(list, 0);
            list[len] = obj;
            texts = list;
        }
    }


    private Material GetInteractableMaterial(Shader shader)
    {
        if (shader == null)
        {
            return null;
        }
        if (materials == null)
            materials = new Dictionary<string, Material>();
        if (!materials.ContainsKey(shader.name))
            materials[shader.name] = new Material(shader);
        return materials[shader.name];
    }

    private void SetMaskableGraphicMaterial(Transform tran, Shader shader)
    {
        MaskableGraphic graphic = tran.GetComponent<MaskableGraphic>();
        if (graphic != null)
            graphic.material = GetInteractableMaterial(shader);
        if (includeChildren)
        {
            for (int i = 0; i < tran.childCount; ++i)
                SetMaskableGraphicMaterial(tran.GetChild(i), shader);
        }
    }

#if UNITY_EDITOR
    private T AddComponent<T>() where T : Component
    {
        T com = gameObject.GetComponent<T>();
        if (com == null)
        {
            com = gameObject.AddComponent<T>();
        }
        return com;
    }
    private void AddComponent()
    {
        if (objectType == ObjectType.Text)
        {
            AddComponent<Text>();
        }
        else if (objectType == ObjectType.Image)
        {
            AddComponent<Image>();
        }
        else if (objectType == ObjectType.RawImage)
        {
            AddComponent<RawImage>();
        }
        else if (objectType == ObjectType.Sprite2D)
        {
            AddComponent<SpriteRenderer>();
        }
        if (!UpdateIndex(_index))
            _index = 0;
    }

    private void RemoveComponent<T>()
    {
        T comp = gameObject.GetComponent<T>();
        if (comp != null)
        {
            UnityEditor.EditorApplication.delayCall += () =>
              {
                  Component.DestroyImmediate(comp as Component);
              };
        }
    }
    public static readonly Dictionary<int, string> propertyName = new Dictionary<int, string>()
    {
        {0, "gameObjects"},
        {1, "texts"},
        {2, "sprites"},
        {3, "textures"},
        {4, "sprites"},
        {5, "shaders"},
    };

    private void ClearFields(string except)
    {
        foreach (var d in propertyName)
        {
            if (d.Value == except)
            {
                continue;
            }
            var fi = typeof(ObjectList).GetField(d.Value,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fi.SetValue(this, null);
        }
    }

    private void OnValidate()
    {
        ClearFields(propertyName[(int)objectType]);
        if (objectType == ObjectType.Text)
        {
            RemoveComponent<Image>();
            RemoveComponent<RawImage>();
            RemoveComponent<SpriteRenderer>();
        }
        else if (objectType == ObjectType.Image)
        {
            RemoveComponent<Text>();
            RemoveComponent<RawImage>();
            RemoveComponent<SpriteRenderer>();
        }
        else if (objectType == ObjectType.RawImage)
        {
            RemoveComponent<Text>();
            RemoveComponent<Image>();
            RemoveComponent<SpriteRenderer>();
        }
        else if (objectType == ObjectType.Sprite2D)
        {
            RemoveComponent<Text>();
            RemoveComponent<Image>();
            RemoveComponent<RawImage>();
        }
        StopAnimation();
        if (Application.isPlaying)
        {
            OnEnable();
        }
        Invoke("AddComponent", 0.05f);
    }
#endif
}
