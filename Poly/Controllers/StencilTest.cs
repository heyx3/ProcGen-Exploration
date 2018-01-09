using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StencilTest : MonoBehaviour
{
    public int NPoints = 10;
    public float Radius = 0.75f,
                 Variance = 0.25f;
    public int RasterWidth = 512,
               RasterHeight = 512;

    public float GizmoPointRadius = 0.02f;

    public float SplitStrength = 0.125f;
    public bool ShouldSplit = false;

    public Shader StencilPass, RenderPass;

    [SerializeField]
    private PolyShape shape = new PolyShape(new PolyShape.Point[] { new PolyShape.Point(Vector2.zero, 1.0f),
                                                                    new PolyShape.Point(Vector2.one, 1.0f),
                                                                    new PolyShape.Point(Vector2.up, 1.0f) });
    private Mesh mesh_quad, mesh_shape;
    private RenderTexture rendTex;

    private void Awake()
    {
        GenerateShape();
        GenerateTex();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GenerateShape();
            GenerateTex();
        }
        else
        {
            GenerateTex();
        }
    }
    private void OnDrawGizmos()
    {
        if (shape == null)
            return;

        shape.DrawGizmos(transform.localToWorldMatrix, GizmoPointRadius);
        
        if (ShouldSplit)
        {
            ShouldSplit = false;
            shape.Subdivide((start, end, variance) =>
            {
                Vector2 tangent = (end - start).normalized,
                        normal = new Vector2(-tangent.y, tangent.x);
                Vector2 midPoint = (start + end) * 0.5f;
                
                return new PolyShape.SplitResult(midPoint + (normal * UnityEngine.Random.Range(-1.0f, 1.0f) * SplitStrength),
                                                 variance, variance);
            });
            GenerateTex();
        }
    }

    private void GenerateShape()
    {
        //Generate the points.
        PolyShape.Point[] verts = new PolyShape.Point[NPoints];
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
    }
    private void GenerateTex()
    {
        //Generate the mesh, with a line strip for the geometry.
        if (mesh_shape == null)
            mesh_shape = new Mesh();
        else
            mesh_shape.Clear();
        mesh_shape.vertices = shape.Points.Select(v => new Vector3(v.x, v.y, 0.01f)).ToArray();
        mesh_shape.SetIndices(shape.NPoints.CountSequence(1).ToArray(),
                              MeshTopology.LineStrip, 0);
        mesh_shape.UploadMeshData(false);

        //Also generate a quad mesh.
        if (mesh_quad == null)
        {
            mesh_quad = new Mesh();
            mesh_quad.vertices = new Vector3[] { new Vector3(-1.0f, -1.0f, 0.01f),
                                                 new Vector3(1.0f, -1.0f, 0.01f),
                                                 new Vector3(-1.0f, 1.0f, 0.01f),
                                                 new Vector3(1.0f, 1.0f, 0.01f) };
            mesh_quad.uv = new Vector2[] { new Vector2(0.0f, 0.0f),
                                           new Vector2(1.0f, 0.0f),
                                           new Vector2(0.0f, 1.0f),
                                           new Vector2(1.0f, 1.0f) };
            mesh_quad.triangles = new int[] { 0, 3, 1, 0, 2, 3 };
            mesh_quad.UploadMeshData(false);
        }

        //Set up the render texture.
        if (rendTex == null || rendTex.width != RasterWidth || rendTex.height != RasterHeight)
        {
            rendTex = new RenderTexture(RasterWidth, RasterHeight, 24,
                                        RenderTextureFormat.ARGB32,
                                        RenderTextureReadWrite.Linear);
            rendTex.Create();
        }

        //Render the shape as white on a black transparent background.
        RenderTexture.active = rendTex;
        GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 1.0f));
        Material mat = new Material(StencilPass);
        mat.SetVector("_ShapeMin", shape.Min);
        mat.SetVector("_ShapeMax", shape.Max);
        mat.SetVector("_PointOnShape",
                      MathF.Lerp(-1.0f, 1.0f,
                                 MathF.InverseLerp(shape.Min, shape.Max, shape.GetPoint(0))));
        mat.SetPass(0);
        Graphics.DrawMeshNow(mesh_shape, Matrix4x4.identity);
        mat.shader = RenderPass;
        mat.color = Color.white;
        mat.SetPass(0);
        Graphics.DrawMeshNow(mesh_quad, Matrix4x4.identity);
        RenderTexture.active = null;

        //Output the texture somewhere.
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = rendTex;
            var mf = renderer.GetComponent<MeshFilter>();
            if (mf != null)
                mf.mesh = mesh_quad;
        }
    }
}