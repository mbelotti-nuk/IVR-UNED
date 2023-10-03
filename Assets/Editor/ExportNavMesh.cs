using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;

public class ExportNavMesh : MonoBehaviour
{
    [MenuItem("Tool/Export NavMesh")]
    public static void Export()
    {
        var navMeshTriangulation = NavMesh.CalculateTriangulation();

        #region write obj file


        //????
        // var scene = SceneManager.GetActiveScene();
        // var sceneFolder = Path.GetDirectoryName(scene.path);
        // var modelFileFolder = Path.Combine(sceneFolder, scene.name);
        // if (!AssetDatabase.IsValidFolder(modelFileFolder))
        // {
        //    var guid = AssetDatabase.CreateFolder(sceneFolder, scene.name);
        //    if (string.IsNullOrEmpty(guid))
        //    {
        //        Debug.LogError($"{modelFileFolder} create fail");
        //        return;
        //    }
        // }


        //var modelFilePath = Path.Combine(modelFileFolder, "NavMeshModel.obj");
        //Debug.Log($"NavMeshModel path: {modelFilePath}");

        //using (var sw = new StreamWriter(modelFilePath))
        //{
        //??
        //    for (var i = 0; i < navMeshTriangulation.vertices.Length; i++)
        //    {
        //        var vert = navMeshTriangulation.vertices[i];
        //        sw.WriteLine($"v  {-vert.x} {vert.y} {vert.z}"); 
        //    }

        // sw.WriteLine("Plane1");

        //??
        //     for (var i = 0; i < navMeshTriangulation.indices.Length; i += 3)
        //    {
        //        var indices = navMeshTriangulation.indices;
        //        sw.WriteLine($"f {(indices[i] + 1)} {(indices[i + 1] + 1)} {(indices[i + 2] + 1)}");
        //    }
        //}

        #endregion


        int numTris = navMeshTriangulation.indices.Length/3;

        Debug.Log("Number of tris: " + numTris.ToString());
        Debug.Log("Number of vertices: " + navMeshTriangulation.vertices.Length.ToString());
        Debug.Log("Number of indices: " + navMeshTriangulation.indices.Length.ToString());


        Mesh walkableMesh = new Mesh();
        //walkableMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        walkableMesh.vertices = navMeshTriangulation.vertices;
        walkableMesh.triangles = navMeshTriangulation.indices;


        // SAVE MESH OBJECT
        // var savePath = "Assets/" + "WalkableMesh" + ".asset";
        // Debug.Log("Saved Mesh to:" + savePath);
        // AssetDatabase.CreateAsset(walkableMesh, savePath);


        GameObject gameObject = new GameObject("walk_mesh", typeof(MeshFilter), typeof(MeshRenderer));
        MeshCollider meshCollider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        gameObject.GetComponent<MeshFilter>().mesh = walkableMesh;
        meshCollider.sharedMesh = walkableMesh;
        gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;


        gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);

        //var savePath = "Assets/" + "walk_Mesh" + ".asset";
        //Debug.Log("Saved Mesh to:" + savePath);
        //AssetDatabase.CreateAsset(gameObject, savePath);


        //AssetDatabase.Refresh();
        Debug.Log("NavMesh Exported");
    }
}
