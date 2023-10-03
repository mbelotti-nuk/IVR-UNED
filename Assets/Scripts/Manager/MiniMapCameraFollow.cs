using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCameraFollow : MonoBehaviour
{
    [SerializeField] private MiniMapSettings settings;
    [SerializeField] private float cameraHeight;
    [SerializeField] private lightManager lightManager;
    // Start is called before the first frame update
    void Start()
    {
        settings = GetComponentInParent<MiniMapSettings>();
        cameraHeight = 15; //transform.position.y;
    }

    // Update is called once per frame    
    void Update()
    {
        Vector3 targetPosition = settings.targetToFollow.transform.position;
        float height = setHeight(targetPosition.y);


        transform.position =  new Vector3 (targetPosition.x, height+cameraHeight, targetPosition.z);
        if (settings.rotateWithTarget)
        {
            Quaternion targetRotation = settings.targetToFollow.rotation;
            transform.rotation = Quaternion.Euler(90, targetRotation.eulerAngles.y, 0);
        }
    }


    float setHeight(float y)
    {
        int i = lightManager.returnCase();
        float height = -1;
        switch (i)
        {
            case 0:
                height = -1;
                break;
            case 1:
                height = 4;
                break;
            case 2:
                height = -6;
                break;
            case 3:
                height = 10;
                break;
            case 4:
                height = -11;
                break;

        }

        return height;



    }

}
