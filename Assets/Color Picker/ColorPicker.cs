using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public Slider hueSlider;
    public SliderArea gradientSlider;
    public Image previewColor;
    public TMPro.TextMeshProUGUI previewText;
    public Image gradientImage;
    
    public Color color;
    
    public UnityEvent onChanged;
    
    public void SetColor(Color newColor) {
        gradientSlider.Init();
        
        float h, s, v;
        Color.RGBToHSV(newColor, out h, out s, out v);
        
        hueSlider.value = h;
        gradientSlider.SetValue(new Vector2(s, v));
        
        color = newColor;
        
        previewColor.color = color;
        gradientImage.color = Color.HSVToRGB(h, 1, 1);
        previewText.SetText("#" + ColorUtility.ToHtmlStringRGB(color));
    }
    
    public void UpdateColor()
    {
        Vector2 gradientSliderValue;
        float h;
        float s;
        float v;
        
        gradientSliderValue = gradientSlider.Value();
        h = hueSlider.value;
        s = gradientSliderValue.x;
        v = gradientSliderValue.y;
        
        color = Color.HSVToRGB(h, s, v);
        
        if (onChanged != null) {
            onChanged.Invoke();
        }
        
        previewColor.color = color;
        gradientImage.color = Color.HSVToRGB(h, 1, 1);
        previewText.SetText("#" + ColorUtility.ToHtmlStringRGB(color));
    }
}
