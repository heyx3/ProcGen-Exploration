using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Tests
{
	/// <summary>
	/// Draws GPU-generated PolyShapes, as Gizmos and as a GUI texture.
	/// </summary>
	public class PolyShapeGPUTester : MonoBehaviour
	{
		public PolyShapeGPU Shape;
		public int NVariations = 3,
				   NBlotchVariations = 50;
		public int RenderSize = 256,
				   BlotchRenderSize = 512;
		public float GizmoSphereRadius = -1.0f;
		public Vector2 BlotchOffset = Vector2.zero,
					   BlotchScale = Vector2.one;
		public Color BlotchColor = new Color(1.0f, 0.0f, 0.0f, 0.02f),
					 BlotchBackColor = new Color(1.0f, 0.95f, 0.8f, 1.0f);

		public UnityEngine.Rendering.BlendMode BlotchBlendSrc = UnityEngine.Rendering.BlendMode.SrcAlpha,
											   BlotchBlendDest = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
		public UnityEngine.Rendering.BlendOp BlotchBlendOp = UnityEngine.Rendering.BlendOp.Add;

		private PolyShape[] cpuShapes;
		private RenderTexture[] cpuShapeRenders;
		private RenderTexture blotch;
		private Vector2 guiScrollPos = Vector2.zero;


		private void Awake()
		{
			Shape.Reset();

			var brush = new WaterColorBrush();
			brush.RenderSize = RenderSize;

			var texBuffer = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);

			cpuShapes = new PolyShape[NVariations + 1];
			cpuShapeRenders = new RenderTexture[cpuShapes.Length];
			for (int i = 0; i < cpuShapes.Length; ++i)
			{
				cpuShapes[i] = Shape.MakeCPUVersion(i > 0, texBuffer);
				cpuShapeRenders[i] = new RenderTexture(brush.RenderSize, brush.RenderSize, 24,
													   RenderTextureFormat.ARGBFloat,
													   RenderTextureReadWrite.Linear);
				cpuShapeRenders[i].Create();
				brush.Render(cpuShapes[i], cpuShapeRenders[i]);
			}

			blotch = new RenderTexture(BlotchRenderSize, BlotchRenderSize, 24,
									   RenderTextureFormat.ARGBFloat,
									   RenderTextureReadWrite.Linear);
			blotch.Create();
			RenderTexture.active = blotch;
			GL.Clear(true, true, BlotchBackColor);
			WaterColorBrushGPU.RenderFull(Shape, blotch, new Mesh(), NBlotchVariations,
										  BlotchOffset, BlotchScale, BlotchColor,
										  BlotchBlendSrc, BlotchBlendDest, BlotchBlendOp);
		}

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying)
				return;

			float gizmoRadius = (GizmoSphereRadius < 0.0f) ? float.NaN : GizmoSphereRadius;

			for (int i = 0; i < cpuShapes.Length; ++i)
				cpuShapes[i].DrawGizmos(Matrix4x4.Translate(new Vector3(0.0f, 0.0f, i)), gizmoRadius);
		}
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();

			if (GUILayout.Button("Regenerate"))
			{
				//Change the seed.
				var rng = new System.Random(Mathf.RoundToInt(Shape.Seed * 2342.23323f));
				rng.Next();
				Shape.Seed = (float)rng.NextDouble() * 10.0f;

				Awake();
			}

			GUILayout.Space(10.0f);

			guiScrollPos = GUILayout.BeginScrollView(guiScrollPos, GUILayout.MinWidth(RenderSize + 10.0f));
			for (int i = 0; i < cpuShapes.Length; ++i)
			{
				string name;
				if (i == 0)
					name = "Base";
				else
					name = "Variation " + i;

				GUILayout.Label(name);
				GUILayout.Box(cpuShapeRenders[i],
							  GUILayout.Width(cpuShapeRenders[i].width),
							  GUILayout.Height(cpuShapeRenders[i].height));

				GUILayout.Space(15.0f);
			}
			GUILayout.EndScrollView();

			GUILayout.EndVertical();

			GUILayout.Box(blotch, GUILayout.Width(blotch.width), GUILayout.Height(blotch.height));

			GUILayout.EndHorizontal();
		}
	}
}
