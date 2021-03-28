using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 5.0f;

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && canvasGroup.alpha != 1.0f) {
            canvasGroup.alpha = 1.0f;
        }
        
        if (canvasGroup.alpha < 0.99f) {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1.0f, Time.deltaTime * fadeSpeed);
        } else if (canvasGroup.alpha != 1.0f) {
            canvasGroup.alpha = 1.0f;
        }
    }
}
