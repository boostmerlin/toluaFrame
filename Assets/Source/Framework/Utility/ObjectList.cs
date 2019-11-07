using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Object = System.Object;

public class ObjectList : MonoBehaviour
{
    public enum ObjectType
    {
        GameObject = 0,
        Text = 1,
        Texture = 2,
        Sprite = 3,
        Shader = 4,
        Material = 5,
        AudioClip = 6,
    }

    //Keep ObjectUnion FieldName equal ObjectType Name
    [Serializable]
    public struct ObjectUnion
    {
        public List<GameObject> GameObject;
        public List<string> Text;
        public List<Texture> Texture;
        public List<Sprite> Sprite;
        public List<Shader> Shader;
        public List<AudioClip> AudioClip;
        public List<Material> Material;
    }

    private static readonly Type[][] targetUserType =
    {
        null,
        new[] {typeof(Text)},
        new[] {typeof(RawImage)},
        new[] {typeof(Image), typeof(SpriteRenderer)},
        new[] {typeof(Graphic)},
        new[] {typeof(Graphic)},
        new[] {typeof(AudioSource)}
    };

    private IList GetTargetObjects()
    {
        switch (_objectType)
        {
            case ObjectType.GameObject:
                return objects.GameObject;
            case ObjectType.Text:
                return objects.Text;
            case ObjectType.Texture:
                return objects.Texture;
            case ObjectType.Sprite:
                return objects.Sprite;
            case ObjectType.Shader:
                return objects.Shader;
            case ObjectType.Material:
                return objects.Material;
            case ObjectType.AudioClip:
                return objects.AudioClip;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [SerializeField] private ObjectType _objectType = ObjectType.GameObject;

    public ObjectType objectType => _objectType;

    public ObjectUnion objects;

    [SerializeField] private int _index;

    public UnityAction<GameObject, int, int> onIndexChanged;
    private Coroutine animationRoutine;

    public bool autoApplyObject = true;
    public bool nativeSize;
    public bool includeChildren;
    [SerializeField]
    private bool _enableAnimation;

    public bool enableAnimation
    {
        get => _enableAnimation;
        set
        {
            _enableAnimation = value;
            if (value)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }
    }

    public bool loopAnimation;
    public int frames = 5;

    private float frameRate => 1.0f / frames;

    public int index
    {
        get => _index;
        set
        {
            if (value == _index)
                return;
            UpdateIndex(value);
        }
    }

    public bool outBound
    {
        get
        {
            var list = GetTargetObjects();
            if (list == null || list.Count == 0) return true;
            return _index < 0 || _index >= list.Count;
        }
    }

    void Start()
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

    public List<T> GetTargetObjects<T>()
    {
        return GetTargetObjects() as List<T>;
    }

    private void StopAnimation()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);
    }

    private void StartAnimation()
    {
        StopAnimation();
        animationRoutine = StartCoroutine(AnimationUpdate());
    }

    private IEnumerator AnimationUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(frameRate);
            int index = _index + 1;
            if (!UpdateIndex(index))
            {
                StopAnimation();
            }
        }
    }

    private bool CheckBounds(ICollection list, ref int value)
    {
        if (list == null || list.Count == 0)
        {
            return false;
        }

        if (value >= list.Count || value < 0)
        {
            if (!loopAnimation)
                return false;
            value = 0;
        }

        return true;
    }

    private object GetComponent()
    {
        int typeIndex = (int) _objectType;
        if (typeIndex < 0 || typeIndex >= targetUserType.Length) return null;
        var types = targetUserType[typeIndex];
        if (types == null) return null;
        foreach (var t in targetUserType[typeIndex])
        {
            var c = GetComponent(t);
            if (c) return c;
        }

        return null;
    }

    private delegate void ObjectUser(ObjectList ins, object user, object objectValue, int objectIndex);

    private static readonly Dictionary<ObjectType, ObjectUser> objectUserMap = new Dictionary<ObjectType, ObjectUser>
    {
        {ObjectType.Text, TextUser},
        {ObjectType.Material, MaterialUser},
        {ObjectType.Shader, ShaderUser},
        {ObjectType.Sprite, SpriteUser},
        {ObjectType.Texture, RawImageUser},
        {ObjectType.GameObject, GameObjectUser},
        {ObjectType.AudioClip, AudioClipUser},
    };

    private static void TextUser(ObjectList ins, object user, object value, int _)
    {
        var text = user as Text;
        if (text == null) return;
        text.text = (string)value;
    }

    private static void RawImageUser(ObjectList ins, object user, object value, int _)
    {
        var img = user as RawImage;
        if(img == null) return;
        img.texture = (Texture)value;
        if (ins.nativeSize)
            img.SetNativeSize();
    }

    private static void SpriteUser(ObjectList ins, object user, object value, int _)
    {
        if(user == null) return;
        if (user is Image img)
        {
            img.sprite = value as Sprite;
            if (ins.nativeSize)
                img.SetNativeSize();
        }
        else if (user is SpriteRenderer sr)
        {
            sr.sprite = value as Sprite;
        }
    }

    private static void ShaderUser(ObjectList ins, object user, object value, int objectIndex)
    {
        if(user == null) return;
        var material = ins.GetMaterial(value as Shader, objectIndex);
        MaterialUser(ins, user, material, objectIndex);
    }

    private static void MaterialUser(ObjectList ins, object user, object value, int objectIndex)
    {
        Graphic graphic = user as Graphic;
        ins.SetGraphicMaterial(graphic, value as Material);
    }

    private static void AudioClipUser(ObjectList ins, object user, object value, int objectIndex)
    {
        var source = user as AudioSource;
        if(source == null) return;
        source.clip = (AudioClip)value;
    }

    private static void GameObjectUser(ObjectList ins, object _, object value, int objectIndex)
    {
        var lastIndex = ins.index;
        var list = ins.GetTargetObjects<GameObject>();
        if (lastIndex != objectIndex && lastIndex>=0 && lastIndex < list.Count)
        {
            list[lastIndex].SetActive(false);
        }

        var go = value as GameObject;
        if(go)
            go.SetActive(true);
    }

    private bool UpdateIndex(int value)
    {
        var list = GetTargetObjects();
        //reach end.
        if (!CheckBounds(list, ref value))
        {
            return false;
        }
        if (autoApplyObject)
        {
            var user = GetComponent();
            objectUserMap[_objectType](this, user, list[value], value);
        }
        if (_index != value)
        {
            onIndexChanged?.Invoke(gameObject, _index, value);
            _index = value;
        }
        return true;
    }

    private Material GetMaterial(Shader shader, int index)
    {
        if (shader == null)
        {
            return null;
        }
        if (objects.Material == null)
        {
            objects.Material = new List<Material>(4);
        }
        var list = objects.Material;
        if (index >= list.Count)
        {
            for (var i = list.Count; i <= index; i++)
            {
                list.Add(null);
            }
        }
        if(list[index] == null)
            list[index] = new Material(shader);
        return list[index];
    }

    private void SetGraphicMaterial(Graphic graphic, Material material)
    {
        if (graphic == null) return;
        graphic.material = material;
        if (!includeChildren) return;
        var trans = graphic.transform;
        foreach (Transform t in trans)
        {
            SetGraphicMaterial(t.GetComponent<Graphic>(), material);
        }
    }

#if UNITY_EDITOR

    private void OnUpdate()
    {
        UpdateIndex(_index);
    }
    private void OnValidate()
    {
        ClearFields(objectType);
        if (objectType == ObjectType.GameObject)
        {
            disableAll();
        }

        var list = GetTargetObjects();
        int n = list?.Count ?? 0;
        _index = Mathf.Clamp(_index, 0, n-1);

        if(!EditorApplication.isPlayingOrWillChangePlaymode)
            EditorApplication.delayCall += OnUpdate;

        if (EditorApplication.isPlaying)
        {
            enableAnimation = _enableAnimation;
        }
    }

    private void disableAll()
    {
        var list = GetTargetObjects<GameObject>();
        if (list == null) return;
        foreach (var go in list)
        {
            if(go)
                go.SetActive(false);
        }
    }

    private void ClearFields(ObjectType except)
    {
        var fieldsInfo = typeof(ObjectUnion).GetFields(System.Reflection.BindingFlags.Public
                                                       | System.Reflection.BindingFlags.Instance);
        foreach (var fi in fieldsInfo)
        {
            if (fi.Name == except.ToString())
            {
                continue;
            }

            Object obj = objects;
            fi.SetValue(obj, null);
            objects = (ObjectUnion)obj;
        }
    }
#endif
}
