using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class PolyShapeTestController : MonoBehaviour
{
    public int NPoints = 10;
    public float Radius = 1.0f,
                 Variance = 0.25f;

    public float GizmoPointRadius = 0.02f;
    public bool Debug_TryAgain = false;

    private PolyShape shape;

    private void Awake()
    {
        GenerateMesh();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            GenerateMesh();
    }
    private void OnDrawGizmos()
    {
        if (shape != null)
            shape.DrawGizmos(transform.localToWorldMatrix, GizmoPointRadius);

        if (Debug_TryAgain)
        {
            Debug_TryAgain = false;

            Vector3[] verts3 = null;
            int[] tris = null;
            shape.Triangulate(ref verts3, ref tris);
        }
    }

    private void GenerateMesh()
    {
        //Generate the points.
        var verts = new PolyShape.Point[NPoints];
        float radianIncrement = Mathf.PI * -2.0f / NPoints;
        float minDist = Radius - Variance,
              maxDist = Radius + Variance;
        for (int i = 0; i < verts.Length; ++i)
        {
            float radians = i * radianIncrement;
            Vector2 pos = new Vector2(Mathf.Cos(radians),
                                      Mathf.Sin(radians));
            pos *= Mathf.Lerp(minDist, maxDist, UnityEngine.Random.value);

            verts[i] = new PolyShape.Point(pos, 1.0f);
        }
        shape = new PolyShape(verts);

        Vector3[] verts3 = null;
        int[] tris = null;
        shape.Triangulate(ref verts3, ref tris);

        var mf = GetComponent<MeshFilter>();
        if (mf.mesh == null)
            mf.mesh = new Mesh();
        mf.mesh.vertices = verts3;
        mf.mesh.triangles = tris;
        mf.mesh.UploadMeshData(false);
    }
}
