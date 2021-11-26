/*******************************
	Author : Mobiano.Hinx
	Email: laizhixin@wonderidea.com
********************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[RequireComponent(typeof(AnimateMeshBakeStl))]
public class Mesh2STLSaver : MonoBehaviour
{
    private bool reverseAxis = true;
    private bool CreateMeshesOnScene = false;

    private void OnGUI()
    {
        GUILayout.Label("This is an 'Editor Only Scripts' create by Hinx.");
        CreateMeshesOnScene = GUILayout.Toggle(CreateMeshesOnScene, "Create Meshes On Scene when shot.");
        reverseAxis = GUILayout.Toggle(reverseAxis, "Reverse Axis for Unity");
        if (GUILayout.Button($"take snap and save '{name}'"))
        {
            string outputFolder = UnityEditor.EditorUtility.SaveFolderPanel("select an save location", "", name);
            if (string.IsNullOrEmpty(outputFolder))
                return;
            var tool = GetComponent<AnimateMeshBakeStl>();
            var items = tool.TakeSnapshot();
            var singleMeshes = items[1];
            int singleMeshesCount = singleMeshes.Length;
            float total = singleMeshesCount + 1;
            string title = "converting..";
            EditorUtility.DisplayProgressBar(title, "begin convert combined mesh", 0);
            if (CreateMeshesOnScene)
                CreateMeshes(combined: items[0][0], separate: items[1]);
            string combinedSaveName = Path.Combine(outputFolder, $"combined_{name}.stl");
            saveMeshToStl(items[0][0], combinedSaveName);
            EditorUtility.DisplayProgressBar(title, "begin convert separated mesh", 1f / total);
            int index = 0;
            for (int i = 0; i < singleMeshesCount; i++)
            {
                var mesh = singleMeshes[i];
                string saveName = Path.Combine(outputFolder, $"separate_{mesh.name}.stl");
                saveMeshToStl(mesh, saveName);
                index++;
                EditorUtility.DisplayProgressBar(title, "begin convert separated mesh", (index + 1) / total);
            }

            //finshed;
            EditorUtility.ClearProgressBar();
            Application.OpenURL(outputFolder);
            Debug.Log("FINISHED.");
        }
    }

    /// <summary>
    /// Convert and save stl
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="location"></param>
    private void saveMeshToStl(Mesh mesh, string location)
    {

        //NEEDS TO OPTIMIZE IF YOU HAVE A LOT OF VERTICES
        STL stl = STL.CreateEmpty();
        stl.header = new STL_Header($"_hinx_stl_unity_{mesh.name}");

        int meshVerticesCount = mesh.vertices.Length;
        var triangles = mesh.triangles;
        int length3 = triangles.Length;
        if (length3 % 3 != 0)
            throw new Exception("error");
        int triangleCount = length3 / 3;
        stl.triangles = new STL_Triangle[triangleCount];
        Func<Vector3, float[]> convert = v =>
        {
            if (reverseAxis)
                return new[] { v.x, v.z, v.y };//x z y
            return new[] { v.x, v.y, v.z };//x y z
        };
        for (int i = 0; i < triangleCount; i++)
        {
            int p01 = triangles[i * 3];
            int p02 = triangles[i * 3 + 1];
            int p03 = triangles[i * 3 + 2];
            stl.triangles[i] = new STL_Triangle
            {
                AttrData = 0,
                Normal = convert(mesh.normals[p01]),//
                Vertex1 = convert(mesh.vertices[p01]),
                Vertex2 = convert(mesh.vertices[p02]),
                Vertex3 = convert(mesh.vertices[p03]),
            };
        }
        stl.TriangleCount = stl.triangles.Length;
        if (File.Exists(location))
            File.Delete(location);
        using (var fs = File.Create(location))
        {
            stl.SaveToBinary(fs);
            fs.Flush();
        }
    }


    private void CreateMeshes(Mesh combined, Mesh[] separate)
    {
        var obj1 = createMeshObj(combined);
        combined.RecalculateBounds();
        obj1.name = "combined";
        obj1.transform.position = Vector3.right * combined.bounds.size.x;

        var obj2 = new GameObject("separate");
        for (int i = 0; i < separate.Length; i++)
        {
            var child = createMeshObj(separate[i]);
            child.SetParent(obj2.transform);
        }
    }


    private Transform createMeshObj(Mesh mesh)
    {
        var go = new GameObject(mesh.name);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        return go.transform;
    }
}
