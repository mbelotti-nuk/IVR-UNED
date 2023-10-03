using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace Geometry
{
    public class Path 
    {

        public string Name { get; set; }
        public GameObject Parent { get; set; }
        public List<inventoryItem> inventory { get; set; }


        public Path(string name, GameObject parent, List<inventoryItem> i)
        {
            this.Name = name;
            this.Parent = parent;   
            this.inventory = i;
        }
    }


    public abstract class maintenanceAction
    {



        public virtual Vector3 lastPos()
        {
            return Vector3.zero;
        }

        public virtual void destroyAction()
        {

        }

    }






public class Walk:maintenanceAction
    {


        public GameObject line { get; set; }


        public string name { get; set; }


        public Walk()
        {

        }

        void readWalkFile(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
        }

        public string ReturnString()
        {
            Vector3[] walkPath = new Vector3[line.GetComponent<LineRenderer>().positionCount];
            line.GetComponent<LineRenderer>().GetPositions(walkPath); // INSTANTIATE PATH TO FOLLOW
            return string.Format("{0,-35} {1,-35} {2,-35} {3,-35} {4,-35}", name, "Walk", walkPath[0].ToString("F0"), walkPath[walkPath.Length-1].ToString("F0"), "300");
        }

        public override Vector3 lastPos()
        {
            return line.GetComponent<LineRenderer>().GetPosition(line.GetComponent<LineRenderer>().positionCount - 1);
        }
        public override void destroyAction()
        {
            Object.Destroy(line);
        }

    }

    public class Mantain : maintenanceAction
    {

        public GameObject txt { get; set; }

        public string name { get; set; }

        public float time { get; set; }

        public Vector3 place { get; set; }

        public Mantain()
        {


        }


        public override Vector3 lastPos()
        {
            return place;
        }

        public string ReturnString()
        {
            return string.Format("{0,-35} {1,-35} {2,-35} {3,-35} {4,-35}", name, "Mantain", place.ToString("F0"), place.ToString("F0"), time.ToString("F0"));
        }
        public override void destroyAction()
        {
            Object.Destroy(txt);
        }

    }

    public class inventoryItem
    {
        public GameObject symbol;
        // public GameObject line;
        public maintenanceAction action;
    }


}
