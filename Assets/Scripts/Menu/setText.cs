using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class setText : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI fluxPrint;
    public GameObject Player;
    void Update()
    {

        Vector3 position = Player.transform.GetChild(2).transform.position;
        fluxPrint.text = "x: " + ((int)position.x).ToString() + "  y: " + ((int)position.y).ToString()+ "  z: " + ((int)position.z).ToString();
    }

}
