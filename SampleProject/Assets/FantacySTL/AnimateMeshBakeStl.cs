using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateMeshBakeStl : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer[] targets;
#if UNITY_EDITOR

    private void Reset()
    {
        targets = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

#endif

    private Mesh[][] takeAnSnap()
    {
        int size = targets.Length;
        List<MeshFilter> filters = new List<MeshFilter>();
        Matrix4x4[] localMatrixs = new Matrix4x4[size];
        CombineInstance[] combine = new CombineInstance[size];
        List<Mesh> singleMeshes = new List<Mesh>();

        int i = 0;
        for (; i < size; i++)
        {
            Mesh target = new Mesh();
            var rdn = targets[i];
            localMatrixs[i] = rdn.transform.localToWorldMatrix;
            rdn.BakeMesh(target);
            target.name = $"bk_{i }_{rdn.sharedMesh.name}";
            combine[i] = new CombineInstance { transform = localMatrixs[i], mesh = target };
            singleMeshes.Add(target);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        return new Mesh[][] {
            new []{ combinedMesh},
           singleMeshes.ToArray()
        };
    }


    public Mesh[][] TakeSnapshot()
    {
        return takeAnSnap();
    }

}
