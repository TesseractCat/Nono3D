using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateMyPuzzles : MonoBehaviour
{
    public GameObject puzzleEntryPrefab;
    
    public UIScreenManager screenManager;
    public RectTransform designerScreen;
    
    public DesignHandler designHandler;
    
    public DialogHandler deleteDialog;
    
    public void Populate()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        if (Directory.Exists(Application.persistentDataPath + "/MyPuzzles")) {
            //May not be that fast
            string[] files = Directory.GetFiles(Application.persistentDataPath + "/MyPuzzles/", "*.json").OrderByDescending(d=>new FileInfo(d).CreationTime).ToArray();;
            
            for (int i = 0; i < files.Length; i++) {
                string filePath = files[i];
                string fileName = Path.GetFileNameWithoutExtension(files[i]);
                
                GameObject newPuzzleEntry = (GameObject)Instantiate(puzzleEntryPrefab, transform);
                newPuzzleEntry.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().SetText(fileName);
                
                newPuzzleEntry.GetComponent<HoldableButton>().tap.AddListener(() => {
                    Puzzle puzzle = SaveLoadHandler.LoadPuzzleJson(File.ReadAllText(filePath));
                    screenManager.SetScreenDown(designerScreen);
                    designHandler.Init(puzzle);
                });
                
                newPuzzleEntry.GetComponent<HoldableButton>().hold.AddListener(() => {
                    deleteDialog.Show();
                    deleteDialog.transform.Find("Panel/Delete Button").GetComponent<Button>().onClick.RemoveAllListeners();
                    
                    deleteDialog.transform.Find("Panel/Delete Button").GetComponent<Button>().onClick.AddListener(() => {
                        deleteDialog.Hide();
                    });
                    deleteDialog.transform.Find("Panel/Delete Button").GetComponent<Button>().onClick.AddListener(() => {
                        File.Delete(filePath);
                        Populate();
                    });
                });
                
                newPuzzleEntry.SetActive(true);
            }
        }
    }
}
