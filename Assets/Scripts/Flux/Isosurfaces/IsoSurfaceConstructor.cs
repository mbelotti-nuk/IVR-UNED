using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoSurfaceConstructor : MonoBehaviour
{
    #region normal computation
    DensityGenerator densityGenerator;
    ComputeShader shader;
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;
    #endregion

    #region save memory computation
    memoryDensityGenerator memorydensityGenerator;
    ComputeShader memoryShader;


    ComputeBuffer pBuffer;
    ComputeBuffer vBuffer;
    ComputeBuffer triangleBuffer1;
    ComputeBuffer triCountBuffer1;
    ComputeBuffer triangleBuffer2;
    ComputeBuffer triCountBuffer2;
    #endregion


    List<float> xstep;
    List<float> ystep;
    List<float> zstep;
    //
    int nx;
    int ny;
    int nz ;
    float[] flux;

    Vector3[] vertices;
    int[] meshTriangles;

    Vector3[] vertices1;
    int[] meshTriangles1;
    Vector3[] vertices2;
    int[] meshTriangles2;


    public IsoSurfaceConstructor(DensityGenerator densityGenerator, ComputeShader shader, memoryDensityGenerator memorydensityGenerator, ComputeShader memoryShader, List<float> xstep, List<float> ystep, List<float> zstep, float[] flux)
    {
        this.densityGenerator = densityGenerator;
        this.shader = shader;
        this.memorydensityGenerator = memorydensityGenerator;
        this.memoryShader = memoryShader;
        this.xstep = xstep;
        this.ystep = ystep;
        this.zstep = zstep;
        this.flux = flux;   
    }

    public void generateVertices(VoxelStructure Meshtal, float isoLevel)
    {
        int threadGroupSize = 8;
        int numThreadsXAxis = Mathf.CeilToInt(nx / (float)threadGroupSize);
        int numThreadsYAxis = Mathf.CeilToInt(ny / (float)threadGroupSize);
        int numThreadsZAxis = Mathf.CeilToInt(nz / (float)threadGroupSize);

        densityGenerator.Generate(pointsBuffer, xstep, ystep, zstep, flux);


        triangleBuffer.SetCounterValue(0);
        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);
        shader.SetInt("nx", nx);
        shader.SetInt("ny", ny);
        shader.SetInt("nz", nz);
        shader.SetFloat("isoLevel", isoLevel);

        shader.Dispatch(0, numThreadsXAxis, numThreadsYAxis, numThreadsZAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];


        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);


        vertices = new Vector3[numTris * 3];
        meshTriangles = new int[numTris * 3];

        if (Meshtal.isRotated)
        {
            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = Meshtal.traslation + Meshtal.rotationMatrix * tris[i][j];
                }
            }
        }
        else
        {
            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;

                    vertices[i * 3 + j] = tris[i][j];
                }
            }
        }

        ReleaseBuffers();
    }

    public void memoryGenerateVertices(VoxelStructure Meshtal, float isoLevel)
    {

        generatePVBuffers(nx * ny * nz);
        memorydensityGenerator.Generate(ref pBuffer, ref vBuffer, xstep, ystep, zstep, flux);

        // LET'S SPLIT THE CALCULATION IN TWO


        // FIRST ONE
        int numTris1 = firstSetVertices(Meshtal, isoLevel);

        // SECOND ONE
        secondSetVertices(Meshtal, isoLevel, numTris1);


        ReleaseMemoryBuffers();
        #region GENERATE VERTICES

        vertices = new Vector3[vertices1.Length + vertices2.Length];
        meshTriangles = new int[meshTriangles1.Length + meshTriangles2.Length];

        vertices1.CopyTo(vertices, 0);
        vertices2.CopyTo(vertices, vertices1.Length);

        meshTriangles1.CopyTo(meshTriangles, 0);
        meshTriangles2.CopyTo(meshTriangles, meshTriangles1.Length);


        // dispose
        vertices1 = null; vertices2 = null; meshTriangles1 = null; meshTriangles2 = null;
        #endregion

    }

    public Vector3[] thisVertices
    {
        get { return vertices; }
    }

    public int[] thisTriangles
    {
        get { return meshTriangles; }
    }

    public void setBounds(int nx, int ny, int nz)
    {
        this.nx = nx;
        this.ny = ny;
        this.nz = nz;
    }

    public void generateBuffers(int n)
    {
        int numVoxels = n;
        int numPoints = 8 * numVoxels;
        int maxTriangleCount = numVoxels * 5;
        if (Application.isPlaying)
        {
            ReleaseBuffers();
        }
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }


    public void generatePVBuffers(int nVoxels)
    {
        int numVoxels = nVoxels;
        int numPoints = 8 * numVoxels;
        pBuffer = new ComputeBuffer(numPoints, sizeof(float) * 3);
        vBuffer = new ComputeBuffer(numPoints, sizeof(float) * 1);
    }


    protected int firstSetVertices(VoxelStructure Meshtal, float isoLevel)
    {
        int threadGroupSize = 8;
        #region FIRST CALCULATION
        int nx1 = Mathf.CeilToInt(nx / 2);
        int ny1 = ny;
        int nz1 = nz;

        int numThreadsXAxis1 = Mathf.CeilToInt((nx1 / (float)threadGroupSize));
        int numThreadsYAxis1 = Mathf.CeilToInt((ny1 / (float)threadGroupSize));
        int numThreadsZAxis1 = Mathf.CeilToInt((nz1 / (float)threadGroupSize));


        int maxTriangleCount = (nx / 2) * ny  * nz  * 5;
        triangleBuffer1 = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer1 = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        triangleBuffer1.SetCounterValue(0);
        memoryShader.SetBuffer(0, "points", pBuffer);
        memoryShader.SetBuffer(0, "value", vBuffer);
        memoryShader.SetBuffer(0, "triangles", triangleBuffer1);
        memoryShader.SetInt("nx", nx);
        memoryShader.SetInt("ny", ny);
        memoryShader.SetInt("nz", nz);
        memoryShader.SetInt("biasx", 0);
        memoryShader.SetInt("biasy", 0);
        memoryShader.SetInt("biasz", 0);
        memoryShader.SetInt("endx", nx1);
        memoryShader.SetInt("endy", ny1);
        memoryShader.SetInt("endz", nz1);
        memoryShader.SetFloat("isoLevel", isoLevel);

        memoryShader.Dispatch(0, numThreadsXAxis1, numThreadsYAxis1, numThreadsZAxis1);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer1, triCountBuffer1, 0);
        int[] triCountArray1 = { 0 };
        triCountBuffer1.GetData(triCountArray1);
        int numTris1 = triCountArray1[0];


        // Get triangle data from shader
        Triangle[] tris1 = new Triangle[numTris1];
        triangleBuffer1.GetData(tris1, 0, 0, numTris1);


        vertices1 = new Vector3[numTris1 * 3];
        meshTriangles1 = new int[numTris1 * 3];

        if (Meshtal.isRotated)
        {
            for (int i = 0; i < numTris1; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles1[i * 3 + j] = i * 3 + j;
                    vertices1[i * 3 + j] = Meshtal.traslation + Meshtal.rotationMatrix * tris1[i][j];
                }
            }
        }
        else
        {
            for (int i = 0; i < numTris1; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles1[i * 3 + j] = i * 3 + j;
                    vertices1[i * 3 + j] = tris1[i][j];
                }
            }
        }

        triangleBuffer1.Dispose();
        triCountBuffer1.Dispose();
        return numTris1;

        #endregion
    }

    protected void secondSetVertices(VoxelStructure Meshtal, float isoLevel, int numTris1)
    {
        int threadGroupSize = 8;
        int nx1 = Mathf.CeilToInt(nx / 2);

        int numThreadsXAxis2 = Mathf.CeilToInt(((nx - nx1) / (float)threadGroupSize));
        int numThreadsYAxis2 = Mathf.CeilToInt((ny  / (float)threadGroupSize));
        int numThreadsZAxis2 = Mathf.CeilToInt((nz / (float)threadGroupSize));


        int maxTriangleCount = (nx / 2) * (ny / 2) * (nz / 2) * 5;
        triangleBuffer2 = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer2 = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);


        triangleBuffer2.SetCounterValue(0);
        memoryShader.SetBuffer(0, "points", pBuffer);
        memoryShader.SetBuffer(0, "value", vBuffer);
        memoryShader.SetBuffer(0, "triangles", triangleBuffer2);
        memoryShader.SetInt("nx", nx);
        memoryShader.SetInt("ny", ny);
        memoryShader.SetInt("nz", nz);
        memoryShader.SetInt("biasx", nx1);
        memoryShader.SetInt("biasy", 0);
        memoryShader.SetInt("biasz", 0);
        memoryShader.SetInt("endx", nx);
        memoryShader.SetInt("endy", ny);
        memoryShader.SetInt("endz", nz);
        memoryShader.SetFloat("isoLevel", isoLevel);

        memoryShader.Dispatch(0, numThreadsXAxis2, numThreadsYAxis2, numThreadsZAxis2);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer2, triCountBuffer2, 0);
        int[] triCountArray2 = { 0 };
        triCountBuffer2.GetData(triCountArray2);
        int numTris2 = triCountArray2[0];


        // Get triangle data from shader
        Triangle[] tris2 = new Triangle[numTris2];
        triangleBuffer2.GetData(tris2, 0, 0, numTris2);

        vertices2 = new Vector3[numTris2 * 3];
        meshTriangles2 = new int[numTris2 * 3];

        if (Meshtal.isRotated)
        {
            for (int i = 0; i < numTris2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles2[i * 3 + j] = numTris1*3 + i * 3 + j;
                    vertices2[i * 3 + j] = Meshtal.traslation + Meshtal.rotationMatrix * tris2[i][j];
                }
            }
        }
        else
        {
            for (int i = 0; i < numTris2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles2[i * 3 + j] = numTris1*3 + i * 3 + j;
                    vertices2[i * 3 + j] = tris2[i][j];
                }
            }
        }

        triangleBuffer2.Dispose();
        triCountBuffer2.Dispose();
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

    void ReleaseMemoryBuffers()
    {
        if (triangleBuffer != null)
        {
            pBuffer.Release();
            vBuffer.Release();
        }

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
}
