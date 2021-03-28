using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulatePreinstalledPuzzles : MonoBehaviour
{
    public GameObject puzzleEntryPrefab;
    
    public UIScreenManager screenManager;
    public RectTransform solutionScreen;
    
    public SolutionHandler solutionHandler;
    
    public void Populate(string resourcesPath)
    {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        TextAsset[] puzzleJsonFiles = Resources.LoadAll<TextAsset>(resourcesPath);
        for (int i = 0; i < puzzleJsonFiles.Length; i++) {
            Puzzle preInstalledPuzzle = SaveLoadHandler.LoadPuzzleJson(puzzleJsonFiles[i].text);
            
            GameObject newPuzzleEntry = (GameObject)Instantiate(puzzleEntryPrefab, transform);
            newPuzzleEntry.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().SetText(
                    String.Format(newPuzzleEntry.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text,
                        preInstalledPuzzle.name));
            newPuzzleEntry.transform.Find("Author").GetComponent<TMPro.TextMeshProUGUI>().SetText(
                    String.Format(newPuzzleEntry.transform.Find("Author").GetComponent<TMPro.TextMeshProUGUI>().text,
                        preInstalledPuzzle.author));
            
            DateTime puzzleTime = DateTime.FromBinary(preInstalledPuzzle.datetime);
            string timeString = puzzleTime.ToString("yyyy/MM/dd");
            newPuzzleEntry.transform.Find("Rating").GetComponent<TMPro.TextMeshProUGUI>().SetText(
                    timeString);
            
            newPuzzleEntry.GetComponent<Button>().onClick.AddListener(() => {
                if (GetComponentInParent<CanvasGroup>().interactable) {
                    screenManager.SetScreenDown(solutionScreen);
                    solutionHandler.Init(preInstalledPuzzle);
                }
            });
            
            newPuzzleEntry.SetActive(true);
        }
    }
}
