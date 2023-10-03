using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using Utils;
using Graph = Geometry.Graph;
using Grid = Geometry.Grid;
using Unity.VisualScripting;
using Unity.IO.LowLevel.Unsafe;
using System.Diagnostics.Tracing;
using UnityEngine.InputSystem;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class buildPath : MonoBehaviour
{
    public IntersectMesh IM;
    public Main main;
    public GameObject Player;
    [SerializeField]
    private GameObject Line;
    [SerializeField]
    private goTosetPath gtp;

    bool coroutineRunning;

    [SerializeField]
    private GameObject circle;
    [SerializeField]
    private GameObject textLoading;

    [SerializeField]
    private pathSetter pathSetter;

    int oldvalue = -1;
    [SerializeField]
    private Material normalMaterial;


    GameObject line;

    // List<AABB> navigationAABB = new List<AABB>();
    Grid grid;
    Graph graph;


    private void Start()
    {
        coroutineRunning = false;
        line = new GameObject();
    }

    public void OnEnable()
    {
        circle.SetActive(false);
    }

    //public GameObject getSelectedLine()
    //{
    //    if (lineHolder.Count > 0)
    //        return lineHolder[listPath.value];
    //    else
    //        return null;
    //}

    public void makePath()
    {


        if (gtp.sphereList.Count < 2) return;

        line = new GameObject();

        #region set start and end point
        Vector3 startPos = gtp.sphereList[1].transform.position;

        Vector3 endPos = gtp.sphereList[0].transform.position;
        #endregion


       // #region build grid

        float dimension = 1;

        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        IM.buildWalkMesh(startPos.y, dimension);
        grid = IM.getNevigationGrid();
        Debug.Log($"Build grid time: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();


        watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        graph = new Graph(grid.navigationAABB);
        Debug.Log($"Build graph time: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();



        #region find shortest path

        Vector3Int structStart = new Vector3Int(grid.IndexOf(startPos.x, 1),
                                        grid.IndexOf(startPos.y, 2),
                                        grid.IndexOf(startPos.z, 3));

        int start = graph.getIndex(structStart);
        Debug.Log($"Position start {startPos}");
        Debug.Log($"Structure start {structStart} at index {start}");

        Vector3Int structEnd = new Vector3Int(grid.IndexOf(endPos.x, 1),
                                       grid.IndexOf(endPos.y, 2),
                                       grid.IndexOf(endPos.z, 3));

        int end = graph.getIndex(structEnd);
        Debug.Log($"Position end {endPos}");
        Debug.Log($"Structure end {structEnd} at index {end}");


        watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        #region OLD


        List<int> path = Astar(start, end);


        if (path == null) { Debug.Log("OUT OF MARGIN"); return; }

        Debug.Log($"Execution Astar: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();

        Debug.Log($"len nav grid {grid.navigationAABB.Length}");

        List<AABB> pathAABB = new List<AABB>();
        foreach (int i in path) { pathAABB.Add(grid.navigationAABB[i]); }

        buildLine(pathAABB);

        #endregion

        //IEnumerator coroutine = calculating(start, end);
        //StartCoroutine(coroutine);
        Debug.Log($"Execution Astar: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();
        #endregion
    }

    public bool IscCoroutinRunning()
    {
        return coroutineRunning;
    }

    IEnumerator calculating(int start, int goal)
    {
        // reference: https://www.redblobgames.com/pathfinding/a-star/introduction.html

        circle.SetActive(true);
        coroutineRunning = true;
        yield return null;
        // PriorityQueue<int> frontier = new PriorityQueue<int>();
        PriorityQueue<int, double> frontier = new PriorityQueue<int, double>(8);
        frontier.Enqueue(start, 0);

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> costSoFar = new Dictionary<int, float>();


        cameFrom[start] = -1;
        costSoFar[start] = 0;

        List<int> miao = new List<int>();


        int count = 0;
        int numSave = grid.navigationAABB.Length * 100;

        while (frontier.Count != 0)
        {
            count++;
            int current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (int neighbour in graph.getNeighbours(current))
            {
                float newCost = costSoFar[current] + getCost(neighbour);

                if ((!costSoFar.ContainsKey(neighbour)) || newCost < (costSoFar[neighbour]))
                {
                    costSoFar[neighbour] = newCost;

                    double priority = newCost + Heuristic(goal, neighbour);
                    frontier.Enqueue(neighbour, priority);
                    cameFrom[neighbour] = current;
                }
            }

            circle.transform.Rotate(new Vector3(0, 0, 5));
            yield return null;  
            if (count > numSave) { break; }

        }

        circle.SetActive(false);
        yield return null;
        
        
        if (count > numSave) { yield break; }

        // Reconstruct PATH
        int reconstruct_current = goal;
        List<int> path = new List<int>();
        while (reconstruct_current != start)
        {
            // Debug.Log(reconstruct_current);
            path.Add(reconstruct_current);
            reconstruct_current = cameFrom[reconstruct_current];
        }
        path.Add(start);


        if (path == null) { Debug.Log("OUT OF MARGIN"); yield break; }

        List<AABB> pathAABB = new List<AABB>();
        foreach (int i in path) { pathAABB.Add(grid.navigationAABB[i]); }

        coroutineRunning = false;
        buildLine(pathAABB);
        yield return null;

    }

    private void buildLine(List<AABB> pathAABB)
    {

        // SMOOTH LINE

        Vector3[] curvePoints = BezierCurve.smoothLine(pathAABB.Select(voxel => voxel.Center).ToArray());
        //


        line = new GameObject();
        line = Instantiate(Line);
        LineRenderer lrend = line.GetComponent<LineRenderer>(); lrend.positionCount = curvePoints.Length;
        int index = 0;

        foreach (Vector3 p in curvePoints)
        { lrend.SetPosition(index, p); index++; }

        pathSetter.setLine(line);

    }





    List<int> Astar(int start, int goal)
    {
        // reference: https://www.redblobgames.com/pathfinding/a-star/introduction.html


        // PriorityQueue<int> frontier = new PriorityQueue<int>();
        PriorityQueue<int,double> frontier = new PriorityQueue<int,double>(8);
        frontier.Enqueue(start, 0);

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> costSoFar = new Dictionary<int, float>();


        cameFrom[start] = -1;
        costSoFar[start] = 0;

        List<int> miao = new List<int>();
       

        int count = 0;
        int numSave = grid.navigationAABB.Length * 100;

        while (frontier.Count != 0)
        {
            count++;
            int current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (int neighbour in graph.getNeighbours(current))
            {
                float newCost = costSoFar[current] + getCost(neighbour);
                
                if ((!costSoFar.ContainsKey(neighbour)) || newCost < (costSoFar[neighbour]))
                {
                    costSoFar[neighbour] = newCost;

                    double priority = newCost + Heuristic(goal, neighbour);
                    frontier.Enqueue(neighbour, priority);
                    cameFrom[neighbour] = current;
                }
            }

            if (count > numSave) { break; }

        }

        if (count > numSave) { return null; }

        // Reconstruct PATH
        int reconstruct_current = goal;
        List<int> path = new List<int>();
        while (reconstruct_current != start)
        {
            // Debug.Log(reconstruct_current);
            path.Add(reconstruct_current);
            reconstruct_current = cameFrom[reconstruct_current];
        }
        path.Add(start);

        return path;

    }

    double Heuristic(int a, int b)
    {
        Vector3 center1 = grid.navigationAABB[a].Center;
        Vector3 center2 = grid.navigationAABB[b].Center;

        // Manhattan distance
        return Math.Abs(center1.x - center2.x) + Math.Abs(center1.z + center2.z);

    }
    float getCost(int index)
    {

        Vector3 center = grid.navigationAABB[index].Center;

        return main.GetMeshtal.GetFlux(center);


    }

}
