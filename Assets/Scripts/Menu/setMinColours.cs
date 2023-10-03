using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class setMinColours : MonoBehaviour
{
    [SerializeField] PauseManager pauseManager;
    Color MinColour;


    public void OnEnable()
    {

        var dropdown = transform.GetComponent<TMP_Dropdown>();

        if(!pauseManager.wasOptionsEnabled)
        {
            string textColour = dropdown.options[0].text; // Default Value
            MinColour = setColour(dropdown.options[0].text);
            print("MinColour setted to " + textColour);
        }

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
    }


    public Color getColour()
    {
        return MinColour;
    }

    protected Color setColour(string colorText)
    {
        Color returnColor = Color.yellow;
        if (colorText == "Yellow")
        { returnColor  = Color.yellow; }
        else if (colorText == "Green")
        { returnColor  = Color.green; }

        return returnColor; 
    }

    void DropdownItemSelected(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;
        MinColour = setColour(dropdown.options[index].text);
    }

    

}
