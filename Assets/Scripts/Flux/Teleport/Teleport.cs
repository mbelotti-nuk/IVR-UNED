using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class Teleport : MonoBehaviour
{
    [SerializeField]
   
    private GameObject Player;
    Vector3 accelerator = new Vector3(-20f, 0f, -6.3f);
    private CharacterController _controller;


    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void takeToAccelerator()
    { //Player.transform.position = L1Level;
        //_controller.Move(L1Level);
        _controller.enabled = false;
        transform.position = accelerator;
        _controller.enabled = true;
    }

}
