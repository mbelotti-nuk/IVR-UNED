using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class goTosetPath : MonoBehaviour
{
    [SerializeField]
    private  Camera playerCamera;
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private buildPath bp;
    [SerializeField]
    Maintenance maintenanceAction;

    public GameObject sphere;
    public List<GameObject> sphereList = new List<GameObject>();    
    // Bit shift the index of the layer (8) to get a bit mask
    int layerMask = 1 << 8;
    public Camera cam;

    [SerializeField]
    private GameObject pathSetter;
    
    float rayLength =20f;

    private void Update()
    {
        if(pathSetter.active) //if(PauseManager.onSetPath)
        {
            if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !bp.IscCoroutinRunning())
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(ray, out hit, rayLength, layerMask))
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.red);
                    Debug.Log(hit.point);
                    Debug.Log($" hit distance: {hit.distance}");

                    setPoints(hit);
                }
            }
        }
    }


    public void setPoints(RaycastHit hit)
    {
        GameObject newSphere = Instantiate(sphere, hit.point, Quaternion.identity);
        newSphere.SetActive(true);
        if(sphereList.Count > 1) {
            Destroy(sphereList[0]);
            sphereList[0] = sphereList[1];
            sphereList[1] = newSphere;
        }
        else { sphereList.Add(newSphere); }

    }

    public void changeCamera()
    {
        playerCamera.enabled = false;
        cam.enabled = true;
        cam.transform.position = Player.transform.position + new Vector3(0, 1, 0);
        //cam = Camera.main;   
    }


    public void exitMode()
    {
        maintenanceAction.endMaintain();
        foreach(GameObject s in sphereList) { Destroy(s); }
        cam.enabled = false;
        playerCamera.enabled = true;
        //playerCamera = Camera.main;
        
    }

}
