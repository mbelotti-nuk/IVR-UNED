using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class setMaxColours : MonoBehaviour
{
    [SerializeField] PauseManager pauseManager;
    Color MaxColour;

    public void OnEnable()
    {
        
        var dropdown = transform.GetComponent<TMP_Dropdown>();
        if(!pauseManager.wasOptionsEnabled)
        {
            string textColour = dropdown.options[0].text; // Default Value
            MaxColour = setColour(dropdown.options[0].text);
            print("MaxColour setted to " + textColour);
        }
        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
    }

    public Color getColour()
    {
        return MaxColour;
    }

    protected Color setColour(string colorText)
    {
        Color returnColor = Color.yellow;
        if (colorText == "Red")
        { returnColor = Color.red; Debug.Log("ross"); }
        else if (colorText == "Blue")
        { returnColor = Color.blue; Debug.Log("blu"); }
        else { Debug.Log("Color not found"); }
        return returnColor;
    }

    void DropdownItemSelected(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;
        MaxColour = setColour(dropdown.options[index].text);
    }
}
