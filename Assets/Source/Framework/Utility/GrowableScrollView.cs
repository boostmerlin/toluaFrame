using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowableScrollView : MonoBehaviour
{
    public int MaxContentCount = 4;
    public bool vertical = true;
    RectTransform content;
    int currentCount;
    List<RectTransform> visibleChild;
    void Awake()
    {
        visibleChild = new List<RectTransform>();
        var sr = GetComponent<ScrollRect>();
        if (sr)
        {
            content = sr.content;
        }
        currentCount = getVisibleChild().Count; 
    }

    List<RectTransform> getVisibleChild()
    {
        visibleChild.Clear();
        foreach (RectTransform rt in content)
        {
            if (rt.gameObject.activeSelf)
            {
                visibleChild.Add(rt);
            }
        }
        return visibleChild;
    }

    private void Update()
    {
        if(MaxContentCount > 0 && currentCount >= MaxContentCount)
        {
            return;
        }
        getVisibleChild();
        if(currentCount != visibleChild.Count)
        {
            currentCount = visibleChild.Count;
            float size = 0;
            foreach(RectTransform rt in visibleChild)
            {
                if(vertical)
                    size += rt.rect.height;
                else
                    size += rt.rect.width;
            }
            var layout = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layout)
            {
                size += (currentCount - 1) * layout.spacing;
            }
            if (vertical)
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            else
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        }
    }

}
