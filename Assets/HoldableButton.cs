using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using RDG;

[RequireComponent(typeof(EventTrigger))]
public class HoldableButton : MonoBehaviour
{
    public float holdThreshold = 0.2f;
    float pointerDownTime = 0.0f;
    bool buttonPressed = false;
    bool vibrated = false;
    
    bool dragged = false;
    
    bool pressedWhileNonInteractable = false;
    
    public UnityEvent tap;
    public UnityEvent hold;
    
    public void OnBeginDrag() {
        dragged = true;
    }
    
    public void OnPointerDown()
    {
        if (gameObject.GetComponentInParent<CanvasGroup>() != null) {
            if (!gameObject.GetComponentInParent<CanvasGroup>().interactable) {
                pressedWhileNonInteractable = true;
                return;
            }
        }
        pointerDownTime = Time.time;
        buttonPressed = true;
        vibrated = false;
        dragged = false;
    }
    
    public void OnPointerStay()
    {
        if (Time.time - pointerDownTime > holdThreshold && !vibrated && !dragged) {
            //Vibrate
            if (Application.isEditor) {
                Handheld.Vibrate();
            } else {
                RDG.Vibration.Vibrate(60, -1, true);
            }
            vibrated = true;
        }
    }
    
    public void OnPointerUp()
    {
        if (pressedWhileNonInteractable) {
            pressedWhileNonInteractable = false;
            return;
        }
        
        if (dragged) {
            buttonPressed = false;
            return;
        }
        
        if (Time.time - pointerDownTime > holdThreshold) {
            hold.Invoke();
        } else {
            tap.Invoke();
        }
        buttonPressed = false;
    }
    
    void Awake() {
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => {OnPointerDown();});
        
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {OnPointerUp();});
        
        EventTrigger.Entry beginDrag = new EventTrigger.Entry();
        beginDrag.eventID = EventTriggerType.BeginDrag;
        beginDrag.callback.AddListener((data) => {OnBeginDrag();});
        
        GetComponent<EventTrigger>().triggers.Add(pointerDown);
        GetComponent<EventTrigger>().triggers.Add(pointerUp);
        GetComponent<EventTrigger>().triggers.Add(beginDrag);
    }
    
    void Update() {
        if (buttonPressed) {
            OnPointerStay();
        }
    }
}
