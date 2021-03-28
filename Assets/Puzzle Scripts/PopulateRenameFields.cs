using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateRenameFields : MonoBehaviour
{
    public DesignHandler designHandler;
    
    public void Populate() {
        transform.Find("Name Field").GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(designHandler.currentPuzzle.name);
        transform.Find("Author Field").GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(designHandler.currentPuzzle.author);
    }
    
    public void PopulateEmpty() {
        transform.Find("Name Field").GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify("");
        transform.Find("Author Field").GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify("");
    }
}
