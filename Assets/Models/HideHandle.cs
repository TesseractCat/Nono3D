using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideHandle : MonoBehaviour
{
    public Transform pivotPoint;
    Renderer[] childRenderers;

    // Start is called before the first frame update
    void Start()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 flattenedVector = new Vector2(transform.position.x, transform.position.z);
        Vector2 flattenedCameraVector = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 flattenedPivotPosition = new Vector2(pivotPoint.position.x, pivotPoint.position.z);
        flattenedVector = (flattenedVector-flattenedPivotPosition).normalized;
        flattenedCameraVector = (flattenedCameraVector-flattenedPivotPosition).normalized;
        
        float angle = Mathf.Atan2(flattenedVector.y, flattenedVector.x) - Mathf.Atan2(flattenedCameraVector.y, flattenedCameraVector.x);
        angle = (angle/(2*Mathf.PI)) * 360;
        
        //Debug.Log(angle);
        
        if (angle < 30 || angle > 135) {
            for (var i = 0; i < childRenderers.Length; i++) {
                childRenderers[i].enabled = false;
            }
        } else {
            for (var i = 0; i < childRenderers.Length; i++) {
                childRenderers[i].enabled = true;
            }
        }
    }
}
