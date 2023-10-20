using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class VoxelStructure
{
    #region Fields
    // Meshtal paramerters
    private protected string structureName;
    private protected string type;
    private protected List<float> xstep = new List<float>();
    private protected List<float> ystep = new List<float>();
    private protected List<float> zstep = new List<float>();

    public Quaternion rotationMatrix;
    public Vector3 traslation;
    protected bool rotate = false;

    private protected float[] flux;
    Vector3[] centers;
    private protected int nx;
    private protected int ny;
    private protected int nz;
    private protected float maxFlux;
    /// <summary>
    /// 
    /// <param name="xstep"> Mesh X-axis divison, </param>
    /// <param name="ystep"> Mesh Y-axis division, </param>
    /// <param name="zstep"> Mehs Z-axis division, </param>
    /// <param name="flux"> Mesh flux values, </param>
    /// <param name="centers"> Mesh centers </param>
    /// 
    /// </summary>
    /// 

    //
    float xMin, xMax, yMin, yMax, zMin, zMax;
    int xStructMax, yStructMax, zStructMax;
    //

    #endregion

    #region text properties
    //TEXT VARIABLES
    string extension;
    string[] lines = null;
    List<int> tallyTextPostion;
    List<string> meshTallyName;

    List<int> meshBegin;
    static List<int> meshEnd;

    List<int> xstepPosition;
    List<int> ystepPosition;
    List<int> zstepPosition;
    #endregion

    #region Constructor
    public VoxelStructure()
    {

    }
    /// <summary>
    /// Initialize radiation field to default by reading TextAsset file
    /// </summary>
    /// <param name="file"></param>
    /// <param name="structureName">Name of radiation field</param>
    /// <param name="type">radiation field type</param>
    public VoxelStructure(TextAsset file, string structureName, string type)
    {

        float multiplicationFactor =1f;
        firstDefaultRead(file);
        secondReadText(0, multiplicationFactor, 4);
        SetValues();
        this.structureName = structureName;
        this.type = type;
    }

    #endregion


    #region Public Methods
    public Vector3[] GetCenters
    { get { return centers; } }
    public List<float> GetXstep
    { get { return xstep; } }
    public List<float> GetYstep
    { get { return ystep; } }
    public List<float> GetZstep
    { get { return zstep; } }
    public float[] GetArrayFlux
    { get { return flux; } }
    public float GetMaxFlux
    { get { return maxFlux; } }
    public string voxelName
    {
        get
        {
            return structureName;
        }
        set
        {
            structureName = value;
        }
    }


    public bool isRotated
    { get { return rotate; } }

    public string thisType
    { get { return type; } }
    public string voxelType
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
        }
    }

    public void SetValues()
    {
        List<float> temp = ystep;
        ystep = zstep; // INVERT COORDINATES
        zstep = temp;
        //this.xstep = xstep; 
        //this.ystep = ystep;
        //this.zstep = zstep; 
        //this.flux = flux;
        //this.centers = centers;
        xStructMax = xstep.Count;
        yStructMax = ystep.Count;
        zStructMax = zstep.Count;
        xMin = xstep[0];
        yMin = ystep[0];
        zMin = zstep[0];
        xMax = xstep[xStructMax - 1];
        yMax = ystep[yStructMax - 1];
        zMax = zstep[zStructMax - 1];
        nx = xstep.Count - 1;
        ny = ystep.Count - 1;
        nz = zstep.Count - 1;
    }
    void Draw(Voxel<float> voxel)
    {
        Gizmos.color = new Color(1, 0, 0);
        Gizmos.DrawWireCube(voxel.Center, voxel.Dimensions);
    }

    public void DrawMeshtalBounds()
    {
        #region Cube sides
        Vector3 vertex1 = new Vector3(xMin, yMin, zMin);
        Vector3 vertex2 = new Vector3(xMax, yMin, zMin);
        Vector3 vertex3 = new Vector3(xMin, yMin, zMax);
        Vector3 vertex4 = new Vector3(xMax, yMin, zMax);
        Vector3 vertex5 = new Vector3(xMin, yMax, zMin);
        Vector3 vertex6 = new Vector3(xMax, yMax, zMin);
        Vector3 vertex7 = new Vector3(xMin, yMax, zMax);
        Vector3 vertex8 = new Vector3(xMax, yMax, zMax);
        #endregion

        Gizmos.color = new Color(1, 0, 0);
        #region DrawMeshtal
        // base side down
        Gizmos.DrawLine(vertex1, vertex2);
        Gizmos.DrawLine(vertex1, vertex3);
        Gizmos.DrawLine(vertex3, vertex4);
        Gizmos.DrawLine(vertex4, vertex2);
        // base side up
        Gizmos.DrawLine(vertex5, vertex6);
        Gizmos.DrawLine(vertex5, vertex7);
        Gizmos.DrawLine(vertex7, vertex8);
        Gizmos.DrawLine(vertex8, vertex6);
        // remaining sides
        Gizmos.DrawLine(vertex1, vertex5);
        Gizmos.DrawLine(vertex2, vertex6);
        Gizmos.DrawLine(vertex3, vertex7);
        Gizmos.DrawLine(vertex4, vertex8);
        #endregion
    }

    public List<string> getTallyNames()
    {
        return meshTallyName;
    }

    public float GetFlux(Vector3 position) { 
    
        // if rotation, transform player position in the coodinate system of meshtal
        if(rotate)
        {
            position = ( Quaternion.Inverse(rotationMatrix) ) * position;
        }


        float xPlayer = position.x;
        float yPlayer = position.y;
        float zPlayer = position.z;


        if ((xPlayer >= xMax) | (yPlayer >= yMax) | (zPlayer >= zMax) | (xPlayer <= xMin) | (yPlayer <= yMin) | (zPlayer <= zMin)) // out of mesh box
        {
            return 0f;
        }
        else // inside mesh box
        {
            int xIndex = IndexOf(xPlayer, 1);
            int yIndex = IndexOf(yPlayer, 2);
            int zIndex = IndexOf(zPlayer, 3);

            Vector3Int structPlayer = new Vector3Int(xIndex, yIndex, zIndex);

            return flux[returnIndex(structPlayer)];
        }

    }


    public float GetFlux(Vector3Int position)
    {
        return flux[returnIndex(position)];
    }



        public int IndexOf(float item, int axis)// List<float> step)
    {
        // DEFINE AXIS
        // RETURN IN CASE OUTSIDE MESHBOUNDS

        List<float> step = new List<float>();
        if (axis == 1)
        {
            step = xstep;
            if(item > xMax)
            {
                return -1;
            }
        }
        else if ( axis==2 )
        {
            step = ystep;
            if (item > yMax)
            {
                return -1;
            }
        }
        else if (axis==3 )
        {
            step = zstep;
            if (item > zMax)
            {
                return -1;
            }
        }
        //

        int lowerBound = 0;
        int upperBound = step.Count - 1;
        int location = -1;

        while ((location == -1) && (lowerBound <= upperBound))
        {
            // find the middle
            // int always convert to lower integer if float
            int middleLocation = lowerBound + (upperBound - lowerBound) / 2;
            float leftSideValue = step[middleLocation];
            float rightSideValue = step[middleLocation + 1];


            // check for match
            if ((item >= leftSideValue) && (item <= rightSideValue))
            {
                location = middleLocation;
            }
            else
            {
                // split data set to search appropriate side
                if (item <= leftSideValue)
                {
                    upperBound = middleLocation - 1;
                }
                else  //  which means (item > rightSideValue)
                {
                    lowerBound = middleLocation + 1;
                }
            }
        }
        return location;
    }

    public void setRotation(Quaternion rotationMatrix, Vector3 traslation)
    {
        this.rotationMatrix = rotationMatrix;
        this.traslation = traslation;
        rotate = true;
    }
 
    public Vector3Int getStructure(int index)
    {
        // the index is referred to flux or centers arrays

        Vector3 center = centers[index];
        Vector3Int result = new Vector3Int(IndexOf(center.x,1), IndexOf(center.y, 2), IndexOf(center.z, 3));
        return result;
    }
    
    #endregion


    #region Protected Methods

    protected int returnIndex(Vector3Int structPlayer)
    {
        return structPlayer.x * (nz * ny) + structPlayer.z * (ny) + structPlayer.y;
    }

    protected void SetValues(List<float> xstep, List<float> ystep, List<float> zstep, List<float> flux, List<Vector3> centers)
    {
        //this.xstep = xstep; 
        //this.ystep = ystep;
        //this.zstep = zstep; 
        //this.flux = flux;
        //this.centers = centers;
        xStructMax = xstep.Count;
        yStructMax = ystep.Count;
        zStructMax = zstep.Count;
        xMin = xstep[0];
        yMin = ystep[0];
        zMin = zstep[0];
        xMax = xstep[xStructMax - 1];
        yMax = ystep[yStructMax - 1];
        zMax = zstep[zStructMax - 1];
        nx = xstep.Count - 1;
        ny = ystep.Count - 1;
        nz = zstep.Count - 1;
    }



    #endregion


    #region reading Methods
    public void firstDefaultRead(TextAsset file)
    {

        tallyTextPostion = new List<int>();
        meshTallyName = new List<string>();

        meshBegin = new List<int>();
        meshEnd = new List<int>();

        xstepPosition = new List<int>();
        ystepPosition = new List<int>();
        zstepPosition = new List<int>();

        bool meshSection = false;

        var splitFile = new string[] { "\r\n", "\r", "\n" };
        lines = file.text.Split(splitFile, StringSplitOptions.RemoveEmptyEntries);


        for (int nLine = 0; nLine < lines.Length; nLine++)
        {

            if (!meshSection)
            {
                string[] lineSplit = Regex.Split(lines[nLine].Trim(), @" +");

                if (!String.IsNullOrEmpty(lines[nLine]))
                {

                    if (lineSplit.Length > 2)
                    {
                        if (lineSplit[0] == "Mesh" & lineSplit[1] == "Tally" & lineSplit[2] == "Number")
                        {
                            meshTallyName.Add(lineSplit[3]);
                            tallyTextPostion.Add(nLine);
                        }

                    }

                    if (lineSplit[0] == "X" && lineSplit[1] == "direction:")
                    {
                        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
                        xstepPosition.Add(nLine);

                    }

                    else if (lineSplit[0] == "Y" && lineSplit[1] == "direction:")
                    {
                        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
                        ystepPosition.Add(nLine);

                    }

                    else if (lineSplit[0] == "Z" && lineSplit[1] == "direction:")
                    {
                        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
                        zstepPosition.Add(nLine);

                        //}
                    }

                    else if (lineSplit[0] == "Energy" && lineSplit[1] == "X" && lineSplit[2] == "Y" && lineSplit[3] == "Z")
                    {
                        meshBegin.Add(nLine + 1);
                        break; // END OF SPECIFICATIONS
                    }
                }
            }

        }
    }

    // override for loading new meshtallies

    public void firstRead(string path, string ext)
    {
        extension = ext;

        if (extension == ".vtr" || extension == ".vtk")
        {
            firstReadXML(path);
        }
        else
        {
            firstReadText(path);
        }
    }

    public void secondRead(int meshTallyNum, float multiplicationFactor, string XMLpath, int fluxIndex = 4)
    {
        if (extension == ".vtr" || extension == ".vtk")
        {
            secondReadXML(XMLpath, multiplicationFactor);
        }
        else
        {
            secondReadText(meshTallyNum, multiplicationFactor, fluxIndex);
        }
    }



    public void firstReadText(string path)
    {
        tallyTextPostion = new List<int>();
        meshTallyName = new List<string>();

        meshBegin = new List<int>();
        meshEnd = new List<int>();

        xstepPosition = new List<int>();
        ystepPosition = new List<int>();
        zstepPosition = new List<int>();

        bool meshSection = false;

        var splitFile = new string[] { "\r\n", "\r", "\n" };
        lines = System.IO.File.ReadAllLines(path);


        for (int nLine = 0; nLine < lines.Length; nLine++)
        {

            if (!meshSection)
            {

                string[] lineSplit = Regex.Split(lines[nLine].Trim(), @" +");

                if (!String.IsNullOrEmpty(lines[nLine]))
                {

                    if (lineSplit.Length > 2)
                    {
                        if (lineSplit[0] == "Mesh" & lineSplit[1] == "Tally" & lineSplit[2] == "Number")
                        {
                            meshTallyName.Add(lineSplit[3]);
                            tallyTextPostion.Add(nLine);
                        }

                    }

                    if (lineSplit[0] == "X" && lineSplit[1] == "direction:")
                    {
                        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
                        xstepPosition.Add(nLine);

                    }

                    else if (lineSplit[0] == "Y" && lineSplit[1] == "direction:")
                    {
                        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
                        ystepPosition.Add(nLine);

                    }

                    else if (lineSplit[0] == "Z" && lineSplit[1] == "direction:")
                    {
                        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
                        zstepPosition.Add(nLine);

                        //}
                    }

                    else if (lineSplit[0] == "Energy" && lineSplit[1] == "X" && lineSplit[2] == "Y" && lineSplit[3] == "Z")
                    {
                        meshBegin.Add(nLine + 1);
                        meshSection = true;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(lines[nLine]))
                {
                    meshSection = false;
                }
            }

        }
    }

    public void secondReadText(int meshTallyNum, float multiplicationFactor, int fluxIndex = 4)
    {
        ClearValues();
        // xstep
        string[] lineSplit = Regex.Split(lines[xstepPosition[meshTallyNum]].Trim(), @" +");
        var sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
        xstep = sliceLine.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * 0.01f).ToList();

        // ystep
        lineSplit = Regex.Split(lines[ystepPosition[meshTallyNum]].Trim(), @" +");
        sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
        ystep = sliceLine.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * 0.01f).ToList();

        // zstep
        lineSplit = Regex.Split(lines[zstepPosition[meshTallyNum]].Trim(), @" +");
        sliceLine = lineSplit.Skip(2).Take(lineSplit.Length - 2);
        zstep = sliceLine.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * 0.01f).ToList();



        int endOfMesh = (xstep.Count - 1) * (ystep.Count - 1) * (zstep.Count - 1);

        flux = new float[endOfMesh];
        centers = new Vector3[endOfMesh];

        for (int j = 0; j < endOfMesh; j++)
        {
            int nLine = meshBegin[meshTallyNum] + j;
            lineSplit = Regex.Split(lines[nLine].Trim(), @" +");
            float xCenter = float.Parse(lineSplit[1], CultureInfo.InvariantCulture) * 0.01f;
            float yCenter = float.Parse(lineSplit[2], CultureInfo.InvariantCulture) * 0.01f;
            float zCenter = float.Parse(lineSplit[3], CultureInfo.InvariantCulture) * 0.01f;
            centers[j] = new Vector3(xCenter, zCenter, yCenter);

            Double convertFlux = Double.Parse(lineSplit[fluxIndex], CultureInfo.InvariantCulture);
            Double currentFlux = (convertFlux * (double)multiplicationFactor);
            flux[j] = (float)currentFlux;
            if (currentFlux > 0)
            {
                if (currentFlux > maxFlux)
                {
                    maxFlux = (float)currentFlux;
                }
            }


        };

    }



    public void firstReadXML(string XMLpath)
    {

        XDocument xDocument = XDocument.Load(XMLpath);
        var nodes = xDocument.DescendantNodes();
        meshTallyName = new List<string>();
        foreach (var node in nodes)
        {
            var element = node as XElement;
            if (element == null) continue;
            //Debug.Log(element.Name);

            if (element.Name == "CellData")
            {
                foreach (var mesh in element.Nodes())
                {
                    var meshElement = mesh as XElement;
                    if (meshElement == null) continue;

                    meshTallyName.Add(meshElement.Attribute("Name").Value);


                }
            }

        }
    }

    public void secondReadXML(string XMLpath, float multiplicationFactor)
    {
        ClearValues();

        List<float> tempFlux = new List<float>();

        XDocument xDocument = XDocument.Load(XMLpath);
        var nodes = xDocument.DescendantNodes();
        foreach (var node in nodes)
        {
            var element = node as XElement;
            if (element == null) continue;
            //Debug.Log(element.Name);


            if (element.Name == "CellData")
            {

                foreach (var mesh in element.Nodes())
                {
                    var meshElement = mesh as XElement;
                    if (meshElement == null) continue;
                    if (meshElement.Attribute("Name").Value == this.voxelName)
                    {
                        string[] lineSplit = Regex.Split(meshElement.Value.Trim(), @" +");
                        tempFlux = lineSplit.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * multiplicationFactor).ToList();
                    }


                }
            }
            else if (element.Name == "Coordinates")
            {
                int coord = 0;
                foreach (var step in element.Nodes())
                {
                    var stepElement = step as XElement;
                    string[] lineSplit = Regex.Split(stepElement.Value.Trim(), @" +");
                    string stepName = stepElement.Attribute("Name").Value;

                    if (coord == 0)
                    {
                        xstep = lineSplit.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * 0.01f).ToList();

                    }
                    else if (coord == 1)
                    {
                        ystep = lineSplit.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * 0.01f).ToList();

                    }
                    else if (coord == 2)
                    {
                        zstep = lineSplit.Select(d => float.Parse(d, CultureInfo.InvariantCulture) * 0.01f).ToList();
                    }

                    coord++;
                }
            }
        }


        // Populate center array and rearrange flux

        int nx = xstep.Count - 1;
        int ny = ystep.Count - 1;
        int nz = zstep.Count - 1;


        centers = new Vector3[nx * ny * nz];
        flux = new float[nx * ny * nz];
        int counter = 0;
        for (int k = 0; k < nz; k++)
        {
            for (int j = 0; j < ny; j++)
            {
                for (int i = 0; i < nx; i++)
                {
                    //  x * (nz * ny) + z * (ny) + y;
                    int index = k + j * nz + i * nz * ny;
                    flux[index] = tempFlux[counter];

                    float xcenter = (xstep[i] + xstep[i + 1]) / 2;
                    float ycenter = (ystep[j] + ystep[j + 1]) / 2;
                    float zcenter = (zstep[k] + zstep[k + 1]) / 2;
                    centers[counter] = new Vector3(xcenter, zcenter, ycenter);
                    counter++;
                }

            }
        }

        maxFlux = flux.Max();

    }






    public void ClearVariables()
    {
        //valueCounter.Clear();

        tallyTextPostion.Clear();
        meshTallyName.Clear();

        meshBegin.Clear();
        meshEnd.Clear();

        xstepPosition.Clear();
        ystepPosition.Clear();
        zstepPosition.Clear();
        lines = null; // free memory
    }

    public void ClearValues()
    {
        xstep.Clear();
        ystep.Clear();
        zstep.Clear();
        flux = null;
        centers = null;
    }
    #endregion


}
