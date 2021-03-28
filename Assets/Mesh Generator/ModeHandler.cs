using System.Collections;
using System.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeHandler : MonoBehaviour
{
    public DesignHandler designHandler;
    public SolutionHandler solutionHandler;
    public bool designMode = false;
    
    public bool selectMode = false;
    public bool scrobbleMode = false;
    public bool colorMode = false;
    
    public Vector3Int scrobbleVector;
    public Slider scrobbleSlider;
    
    public Renderer voxelRenderer;
    public VoxelGenerator voxelGenerator;
    public float lerpSpeed = 3.0f;
    
    public float cameraRectShrink = 0.1f;
    
    public RectTransform selectPanel;
    public RectTransform scrobblePanel;
    public RectTransform colorPanel;
    
    public CameraPanScript cameraHandler;
    
    public void Reset()
    {
        cameraHandler.selectionArray = new List<Vector3Int>();
        
        selectMode = false;
        scrobbleMode = false;
        colorMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Voxel shader adjustments
        if (selectMode || scrobbleMode) {
            Camera.main.rect = new Rect(0.0f, Mathf.Lerp(Camera.main.rect.y,cameraRectShrink,Time.deltaTime * lerpSpeed), 1.0f, Mathf.Lerp(Camera.main.rect.height,1.0f-cameraRectShrink,Time.deltaTime * lerpSpeed));
        } else {
            Camera.main.rect = new Rect(0.0f, Mathf.Lerp(Camera.main.rect.y,0.0f,Time.deltaTime * lerpSpeed), 1.0f, Mathf.Lerp(Camera.main.rect.height,1.0f,Time.deltaTime * lerpSpeed));
        }

        if (selectMode) {
            voxelRenderer.material.SetFloat("_InnerOutlineThickness", Mathf.Lerp(voxelRenderer.material.GetFloat("_InnerOutlineThickness"), 0.1f, Time.deltaTime * lerpSpeed));
            voxelRenderer.material.SetFloat("_OutlineThickness", Mathf.Lerp(voxelRenderer.material.GetFloat("_OutlineThickness"), 0.02f, Time.deltaTime * lerpSpeed));
            selectPanel.anchoredPosition = new Vector2(0,Mathf.Lerp(selectPanel.anchoredPosition.y, 112.5f, Time.deltaTime * lerpSpeed));
        } else {
            voxelRenderer.material.SetFloat("_InnerOutlineThickness", Mathf.Lerp(voxelRenderer.material.GetFloat("_InnerOutlineThickness"), 0.05f, Time.deltaTime * lerpSpeed));
            voxelRenderer.material.SetFloat("_OutlineThickness", Mathf.Lerp(voxelRenderer.material.GetFloat("_OutlineThickness"), 0.01f, Time.deltaTime * lerpSpeed));
            selectPanel.anchoredPosition = new Vector2(0,Mathf.Lerp(selectPanel.anchoredPosition.y, 0, Time.deltaTime * lerpSpeed));
        }
        
        if (scrobbleMode) {
            scrobblePanel.anchoredPosition = new Vector2(0,Mathf.Lerp(scrobblePanel.anchoredPosition.y, 112.5f, Time.deltaTime * lerpSpeed));
        } else {
            scrobblePanel.anchoredPosition = new Vector2(0,Mathf.Lerp(scrobblePanel.anchoredPosition.y, 0, Time.deltaTime * lerpSpeed));
        }
        
        if (!colorMode) {
            colorPanel.anchoredPosition = new Vector2(Mathf.Lerp(colorPanel.anchoredPosition.x, 75.0f, Time.deltaTime * lerpSpeed),colorPanel.anchoredPosition.y);
        } else {
            colorPanel.anchoredPosition = new Vector2(Mathf.Lerp(colorPanel.anchoredPosition.x, 0.0f, Time.deltaTime * lerpSpeed),colorPanel.anchoredPosition.y);
        }
    }
    
    public void EnableSelectMode() {
        if (colorMode)
            return;
        if (solutionHandler.isSolved && !designMode)
            return;
        
        cameraHandler.selectionArray = new List<Vector3Int>();
        selectMode = true;
    }
    
    public void EnableScrobbleMode(Vector3 normal) {
        if (colorMode) {
            return;   
        }
        
        scrobbleMode = true;
        scrobbleVector = new Vector3Int((int)-normal.x, (int)-normal.y, (int)-normal.z);
        voxelGenerator.sliceVector = Vector3Int.zero;
        
        scrobbleSlider.maxValue =
            voxelGenerator.chunkSize[Array.FindIndex(new int[3]{scrobbleVector.x,scrobbleVector.y,scrobbleVector.z}, x => x != 0)]-1;
        scrobbleSlider.value = 0.0f;//voxelGenerator.chunkSize;
        
        ScrobbleUpdated();
    }
    
    public void ScrobbleUpdated() {
        voxelGenerator.sliceVector = scrobbleVector * Mathf.RoundToInt(scrobbleSlider.value);
        
        Vector4 _DimVector = 
                new Vector4(voxelGenerator.sliceVector.x+scrobbleVector.x, voxelGenerator.sliceVector.y+scrobbleVector.y, voxelGenerator.sliceVector.z+scrobbleVector.z, 1.0f);
        
        for (int i = 0; i < 3; i++) {
            if (_DimVector[i] < 0) {
                _DimVector[i] = (voxelGenerator.chunkSize[i] + 1) - Mathf.Abs(_DimVector[i]);
            }
        }
        
        voxelRenderer.material.SetVector("_DimVector", _DimVector);
        voxelGenerator.GenerateAndApplyMesh();
    }
    
    public void DisableModes() {
        StartCoroutine(DisableModesCoroutine());
    }
    public IEnumerator DisableModesCoroutine() {
        for (var i = 0; i < cameraHandler.selectionArray.Count; i++) {
            //Disable selections
            Color32 currentColor = FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.GetPixel(
                        cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z,0);
            //Set the bit to zero
            currentColor.a = (byte)(currentColor.a & 0b11111101);
            
            FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.SetPixel(
                    cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z, currentColor, 0);
        }
        FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.Apply();
        
        cameraHandler.selectionArray = new List<Vector3Int>();
        
        cameraHandler.touchStartTime = Time.time;
        cameraHandler.velocity = Vector2.zero;
        if (Input.touchCount > 0) {
            cameraHandler.lastTouchPos = cameraHandler.normalizedTouchPosition(Input.GetTouch(0));
        }
        
        yield return new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 
        selectMode = false;
        scrobbleMode = false;
        voxelGenerator.sliceVector = Vector3Int.zero;
        voxelGenerator.GenerateAndApplyMesh();
        
        voxelRenderer.material.SetVector("_DimVector", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
    }
    
    public void DisableSelectMode() {
        StartCoroutine(DisableSelectModeCoroutine());
    }
    void ClearSelection() {
        for (var i = 0; i < cameraHandler.selectionArray.Count; i++) {
            //Disable selections
            Color32 currentColor = FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.GetPixel(
                        cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z,0);
            //Set the bit to zero
            currentColor.a = (byte)(currentColor.a & 0b11111101);
            
            FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.SetPixel(
                    cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z, currentColor, 0);
        }
        FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.Apply();
        
        cameraHandler.selectionArray = new List<Vector3Int>();
        
        cameraHandler.lastHitPos = -Vector4.one;
    }
    public IEnumerator DisableSelectModeCoroutine() {
        ClearSelection();
        
        cameraHandler.touchStartTime = Time.time;
        cameraHandler.velocity = Vector2.zero;
        if (Input.touchCount > 0) {
            cameraHandler.lastTouchPos = cameraHandler.normalizedTouchPosition(Input.GetTouch(0));
        }
        
        yield return new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 
        selectMode = false;
    }
    
    public void RemoveSelectedVoxels() {
        if (designMode) {
            if (FindObjectOfType<VoxelGenerator>().CountVoxels() == cameraHandler.selectionArray.Count) {
                FindObjectOfType<VoxelGenerator>().RemoveVoxels(cameraHandler.selectionArray);
                FindObjectOfType<VoxelGenerator>().AddVoxel(new Vector3Int(0,0,0));
                DisableSelectMode();
                designHandler.unsavedChanges = true;
                return;
            }
        } else {
            for (var i = 0; i < cameraHandler.selectionArray.Count; i++) {
                //Check if it is in the solution, if so, return, a mistake has been made!
                if (solutionHandler.solutionArray[cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z] == 1) {
                    solutionHandler.errors += 1;
                    //DisableSelectMode();
                    //TODO Add some sort of UI feedback here!
                    ClearSelection();
                    return;
                }
            }
        }
        FindObjectOfType<VoxelGenerator>().RemoveVoxels(cameraHandler.selectionArray);
        if (designMode) {
            FindObjectOfType<VoxelGenerator>().ShiftToOrigin();
            designHandler.unsavedChanges = true;
        } else {
            solutionHandler.CheckIfSolved();
        }
        //DisableSelectMode();
        ClearSelection();
    }
    
    public void ColorSelectedVoxels(Color color) {
        for (var i = 0; i < cameraHandler.selectionArray.Count; i++) {
            FindObjectOfType<VoxelGenerator>().colorArray[cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z] = color;
            FindObjectOfType<VoxelGenerator>().UpdateColorVolumeTexture();
        }
        FindObjectOfType<VoxelGenerator>().colorVolumeTexture.Apply();
        colorMode = false;
        DisableSelectMode();
        designHandler.unsavedChanges = true;
    }
    
    public void HighlightSelectedVoxels() {
        if (designMode) {
            colorMode = true;
            selectMode = false;
        } else {
            for (var i = 0; i < cameraHandler.selectionArray.Count; i++) {
                //Toggle highlights
                Color32 currentColor = FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.GetPixel(
                            cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z,0);
                if ((currentColor.a & 0b00000001) > 0) {
                    //Set the bit to zero
                    currentColor.a = (byte)(currentColor.a & 0b11111110);
                } else {
                    //Set the bit to one
                    currentColor.a = (byte)(currentColor.a | 0b00000001);
                }
                FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.SetPixel(
                        cameraHandler.selectionArray[i].x,cameraHandler.selectionArray[i].y,cameraHandler.selectionArray[i].z, currentColor, 0);
            }
            FindObjectOfType<VoxelGenerator>().highlightVolumeTexture.Apply();
            //DisableSelectMode();
            ClearSelection();
        }
    }
}
