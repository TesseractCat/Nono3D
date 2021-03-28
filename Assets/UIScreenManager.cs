using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenManager : MonoBehaviour
{
    public float lerpSpeed = 10.0f;
    
    float activeScreenTargetX = 0;
    float activeScreenTargetY = 0;
    public RectTransform activeScreen;
    
    float newScreenTargetX = 400;
    float newScreenTargetY = 0;
    RectTransform newScreen = null;
    
    bool moving = false;
    bool movingVertically = false;
    
    public void SetScreenLeft(RectTransform newScreen) {//, bool moveLeft) {
        if (moving)
            return;
        
        this.newScreen = newScreen;
        this.newScreen.GetComponent<CanvasGroup>().interactable = false;
        this.newScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
        
        this.activeScreenTargetX = 400;
        this.newScreenTargetX = 0;
        
        this.newScreen.localPosition = new Vector3(-400, 0, 0);
        
        //this.newScreen.gameObject.SetActive(true);
        
        moving = true;
    }
    
    public void SetScreenUp(RectTransform newScreen) {
        if (moving)
            return;
        
        this.newScreen = newScreen;
        this.newScreen.GetComponent<CanvasGroup>().interactable = false;
        this.newScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
        
        float height = this.activeScreen.rect.height;
        
        this.activeScreenTargetY = height;
        this.newScreenTargetY = 0;
        
        this.newScreen.localPosition = new Vector3(0, -height, 0);
        //this.newScreen.gameObject.SetActive(true);
        
        moving = true;
        movingVertically = true;
    }
    
    public void SetScreenDown(RectTransform newScreen) {
        if (moving)
            return;
        
        this.newScreen = newScreen;
        this.newScreen.GetComponent<CanvasGroup>().interactable = false;
        this.newScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
        
        float height = this.activeScreen.rect.height;
        
        this.activeScreenTargetY = -height;
        this.newScreenTargetY = 0;
        
        this.newScreen.localPosition = new Vector3(0, height, 0);
        //this.newScreen.gameObject.SetActive(true);
        
        moving = true;
        movingVertically = true;
    }
    
    public void SetScreen(RectTransform newScreen) {//, bool moveLeft) {
        if (moving)
            return;
        
        this.newScreen = newScreen;
        this.newScreen.GetComponent<CanvasGroup>().interactable = false;
        this.newScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
        
        this.activeScreenTargetX = -400;
        this.newScreenTargetX = 0;
        
        this.newScreen.localPosition = new Vector3(400, 0, 0);
        
        //this.newScreen.gameObject.SetActive(true);
        
        moving = true;
    }

    void Update()
    {
        if (moving && !movingVertically) {
            activeScreen.localPosition = new Vector3(Mathf.Lerp(activeScreen.localPosition.x, activeScreenTargetX, Time.deltaTime * lerpSpeed), 0, 0);
            newScreen.localPosition = new Vector3(Mathf.Lerp(newScreen.localPosition.x, newScreenTargetX, Time.deltaTime * lerpSpeed), 0, 0);
        } else if (moving && movingVertically) {
            activeScreen.localPosition = new Vector3(0, Mathf.Lerp(activeScreen.localPosition.y, activeScreenTargetY, Time.deltaTime * lerpSpeed), 0);
            newScreen.localPosition = new Vector3(0, Mathf.Lerp(newScreen.localPosition.y, newScreenTargetY, Time.deltaTime * lerpSpeed), 0);
        }
        
        if (!movingVertically) {
            if (moving && Mathf.RoundToInt(activeScreen.localPosition.x) == Mathf.RoundToInt(activeScreenTargetX)) {
                activeScreen.transform.localPosition = new Vector3(400,0,0);
                activeScreen.GetComponent<CanvasGroup>().alpha = 0.0f;
                //activeScreen.gameObject.SetActive(false);
                activeScreen = newScreen;
                activeScreen.transform.localPosition = new Vector3(0,0,0);
                activeScreen.GetComponent<CanvasGroup>().interactable = true;
                newScreen = null;
                
                moving = false;
            }
        } else {
            if (moving && Mathf.RoundToInt(activeScreen.localPosition.y) == Mathf.RoundToInt(activeScreenTargetY)) {
                activeScreen.transform.localPosition = new Vector3(400,0,0);
                activeScreen.GetComponent<CanvasGroup>().alpha = 0.0f;
                //activeScreen.gameObject.SetActive(false);
                activeScreen = newScreen;
                activeScreen.transform.localPosition = new Vector3(0,0,0);
                activeScreen.GetComponent<CanvasGroup>().interactable = true;
                newScreen = null;
                
                moving = false;
                movingVertically = false;
            }
        }
    }
}
