using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Slider))]
public class SliderOverlayTip : MonoBehaviour
{
    private Slider slider;
    public Text textWidget;

    public string format = "{0}/{1}";

    void Awake()
    {
        slider = GetComponent<Slider>();
        Invalidate();
    }
    private void OnEnable()
    {
        slider.onValueChanged.AddListener(onValueChange);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(onValueChange);
    }

    private void onValueChange(float v)
    {
        if (textWidget)
        {
            if (format.Contains(@"{1}"))
            {
                textWidget.text = string.Format(format, slider.value, slider.maxValue);
            }
            else
            {
                textWidget.text = string.Format(format, slider.value);
            }
        }
    }

    public void Invalidate()
    {
        onValueChange(slider.value);
    }
}
