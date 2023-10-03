using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class visibilityController : MonoBehaviour
{
    public List<Material> invisibleMaterials;
    List<Material> myMaterials;
    Renderer[] rend;


    List<string> ListOfMaterials = new List<string>{
    "DEEPSKYBLUE3","Floor","GRAY","GRAY25","GRAY38",
     "GRAY49","GRAY50","GRAY62","GRAY64","GRAY88",
     "LIGHSLATERGRAY","MATRAGRAY","ROSYBROWN","SIENNA1"};
    // Start is called before the first frame update
    void Start()
    {
    
        rend = GetComponentsInChildren<Renderer>();
        myMaterials = new List<Material>();

        foreach (Renderer r in rend)
        {
            myMaterials.Add(r.material);
        }


    }


    public void changeVisibility(float alpha)
    {
        // SET VISIBILITY   
        for (int i = 0; i < invisibleMaterials.Count; i++)
        {
            Color thisColor = new Color(invisibleMaterials[i].color.r, invisibleMaterials[i].color.g, invisibleMaterials[i].color.b, alpha);
            invisibleMaterials[i].color = thisColor;
        }

        if(alpha == 1)
        {
            int count = 0;
            foreach(Renderer r in rend)
            {
                r.material = myMaterials[count];
                count++;
            }
        }
        else // SET INVISIBLE MATERIALS
        {
            foreach(Renderer r in rend)
            {
                foreach(string matName in ListOfMaterials)
                {
                    if (r.material.name.Contains(matName))
                    {
                        try
                        {
                            r.material = invisibleMaterials.Find(x => x.name.Contains(matName));
                            break;
                        }
                        catch
                        {

                        }

                    }
                }
            }
        }
        
    }
}
