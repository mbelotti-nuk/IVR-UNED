using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Globalization;
using TMPro;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using System.IO;


public class ParallelGeneratorStreamLines : MonoBehaviour
{
    public Main main;
    public SurfValueGenerator surfgenerator;
    public GradientDensityGenerator gradientdensitygenerator;
    public PauseManager pauseManager;
    VoxelStructure Meshtal = new VoxelStructure();
    ComputeBuffer surfbuffer;
    ComputeBuffer pointsBuffer;
    const int threadGroupSize = 8;
    protected Vector2[] SurfData; // SurfData.x = area of face ; SurfData.y = flux value at face
    protected Vector3[] Gradient;
    protected Vector3[] pointGradient;
    int nx;
    int ny;
    int nz;

    List<Vector3>[] streamLines;
    int nSeeds;
    bool rk45;
    bool rk4;

    [Header("Input")]

    public TMP_InputField XuserInputField;
    public TMP_InputField YuserInputField;
    public TMP_InputField ZuserInputField;

    public TMP_InputField deltaXInput;
    public TMP_InputField deltaYInput;
    public TMP_InputField deltaZInput;

    public TMP_InputField nSeeedsInput;

    public Toggle rk45mehtod;
    public Toggle rk4method;

    public setMinColours minColoursSetter;
    public setMaxColours maxColoursSetter;
    public setFluxText fluxSetter;
    public Toggle logScaleMehtod;
    bool logScale;

    [SerializeField]
    private GameObject Line;
    List<GameObject> lineHolder = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        buildEnvironment();
        
    }


    public void buildEnvironment()
    {
        Meshtal = main.GetMeshtal;;
        //List<Vector3> centers = Meshtal.GetCenters;
        Vector3[] centers = Meshtal.GetCenters;

        nx = Meshtal.GetXstep.Count - 1;
        ny = Meshtal.GetYstep.Count - 1;
        nz = Meshtal.GetZstep.Count - 1;


        // GENERATE GRADIENT DATA
        int numThreadsXAxis = Mathf.CeilToInt(nx / (float)threadGroupSize);
        int numThreadsYAxis = Mathf.CeilToInt(ny / (float)threadGroupSize);
        int numThreadsZAxis = Mathf.CeilToInt(nz / (float)threadGroupSize);

        CreateBuffers(nx, ny, nz);

        #region Generate Surface data
        SurfData = new Vector2[centers.Length * 6];

        List<float> xCenter = new List<float>();
        List<float> yCenter = new List<float>();
        List<float> zCenter = new List<float>();
        foreach (Vector3 v in centers)
        {
            xCenter.Add(v.x);
            yCenter.Add(v.y);
            zCenter.Add(v.z);
        }


        SurfData = surfgenerator.Generate(surfbuffer, Meshtal.GetXstep, Meshtal.GetYstep, Meshtal.GetZstep, xCenter, yCenter, zCenter, Meshtal.GetArrayFlux);
        surfbuffer.GetData(SurfData);

        #endregion

        #region Generate Gradient

        Gradient = new Vector3[Meshtal.GetArrayFlux.Length];

        GenerateGradient(SurfData, Meshtal.GetArrayFlux, Meshtal.GetXstep, Meshtal.GetYstep, Meshtal.GetZstep);
        SurfData = null; // free memory

        #endregion

        #region Expand gradient

        pointGradient = new Vector3[8 * Meshtal.GetArrayFlux.Length];
        gradientdensitygenerator.Generate(pointsBuffer, Meshtal.GetXstep, Meshtal.GetYstep, Meshtal.GetZstep, Gradient);
        Gradient = null; // free memory
        pointsBuffer.GetData(pointGradient);
        ReleaseBuffers();
        #endregion

        #region PROVA
        /// PROVA
        // *********************************************************************************************************
        //CartesianGradient(Meshtal.GetArrayFlux, Meshtal.GetXstep, Meshtal.GetYstep, Meshtal.GetZstep);

        //makeDataPoint(centers);

        // *********************************************************************************************************
        #endregion

        normalizePointGrad();
        ///


    }



    public void makeDataPoint(Vector3[] centers)
    {

        for(int ind=0; ind<Meshtal.GetArrayFlux.Length;ind++)
        {
            Vector3 centerMesh = centers[ind];
            int i = Meshtal.IndexOf(centerMesh.x, 1);
            int j = Meshtal.IndexOf(centerMesh.y, 2);
            int k = Meshtal.IndexOf(centerMesh.z, 3);

            if( i == 0 || j == 0 || k == 0 || i == nx-1 || j == ny-1|| k == nz-1)
            {
                for (int vert = 0; vert < 8; vert++)
                {
                    pointGradient[GetTotalIndex(ind, vert)] = new Vector3(0,0,0);
                }
            }
            else
            {
                Calculate(ind, centers, i, j, k);
            }

        }


    }

    
    void Calculate(int ind, Vector3[] centers, int i, int j, int k)
    {
        for (int vert = 0; vert < 8; vert++)
        {
            Vector3 vertex = new Vector3();
            Vector3Int[] neighbours = new Vector3Int[8];
            float[] weights = new float[8];

            switch (vert)
            {
                case 0:
                    vertex = new Vector3(Meshtal.GetXstep[i + 1], Meshtal.GetYstep[j], Meshtal.GetZstep[k]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i - 1, j, k); neighbours[2] = new Vector3Int(i, j, k + 1);
                    neighbours[3] = new Vector3Int(i - 1, j, k + 1); neighbours[4] = new Vector3Int(i, j - 1, k); neighbours[5] = new Vector3Int(i - 1, j - 1, k);
                    neighbours[6] = new Vector3Int(i, j - 1, k + 1); neighbours[7] = new Vector3Int(i - 1, j - 1, k + 1);
                    break;
                case 1:
                    vertex = new Vector3(Meshtal.GetXstep[i + 1], Meshtal.GetYstep[j], Meshtal.GetZstep[k + 1]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i + 1, j, k); neighbours[2] = new Vector3Int(i, j, k + 1);
                    neighbours[3] = new Vector3Int(i + 1, j, k + 1); neighbours[4] = new Vector3Int(i, j - 1, k); neighbours[5] = new Vector3Int(i + 1, j - 1, k);
                    neighbours[6] = new Vector3Int(i, j - 1, k + 1); neighbours[7] = new Vector3Int(i + 1, j - 1, k + 1);
                    break;

                case 2:
                    vertex = new Vector3(Meshtal.GetXstep[i], Meshtal.GetYstep[j], Meshtal.GetZstep[k + 1]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i + 1, j, k); neighbours[2] = new Vector3Int(i, j, k - 1);
                    neighbours[3] = new Vector3Int(i + 1, j, k - 1); neighbours[4] = new Vector3Int(i, j - 1, k); neighbours[5] = new Vector3Int(i + 1, j - 1, k);
                    neighbours[6] = new Vector3Int(i, j - 1, k - 1); neighbours[7] = new Vector3Int(i + 1, j - 1, k - 1);
                    break;
                case 3:
                    vertex = new Vector3(Meshtal.GetXstep[i], Meshtal.GetYstep[j], Meshtal.GetZstep[k]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i - 1, j, k); neighbours[2] = new Vector3Int(i, j, k - 1);
                    neighbours[3] = new Vector3Int(i - 1, j, k - 1); neighbours[4] = new Vector3Int(i, j - 1, k); neighbours[5] = new Vector3Int(i - 1, j - 1, k);
                    neighbours[6] = new Vector3Int(i, j - 1, k - 1); neighbours[7] = new Vector3Int(i - 1, j - 1, k - 1);
                    break;
                case 4:
                    vertex = new Vector3(Meshtal.GetXstep[i + 1], Meshtal.GetYstep[j + 1], Meshtal.GetZstep[k]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i - 1, j, k); neighbours[2] = new Vector3Int(i, j, k + 1);
                    neighbours[3] = new Vector3Int(i - 1, j, k + 1); neighbours[4] = new Vector3Int(i, j + 1, k); neighbours[5] = new Vector3Int(i - 1, j + 1, k);
                    neighbours[6] = new Vector3Int(i, j + 1, k + 1); neighbours[7] = new Vector3Int(i - 1, j + 1, k + 1);
                    break;
                case 5:
                    vertex = new Vector3(Meshtal.GetXstep[i + 1], Meshtal.GetYstep[j + 1], Meshtal.GetZstep[k + 1]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i + 1, j, k); neighbours[2] = new Vector3Int(i, j, k + 1);
                    neighbours[3] = new Vector3Int(i + 1, j, k + 1); neighbours[4] = new Vector3Int(i, j + 1, k); neighbours[5] = new Vector3Int(i + 1, j + 1, k);
                    neighbours[6] = new Vector3Int(i, j + 1, k + 1); neighbours[7] = new Vector3Int(i + 1, j + 1, k + 1);
                    break;
                case 6:
                    vertex = new Vector3(Meshtal.GetXstep[i], Meshtal.GetYstep[j + 1], Meshtal.GetZstep[k + 1]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i + 1, j, k); neighbours[2] = new Vector3Int(i, j, k - 1);
                    neighbours[3] = new Vector3Int(i + 1, j, k - 1); neighbours[4] = new Vector3Int(i, j + 1, k); neighbours[5] = new Vector3Int(i + 1, j + 1, k);
                    neighbours[6] = new Vector3Int(i, j + 1, k - 1); neighbours[7] = new Vector3Int(i + 1, j + 1, k - 1);
                    break;
                case 7:
                    vertex = new Vector3(Meshtal.GetXstep[i + 1], Meshtal.GetYstep[j + 1], Meshtal.GetZstep[k + 1]);

                    neighbours[0] = new Vector3Int(i, j, k); neighbours[1] = new Vector3Int(i - 1, j, k); neighbours[2] = new Vector3Int(i, j, k - 1);
                    neighbours[3] = new Vector3Int(i - 1, j, k - 1); neighbours[4] = new Vector3Int(i, j + 1, k); neighbours[5] = new Vector3Int(i - 1, j + 1, k);
                    neighbours[6] = new Vector3Int(i, j + 1, k - 1); neighbours[7] = new Vector3Int(i - 1, j + 1, k - 1);
                    break;
            }

            float sumDist = 0;
            foreach (Vector3Int neighbour in neighbours)
            {
                int neighIndex = voxelIndex(neighbour.x, neighbour.y, neighbour.z);
                float dist = distance(vertex, centers[neighIndex]);
                pointGradient[GetTotalIndex(ind, vert)] += Gradient[neighIndex] * dist;
                sumDist += dist;
            }

            pointGradient[GetTotalIndex(ind, vert)] = pointGradient[GetTotalIndex(ind, vert)] / sumDist;

        }
    }


    float distance(Vector3 p1, Vector3 p2)
    {
        return (float)Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2) + Math.Pow(p1.z - p2.z, 2));
    }
    public void normalizePointGrad()
    {
        float[] norm = new float[Meshtal.GetArrayFlux.Length];

        for(int i = 0; i < Meshtal.GetArrayFlux.Length; i++)
        {
            Vector3[] voxelGrad = new Vector3[8];
            float sumModulesVelocities = 0;
            gradientInVoxel(ref voxelGrad, pointGradient, i, ref sumModulesVelocities);
            float[] MagVox = new float[8];
            for (int j = 0; j < 8; j++) { MagVox[j] = voxelGrad[j].magnitude; }
            norm[i] = MagVox.Max();
        }


        for (int i = 0; i < Meshtal.GetArrayFlux.Length; i++)
        {
            pointGradient[GetTotalIndex(i, 0)] = pointGradient[GetTotalIndex(i, 0)] / norm[i];

            pointGradient[GetTotalIndex(i, 1)] = pointGradient[GetTotalIndex(i, 1)] / norm[i];

            pointGradient[GetTotalIndex(i, 2)] = pointGradient[GetTotalIndex(i, 2)] / norm[i];

            pointGradient[GetTotalIndex(i, 3)] = pointGradient[GetTotalIndex(i, 3)] / norm[i];

            pointGradient[GetTotalIndex(i, 4)] = pointGradient[GetTotalIndex(i, 4)] / norm[i];

            pointGradient[GetTotalIndex(i, 5)] = pointGradient[GetTotalIndex(i, 5)] / norm[i];

            pointGradient[GetTotalIndex(i, 6)] = pointGradient[GetTotalIndex(i, 6)] / norm[i];

            pointGradient[GetTotalIndex(i, 7)] = pointGradient[GetTotalIndex(i, 7)] / norm[i];

        }




        }

    public void parallelStreamLines()
    {

        logScale = logScaleMehtod.isOn;

        #region Initialize Data
        float xStart, yStart, zStart;
        float deltaX, deltaY, deltaZ;

        try
        {
            xStart = float.Parse(XuserInputField.text, CultureInfo.InvariantCulture);
            yStart = float.Parse(YuserInputField.text, CultureInfo.InvariantCulture);
            zStart = float.Parse(ZuserInputField.text, CultureInfo.InvariantCulture);

            deltaX = float.Parse(deltaXInput.text, CultureInfo.InvariantCulture);
            deltaY = float.Parse(deltaYInput.text, CultureInfo.InvariantCulture);
            deltaZ = float.Parse(deltaZInput.text, CultureInfo.InvariantCulture);

            nSeeds = int.Parse(nSeeedsInput.text, CultureInfo.InvariantCulture);
        }
        catch
        {
            return;
        }


        if(nSeeds <= 0) { return; }

        rk45 = rk45mehtod.isOn;
        rk4 = rk4method.isOn;
        if(!rk45 & !rk4) { return; }


        Vector3[] positions = new Vector3[nSeeds];
        initializeData(positions, xStart, yStart, zStart, deltaX, deltaY, deltaZ, nSeeds);

        #endregion


        //List<Vector3>[] streamLines = new List<Vector3>[nSeeds];
        streamLines = new List<Vector3>[nSeeds];

        GameObject[] newLine = new GameObject[nSeeds];
        LineRenderer[] lRend = new LineRenderer[nSeeds];


        Parallel.For(0, nSeeds, number => {
            if (rk45) { streamLines[number] = RungeKuttaCashKarp(positions[number].x, positions[number].y, positions[number].z); }
            else { streamLines[number] = RK4(positions[number].x, positions[number].y, positions[number].z); }
        });

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


        for (int i = 0; i < nSeeds; i++)
        {
            newLine[i] = Instantiate(Line);
            lRend[i] = newLine[i].GetComponent<LineRenderer>();
            lRend[i].positionCount = streamLines[i].Count;
            lRend[i].material = new Material(Shader.Find("Sprites/Default"));
            int number = 0; // point counter for line renderer

            if(Meshtal.isRotated)
            {
                foreach (Vector3 p in streamLines[i])
                {  //foreach (Vector3 p in points) foreach (Vector3 p in bezierPoints)
                    lRend[i].SetPosition(number, Meshtal.traslation + Meshtal.rotationMatrix*p);
                    number++;
                }

            }
            else
            {
                foreach (Vector3 p in streamLines[i])
                {  //foreach (Vector3 p in points) foreach (Vector3 p in bezierPoints)
                    lRend[i].SetPosition(number, p);
                    number++;
                }

            }

            #region STREAMLINE COLOUR

            //float alpha = 1;
            //Gradient gradient = new Gradient();
            //gradient.SetKeys(
            //    new GradientColorKey[] { new GradientColorKey(minColor, 0.0f), new GradientColorKey(maxColor, 1.0f) },
            //    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            //);

            Gradient gradient = setColor(minColor, maxColor, i);

            lRend[i].colorGradient = gradient;
            #endregion
            lineHolder.Add(newLine[i]);
        }
    }

    public void Continue()
    {
        bool outBounds = false;
        if(streamLines.Length == 0) { return; }
        if (nSeeds <= 0) { return; }
        Vector3[] positions = new Vector3[streamLines.Length]; 
        for(int i=0;i<streamLines.Length;i++) { 
            // Get position
            positions[i] = streamLines[i][streamLines[i].Count - 1];
            Debug.Log(positions[i]);
            // Get perturbation
            Vector3 perturbation = getPerturbation(positions[i], ref outBounds);
            if (outBounds) { return; }
            // perturbate initial position
            positions[i].x = positions[i].x + UnityEngine.Random.Range(0.1f, 0.3f) * perturbation.x; 
            positions[i].y = positions[i].y + UnityEngine.Random.Range(0.1f, 0.3f) * perturbation.y; 
            positions[i].z = positions[i].z + UnityEngine.Random.Range(0.1f, 0.3f) * perturbation.z;
        }

        List<Vector3>[] continueStreamLines = new List<Vector3>[nSeeds];

        Parallel.For(0, nSeeds, number => {
            if (rk45) { continueStreamLines[number] = RungeKuttaCashKarp(positions[number].x, positions[number].y, positions[number].z); }
            else { continueStreamLines[number] = RK4(positions[number].x, positions[number].y, positions[number].z); }
        });

        for (int i = 0; i < nSeeds; i++)
        {
            for (int j = 0; j < continueStreamLines[i].Count - 1; j++)
            {
                streamLines[i].Add(continueStreamLines[i][j]);
            }
        }
        GameObject[] newLine = new GameObject[nSeeds];
        LineRenderer[] lRend = new LineRenderer[nSeeds];


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


        for (int i = 0; i < nSeeds; i++)
        {
            newLine[i] = Instantiate(Line);
            lRend[i] = newLine[i].GetComponent<LineRenderer>();
            lRend[i].positionCount = continueStreamLines[i].Count;
            lRend[i].material = new Material(Shader.Find("Sprites/Default"));
            int number = 0; // point counter for line renderer
            foreach (Vector3 p in continueStreamLines[i])
            {  //foreach (Vector3 p in points) foreach (Vector3 p in bezierPoints)
                lRend[i].SetPosition(number, p);
                number++;
            }

            #region STREAMLINE COLOUR
            //float alpha = 1;
            //Gradient gradient = new Gradient();
            //gradient.SetKeys(
            //    new GradientColorKey[] { new GradientColorKey(minColor, 0.0f), new GradientColorKey(maxColor, 1.0f) },
            //    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            //);

            Gradient gradient = setColor(minColor, maxColor, i);
            lRend[i].colorGradient = gradient;
            #endregion
            lineHolder.Add(newLine[i]);
        }




    }

    protected Vector3 getPerturbation(Vector3 position, ref bool outBounds)
    {
        int i = Meshtal.IndexOf(position.x, 1);
        int j = Meshtal.IndexOf(position.y, 2);
        int k = Meshtal.IndexOf(position.z, 3);
        if (i == -1 || j == -1 || k == -1)
        {
            outBounds = true;
        }
        float[] bounds = voxelBounds(Meshtal.GetXstep, Meshtal.GetYstep, Meshtal.GetZstep, i, j, k);

        return new Vector3(Mathf.Abs(bounds[1] - bounds[0]), Mathf.Abs(bounds[3] - bounds[2]), Mathf.Abs(bounds[5] - bounds[4]));
    }

    protected void initializeData(Vector3[] positions, float xStart, float yStart, float zStart, float deltaX, float deltaY, float deltaZ, int nSeeds)
    {

        positions[0] = new Vector3(xStart, yStart, zStart);

        for(int i = 1; i < nSeeds;  i++)
        {
            positions[i] = new Vector3(UnityEngine.Random.Range(xStart -deltaX/2, xStart + deltaX / 2), UnityEngine.Random.Range(yStart - deltaY / 2, yStart + deltaY / 2), UnityEngine.Random.Range(zStart - deltaZ / 2, zStart + deltaZ / 2));
        }

    }

    public void DeleteStreamLines()
    {
        foreach (GameObject line in lineHolder)
        {
            Destroy(line);
        }
    }


    public Gradient setColor(Color minColor, Color maxColor, int index)
    {
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

        Color beginColor = Color.Lerp(minColor, maxColor, setScale(Meshtal.GetFlux(streamLines[index][0]), min, max));
        Color endColor = Color.Lerp(minColor, maxColor, setScale(Meshtal.GetFlux(streamLines[index][streamLines[index].Count-1]), min, max));

        float alpha = 1;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(beginColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        return gradient;
    }

    public float setScale(float value, float min, float max)
    {
        float relValue;

        if (min > max) { min = 0; max = main.GetMaxFlux; }
        if (value < min) { min = 0; }

        if(value > max)
        {
            relValue = 1;
        }
        else
        {
            if (logScale)
            {
                try
                {
                    relValue = (Mathf.Log10(value - min)) / (Mathf.Log10(max - min));
                }
                catch
                {
                    relValue = (value - min) / (max - min);
                }

            }
            else { relValue = (value - min) / (max - min); }
        }


        return relValue;
    }

    public List<Vector3> RungeKuttaCashKarp(float xStart, float yStart, float zStart)
    {
        List<float> xstep = Meshtal.GetXstep;
        List<float> ystep = Meshtal.GetYstep;
        List<float> zstep = Meshtal.GetZstep;
        float[] flux = Meshtal.GetArrayFlux;
        int nx = xstep.Count - 1;
        int ny = ystep.Count - 1;
        int nz = zstep.Count - 1;


        #region STREAM LINE GENERATION
        // START
        int i = Meshtal.IndexOf(xStart, 1);
        int j = Meshtal.IndexOf(yStart, 2);
        int k = Meshtal.IndexOf(zStart, 3);


        int indexVox = voxelIndex(i, j, k);
        float[] bounds;
        bounds = voxelBounds(xstep, ystep, zstep, i, j, k);
        Vector3[] voxelGrad = null;

        float sumModulesVelocities = 0f;
        gradientInVoxel(ref voxelGrad, pointGradient, indexVox, ref sumModulesVelocities);

        float h = 0.1f;
        // *********************************************** //


        // INITIALIZE
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(xStart, yStart, zStart)); // add first point
        Vector3 lastPoint = new Vector3(xStart, yStart, zStart);

        // *********************************************** //

        int count = 1;
        int maxCount = 30000;
        int upperLimitCount = 80000;
        bool outBounds = false;

        float length = 0;
        float maxLenght = 81;
        float minLength = 8;
        bool stop = false;
        while ((!outBounds) && (length < maxLenght))
        {
            if (count > maxCount && length > minLength)
            {
                break;
            }
            if (count > upperLimitCount)
            {
                break;
            }


            Vector3 point = new Vector3();

            CashKarpRungeKutta(ref point, ref h, ref stop, lastPoint, voxelGrad, bounds);

            // update List
            if (stop)
            {
                break;
            }
            points.Add(point);
            count++;

            length += new Vector3(point.x - lastPoint.x, point.y - lastPoint.y, point.z - lastPoint.z).magnitude;

            lastPoint = point;

            #region ESTABLISH NEW POINT LOCATION
            int ip = i;
            int jp = j;
            int kp = k;


            i = Meshtal.IndexOf(point.x, 1);
            j = Meshtal.IndexOf(point.y, 2);
            k = Meshtal.IndexOf(point.z, 3);
            if (i == -1 || j == -1 || k == -1)
            {
                outBounds = true;
                break;
            }
            else if ((i != ip || j != jp || k != kp) & !outBounds)
            {
                indexVox = voxelIndex(i, j, k);
                gradientInVoxel(ref voxelGrad, pointGradient, indexVox, ref sumModulesVelocities);
                bounds = voxelBounds(xstep, ystep, zstep, i, j, k);
            }

            #endregion

        }
        #endregion



        return points;

    }

    public List<Vector3> RK4(float xStart, float yStart, float zStart)
    {
        List<float> xstep = Meshtal.GetXstep;
        List<float> ystep = Meshtal.GetYstep;
        List<float> zstep = Meshtal.GetZstep;
        float[] flux = Meshtal.GetArrayFlux;
        int nx = xstep.Count - 1;
        int ny = ystep.Count - 1;
        int nz = zstep.Count - 1;

        #region STREAM LINE GENERATION
        // START
        int i = Meshtal.IndexOf(xStart, 1);
        int j = Meshtal.IndexOf(yStart, 2);
        int k = Meshtal.IndexOf(zStart, 3);


        int indexVox = voxelIndex(i, j, k);
        float[] bounds;
        bounds = voxelBounds(xstep, ystep, zstep, i, j, k);
        Vector3[] voxelGrad = null;
        int NstepInVoxel = 3;
        float sumModulesVelocities = 0f;
        gradientInVoxel(ref voxelGrad, pointGradient, indexVox, ref sumModulesVelocities);
        float vAverage = sumModulesVelocities / 8;
        float dt = 1 / (vAverage * NstepInVoxel); // Runge Kutta step
        // *********************************************** //


        // INITIALIZE
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(xStart, yStart, zStart)); // add first point
        Vector3 lastPoint = new Vector3(xStart, yStart, zStart);

        // *********************************************** //

        int count = 1;
        float maxCount = 30000;
        bool outBounds = false;
        float length = 0;
        float maxLenght = 15;
        while ((!outBounds) && (length < maxLenght))
        {
            if (count > maxCount)
            {
                break;
            }

            Vector3 point = RungeKutta4(lastPoint, voxelGrad, bounds, dt);

            // update List
            points.Add(point);
            count++;

            length += new Vector3(point.x - lastPoint.x, point.y - lastPoint.y, point.z - lastPoint.z).magnitude;

            lastPoint = point;

            #region ESTABLISH NEW POINT LOCATION
            int ip = i;
            int jp = j;
            int kp = k;
            i = Meshtal.IndexOf(point.x, 1);
            j = Meshtal.IndexOf(point.y, 2);
            k = Meshtal.IndexOf(point.z, 3);
            if (i == -1 || j == -1 || k == -1)
            {
                outBounds = true;
                break;
            }
            else if ((i != ip || j != jp || k != kp) & !outBounds)
            {
                // currentStruct = new Vector3Int(i, j, k);
                indexVox = voxelIndex(i, j, k);
                gradientInVoxel(ref voxelGrad, pointGradient, indexVox, ref vAverage);
                dt = 1 / (vAverage * NstepInVoxel); // Update Runge Kutta step
                bounds = voxelBounds(xstep, ystep, zstep, i, j, k);
            }
            #endregion

        }
        #endregion

        return points;

    }

    protected Vector3 RungeKutta4(Vector3 point, Vector3[] fluxVoxGradient, float[] bounds, float dt)
    {
        Vector3 RK1 = getVelocity(point, fluxVoxGradient, bounds);
        Vector3 RK2 = getVelocity(point + (RK1 * dt / 2), fluxVoxGradient, bounds);
        Vector3 RK3 = getVelocity(point + (RK2 * dt / 2), fluxVoxGradient, bounds);
        Vector3 RK4 = getVelocity(point + (RK3 * dt), fluxVoxGradient, bounds);


        // Runge Kutta 4
        Vector3 output = point + ((RK1 + 2 * RK2 + 2 * RK3 + RK4) * dt) / 6;

        // Runge Kutta 2
        // Vector3 output = point + RK2*dt;

        return output;
    }

    protected int voxelIndex(int x, int y, int z)
    {
        return x * (nz * ny) + z * (ny) + y;
    }

    protected int GetTotalIndex(int indVoxel, int localIndex)
    {
        return 8 * indVoxel + localIndex;
    }
    protected float[] voxelBounds(List<float> xstep, List<float> ystep, List<float> zstep, int i, int j, int k)
    {
        float[] bounds = new float[6];
        bounds[0] = xstep[i];        // x0
        bounds[1] = xstep[i + 1];    // x1
        bounds[2] = ystep[j];        // y0
        bounds[3] = ystep[j + 1];    // y1
        bounds[4] = zstep[k];        // z0
        bounds[5] = zstep[k + 1];    // z1
        return bounds;

    }
    protected void gradientInVoxel(ref Vector3[] voxelGrad, Vector3[] fluxGradient, int indexVox, ref float sumModulesVelocities)
    {
        voxelGrad = new Vector3[8];
        // float sumModulesVelocities = 0;
        voxelGrad[0] = fluxGradient[GetTotalIndex(indexVox, 0)]; // f0
        sumModulesVelocities += voxelGrad[0].magnitude;
        voxelGrad[1] = fluxGradient[GetTotalIndex(indexVox, 1)]; // f1
        sumModulesVelocities += voxelGrad[1].magnitude;
        voxelGrad[2] = fluxGradient[GetTotalIndex(indexVox, 2)]; // f2
        sumModulesVelocities += voxelGrad[2].magnitude;
        voxelGrad[3] = fluxGradient[GetTotalIndex(indexVox, 3)]; // f3
        sumModulesVelocities += voxelGrad[3].magnitude;
        voxelGrad[4] = fluxGradient[GetTotalIndex(indexVox, 4)]; // f4
        sumModulesVelocities += voxelGrad[4].magnitude;
        voxelGrad[5] = fluxGradient[GetTotalIndex(indexVox, 5)]; // f5
        sumModulesVelocities += voxelGrad[5].magnitude;
        voxelGrad[6] = fluxGradient[GetTotalIndex(indexVox, 6)]; // f6
        sumModulesVelocities += voxelGrad[6].magnitude;
        voxelGrad[7] = fluxGradient[GetTotalIndex(indexVox, 7)]; // f7
        sumModulesVelocities += voxelGrad[7].magnitude;
        // vAverage = (1 / 8) * sumModulesVelocities;

    }

    protected int surfTotalIndex(int nvoxel, int localIndex)
    {
        return 6 * nvoxel + localIndex;
    }
    
    void GenerateGradient(Vector2[] SurfData, float[] flux, List<float> xstep, List<float> ystep, List<float> zstep)
    {
        Vector3Int structure = new Vector3Int(0, 0, 0);
        float volume;
        for (int voxIndex = 0; voxIndex < flux.Length; voxIndex++)
        {
            volume = getVolume(structure, xstep, ystep, zstep);
            Gradient[voxIndex].x = (1 / volume) * (SurfData[surfTotalIndex(voxIndex, 3)].x * SurfData[surfTotalIndex(voxIndex, 3)].y - SurfData[surfTotalIndex(voxIndex, 2)].x * SurfData[surfTotalIndex(voxIndex, 2)].y);
            Gradient[voxIndex].y = (1 / volume) * (SurfData[surfTotalIndex(voxIndex, 1)].x * SurfData[surfTotalIndex(voxIndex, 1)].y - SurfData[surfTotalIndex(voxIndex, 0)].x * SurfData[surfTotalIndex(voxIndex, 0)].y);
            Gradient[voxIndex].z = (1 / volume) * (SurfData[surfTotalIndex(voxIndex, 5)].x * SurfData[surfTotalIndex(voxIndex, 5)].y - SurfData[surfTotalIndex(voxIndex, 4)].x * SurfData[surfTotalIndex(voxIndex, 4)].y);

            updateStructrue(ref structure);
        }
    }


    #region PROVA
    // ****************************************************************************************************
    void CartesianGradient(float[] flux, List<float> xstep, List<float> ystep, List<float> zstep)
    {
        Vector3Int structure = new Vector3Int(0, 0, 0);
   
        for (int voxIndex = 0; voxIndex < flux.Length; voxIndex++)
        {

            if (structure.x > 0 & structure.x < nx-1)
            {
                float deltax = xstep[structure.x+1] - xstep[structure.x - 1];
                int ind_1 = voxelIndex(structure.x+1, structure.y, structure.z);
                int ind_0 = voxelIndex(structure.x-1, structure.y, structure.z);
                Gradient[voxIndex].x = (flux[ind_1] - flux[ind_0]) / deltax;
            }
            else
            {
                Gradient[voxIndex].x = 0;
            }


            if (structure.y > 0 & structure.y < ny - 1)
            {
                float deltay = ystep[structure.y + 1] - ystep[structure.y - 1];
                int ind_1 = voxelIndex(structure.x, structure.y+1, structure.z);
                int ind_0 = voxelIndex(structure.x, structure.y-1, structure.z);
                Gradient[voxIndex].y = (flux[ind_1] - flux[ind_0]) / deltay;
            }
            else
            {
                Gradient[voxIndex].y = 0;
            }


            if (structure.z > 0 & structure.z < nz - 1)
            {
                float deltaz = zstep[structure.z + 1] - zstep[structure.z - 1];
                int ind_1 = voxelIndex(structure.x, structure.y, structure.z+1);
                int ind_0 = voxelIndex(structure.x, structure.y, structure.z-1);
                Gradient[voxIndex].z = (flux[ind_1] - flux[ind_0]) / deltaz;
            }
            else
            {
                Gradient[voxIndex].y = 0;
            }


            updateStructrue(ref structure);
        }
    }


    // ****************************************************************************************************

    #endregion



    // **********************************************************************************************************************************


    protected void updateStructrue(ref Vector3Int structure)
    {
        if (structure.y < ny -1 )
        {
            structure.y += 1;
        }
        else
        {
            structure.y = 0;
            if (structure.z < nz - 1)
            {
                structure.z += 1;
            }
            else
            {
                structure.z = 0;
                structure.x += 1;
            }
        }
    }
    
    protected float getVolume(Vector3Int structure, List<float> xstep, List<float> ystep, List<float> zstep)
    {
        float deltax = xstep[structure.x + 1] - xstep[structure.x];
        float deltay = ystep[structure.y + 1] - ystep[structure.y];
        float deltaz = zstep[structure.z + 1] - zstep[structure.z];
        return deltax * deltay * deltaz;
    }

    void CreateBuffers(int nx, int ny, int nz)
    {
        int numVoxels = nx * ny * nz;
        int numPoints = 8 * numVoxels;
        int numPointsSurf = 6 * numVoxels;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        //if ( (!Application.isPlaying) || (pointsBuffer == null || numPoints != pointsBuffer.count))
        //{
        if (Application.isPlaying)
        {
            ReleaseBuffers();
        }
        surfbuffer = new ComputeBuffer(numPointsSurf, sizeof(float) * 2);

        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 3);


        //}
    }

    void ReleaseBuffers()
    {
        if (surfbuffer != null)
        {
            surfbuffer.Release();
            pointsBuffer.Release();
        }
    }


    public void clear()
    {
        pointGradient = null;

    }

    #region RUNGE KUTTA
    protected void CashKarpRungeKutta(ref Vector3 output, ref float hstep, ref bool stop, Vector3 point, Vector3[] fluxVoxGradient, float[] bounds)
    {
        coefficients coeff = new coefficients();

        float MaxErr = 1e-6f,
        estError = 2 * MaxErr, errRatio,
        hstepMax = 1,
        hstepMin = 0.01f; 
        int countIterations = 0;


        while (estError > MaxErr)
        {
            Vector3 dy4 = new Vector3();
            #region compute Cash Karp runge kutta
            ComputeCKRK(ref dy4, ref estError, hstep, point, fluxVoxGradient, bounds);


            if (estError == 0)
            {
                stop = true;
                //output = point + dy4;
                break;
            }


            if (double.IsNaN(estError))
            {
                // TRY LAST
                int NstepInVoxel = 3;
                float sumModulesVelocities = 0; foreach (Vector3 v in fluxVoxGradient) { sumModulesVelocities += v.magnitude; }
                float vAverage = sumModulesVelocities / 8;
                hstep = 1 / (vAverage * NstepInVoxel); // Runge Kutta step
                dy4 = new Vector3();
                ComputeCKRK(ref dy4, ref estError, hstep, point, fluxVoxGradient, bounds);

            }

            #endregion
            // MaxErr = 1e-6f * hstep * (float)getVelocity(point, fluxVoxGradient, bounds).magnitude;
            errRatio = estError / MaxErr;


            if (hstep > hstepMax)
            {
                ComputeCKRK(ref dy4, ref estError, hstepMax, point, fluxVoxGradient, bounds);
                hstep = 0.1f;
                //hstep = hstepMax;
                // Debug.Log("BIGGER THAN MAX");
                output = point + dy4;
                break;
            }

            else if (hstep < hstepMin)
            {
                ComputeCKRK(ref dy4, ref estError, hstepMin, point, fluxVoxGradient, bounds);
                hstep = 0.1f;
                // Debug.Log("LESS THAN MIN");
                //hstep = hstepMin;
                output = point + dy4;
                break;
            }

            else
            {

                #region check error compliance
                if (estError > MaxErr)
                {
                    hstep = (float)0.9 * hstep * Mathf.Pow(errRatio, -0.25f);
                }
                else
                {
                    hstep = (float)0.9 * hstep * Mathf.Pow(errRatio, -0.2f);
                    output = point + dy4;
                    if (dy4.magnitude < 1e-5f) { stop = true; } //STOP BECAUSE OF STAGNATION   Debug.Log("STAGNATION")
                }

                #endregion

            }


            countIterations++;
            if (countIterations > 1000)
            {
                Debug.Log("OUT OF ITERATIONS");
                break;
            }

        }
    }

    void ComputeCKRK(ref Vector3 dy4, ref float estError, float hstep, Vector3 point, Vector3[] fluxVoxGradient, float[] bounds)
    {
        coefficients coeff = new coefficients();
        Vector3 RK1 = hstep * getVelocity(point, fluxVoxGradient, bounds);
        Vector3 RK2 = hstep * getVelocity(point + (float)coeff.B2[0] * RK1, fluxVoxGradient, bounds);
        Vector3 RK3 = hstep * getVelocity(point + (float)coeff.B3[0] * RK1 + (float)coeff.B3[1] * RK2, fluxVoxGradient, bounds);
        Vector3 RK4 = hstep * getVelocity(point + (float)coeff.B4[0] * RK1 + (float)coeff.B4[1] * RK2 + (float)coeff.B4[2] * RK3, fluxVoxGradient, bounds);
        Vector3 RK5 = hstep * getVelocity(point + (float)coeff.B5[0] * RK1 + (float)coeff.B5[1] * RK2 + (float)coeff.B5[2] * RK3 + (float)coeff.B5[3] * RK4, fluxVoxGradient, bounds);
        Vector3 RK6 = hstep * getVelocity(point + (float)coeff.B6[0] * RK1 + (float)coeff.B6[1] * RK2 + (float)coeff.B6[2] * RK3 + (float)coeff.B6[3] * RK4 + (float)coeff.B6[4] * RK5, fluxVoxGradient, bounds);

        dy4 = (float)coeff.C[0] * RK1 + (float)coeff.C[2] * RK3 + (float)coeff.C[3] * RK4 + (float)coeff.C[4] * RK5 + (float)coeff.C[5] * RK6;
        Vector3 dy5 = (float)coeff.CT[0] * RK1 + (float)coeff.CT[2] * RK3 + (float)coeff.CT[3] * RK4 + (float)coeff.CT[4] * RK5 + (float)coeff.CT[5] * RK6;


        estError = 0;
        for (int i = 0; i < 3; i++)
        {
            estError += Mathf.Pow((dy4[i] - dy5[i]), 2);
        }
        estError = Mathf.Sqrt(estError);
        if (double.IsNaN(estError)) { Debug.Log("this h: " + hstep.ToString()); }
    }

    public static Vector3 getVelocity(Vector3 point, Vector3[] fluxVoxGradient, float[] bounds)
    {
        float u = (point.x - bounds[0]) / (bounds[1] - bounds[0]);
        float v = (point.z - bounds[4]) / (bounds[5] - bounds[4]); // ASSE Z
        float w = (point.y - bounds[2]) / (bounds[3] - bounds[2]); // ASSE Y
        Vector3 P0 = fluxVoxGradient[0];
        Vector3 P1 = fluxVoxGradient[1];
        Vector3 P2 = fluxVoxGradient[2];
        Vector3 P3 = fluxVoxGradient[3];
        Vector3 P4 = fluxVoxGradient[4];
        Vector3 P5 = fluxVoxGradient[5];
        Vector3 P6 = fluxVoxGradient[6];
        Vector3 P7 = fluxVoxGradient[7];

        return trilinearInterpolation(P0, P1, P2, P3, P4, P5, P6, P7, u, v, w);

    }

    protected static Vector3 trilinearInterpolation(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, Vector3 P5, Vector3 P6, Vector3 P7, float u, float v, float w)
    {
        // Calculate the required terms for the interpolation
        Vector3 c01 = P0 * (1 - u) + P1 * u;
        Vector3 c32 = P3 * (1 - u) + P2 * u;
        Vector3 c45 = P4 * (1 - u) + P5 * u;
        Vector3 c76 = P7 * (1 - u) + P6 * u;
        Vector3 c0 = c32 * (1 - v) + c01 * v;
        Vector3 c1 =  c76 * (1 - v) + c45 * v;
        Vector3 result = c0 * (1 - w) + c1 * w;

        // Return the interpolated value
        return result;
    }

    class coefficients
    {
        public double[] A = { 1.0 / 5.0, 3.0 / 10.0, 3.0 / 5.0, 1.0, 7.0 / 8.0 };
        public double[] B2 = { 1.0 / 5.0, 0, 0, 0, 0 };
        public double[] B3 = { 3.0 / 40.0, 9.0 / 40.0, 0, 0, 0 };
        public double[] B4 = { 3.0 / 10.0, -9.0 / 10.0, 6.0 / 5.0, 0, 0 };
        public double[] B5 = { -11.0 / 54.0, 5.0 / 2.0, -70.0 / 27.0, 35.0 / 27.0, 0 };
        public double[] B6 = { 1631.0 / 55296.0, 175.0 / 512.0, 575.0 / 13824.0, 44275.0 / 110592.0, 253.0 / 4096.0 };
        public double[] C = { 37.0 / 378.0, 0, 250.0 / 621.0, 125.0 / 594.0, 0, 512.0 / 1771.0 };
        public double[] CT = {2825.0 / 27648.0, 0,
                            18575.0 / 48384.0, 13525.0 / 55296.0, 277.0 / 14336.0,
                            1.0 / 4.0};

    }


    #endregion
}
