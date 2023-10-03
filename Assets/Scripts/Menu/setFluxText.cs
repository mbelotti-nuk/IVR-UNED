using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class setFluxText : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public TextMeshProUGUI fluxMaxPrint;
    public TextMeshProUGUI fluxMinPrint;
    public TMP_InputField maxFlux;
    public TMP_InputField minFlux;
    public Main main;

    string max;
    float maxValue;
    string min;
    float minValue;

   

    public void OnEnable()
    {
        fluxMaxPrint.text = string.Format("{0:#.##E+00}", main.GetMaxFlux);
        fluxMinPrint.text = "0";

        maxValue = main.GetMaxFlux;
        Debug.Log("Max value setted to: " + maxValue.ToString());
        max = string.Format("{0:#.##E+00}", maxValue);
        minValue = 0f;
        min = "0";

    }


    public void MinMaxSetter()
    {
        setMaxFlux(maxFlux);
        setMinFlux(minFlux);
        fluxMaxPrint.text = max;
        fluxMinPrint.text = min;
    }


    public float getMaxFlux()
    {
        return maxValue;

    }

    public float getMinFlux()
    {
        return minValue;
    }

    void setMaxFlux(TMP_InputField maxFlux)
    {
        if(maxFlux.text.Length>0)
        {
            try
            {
                maxValue = float.Parse(maxFlux.text, CultureInfo.InvariantCulture);
                max = string.Format("{0:#.##E+00}", maxValue);
            }
            catch
            {

            }
        }

    }

    void setMinFlux(TMP_InputField minFlux)
    {
        if(minFlux.text.Length>0)
        {
            try
            {
                minValue = float.Parse(minFlux.text, CultureInfo.InvariantCulture);
                min = string.Format("{0:#.##E+00}", minValue);
            }
            catch
            {

            }

        }

    }

}
