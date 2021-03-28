using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public ModeHandler modeHandler;
    public DialogHandler colorPickerDialog;
    
    public void Click() {
        modeHandler.ColorSelectedVoxels(GetComponent<Image>().color);
    }
    
    public void Hold() {
        colorPickerDialog.Show();
        colorPickerDialog.GetComponent<ColorPicker>().SetColor(GetComponent<Image>().color);
        colorPickerDialog.transform.Find("Panel/Apply Button").GetComponent<Button>().onClick.RemoveAllListeners();
        colorPickerDialog.transform.Find("Panel/Apply Button").GetComponent<Button>().onClick.AddListener(() => {
            if (colorPickerDialog.GetComponent<ColorPicker>().color != null) {
                GetComponent<Image>().color = colorPickerDialog.GetComponent<ColorPicker>().color;
            }
        });
    }
}
