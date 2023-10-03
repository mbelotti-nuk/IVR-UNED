using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Geometry;
using Unity.VisualScripting;
using UnityEngine.Rendering.VirtualTexturing;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using System;
using Grid = Geometry.Grid;
using Unity.AI.Navigation.Samples;

public class IntersectMesh : MonoBehaviour
{

    public GameObject walkMesh;
    public Main main;
    // public GameObject building;


    List<AABB> nav = new List<AABB>();
    List<Triangle> trianglesWalk = new List<Triangle>();
    Grid grid = new Grid();

    #region buffers
    public ComputeShader intersectionShader;
    public ComputeShader excludeMeshShader;

    ComputeBuffer buildingTriangleBuffer;
    ComputeBuffer buildingIntersection;

    ComputeBuffer triangleBuffer;
    ComputeBuffer intersection;
    ComputeBuffer aabbBuffer;
    #endregion


    // Start is called before the first frame update
    public void buildWalkMesh(float ypos, float dimension)
    {

        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        Triangle[] trianglesWalkMesh = fetchTriangles(walkMesh.transform.GetComponent<MeshFilter>().mesh);
        trianglesWalk = new List<Triangle>();

        AABB meshDomain = getMeshtalAABB();


        // LOWER NUMBER OF TRIANLGES TO BE INVESTIGATED;
        HashSet<float> heightsTriangles = new HashSet<float>();
        foreach (Triangle triangle in trianglesWalkMesh)
        {
            if (triangle.Intersects(meshDomain))
            {
                if (ypos > triangle.Centroid.y - dimension && ypos < triangle.Centroid.y + dimension)
                {
                    trianglesWalk.Add(triangle); heightsTriangles.Add(triangle.Centroid.y);
                }

            }
        }
        Debug.Log($"Execution FIRST PART: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();

        // foreach(float height in heightsTriangles) { Debug.Log($"Heigth {height}"); }


        watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        grid = new Grid();
        float dimGrid = 0.1f;

        AABB[] aabbWalkMesh = grid.buildGrid(dimGrid, meshDomain.Min, meshDomain.Max, heightsTriangles.ToArray());

        // LOWER THE OTHERS MESH TRIANGLES

        // setAndDispatchBuildingShader(meshDomain, ref trianglesWalls);


        setAndDispatchIntersection(aabbWalkMesh, trianglesWalk);

        Debug.Log($"Execution grid: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();

        nav = grid.navigationAABB.ToList();

        Debug.Log($"num AABB walkable {grid.navigationAABB.Length}");

    }


    public Grid getNevigationGrid()
    {
        return grid;
    }


    private void OnDrawGizmos()
    {
        foreach (AABB b in nav)
        {
            b.DrawGizmos(true);
        }

        //foreach (Triangle b in trianglesWalk)
        //{
        //    b.DrawGizmos(false);
        //}

    }



    private Triangle[] fetchTriangles(Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        mesh.GetVertices(vertices);

        Triangle[] triangles = new Triangle[mesh.triangles.Length / 3];

        // var vertices = mesh.vertices;
        var trianglesMesh = mesh.triangles;

        int j = 0;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            //Vector3 p1 = Quaternion.AngleAxis(-180, Vector3.up) * mesh.vertices[mesh.triangles[i + 0]];
            //Vector3 p2 = Quaternion.AngleAxis(-180, Vector3.up) * mesh.vertices[mesh.triangles[i + 1]];
            //Vector3 p3 = Quaternion.AngleAxis(-180, Vector3.up) * mesh.vertices[mesh.triangles[i + 2]];
            Vector3 p1 = vertices[trianglesMesh[i + 0]];
            Vector3 p2 = vertices[trianglesMesh[i + 1]];
            Vector3 p3 = vertices[trianglesMesh[i + 2]];
            triangles[j] = new Triangle(p1, p2, p3);
            j++;
        }

        return triangles;

    }

    private AABB setAABB(Vector3Int structure)
    {
        Vector3 min = new Vector3(main.GetMeshtal.GetXstep[structure.x],
                                  main.GetMeshtal.GetYstep[structure.y],
                                  main.GetMeshtal.GetZstep[structure.z]);

        Vector3 max = new Vector3(main.GetMeshtal.GetXstep[structure.x + 1],
                                    main.GetMeshtal.GetYstep[structure.y + 1],
                                    main.GetMeshtal.GetZstep[structure.z + 1]);


        return new AABB(min, max);
    }

    private AABB getMeshtalAABB()
    {

        int xStructMax = main.GetMeshtal.GetXstep.Count;
        int yStructMax = main.GetMeshtal.GetYstep.Count;
        int zStructMax = main.GetMeshtal.GetZstep.Count;
        float xMin = main.GetMeshtal.GetXstep[0];
        float yMin = main.GetMeshtal.GetYstep[0];
        float zMin = main.GetMeshtal.GetZstep[0];
        float xMax = main.GetMeshtal.GetXstep[xStructMax - 1];
        float yMax = main.GetMeshtal.GetYstep[yStructMax - 1];
        float zMax = main.GetMeshtal.GetZstep[zStructMax - 1];

        Vector3 min = new Vector3(xMin, yMin, zMin);

        Vector3 max = new Vector3(xMax, yMax, zMax);

        if (main.GetMeshtal.isRotated)
        {
            min = main.GetMeshtal.rotationMatrix * min;
            max = main.GetMeshtal.rotationMatrix * max;
        }


        return new AABB(min, max);
    }


    #region SHADER TO EXCLUDE MESH OF BUILDING

    //private void setAndDispatchBuildingShader(AABB meshDomain, ref List<Triangle> trianglesWalls)
    //{

    //    MeshFilter[] meshFilters = building.transform.GetComponentsInChildren<MeshFilter>();

    //    int len = meshFilters.Select<MeshFilter, int>(filter => filter.mesh.triangles.Length / 3).Sum();

    //    List<Triangle> buildingsTriangles = new List<Triangle>();
    //    buildingsTriangles.Capacity = len;


    //    Mesh[] meshes = meshFilters.Select<MeshFilter, Mesh>(filter => filter.mesh).ToArray();
    //    //Mesh[] meshes = miao.Select<MeshCollider, Mesh>(filter => filter.sharedMesh).ToArray();


    //    var watch = new System.Diagnostics.Stopwatch();
    //    watch.Start();


    //    int index = 0;

    //    #region EXTRACT DATA FROM MESH
    //    using (var dataArray = Mesh.AcquireReadOnlyMeshData(meshes.ToList()))
    //    {
    //        for (int n = 0; n < dataArray.Length; n++)
    //        {
    //            var data = dataArray[n];
    //            var indices = data.GetIndexData<UInt16>();// data.GetIndexData<uint>();
    //            var verts = new NativeArray<Vector3>(meshes[n].vertexCount, Allocator.TempJob);
    //            data.GetVertices(verts);


    //            for (int i = 0; i < indices.Length; i += 3)
    //            {
    //                Vector3 p1 = Quaternion.Euler(-90, 180, 0) * verts[indices[i + 0]] * 0.01f;
    //                Vector3 p2 = Quaternion.Euler(-90, 180, 0) * verts[indices[i + 1]] * 0.01f;
    //                Vector3 p3 = Quaternion.Euler(-90, 180, 0) * verts[indices[i + 2]] * 0.01f;

    //                // buildingsTriangles[index] = new Triangle(p1, p2, p3);
    //                buildingsTriangles.Add(new Triangle(p1, p2, p3));
    //                index += 1;
    //            }


    //            verts.Dispose();
    //            indices.Dispose();
    //        }

    //    }
    //    #endregion


    //    setBufferBuilding(meshDomain, buildingsTriangles);
    //    int threadGroupSize = 64;
    //    int numThreads = Mathf.CeilToInt((float)len / (float)threadGroupSize);

    //    int[] isIntersected = new int[len];

    //    excludeMeshShader.SetBuffer(0, "buildingTriangles", buildingTriangleBuffer);
    //    excludeMeshShader.SetVector("centerAABB", new Vector4(meshDomain.Center.x, meshDomain.Center.y, meshDomain.Center.z, 0));
    //    excludeMeshShader.SetVector("extentAABB", new Vector4(meshDomain.Extent.x, meshDomain.Extent.y, meshDomain.Extent.z, 0));
    //    excludeMeshShader.SetBuffer(0, "intersects", buildingIntersection);
    //    excludeMeshShader.SetInt("_TriangleCount", len);

    //    excludeMeshShader.Dispatch(0, numThreads, 1, 1);

    //    buildingIntersection.GetData(isIntersected);

    //    for (int i = 0; i < isIntersected.Length; i++)
    //    {
    //        if (isIntersected[i] == 1) trianglesWalls.Add(buildingsTriangles[i]);
    //    }
    //    releaseBufferBuilding();

    //    Debug.Log($"Number of triangles to be looked now is {trianglesWalls.Count}");


    //}

    //private void setBufferBuilding(AABB boundary, List<Triangle> buildingTriangles)
    //{

    //    int size = sizeof(float) * 3 * 3;
    //    buildingTriangleBuffer = new ComputeBuffer(buildingTriangles.Count, size);
    //    triBuff[] tris = new triBuff[buildingTriangles.Count];
    //    for (int i = 0; i < buildingTriangles.Count; i++) { tris[i] = new triBuff(); tris[i].a = buildingTriangles[i].A; tris[i].b = buildingTriangles[i].B; tris[i].c = buildingTriangles[i].C; }
    //    buildingTriangleBuffer.SetData(tris);


    //    buildingIntersection = new ComputeBuffer(buildingTriangles.Count, sizeof(int));


    //}

    //private void releaseBufferBuilding()
    //{
    //    buildingTriangleBuffer.Dispose();
    //    buildingIntersection.Dispose();
    //}
    #endregion


    #region SHADER TO SET WALKABLE AABB


    private void setAndDispatchIntersection(AABB[] aabbWalkMesh, List<Triangle> trianglesWalk)
    {
        setBuffersWalk(aabbWalkMesh, trianglesWalk);

        int threadGroupSize = 256;
        int nVoxels = aabbWalkMesh.Length;
        int numThreads = Mathf.CeilToInt((float)nVoxels / (float)threadGroupSize);

        int[] isIntersected = new int[aabbWalkMesh.Length];


        intersectionShader.SetBuffer(0, "walkTriangles", triangleBuffer);
        intersectionShader.SetBuffer(0, "walkAABB", aabbBuffer);
        intersectionShader.SetBuffer(0, "intersects", intersection);
        intersectionShader.SetInt("_TriangleCount", trianglesWalk.Count);
        intersectionShader.SetInt("_AABBCount", nVoxels);

        intersectionShader.Dispatch(0, numThreads, 1, 1);


        intersection.GetData(isIntersected);

        AABB[] navigationAABB = new AABB[isIntersected.Sum()]; // THE WALKABLE AABB HAVE isIntersected = 1;

        int index = 0;
        for (int i = 0; i < isIntersected.Length; i++)
        {
            if (isIntersected[i] == 1) { navigationAABB[index] = aabbWalkMesh[i]; index++; }
        }
        releaseBuffersWalk();

        grid.navigationAABB = navigationAABB;
    }





    private void setBuffersWalk(AABB[] aabbWalkMesh, List<Triangle> trianglesWalk)
    {
        int size = sizeof(float) * 3 * 3;
        triangleBuffer = new ComputeBuffer(trianglesWalk.Count, size);
        triBuff[] tris = new triBuff[trianglesWalk.Count];
        for (int i = 0; i < trianglesWalk.Count; i++) { tris[i] = new triBuff(); tris[i].a = trianglesWalk[i].A; tris[i].b = trianglesWalk[i].B; tris[i].c = trianglesWalk[i].C; }
        triangleBuffer.SetData(tris);


        size = sizeof(float) * 3 * 2;
        aabbBuffer = new ComputeBuffer(aabbWalkMesh.Length, size);
        AABBbuff[] quads = new AABBbuff[aabbWalkMesh.Length];
        for (int i = 0; i < aabbWalkMesh.Length; i++) { quads[i] = new AABBbuff(); quads[i].center = aabbWalkMesh[i].Center; quads[i].extents = aabbWalkMesh[i].Extent; }
        aabbBuffer.SetData(quads);

        intersection = new ComputeBuffer(aabbWalkMesh.Length, sizeof(int));

    }

    private void releaseBuffersWalk()
    {
        triangleBuffer.Dispose();
        aabbBuffer.Dispose();
        intersection.Dispose();
    }

    #endregion

    struct triBuff
    {
        public float3 a, b, c;
    }

    struct AABBbuff
    {
        public float3 center;
        public float3 extents;
    };

}
