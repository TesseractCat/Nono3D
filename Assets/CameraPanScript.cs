using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraPanScript : MonoBehaviour
{
    public ModeHandler modeHandler;
    
    public Transform pivotPoint;
    Vector3 pivotPosition;
    public Vector3 pivotPointOffset = Vector3.zero;
    public Vector2 rotSpeed = new Vector2(10000,10000);
    public float velocityDampening = 2.0f;
    
    public float lerpSpeed = 10.0f;
    
    public float baseViewDistance = 12.0f;
    [System.NonSerialized]
    public float maxSize = 0.0f;
    
    public Renderer voxelRenderer;
    
    Vector2 startTouchPos = Vector2.zero;
    
    [System.NonSerialized]
    public Vector2 lastTouchPos = Vector2.zero;
    
    [System.NonSerialized]
    public Vector2 velocity = Vector2.zero;
    
    [System.NonSerialized]
    public Vector4 lastHitPos = -Vector4.one;
    
    [System.NonSerialized]
    public List<Vector3Int> selectionArray;
    
    //Don't use Time.time for this, use something like the Stopwatch class... Time.time loses accuracy after 4.5-9 hrs
    [System.NonSerialized]
    public float touchStartTime = 0.0f;
    
    bool WaitingForDoubleTap = false;
    bool DoubleTapped = false;
    Touch DoubleTapTouch;

    // Start is called before the first frame update
    void Start()
    {
        selectionArray = new List<Vector3Int>();
    }
    
    void FixedUpdate() {
        if (Input.touchCount == 0 || (Input.touchCount == 1 && TouchOverUI())) {
            velocity = velocity*velocityDampening;
        }
    }
    
    bool ToggleSelection(Vector3Int position) {
        if (position.x > -1.0 && !selectionArray.Contains(position)) {
            selectionArray.Add(position);
            return true;
        } else if (position.x > -1.0) {
            selectionArray.Remove(position);
            return false;
        }
        return false;
    }
    
    float normalizeAngle(float angle) {
        float newAngle = angle;
        while (newAngle <= -180) {
            newAngle += 360;
        }
        while (newAngle > 180) {
            newAngle -= 360;
        }
        return newAngle;
    }
    
    public Vector2 normalizedTouchPosition(Touch touch) {
        return touch.position/new Vector2(Screen.width, Screen.height);
    }
    
    IEnumerator WaitForDoubleTap() {
        yield return new WaitForSeconds(0.2f);
        WaitingForDoubleTap = false;
        
        if (DoubleTapped) {
            //Vector3 normal = ScreenToChunkNormal(DoubleTapTouch.position);
            //modeHandler.EnableScrobbleMode(normal);
            //velocity = Vector2.zero;
            DoubleTapDetected();
        } else {
            //modeHandler.EnableSelectMode();
            //velocity = Vector2.zero;
            NoDoubleTap();
        }
        DoubleTapped = false;
    }
    
    bool TouchOverUI() {
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = touch.position;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        } else {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        pivotPosition = Vector3.Lerp(pivotPosition, pivotPoint.transform.position + pivotPointOffset, Time.deltaTime * lerpSpeed);

        Camera.main.transform.LookAt(pivotPosition);
        
        Camera.main.transform.position = pivotPosition + (Camera.main.transform.position - pivotPosition).normalized * (baseViewDistance + maxSize);
       
        //Touch detection
        if (!WaitingForDoubleTap) {
            if (modeHandler.colorMode) {
                //Do nothing!
            } else if (Input.touchCount == 1 && !modeHandler.selectMode && !TouchOverUI()) {
                //Panning/rotation mode
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = normalizedTouchPosition(touch);
                (Vector4 hitPosition, Vector3 hitNormal) = ScreenToChunkPosition(touch.position);
                
                if (touch.phase == TouchPhase.Began) {
                    lastTouchPos = touchPos;
                    startTouchPos = touchPos;
                    touchStartTime = Time.time;
                } else if (touch.phase == TouchPhase.Moved) {
                    Vector2 deltaPos = -(touchPos - lastTouchPos);
                    lastTouchPos = touchPos;
                    
                    Camera.main.transform.RotateAround(pivotPosition, Vector3.up, -Time.deltaTime * rotSpeed.x * deltaPos.x);
                    Camera.main.transform.RotateAround(pivotPosition, Camera.main.transform.right, Time.deltaTime * rotSpeed.y * deltaPos.y);
                } else if (touch.phase == TouchPhase.Ended) {
                    //Check for distance and delay -- TODO make delay and distance variables
                    if (Time.time - touchStartTime < 200.0f/1000.0f && Vector3.Distance(startTouchPos, touchPos) < 0.05) {
                        //Tap < 200 ms
                        
                        //Wait for doubletap if tapped cube, and not already in scrobble mode
                        if (hitPosition != -Vector4.one && !modeHandler.scrobbleMode) {
                            //WaitingForDoubleTap = true;
                            //StartCoroutine(WaitForDoubleTap());
                            TappedCube(hitPosition, hitNormal);
                        } else {
                            //modeHandler.EnableSelectMode();
                            //velocity = Vector2.zero;
                            TappedBackground();
                        }
                    } else {
                        //Drag/swipe >= 200 ms
                        Vector2 deltaPos = -(touchPos - lastTouchPos);
                        lastTouchPos = touchPos;
                        
                        velocity = deltaPos;// * Mathf.Clamp(Time.time - touchStartTime,0.0f,1.0f/5.0f)*5.0f;
                    }
                }
            } else if (!modeHandler.selectMode) {
                //Panning mode no touching
                Camera.main.transform.RotateAround(pivotPosition, Vector3.up, -Time.deltaTime * rotSpeed.x * velocity.x);
                Camera.main.transform.RotateAround(pivotPosition, Camera.main.transform.right, Time.deltaTime * rotSpeed.y * velocity.y);
            } else if (modeHandler.selectMode && Input.touchCount == 1) {
                //Selection mode
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = normalizedTouchPosition(touch);
                (Vector4 hitPosition, Vector3 hitNormal) = ScreenToChunkPosition(touch.position);
                
                if (touch.phase == TouchPhase.Began) {
                    lastTouchPos = touchPos;
                    startTouchPos = touchPos;
                    touchStartTime = Time.time;
                } else if (touch.phase == TouchPhase.Moved) {
                    if (hitPosition != -Vector4.one) {
                        SelectModeDragged(hitPosition);
                    }
                } else if (touch.phase == TouchPhase.Ended) {
                    //If tap
                    lastHitPos = -Vector4.one;
                }
            }
        } else if (WaitingForDoubleTap && Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended) {
                DoubleTapped = true;
                DoubleTapTouch = touch;
            }
        }
        
        //Keyboard fallback
        if (Application.isEditor) {
            if (Input.GetKey(KeyCode.A)) {
                Camera.main.transform.RotateAround(pivotPosition, Vector3.up, Time.deltaTime * rotSpeed.x/50.0f);
            }
            if (Input.GetKey(KeyCode.D)) {
                Camera.main.transform.RotateAround(pivotPosition, Vector3.up, -Time.deltaTime * rotSpeed.x/50.0f);
            }
            
            if (Input.GetKey(KeyCode.W)) {
                Camera.main.transform.RotateAround(pivotPosition, Camera.main.transform.right, Time.deltaTime * rotSpeed.y/50.0f);
            }
            if (Input.GetKey(KeyCode.S)) {
                Camera.main.transform.RotateAround(pivotPosition, Camera.main.transform.right, -Time.deltaTime * rotSpeed.y/50.0f);
            }
            
            if (Input.GetKeyDown(KeyCode.E)) {
                modeHandler.selectMode = !modeHandler.selectMode;
            }
            
            if (Input.GetKeyDown(KeyCode.F)) {
                modeHandler.scrobbleMode = !modeHandler.scrobbleMode;
            }
            if (Input.GetMouseButton(0)) {
                (Vector4 hitPosition, Vector3 hitNormal) = ScreenToChunkPosition(Input.mousePosition);
                if (hitPosition != -Vector4.one) {
                    SelectModeDragged(hitPosition);
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                (Vector4 hitPosition, Vector3 hitNormal) = ScreenToChunkPosition(Input.mousePosition);
                if (hitPosition != -Vector4.one) {
                    TappedCube(hitPosition,hitNormal);
                }
            }
        }
        
        //Camera clamping, doesn't quite work well
        if (normalizeAngle(Camera.main.transform.rotation.eulerAngles.x) < -85) {
            Camera.main.transform.RotateAround(pivotPosition, Camera.main.transform.right, -(normalizeAngle(Camera.main.transform.rotation.eulerAngles.x) + 85));
        }
        else if (normalizeAngle(Camera.main.transform.rotation.eulerAngles.x) > 85) {
            Camera.main.transform.RotateAround(pivotPosition, Camera.main.transform.right, -(normalizeAngle(Camera.main.transform.rotation.eulerAngles.x) - 85));
        }
    }
    
    // ----- TAP EVENTS -----
    
    void TappedBackground() {
        modeHandler.EnableSelectMode();
        velocity = Vector2.zero;
    }
    
    void TappedCube(Vector4 hitPosition, Vector3 hitNormal) {
        if (!modeHandler.designMode) {
            WaitingForDoubleTap = true;
            StartCoroutine(WaitForDoubleTap());
        } else {
            FindObjectOfType<DesignHandler>().unsavedChanges = true;
            FindObjectOfType<VoxelGenerator>().AddVoxel(
                    new Vector3Int((int)hitPosition.x+(int)hitNormal.x,
                        (int)hitPosition.y+(int)hitNormal.y,
                        (int)hitPosition.z+(int)hitNormal.z));
        }
    }
    
    void DoubleTapDetected() {
        Vector3 normal = ScreenToChunkNormal(DoubleTapTouch.position);
        modeHandler.EnableScrobbleMode(normal);
        velocity = Vector2.zero;
    }
    
    void NoDoubleTap() {
        TappedBackground();
    }
    
    void SelectModeDragged(Vector4 hitPosition) {
        //Drag highlighting
        if (hitPosition != lastHitPos) {
            bool toggledOn = ToggleSelection(new Vector3Int((int)hitPosition.x,(int)hitPosition.y,(int)hitPosition.z));
            
            //Apply to highlightVolumeTexture
            Color32 currentColor = FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.GetPixel(
                        (int)hitPosition.x,(int)hitPosition.y,(int)hitPosition.z,0);
            if (!toggledOn) {
                //Set the bit to zero
                currentColor.a = (byte)(currentColor.a & 0b11111101);
            } else {
                //Set the bit to one
                currentColor.a = (byte)(currentColor.a | 0b00000010);
            }
            FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.SetPixel(
                    (int)hitPosition.x,(int)hitPosition.y,(int)hitPosition.z, currentColor, 0);
            
            FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.Apply();
        }
        
        lastHitPos = hitPosition;
    }
    
    // ----- END -----
        
    (Vector4, Vector3) ScreenToChunkPosition(Vector2 screenPoint) {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit)) {
            if (hit.transform.tag == "Chunk") {
                MeshCollider coll = hit.collider as MeshCollider;
                Vector4 position = (Vector4) coll.sharedMesh.colors[coll.sharedMesh.triangles[hit.triangleIndex*3]];
                position.w = 0.0f;
                position = Vector3.Scale(position, FindObjectOfType<VoxelGenerator>().chunkSize);
                return (position, hit.normal);
            } else {
                return (-Vector4.one, -Vector3.one);
            }
        } else {
            return (-Vector4.one, -Vector3.one);
        }
    }
    
    Vector4 ScreenToChunkNormal(Vector2 screenPoint) {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit)) {
            if (hit.transform.tag == "Chunk") {
                return hit.normal;
            } else {
                return Vector3.zero;
            }
        } else {
            return Vector3.zero;
        }
    }
    
    public Vector4[] shiftRight(Vector4[] arr) 
    {
        Vector4[] demo = new Vector4[arr.Length];

        for (int i = 1; i < arr.Length; i++) 
        {
            demo[i] = arr[i - 1];
        }

        demo[0] = arr[demo.Length - 1];

        return demo;
    } 
}
