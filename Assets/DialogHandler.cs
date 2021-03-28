using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogHandler : MonoBehaviour
{
    CanvasGroup canvasGroup;
    
    float targetAlpha = 0.0f;
    public float lerpSpeed = 1.0f;
    
    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    
    public void Show() {
        gameObject.SetActive(true);
        
        targetAlpha = 1.0f;
    }
    
    public void Hide() {
        targetAlpha = 0.0f;
    }
    
    void Update() {
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * lerpSpeed);
        
        if (canvasGroup.alpha < 0.001f && targetAlpha == 0.0f) {
            canvasGroup.alpha = 0.0f;
            gameObject.SetActive(false);
        }
    }
}
