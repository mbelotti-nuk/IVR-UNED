using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class setLoadingText : MonoBehaviour
{
    public TextMeshProUGUI load;
    // Start is called before the first frame update

    public void activateText()
    {
        load.SetText("LOADING..");

    }
}
