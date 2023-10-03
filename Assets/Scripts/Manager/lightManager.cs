using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private GameObject Player;

    //[SerializeField]
    //private GameObject L1Level;
    //[SerializeField]
    //private GameObject L2Level;
    //[SerializeField]
    //private GameObject B1Level;
    //[SerializeField]
    //private GameObject B2Level;
    //[SerializeField]
    //private GameObject L3Level;

    int i;

    void Start()
    {
        i = setCase1(Player.transform.GetChild(2).transform.position.y);

    }

    // Update is called once per frame
    void Update()
    {
        i = setCase1(Player.transform.GetChild(2).transform.position.y);

    }

    public int returnCase()
    {
        return i;
    }

    int setCase1(float y)
    {
        int thisCase = 0;
        if (Player.transform.GetChild(2).transform.position.y >= -2 && Player.transform.GetChild(2).transform.position.y < 3)
        {
            thisCase = 0;
        }
        else if (Player.transform.GetChild(2).transform.position.y >= 3 && Player.transform.GetChild(2).transform.position.y < 8)
        {
            thisCase = 1;
        }
        else if (Player.transform.GetChild(2).transform.position.y >= -8 && Player.transform.GetChild(2).transform.position.y < -2)
        {
            thisCase = 2;
        }
        else if (Player.transform.GetChild(2).transform.position.y >= 8 && Player.transform.GetChild(2).transform.position.y < 16)
        {
            thisCase = 3;
        }
        else if (Player.transform.GetChild(2).transform.position.y >= -15 && Player.transform.GetChild(2).transform.position.y < -8)
        {
            thisCase = 4;
        }
        return thisCase;

    }

}
