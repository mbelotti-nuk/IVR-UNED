using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using TMPro;

public class Main : MonoBehaviour
{

    [SerializeField]
    private ParallelGeneratorStreamLines parallelStreamLines;
    public TMP_Dropdown dropdown;
    // Initialize meshtal
    List<VoxelStructure> Meshtallies = new List<VoxelStructure>();
    int indexMesh;
    //
    List<string> tallyType = new List<string> { "flux", "Dose" };
    List<string> tallyPrint = new List<string> { "  particles/cm²s", "  uSv/h" };


    // Player
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    public TextMeshProUGUI fluxPrint;
    double myFlux;


    #region rotation objects
    public TMP_InputField b1, b2, b3;
    #endregion


    // First load meshtal data
    void Awake()
    {
        indexMesh = 0;
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        string txtfile = "meshtal";
        TextAsset txtAassets = (TextAsset)Resources.Load(txtfile);

        string name = "Shutdown dose rate";
        dropdown.AddOptions(new List<string> { name + " - " + tallyType[1] });
        VoxelStructure Meshtal = new VoxelStructure(txtAassets, name, tallyPrint[1]);
        Meshtal.ClearVariables();
        Meshtallies.Add(Meshtal);

        Debug.Log($"Execution Time MAIN: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();

    }

    // Update is called once per frame
    void Update()
    {
        myFlux = (double)Meshtallies[indexMesh].GetFlux(Player.transform.GetChild(2).transform.position);
        fluxPrint.text = string.Format("{0:#.##E+00}", myFlux) + "\n" + Meshtallies[indexMesh].thisType;
    }


    void OnApplicationQuit()
    {
        Debug.Log("QUIT");
        for (int i = 0; i < Meshtallies.Count; i++)
        {
            Meshtallies[i].ClearValues();
        }
        parallelStreamLines.clear();
    }


    #region Public methods


    //void OnDrawGizmos()
    //{
    //    if (Application.isPlaying)
    //    {
    //        Meshtallies[indexMesh].DrawMeshtalBounds();
    //    }  
    //}

    public double getFlux
    { get { return myFlux; } }

    public void readMesh(string extension, string path, ref VoxelStructure Meshtal)
    {
        Meshtallies[Meshtallies.Count - 1].ClearVariables();
        Meshtal = new VoxelStructure();
        Meshtal.firstRead(path, extension);
    }

    public void newMesh(string extension, VoxelStructure Meshtal, int index, string name, int iType, float multiplicator)
    {
        Meshtal.voxelName = name;
        dropdown.AddOptions(new List<string> { name + " - " + tallyType[iType] });
        Meshtal.voxelType = tallyPrint[iType];
        Meshtal.secondRead(index, multiplicator, extension);
        Meshtal.SetValues();
        Meshtallies.Add(Meshtal);
        indexMesh = Meshtallies.IndexOf(Meshtal);
        parallelStreamLines.buildEnvironment();

    }

    public void newMesh(string extension, VoxelStructure Meshtal, int index, string name, int iType, float multiplicator, Quaternion rotationMatrix, Vector3 traslation)
    {
        Debug.Log("Rotated");
        Meshtal.voxelName = name;
        dropdown.AddOptions(new List<string> { name + " - " + tallyType[iType] });
        Meshtal.voxelType = tallyPrint[iType];
        Meshtal.secondRead(index, multiplicator, extension);
        Meshtal.setRotation(rotationMatrix, traslation);
        Meshtal.SetValues();
        Meshtallies.Add(Meshtal);
        indexMesh = Meshtallies.IndexOf(Meshtal);
        parallelStreamLines.buildEnvironment();

    }

    public float GetMaxFlux
    { get { return Meshtallies[indexMesh].GetMaxFlux; } }
    public VoxelStructure GetMeshtal
    { get { return Meshtallies[indexMesh]; } }

    public void changeMesh()
    {
        int index = dropdown.value;
        Debug.Log(index);
        indexMesh = index;
        parallelStreamLines.buildEnvironment();
    }

    public void changeMeshRotation()
    {
        // INVERT Y WITH Z AXIS 
        float alfa = float.Parse(b1.text, CultureInfo.InvariantCulture);
        float beta = float.Parse(b2.text, CultureInfo.InvariantCulture);
        float gamma = float.Parse(b3.text, CultureInfo.InvariantCulture);
        Meshtallies[indexMesh].setRotation(Quaternion.Euler(alfa, gamma, beta), new Vector3(0, 0, 0));
    }

    public void deleteMesh()
    {
        int index = dropdown.value;
        Debug.Log(index);
        if (Meshtallies.Count <= 1) { return; }
        if (indexMesh >= index) { indexMesh = indexMesh - 1; }
        Meshtallies.RemoveAt(index);
        dropdown.options.RemoveAt(index);
    }

    public List<string> getPrintTypes
    { get { return tallyPrint; } }



    #endregion



}