using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Generates and renders watercolor blotches entirely on the GPU.
/// </summary>
[Serializable]
public class WaterColorBrushGPU
{
	private static Material shapeRenderMat;


	/// <summary>
	/// Renders this shape into the given texture.
	/// </summary>
	/// <param name="outputTex">
	/// The texture that will contain the result.
	/// Must have stencil bits in the depth buffer.
	/// Its depth/stencil buffer will be modified during rendering.
	/// </param>
	/// <param name="meshBuffer">
	/// A mesh to use during rendering.
	/// If the mesh does not have the right number of vertices, it will be updated.
	/// </param>
	/// <param name="offset">
	/// The offset of this shape in the output texture.
	/// </param>
	/// <param name="scale">
	/// The scale of this shape in the output texture.
	/// </param>
	public static void Render(Texture shapeTex, RenderTexture outputTex, Mesh meshBuffer,
							  Vector2 offset, Vector2 scale, Color color,
							  BlendMode blendSrc = BlendMode.SrcAlpha,
							  BlendMode blendDest = BlendMode.OneMinusSrcAlpha,
							  BlendOp blendOp = BlendOp.Add)
	{
		//Make sure we have the Material/shaders for this.
		if (shapeRenderMat == null)
		{
			var shader = Shader.Find("PolyShape/Renderer");
			if (shader == null)
				throw new InvalidOperationException("No Renderer shader found!");

			shapeRenderMat = new Material(shader);
		}

		//Reset the mesh if it's not correct.
		int nVerts = shapeTex.width;
		if (meshBuffer.GetTopology(0) != MeshTopology.LineStrip ||
			meshBuffer.vertexCount != nVerts)
		{
			var verts = new Vector3[nVerts];
			var inds = new int[nVerts];
			for (int i = 0; i < nVerts; ++i)
			{
				//Set the vertex's X position to be
				//    the corresponding texel in the shape texture.
				verts[i] = new Vector3((i + 0.5f) / nVerts, 0.0f, 0.0f);
				inds[i] = i;
			}

			meshBuffer.vertices = verts;
			meshBuffer.SetIndices(inds, MeshTopology.LineStrip, 0);
			meshBuffer.UploadMeshData(true);
		}

		//Prepare the material/shape texture.
		shapeTex.filterMode = FilterMode.Point;
		shapeTex.wrapMode = TextureWrapMode.Repeat;
		shapeRenderMat.mainTexture = shapeTex;
		shapeRenderMat.SetVector("_ShapeOffsetAndScale",
								 new Vector4(offset.x, offset.y, scale.x, scale.y));
		shapeRenderMat.color = color;
		shapeRenderMat.SetInt("_BlendSrc", (int)blendSrc);
		shapeRenderMat.SetInt("_BlendDest", (int)blendDest);
		shapeRenderMat.SetInt("_BlendOp", (int)blendOp);

		//Render the passes.
		var oldRendTex = RenderTexture.active;
		RenderTexture.active = outputTex;
		GL.Clear(true, false, Color.clear);
		shapeRenderMat.SetPass(0);
		Graphics.DrawMeshNow(meshBuffer, Matrix4x4.identity);
		shapeRenderMat.SetPass(1);
		Graphics.DrawMeshNow(ScreenQuadRenderer.QuadMesh, Matrix4x4.identity);
		RenderTexture.active = oldRendTex;
	}
	/// <summary>
	/// Renders a full watercolor brush into the given texture.
	/// </summary>
	/// <param name="outputTex">
	/// The RenderTexture used to hold the result.
	/// Must have stencil bits in the depth buffer.
	/// Its depth/stencil buffer will be modified during rendering.
	/// </param>
	/// <param name="nVariants">
	/// The number of individual shapes to blend together.
	/// </param>
	/// <param name="color">
	/// The color of each variant.
	/// </param>
	public static void RenderFull(PolyShapeGPU shape, RenderTexture outputTex, Mesh meshBuffer,
								  int nVariants, Vector2 offset, Vector2 scale, Color color,
								  BlendMode blendSrc = BlendMode.SrcAlpha,
								  BlendMode blendDest = BlendMode.OneMinusSrcAlpha,
								  BlendOp blendOp = BlendOp.Add)
	{
		var shapeVariantTex = RenderTexture.GetTemporary(shape.VariationShapeSize, 1, 0,
														 RenderTextureFormat.ARGBFloat,
														 RenderTextureReadWrite.Linear);
		shapeVariantTex.filterMode = FilterMode.Point;
		shapeVariantTex.wrapMode = TextureWrapMode.Repeat;

		for (int i = 0; i < nVariants; ++i)
		{
			shape.GenerateVariation(shapeVariantTex);
			Render(shapeVariantTex, outputTex, meshBuffer, offset, scale,
				   color, blendSrc, blendDest, blendOp);
		}

		RenderTexture.ReleaseTemporary(shapeVariantTex);
	}
}