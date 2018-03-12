using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WatercolorConverter.Tests
{
    /// <summary>
    /// Tests the various filters.
    /// </summary>
    public class FilterTester : MonoBehaviour
    {
        public Texture2D InitialTex;
		
		public int Gaussian_NIterations = 3;
		public Filters.GaussianBlurQuality GreyscaleInput;
		public Filters.GreyscaleTypes EdgeDetectInput;
		public Filters.EdgeFilters CannyInput;
		public float Canny_MinWeakEdge = 0.0f,
					 Canny_MaxWeakEdge = 0.1f;

		[SerializeField]
		private bool recompute = false;

		private Texture out_gauss5, out_gauss9,
						out_luminanceGrey,
						out_sobel, out_prewitt, out_sobelDisplay, out_prewittDisplay,
					    out_canny, out_cannyDisplay;


		/// <summary>
		/// Generates the blurred textures.
		/// </summary>
        public void Start()
        {
			//Run the gaussian filters.
			out_gauss5 = InitialTex;
			out_gauss9 = InitialTex;
			for (int i = 0; i < Gaussian_NIterations; ++i)
			{
				out_gauss5 = Filters.Blur(Filters.GaussianBlurQuality.Low, out_gauss5);
				out_gauss9 = Filters.Blur(Filters.GaussianBlurQuality.High, out_gauss9);
			}
			
			//Run the greyscale filters.
			Texture greyInput;
			switch (GreyscaleInput)
			{
				case Filters.GaussianBlurQuality.Low:
					greyInput = out_gauss5;
					break;
				case Filters.GaussianBlurQuality.High:
					greyInput = out_gauss9;
					break;
				default: throw new NotImplementedException(GreyscaleInput.ToString());
			}
			out_luminanceGrey = Filters.Greyscale(Filters.GreyscaleTypes.PreserveLuminance,
												  greyInput, RenderTextureFormat.ARGB32);

			//Run the edge detection filters.
			Texture edgeInput;
			switch (EdgeDetectInput)
			{
				case Filters.GreyscaleTypes.PreserveLuminance:
					edgeInput = out_luminanceGrey;
					break;
				default: throw new NotImplementedException(EdgeDetectInput.ToString());
			}
			out_sobel = Filters.EdgeFilter(Filters.EdgeFilters.Sobel, edgeInput);
			out_prewitt = Filters.EdgeFilter(Filters.EdgeFilters.Prewitt, edgeInput);
			//Filter the results for display.
			out_sobelDisplay = Filters.PackVectors(out_sobel, new Vector4(1, 1, 1, 0));
			out_prewittDisplay = Filters.PackVectors(out_prewitt, new Vector4(1, 1, 1, 0));

			//Run the "round angles" filter.
			Texture cannyInput;
			switch (CannyInput)
			{
				case Filters.EdgeFilters.Sobel:
					cannyInput = out_sobel;
					break;
				case Filters.EdgeFilters.Prewitt:
					cannyInput = out_prewitt;
					break;
				default: throw new NotImplementedException(CannyInput.ToString());
			}
			out_canny = Filters.CannyEdgeFilter(cannyInput, Canny_MinWeakEdge, Canny_MaxWeakEdge);
			//Filter the results for display.
			out_cannyDisplay = Filters.PackVectors(out_canny, new Vector4(1, 1, 0, 0));
        }
		public void Update()
		{
			if (recompute)
			{
				recompute = false;
				Start();
			}
		}

		private Vector2 scrollPos = Vector2.zero;
		private void OnGUI()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos,
												  GUILayout.MaxWidth(Screen.width - 50));
			{
				GUILayoutTex("Initial", InitialTex);
				GUILayoutTex("Gaussian 5", out_gauss5);
				GUILayoutTex("Gaussian 9", out_gauss9);
				GUILayoutTex("Luminance Grey", out_luminanceGrey);
				GUILayoutTex("Sobel", out_sobelDisplay);
				GUILayoutTex("Prewitt", out_prewittDisplay);
				GUILayoutTex("Canny", out_cannyDisplay);

			}
			GUILayout.EndScrollView();
		}
		private void GUILayoutTex(string name, Texture tex)
		{
			GUILayout.Label(name);
			GUILayout.Box(tex, GUILayout.Width(tex.width), GUILayout.Height(tex.height));
			GUILayout.Space(30.0f);
		}
	}
}