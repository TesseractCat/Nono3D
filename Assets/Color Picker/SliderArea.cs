using System.Collections;
using UnityEngine;
using UnityEngine.Events;
 
public class SliderArea : MonoBehaviour
{
    public RectTransform Handle;
    public RectTransform Area;
 
    float Width;
    float Height;
 
    public UnityEvent onValueChanged;
 
    // Start is called before the first frame update
    public void Init()
    {
        if (onValueChanged == null)
            onValueChanged = new UnityEvent();
 
        Width = Area.rect.width;
        Height = Area.rect.height;
    }
    
    public void SetValue(Vector2 newValue) {
        Handle.anchoredPosition = Vector2.Scale(newValue, new Vector2(Width, Height));
    }
 
    public Vector2 Value()
    {
        float x = 0;
        float y = 0;
        Vector2 returnValue;
 
        x = Handle.anchoredPosition.x / Width;
        y = Handle.anchoredPosition.y / Height;
 
        returnValue = new Vector2(x, y);
 
        return returnValue;
    }
 
    IEnumerator Drag()
    {
        Vector2 currentValue;
        Vector2 prevValue;
 
        while (Input.GetMouseButton(0))
        {
            prevValue = Value();
 
            Handle.position = Input.mousePosition;
             
 
            if (Handle.anchoredPosition.x < 0)
            {
                Handle.anchoredPosition = new Vector2(0, Handle.anchoredPosition.y);
            }else if(Handle.anchoredPosition.x > Width)
            {
                Handle.anchoredPosition = new Vector2(Width, Handle.anchoredPosition.y);
            }
 
            if (Handle.anchoredPosition.y < 0)
            {
                Handle.anchoredPosition = new Vector2(Handle.anchoredPosition.x, 0);
            }
            else if (Handle.anchoredPosition.y > Height)
            {
                Handle.anchoredPosition = new Vector2(Handle.anchoredPosition.x, Height);
            }
 
            currentValue = Value();
            bool valueChange = currentValue.x != prevValue.x || currentValue.y != prevValue.y;
            if (valueChange)
            {
                onValueChanged.Invoke();
            }
             
 
            yield return null;
        }
 
        yield return null;
    }
 
    public void MouseDown()
    {
        StartCoroutine(Drag());
    }
}
