using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Geometry;

public class pathSetter : MonoBehaviour
{

    [SerializeField]
    private PathHolder pathHolder;
    //[SerializeField]
    //private TMP_InputField PathName;
    [SerializeField]
    private GameObject pathPrefab;
    [SerializeField]
    private GameObject Content;
    [SerializeField]
    private GameObject ScrollView;
    [SerializeField]
    private GameObject ViewPort;
    [SerializeField]
    private TMP_InputField timeMaintenanceInput;
    [SerializeField]
    private Material selectionMaterial;
    [SerializeField]
    private Material normalMaterial;
    [SerializeField]
    private GameObject maintenaceTXT;
    [SerializeField]
    private Camera cam;

    private List<inventoryItem> inventory = new List<inventoryItem> ();

    private GameObject Parent;
    private GameObject thisLine;
    private List<float[]> slots = new List<float[]>();
    private List<Vector3> positions = new List<Vector3>();

    private int selectionIndex;

    bool wasInstantiated = false;

    private drag Drag = new drag();

    public float Y_CURRENT;
    public float X_CURRENT;
    public float Y_SPACE_BETWEEN_ITEMS;
    int currentSibilIndex;
   


    private void OnEnable()
    {
        setVariables ();
    }

    public void deleteAll()
    {
        foreach (inventoryItem i in inventory) { Destroy(i.symbol); i.action.destroyAction(); }
        Destroy(Parent);
        setVariables();
    }


    void setVariables()
    {

        // Set parent to store variables
        Parent = new GameObject();
        Parent = Instantiate(Content, Content.transform.position,Quaternion.identity); Parent.name = "objectBin_" + pathHolder.pathCount().ToString(); Parent.SetActive(true);
        Parent.transform.SetParent(ViewPort.transform);

        // Put Parent inside visible window

        ScrollView.GetComponent<ScrollRect>().content = Parent.GetComponent<RectTransform>();

        currentSibilIndex = 0;
        thisLine = new GameObject();
        inventory = new List<inventoryItem>();
        slots = new List<float[]>();
        positions = new List<Vector3>();

        Y_CURRENT = -47.5f + 35;
        X_CURRENT = 0;
        Y_SPACE_BETWEEN_ITEMS = 35;

        
        GameObject types = new GameObject();
        for (int i = 0; i < Parent.transform.childCount; i++) { if (Parent.transform.GetChild(i).name == "txt_type") types = Parent.transform.GetChild(i).gameObject; }
        
        Debug.Log(types.name);

        types.SetActive(true);
        types.transform.SetSiblingIndex(currentSibilIndex); currentSibilIndex++;
        TextMeshProUGUI text = types.GetComponent<TextMeshProUGUI>();
        text.text = string.Format("{0,-35} {1,-35} {2,-35} {3,-35} {4,-35}", "Name", "Type", "Coord start", "Coord end", "Time");
    }
    
    GameObject getItem()
    {
        // SET OBJECT PROPERTIES
        var obj = Instantiate(pathPrefab, Vector3.zero, Quaternion.identity, transform);
        obj.name = (inventory.Count + 1 ).ToString();
        obj.SetActive(true);

        obj.transform.SetParent(Parent.transform);
        obj.transform.SetSiblingIndex(currentSibilIndex); currentSibilIndex ++;// inventory.Count


        // ADD EVENTS FOR CHANGING POSITION
        AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
        AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
        AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
        AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
        AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

        
        // obj.GetComponent<RectTransform>().localPosition = GetPosition();
        positions.Add(obj.GetComponent<RectTransform>().localPosition);
        Y_CURRENT -= Y_SPACE_BETWEEN_ITEMS;
        slots.Add(new float[] { Y_CURRENT + Y_SPACE_BETWEEN_ITEMS / 2, Y_CURRENT - Y_SPACE_BETWEEN_ITEMS / 2 });

        return obj;

    }

    
    public void savePath()
    {
        string name = "PATH "+ pathHolder.pathCount().ToString();
        //if(PathName.text.Length > 0) { name = PathName.text; }
        //else { name = "PATH"; }

        Parent.GetComponent<wasSaved>().isSaved = true;

        Path PATH = new Path(name, this.Parent, this.inventory);
        pathHolder.addPath(PATH);

        Parent.SetActive(false);
        
        // RESTART
        setVariables();
        
    }


    public void loadPath()
    {
        if (Parent.GetComponent<wasSaved>().isSaved == false) { deleteAll(); }
        Parent.SetActive(false);

        Path path = pathHolder.getSelectedPath();

        // SET PARENT OPTIONS
        Parent = path.Parent; Parent.SetActive(true);
        Parent.transform.SetParent(ViewPort.transform);

        // Put Parent inside visible window
        ScrollView.GetComponent<ScrollRect>().content = Parent.GetComponent<RectTransform>();



        // SET INVENTORY
        inventory = path.inventory;
        currentSibilIndex = inventory.Count + 1;
        positions = inventory.ConvertAll<Vector3>(obj => obj.symbol.GetComponent<RectTransform>().localPosition);

        slots = new List<float[]>();
        Y_CURRENT = -47.5f + 35;  
        for(int i=0; i<inventory.Count; i++)
        {
            Y_CURRENT -= Y_SPACE_BETWEEN_ITEMS;
            slots.Add(new float[] { Y_CURRENT + Y_SPACE_BETWEEN_ITEMS / 2, Y_CURRENT - Y_SPACE_BETWEEN_ITEMS / 2 });
        }

        thisLine = new GameObject();


    }


    public void setLine(GameObject line)
    {
        if(thisLine!= null && !wasInstantiated) { Debug.Log("DESTROY"); Destroy(thisLine); }
        thisLine = line;
        thisLine.SetActive(true);
        wasInstantiated = false;
    }

    public void addLine()
    {
        if (thisLine == null) return;
        wasInstantiated = true;


        GameObject obj = getItem();
        Walk walk = new Walk(); walk.name = "W";

        walk.line = thisLine;
        thisLine.transform.parent = obj.transform;
        obj.GetComponentInChildren<TextMeshProUGUI>().text  = walk.ReturnString();

        inventoryItem it = new inventoryItem(); it.symbol = obj; it.action = walk;
        inventory.Add(it);

        // thisLine = null;


    }


    public void addMaintenance()
    {
        if (thisLine != null && !wasInstantiated) { Debug.Log("DESTROY"); Destroy(thisLine); }
        wasInstantiated = true;

        if (timeMaintenanceInput.text == null) return;
        GameObject obj = getItem();
        Mantain m = new Mantain(); m.name = "M"; m.time = float.Parse(timeMaintenanceInput.text, CultureInfo.InvariantCulture);

        // GET POSITION MAINTENANCE
        Vector3 position;

        if (inventory.Count > 0)
        {
            position = inventory[inventory.Count - 1].action.lastPos();
        }
        else
        {
            Debug.Log("QUI");
            position = new Vector3(0,0,0);
        }

        Debug.Log(position);
        
        m.txt = Instantiate(maintenaceTXT, position, Quaternion.identity);
        m.txt.name = "Maintenance text";
        m.txt.SetActive(false);

        m.place=position;

        obj.GetComponentInChildren<TextMeshProUGUI>().text = m.ReturnString();

        inventoryItem it = new inventoryItem(); it.symbol = obj; it.action = m;

        inventory.Add(it);

    }


    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj)
    {
        selectionIndex = inventory.ConvertAll<GameObject>(inv => inv.symbol).IndexOf(obj);
        var v = inventory[selectionIndex].action as Walk;        
        if(v!= null) { v.line.GetComponent<LineRenderer>().material = selectionMaterial; return; }

        var m = inventory[selectionIndex].action as Mantain;
        if(m!= null) { m.txt.SetActive(true); m.txt.transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up); }
    }
    public void OnExit(GameObject obj)
    {
        var v = inventory[selectionIndex].action as Walk;
        if (v != null) { v.line.GetComponent<LineRenderer>().material = normalMaterial; return; }

        var m = inventory[selectionIndex].action as Mantain;
        if (m != null) { m.txt.SetActive(false); }
    }
    public void OnDragStart(GameObject obj)
    {
        //Drag = new drag();
        //Drag.dragCopy = Instantiate(obj);
        //Drag.dragCopy.transform.SetParent(obj.transform.parent);
        //Drag.indTarget = inventory.ConvertAll<GameObject>(inv => inv.symbol).IndexOf(obj); 

    }
    public void OnDragEnd(GameObject obj)
    {
        //Debug.Log($"ON DRAG END");

        //if(Drag.dragCopy != null)
        //{
        //    int index = finder(Drag.dragCopy.GetComponent<RectTransform>().localPosition.y);
        //    Debug.Log($"INDEX {index} with position {Drag.dragCopy.GetComponent<RectTransform>().localPosition.y}");
        //    if (index >= 0)
        //    {
        //        Debug.Log($" found at y {Drag.dragCopy.GetComponent<RectTransform>().localPosition.y} for slot {slots[index][0]} and {slots[index][1]}");
        //        swapInventory(inventory[index], inventory[Drag.indTarget]);
        //    }
        //}
        //else
        //{

        //}
        
        //Destroy(Drag.dragCopy);
        //Drag.dragCopy = null;
    }
    public void OnDrag(GameObject obj)
    {
        //Drag.dragCopy.GetComponent<RectTransform>().position = Input.mousePosition - new Vector3(X_CURRENT,0,0);
    }

    public Vector3 GetPosition()
    {
        return new Vector3(X_CURRENT, Y_CURRENT - Y_SPACE_BETWEEN_ITEMS, 0f);
        
    }


    void swapInventory(inventoryItem obj1, inventoryItem obj2)
    {
        int i1 = inventory.IndexOf(obj1);
        int i2 = inventory.IndexOf(obj2);
        Debug.Log($"swapping indexes {i1} and {i2}");
        inventory[i1] = obj2;
        inventory[i2] = obj1;

        // reset sibiling index

        for(int i=1; i<currentSibilIndex;i++)
        {
            //inventory[i].transform.SetSiblingIndex(i + 1);
            inventory[i].symbol.GetComponent<RectTransform>().localPosition = positions[i];
            inventory[i].symbol.transform.SetSiblingIndex(i);
        }
    }


    int finder(float y)
    {

        if(y > slots[0][0]) { return 0; }

        for(int i=0; i<slots.Count;i++)
        {
            
            if (y < slots[i][0] && y > slots[i][1])
            {
                return i;
            }
        }

        if(y < slots[slots.Count-1][1]) { return slots.Count-1; }
        else
        return -1;
    
    }
    

}

public class drag
{
    public GameObject dragCopy;
    public int indTarget;
}
