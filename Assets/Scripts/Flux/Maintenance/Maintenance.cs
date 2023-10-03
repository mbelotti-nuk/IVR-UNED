using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Maintenance : MonoBehaviour
{
    [SerializeField]
    private Main main;
    [SerializeField]
    public TextMeshProUGUI dosePrint;
    double intDose;
    public GameObject dosimeter;

    IEnumerator doseIntegration;

    public void Awake()
    {
        dosimeter.SetActive(false);
        doseIntegration = integratedDose();
    }

    public void startMaintain()
    {
        intDose = 0f;
        dosimeter.SetActive(true);
        //if( main.GetMeshtal.thisType == main.getPrintTypes[1])
        //{
        
            StartCoroutine(doseIntegration);
        //}

    }

    public void stopCounting()
    {
        
        StopCoroutine(doseIntegration);
    }


    public void resumeCounting()
    {
        StartCoroutine(doseIntegration);
    }

    public void deltaCounting(float delta)
    {
        intDose += main.getFlux * delta * (1 / 3600f);
    }


    public void endMaintain()
    {

        StopCoroutine(doseIntegration);
        intDose = 0f;
        dosimeter.SetActive(false);
    }
    
    public void resetDose()
    {
        intDose = 0f;
    }

    public void activateDosimeter()
    {
        dosimeter.SetActive(true);
    }

    public void Hide_Show_Dosimeter()
    {
        if (dosimeter.active) { dosimeter.SetActive(false); }
        else { dosimeter.SetActive(true); } 
      
    }

    public void exitMaintenance()
    {
        intDose = 0f;
        dosimeter.SetActive(true);
    }

    IEnumerator integratedDose()
    {
        while(true)
        {
            // conversion seconds --> hours
            intDose += main.getFlux * Time.deltaTime * (1 / 3600f);
            // Debug.Log(main.getFlux * Time.deltaTime *(1/3600f));
            dosePrint.text = string.Format("{0:#.##E+00}", intDose) + "\n" + "uSv";
            yield return null;
        }
    }
}
