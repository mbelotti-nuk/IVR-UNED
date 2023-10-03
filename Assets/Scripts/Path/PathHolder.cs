using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geometry;
using TMPro;

public class PathHolder : MonoBehaviour
{

    List<Path> paths = new List<Path>();

    [SerializeField]
    private TMP_Dropdown dropdown;

    private void Start()
    {
        //dropdown.onValueChanged.AddListener(delegate { dropdown.onValueChanged(dropdown);
        //});
    }


    public Path getSelectedPath()
    {
        int i = dropdown.value;
        return paths[i];
    }

    public void addPath(Path p)
    {
        paths.Add(p);
        addToDropdown(p.Name);
    }

    public int pathCount()
        { return paths.Count + 1; }

    void addToDropdown(string pathName)
    {
        dropdown.AddOptions(new List<string>() { pathName });
    }


}
