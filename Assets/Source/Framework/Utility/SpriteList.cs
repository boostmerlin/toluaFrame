using UnityEngine;
using UnityEngine.UI;
using LuaInterface;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class SpriteList : MonoBehaviour
{
    public Sprite[] sprites;
    public int defaultIndex;
    //public bool animateSprite = false;
    //public bool loop = false;
    //public float intervalue = 0.1f;

    Image m_Image;
    SpriteRenderer m_SpriteRender;

    int currentIndex = -1;
    public int Length
    {
        get
        {
            return sprites.Length;
        }
    }
    public int CurrentIndex
    {
        get
        {
            return currentIndex;
        }
    }

    private void Awake()
    {
        m_Image = GetComponent<Image>();
        m_SpriteRender = GetComponent<SpriteRenderer>();
        ChangeSprite(defaultIndex);
    }

    public void ChangeSpriteByName(string name)
    {
        if(sprites == null || Length == 0)
        for(int i = 0; i < sprites.Length; i++)
        {
                if(name == sprites[i].name)
                {
                    ChangeSprite(i);
                    break;
                }
        }
    }

    public void ChangeSprite(int index, bool setDefault = false)
    {
        if (sprites != null && index >= 0 && Length != 0)
        {
            int i = index % Length;
            if (m_Image)
                m_Image.sprite = sprites[i];
            if (m_SpriteRender)
                m_SpriteRender.sprite = sprites[i];
            currentIndex = i;
            if (setDefault)
            {
                defaultIndex = currentIndex;
            }
        }
    }
}
