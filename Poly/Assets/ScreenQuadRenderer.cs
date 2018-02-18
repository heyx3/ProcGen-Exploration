using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Allows for easy rendering of screen-space squares, with a texture and/or color.
/// </summary>
public class ScreenQuadRenderer
{
    /// <summary>
    /// The material used for this class's rendering.
    /// </summary>
    public static Material ScreenColorTexMat
    {
        get
        {
            if (renderMat == null)
                renderMat = new Material(Shader.Find("Unlit/ScreenColorTex"));
            return renderMat;
        }
    }
    /// <summary>
    /// Spans the range [-1, +1], unlike the default Unity quad which spans [-0.5, +0.5].
    /// </summary>
    public static Mesh QuadMesh
    {
        get
        {
            if (quadMesh == null)
            {
                quadMesh = new Mesh();
                quadMesh.vertices = new Vector3[] { new Vector3(-1.0f, -1.0f, 0.01f),
                                                    new Vector3(1.0f, -1.0f, 0.01f),
                                                    new Vector3(-1.0f, 1.0f, 0.01f),
                                                    new Vector3(1.0f, 1.0f, 0.01f) };
                quadMesh.uv = new Vector2[] { new Vector2(0.0f, 0.0f),
                                              new Vector2(1.0f, 0.0f),
                                              new Vector2(0.0f, 1.0f),
                                              new Vector2(1.0f, 1.0f) };
                quadMesh.triangles = new int[] { 0, 3, 1,   0, 2, 3 };
                quadMesh.UploadMeshData(false);
            }

            return quadMesh;
        }
    }

    private static Material renderMat = null;
    private static Mesh quadMesh = null;


    public BlendMode SrcBlend = BlendMode.SrcAlpha,
                     DstBlend = BlendMode.OneMinusSrcAlpha;
    public CullMode Cull = CullMode.Off;
    public bool ZWrite = false;
    public CompareFunction ZTest = CompareFunction.Always;

    public Color Color = Color.white;
    public Texture Tex = null;


    /// <summary>
    /// Turns off culling and depth-testing.
    /// </summary>
    public void TurnOffTests()
    {
        Cull = CullMode.Off;
        ZWrite = false;
        ZTest = CompareFunction.Always;
    }

    public void UsePlainColor(Color col)
    {
        Color = col;
        Tex = null;
    }
    public void UsePlainTex(Texture tex)
    {
        Tex = tex;
        Color = Color.white;
    }
    public void UseTintTex(Texture tex, Color tint)
    {
        Tex = tex;
        Color = tint;
    }

    public void UseAlphaBlending()
    {
        SrcBlend = BlendMode.SrcAlpha;
        DstBlend = BlendMode.OneMinusSrcAlpha;
    }
    public void UseOpaqueBlending()
    {
        SrcBlend = BlendMode.One;
        DstBlend = BlendMode.Zero;
    }
    public void UseAdditiveBlending()
    {
        SrcBlend = BlendMode.One;
        DstBlend = BlendMode.One;
    }


    public void Draw(Vector2 pos, float rotDegrees, float scale)
    {
        Draw(pos, rotDegrees, new Vector2(scale, scale));
    }
    public void Draw(Vector2 pos, float rotDegrees, Vector2 scale)
    {
        var mtl = ScreenColorTexMat;
        var msh = QuadMesh;
        var mtx = Matrix4x4.TRS(new Vector3(pos.x, pos.y, 0.01f),
                                Quaternion.AngleAxis(rotDegrees, new Vector3(0.0f, 0.0f, 1.0f)),
                                new Vector3(scale.x, scale.x, 1.0f));

        mtl.SetInt("_SrcBlend", (int)SrcBlend);
        mtl.SetInt("_DstBlend", (int)DstBlend);
        mtl.SetInt("_Cull", (int)Cull);
        mtl.SetInt("_ZWrite", ZWrite ? 1 : 0);
        mtl.SetInt("_ZTest", (int)ZTest);
        mtl.color = Color;
        mtl.mainTexture = Tex;

        mtl.SetPass(0);
        Graphics.DrawMeshNow(msh, mtx);
    }
}