using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class textureMaker : MonoBehaviour
{

    public ComputeShader shader;
    public memoryDensityGenerator densityGenerator;
    int texResolution = 2048;
    public TMP_InputField userInputField;
    public TMP_InputField userInputField2;

    RenderTexture outputTexture;
    Renderer rend;
    int kernelHandle;
    public Material mat;
    public Main main;

    float level;
    float altitude;

    ComputeBuffer xbuffer;
    ComputeBuffer ybuffer;
    ComputeBuffer zbuffer;
    ComputeBuffer bufferFlux;

    ComputeBuffer pointsBuffer;
    ComputeBuffer vbuffer;

    List<GameObject> listOfPlanes = new List<GameObject>();

    // Use this for initialization

    public void makeTexture()
    {
        bool stop = false;
        setLevel(ref stop); if (stop) { return; }

        float x0 = main.GetMeshtal.GetXstep[0];
        float x1 = main.GetMeshtal.GetXstep[main.GetMeshtal.GetXstep.Count - 1];
        float z0 = main.GetMeshtal.GetZstep[0];
        float z1 = main.GetMeshtal.GetZstep[main.GetMeshtal.GetZstep.Count - 1];

        GameObject plane = createPlane(x0,x1,z0,z1);
        Material mat2 = new Material(mat);
        plane.GetComponent<MeshRenderer>().material = mat2;
        //plane.transform.Translate(new Vector3(-width/2, -1.2f, -height/2));

        rend = plane.GetComponent<Renderer>();
        rend.enabled = true;

        generatePointBuffer(main.GetMeshtal.GetCenters.Length);

        float[] logFlux = new float[main.GetMeshtal.GetArrayFlux.Length];
        makeLogFlux(ref logFlux);

        Debug.Log(logFlux[10].ToString());

        densityGenerator.Generate(ref pointsBuffer, ref vbuffer, main.GetMeshtal.GetXstep, main.GetMeshtal.GetYstep, main.GetMeshtal.GetZstep, logFlux);

        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        InitShader();

        mat2.mainTexture = outputTexture;
        plane.GetComponent<MeshRenderer>().material = mat2;
        plane.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        listOfPlanes.Add(plane);

    }

    public void deletePlanes()
    {
        foreach(GameObject g in listOfPlanes)
        {
            Destroy(g);
        }
    }

    private void InitShader()
    {
        int nVoxX = (main.GetMeshtal.GetXstep.Count - 1); // number of voxels on x axis
        int nVoxY = (main.GetMeshtal.GetYstep.Count - 1); // number of voxels on y axis
        int nVoxZ = (main.GetMeshtal.GetZstep.Count - 1); // number of voxels on z axis

        kernelHandle = shader.FindKernel("CSMain");

        shader.SetTexture(kernelHandle, "Result", outputTexture);

        int stride = 1 * 4; // 1 float (x, y or z) - 4 bytes per float
        xbuffer = new ComputeBuffer(main.GetMeshtal.GetXstep.ToArray().Length, stride);
        xbuffer.SetData(main.GetMeshtal.GetXstep.ToArray());
        shader.SetBuffer(0, "xdiv", xbuffer);

        ybuffer = new ComputeBuffer(main.GetMeshtal.GetYstep.ToArray().Length, stride);
        ybuffer.SetData(main.GetMeshtal.GetYstep.ToArray());
        shader.SetBuffer(0, "ydiv", ybuffer);

        zbuffer = new ComputeBuffer(main.GetMeshtal.GetZstep.ToArray().Length, stride);
        zbuffer.SetData(main.GetMeshtal.GetZstep.ToArray());
        shader.SetBuffer(0, "zdiv", zbuffer);

        //bufferFlux = new ComputeBuffer(main.GetMeshtal.GetArrayFlux.Length, stride);
        //bufferFlux.SetData(logFlux);
        //shader.SetBuffer(0, "Flux", bufferFlux);
        shader.SetBuffer(0, "Flux", vbuffer);

        shader.SetInt("nx", nVoxX);
        shader.SetInt("ny", nVoxY);
        shader.SetInt("nz", nVoxZ);

        shader.SetInt("idy", main.GetMeshtal.IndexOf(level, 2));

        shader.SetFloat("yLevel", level);

        shader.SetFloat("max", Mathf.Log10(main.GetMaxFlux));

        shader.SetInt("resolution", texResolution);

        DispatchShader(texResolution / 8, texResolution / 8);

        dispose();
    }

    private void DispatchShader(int x, int y)
    {
        shader.Dispatch(kernelHandle, x, y, 1);
    }

    private void dispose()
    {
        xbuffer.Dispose();
        ybuffer.Dispose();
        zbuffer.Dispose();
        vbuffer.Dispose();
        pointsBuffer.Dispose();
    }
    public GameObject createPlane(float x0, float x1, float z0, float z1)
    {
        GameObject plane = new GameObject("plane");
        MeshFilter mf = plane.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mr = plane.AddComponent(typeof (MeshRenderer)) as MeshRenderer;

        Mesh m = new Mesh();

        Vector3[] vertices = new Vector3[4]
{
            //new Vector3(0, 0, 0),
            //new Vector3(width, 0, 0),
            //new Vector3(0, 0,height),
            //new Vector3(width, 0, height)
            
            new Vector3(x0, this.altitude, z0),
            new Vector3(x1, this.altitude, z0),
            new Vector3(x0, this.altitude, z1),
            new Vector3(x1, this.altitude, z1)
};
        m.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        m.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
            //new Vector3(0,0,1),
            //new Vector3(0,0,1),
            //new Vector3(0,0,1),
            //new Vector3(0,0,1)
        };
        m.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        m.uv = uv;

        mf.mesh = m;
        m.RecalculateBounds();
        m.RecalculateNormals();

        return plane;   
    }

    private void setLevel(ref bool stop)
    {
        try
        {
            level = float.Parse(userInputField.text, CultureInfo.InvariantCulture);
            altitude = float.Parse(userInputField2.text, CultureInfo.InvariantCulture);
        }
        catch
        {
            stop = true;
        }
    }

    private void generatePointBuffer(int n)
    {
        int numVoxels = n;
        int numPoints = 8 * numVoxels;
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float)*3 );
        vbuffer = new ComputeBuffer(numPoints, sizeof (float) );

    }

    private void makeLogFlux(ref float[] logFlux)
    {
        for (int i = 0; i < logFlux.Length; i++)
        {
            float f = main.GetMeshtal.GetArrayFlux[i];
            if (f > 0)
            { logFlux[i] = Mathf.Log10(f); }
            else
            {
                logFlux[i] = 0;
            }
        }
    }
}
