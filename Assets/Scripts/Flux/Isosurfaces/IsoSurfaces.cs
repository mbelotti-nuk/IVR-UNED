using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class IsoSurfaces : MonoBehaviour
{
    [Header("General Settings")]
    public DensityGenerator densityGenerator;
    //
    [SerializeField]

    public Main main;
    VoxelStructure Meshtal = new VoxelStructure();
    public ComputeShader shader;

    [Header("Save Memory")]
    public memoryDensityGenerator memorydensityGenerator;
    public ComputeShader memoryShader;
    //public ComputeShader memoryShader2;


    [Header("Input")]
    public TMP_InputField userInputField;
    public setFluxText fluxSetter;
    public Toggle logScaleMehtod;
    public setMinColours minColoursSetter;
    public setMaxColours maxColoursSetter;
    public PauseManager pauseManager;
    bool logScale;


    [Header("Voxel Settings")]
    float isoLevel;
    const int threadGroupSize = 8;

    public List<Mesh> Isosurfaces = new List<Mesh>();

    // Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    public Material mat;

    public GameObject panel;
    public GameObject list1;
    List<GameObject> listOfSurfaces = new List<GameObject>();



    private void Update()
    {
        if (!pauseManager.isPaused())
        {
            if (Isosurfaces.Count < 1)
            {
                panel.SetActive(false);
            }
        }
        else if(pauseManager.isPaused() & !pauseManager.isOnPath())
        { reactivateList(); }
    }

    public void changeListSurfState()
    {
        if (Isosurfaces.Count > 0) { panel.SetActive(!panel.activeSelf); }

        foreach (GameObject g in listOfSurfaces)
        {
            g.SetActive(!g.activeSelf);
        }
    }

    public void reactivateList()
    {
        if(Isosurfaces.Count > 0 & !panel.activeSelf)
        {
            changeListSurfState();
        }
    }


    // Start is called before the first frame update
    public void MakeIsosurf()
    {
        bool direct = true;

        Debug.Log("Start");
        Debug.Log(System.DateTime.Now);

        logScale = logScaleMehtod.isOn;
        // GET COLORS
        Color minColor;
        Color maxColor;
        if (pauseManager.wasOptionsEnabled)
        {
            minColor = minColoursSetter.getColour();
            maxColor = maxColoursSetter.getColour();
        }
        else
        {
            minColor = Color.yellow;
            maxColor = Color.red;
        }


        bool stop = false;
        SetIsoLevel(ref stop); if (stop) { return; }

        Meshtal = main.GetMeshtal;


        List<float> xstep = Meshtal.GetXstep;
        List<float> ystep = Meshtal.GetYstep;
        List<float> zstep = Meshtal.GetZstep;
        //
        int nx = xstep.Count - 1;
        int ny = ystep.Count - 1;
        int nz = zstep.Count - 1;
        float[] flux = Meshtal.GetArrayFlux;


        Vector3[] vertices;
        int[] meshTriangles;

        IsoSurfaceConstructor isoMaker = new IsoSurfaceConstructor(densityGenerator, shader, memorydensityGenerator, memoryShader, xstep, ystep, zstep, flux);


        try
        {
            isoMaker.generateBuffers(nx * ny * nz);
        }
        catch
        {
            Debug.Log("TRY THIS");
            direct = false;
        }

        if (direct)
        {
            isoMaker.setBounds(nx, ny, nz);
            isoMaker.generateVertices(Meshtal, isoLevel);
        }
        else
        {
            isoMaker.setBounds(nx, ny, nz);
            isoMaker.memoryGenerateVertices(Meshtal, isoLevel);
        }

        vertices = isoMaker.thisVertices;
        meshTriangles = isoMaker.thisTriangles;



        // design mesh
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
       

        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        // Set mesh color

        float relFlux = SetScale(ref stop); if (stop) { return; }
        Color color = setColor(relFlux, minColor, maxColor);
        color.a = 0.5f;
        // mat.color = color;
        Material mat2 = new Material(mat);
        mat2.color = color;

        //
        gameObject.GetComponent<MeshRenderer>().material = mat2;
        gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        ReleaseBuffers();

        Isosurfaces.Add(mesh);

        listManager(color, isoLevel);

    }

    public void ClearIsoSurface()
    {
        ReleaseBuffers();
        panel.SetActive(false);
        foreach (Mesh mesh in Isosurfaces)
        {
            mesh.Clear();
        }
        Isosurfaces = new List<Mesh>();
        foreach (GameObject g in listOfSurfaces) { Destroy(g); }
        listOfSurfaces = new List<GameObject>();
    }

    void OnDestroy()
    {
        ReleaseBuffers();

    }

    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }


    void listManager(Color color, float level)
    {

        int counter = Isosurfaces.Count;

        float yPos = (counter - 1) * 50;

        GameObject go = new GameObject();
        go = Instantiate(list1, panel.transform, false);
        go.transform.localPosition = go.transform.localPosition - new Vector3(0, yPos, 0);

        TextMeshProUGUI txt1 = go.GetComponentInChildren<TextMeshProUGUI>();
        txt1.text = level.ToString() + main.GetMeshtal.thisType;
        Image img1 = go.GetComponentInChildren<Image>();
        img1.color = color;

        listOfSurfaces.Add(go);

    }

    void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            pointsBuffer.Release();
            triCountBuffer.Release();
        }
    }

    void SetIsoLevel(ref bool stop)
    {
        Debug.Log(userInputField.text);
        try
        {
            isoLevel = float.Parse(userInputField.text, CultureInfo.InvariantCulture);
        }
        catch
        {
            stop = true;
        }
    }

    float SetScale(ref bool stop)
    {
        float relFlux;
        float min;
        float max;
        if (pauseManager.wasOptionsEnabled)
        {
            min = fluxSetter.getMinFlux();
            max = fluxSetter.getMaxFlux();
        }
        else
        {
            min = 0;
            max = main.GetMaxFlux;
        }

        if (min > max) { stop = true; }
        if (isoLevel < min) { stop = true; }



        if (isoLevel > max)
        {
            relFlux = 1;
        }
        else
        {
            if (logScale)
            {
                try
                {
                    relFlux = (Mathf.Log10(isoLevel - min)) / (Mathf.Log10(max - min));
                }
                catch
                {
                    relFlux = (isoLevel - min) / (max - min);
                }
            }
            else { relFlux = (isoLevel - min) / (max - min); }
        }

        return relFlux;
    }

    public Color setColor(float f, Color min, Color max)
    {
        if (f > 1) { f = 1; }
        if (f < 0) { f = 0; }
        Debug.Log("Set color min " + min.ToString());
        Debug.Log("Set color max " + max.ToString());
        return Color.Lerp(min, max, f);
    }

}