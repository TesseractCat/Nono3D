using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesignHandler : MonoBehaviour
{
    public VoxelGenerator voxelGenerator;
    
    public GameObject boundingBox;
    
    public RectTransform selectPanel;
    public RectTransform scrobblePanel;
    
    public RectTransform renameScreen;
    public RectTransform mainMenuScreen;
    
    public TMPro.TextMeshProUGUI saveButtonText;
    
    static Color HexToColor(string hex) {
        byte r = Convert.ToByte(hex.Substring(1,2), 16);
        byte g = Convert.ToByte(hex.Substring(3,2), 16);
        byte b = Convert.ToByte(hex.Substring(5,2), 16);
        return new Color32(r, g, b, 255);
    }
    static Color[] defaultColors = new Color[8]{
        HexToColor("#222222"),
        HexToColor("#CCCCCC"),
        HexToColor("#8BC90B"),
        HexToColor("#F4E02B"),
        HexToColor("#EFA43A"),
        HexToColor("#E84947"),
        HexToColor("#5F4493"),
        HexToColor("#39A7DE")
    };
    public Transform colorList;
    
    public GameObject exitTestButton;
    
    public DialogHandler exitDialog;
    
    public Puzzle currentPuzzle;
    bool newPuzzle = true;
    
    public bool unsavedChanges;
    
    public void InitEmpty()
    {
        this.Init();
    }
    
    public void Init(Puzzle puzzle = null)
    {
        FindObjectOfType<ModeHandler>().Reset();
        FindObjectOfType<ModeHandler>().designMode = true;
        FindObjectOfType<ModeHandler>().selectPanel = selectPanel;
        FindObjectOfType<ModeHandler>().scrobblePanel = scrobblePanel;
        boundingBox.SetActive(false);
        
        if (puzzle == null) {
            Texture3D emptyTexture3D = EmptyTexture3D(new Vector3Int(9,9,9));
            voxelGenerator.Init(new Vector3Int(9,9,9), emptyTexture3D, true);
            voxelGenerator.AddVoxel(new Vector3Int(0,0,0));
            
            //Restore colorPalette
            Image[] colorImages = colorList.GetComponentsInChildren<Image>();
            for (int i = 0; i < colorImages.Length; i++)
                colorImages[i].color = defaultColors[i];
            
            currentPuzzle = new Puzzle();
            newPuzzle = true;
            unsavedChanges = true;
        } else {
            int[,,] puzzleVoxelArray = SaveLoadHandler.Pad3DArray<int>(SaveLoadHandler.ExpandFlattened3DArray<int>(puzzle.flattenedVoxelArray, puzzle.puzzleSize), new Vector3Int(9,9,9));
            Color[,,] puzzleColorArray = SaveLoadHandler.Pad3DArray<Color>(SaveLoadHandler.ExpandFlattened3DArray<Color>(puzzle.flattenedColorArray, puzzle.puzzleSize), new Vector3Int(9,9,9));
            
            Texture3D emptyTexture3D = EmptyTexture3D(new Vector3Int(9,9,9));
            voxelGenerator.Init(new Vector3Int(9,9,9), emptyTexture3D, true, puzzleVoxelArray, puzzleColorArray);
            
            //Restore colorPalette
            if (puzzle.colorPalette != null) {
                Image[] colorImages = colorList.GetComponentsInChildren<Image>();
                for (int i = 0; i < colorImages.Length; i++) {
                    if (puzzle.colorPalette[i] != null) {
                        colorImages[i].color = puzzle.colorPalette[i];
                    }
                }
            } else {
                Image[] colorImages = colorList.GetComponentsInChildren<Image>();
                for (int i = 0; i < colorImages.Length; i++)
                    colorImages[i].color = defaultColors[i];
            }
            
            currentPuzzle = puzzle;
            newPuzzle = false;
            unsavedChanges = false;
        }
    }
    
    public void SetPuzzleName(TMPro.TMP_InputField textField) {
        string newName = textField.text;
        
        if (newName == "")
            return;
        
        currentPuzzle.name = newName;
    }
    public void SetPuzzleAuthor(TMPro.TMP_InputField textField) {
        string newAuthor = textField.text;
        
        if (newAuthor == "")
            return;
        
        currentPuzzle.author = newAuthor;
    }
    
    public void Save()
    {
        if (newPuzzle) {
            //Rename if new puzzle
            FindObjectOfType<UIScreenManager>().SetScreen(renameScreen);
            renameScreen.GetComponent<PopulateRenameFields>().PopulateEmpty();
            newPuzzle = false;
            return;
        }
        
        //Calculate dimensions
        Vector3Int dimensions = voxelGenerator.calculateBoundingBox().Item2 + Vector3Int.one;
        
        //Save puzzle arrays
        currentPuzzle.flattenedVoxelArray = SaveLoadHandler.Flatten3DArray<int>(voxelGenerator.voxelArray, dimensions);
        currentPuzzle.flattenedColorArray = SaveLoadHandler.Flatten3DArray<Color>(voxelGenerator.colorArray, dimensions);
        
        //Save colorPalette
        List<Color> colorPalette = new List<Color>();
        Image[] colorImages = colorList.GetComponentsInChildren<Image>();
        for (int i = 0; i < colorImages.Length; i++)
            colorPalette.Add(colorImages[i].color);
        currentPuzzle.colorPalette = colorPalette.ToArray();
        
        //Serialize date and dimensions
        currentPuzzle.datetime = DateTime.Now.ToBinary();
        currentPuzzle.puzzleSize = dimensions;
        
        //Save to JSON
        SaveLoadHandler.SavePuzzle(currentPuzzle);
        
        saveButtonText.SetText("saved!");
        StartCoroutine(ResetSaveButton());
        
        unsavedChanges = false;
    }
    
    IEnumerator ResetSaveButton() {
        yield return new WaitForSeconds(0.5f);
        saveButtonText.SetText("save");
    }
    
    public void Exit()
    {
        exitTestButton.SetActive(false);
        
        if (!unsavedChanges) {
            FindObjectOfType<UIScreenManager>().SetScreenLeft(mainMenuScreen);
        } else {
            exitDialog.Show();
        }
    }
    
    public void TestPuzzle()
    {
        if (FindObjectOfType<SolutionHandler>().initializing)
            return;
        
        //Update puzzle object
        Vector3Int dimensions = voxelGenerator.calculateBoundingBox().Item2 + Vector3Int.one;
        
        currentPuzzle.flattenedVoxelArray = SaveLoadHandler.Flatten3DArray<int>(voxelGenerator.voxelArray, dimensions);
        currentPuzzle.flattenedColorArray = SaveLoadHandler.Flatten3DArray<Color>(voxelGenerator.colorArray, dimensions);
        
        currentPuzzle.datetime = DateTime.Now.ToBinary();
        currentPuzzle.puzzleSize = dimensions;
        
        //Initiate solution handler
        FindObjectOfType<SolutionHandler>().Init(currentPuzzle);
        
        exitTestButton.SetActive(true);
    }
    
    public void StopTestingPuzzle() {
        bool hasUnsavedChanges = unsavedChanges;
        bool isNewPuzzle = newPuzzle;
        this.Init(currentPuzzle);
        unsavedChanges = hasUnsavedChanges;
        newPuzzle = isNewPuzzle;
    }
    
    public static Texture3D EmptyTexture3D(Vector3Int dimensions) {
        Texture3D volumeTexture = new Texture3D(dimensions.x, dimensions.y, dimensions.z, TextureFormat.RGBA32, 0);
        
        for (int x = 0; x < dimensions.x; x++) {
            for (int y = 0; y < dimensions.y; y++) {
                for (int z = 0; z < dimensions.z; z++) {
                    Color32 color = new Color(0.0f,0.0f,0.0f,0.0f);
                    color.r = 0;
                    color.g = 0;
                    color.b = 0;
                    color.a = 0;
                    volumeTexture.SetPixel(x, y, z, color, 0);
                }
            }
        }
        
        volumeTexture.Apply();
        
        return volumeTexture;
    }
}
