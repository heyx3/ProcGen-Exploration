using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WatercolorConverter
{
	public static class GaussianBlur
	{
		/// <summary>
		/// The various quality levels for the blur.
		/// Note that higher quality means worse performance.
		/// </summary>
		public enum Qualities
		{
			/// <summary>
			/// A 5x5 blur filter, using a total of 6 texture samples per pixel.
			/// </summary>
			Five,
			/// <summary>
			/// A 9x9 blur filter, using a total of 10 texture samples per pixel.
			/// </summary>
			Nine,
		}

		/// <summary>
		/// Does a Gaussian blur, allocating and returning a new texture to hold the result.
		/// </summary>
		public static RenderTexture Blur(Qualities quality, Texture inputTex)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height,
										   0, RenderTextureFormat.ARGB32);
			outTex.filterMode = FilterMode.Bilinear;
			outTex.wrapMode = TextureWrapMode.Clamp;

			Blur(quality, inputTex, outTex);
			return outTex;
		}

		/// <summary>
		/// Does a Gaussian blur, storing the result in the given output texture.
		/// </summary>
		public static void Blur(Qualities quality, Texture inputTex, RenderTexture outTex)
		{
			//Get the shader/material for this blur level.
			if (!allocs.ContainsKey(quality))
				allocs.Add(quality, new Allocations(quality));
			var data = allocs[quality];

			//Allocate an intermediate texture for the two-pass blur.
			var tempTex = RenderTexture.GetTemporary(outTex.width, outTex.height, 0, outTex.format);
			tempTex.filterMode = FilterMode.Bilinear;
			tempTex.wrapMode = TextureWrapMode.Clamp;

			//Do the blur.
			Graphics.Blit(inputTex, tempTex, data.Mat, 0);
			Graphics.Blit(tempTex, outTex, data.Mat, 1);

			//Clean up.
			RenderTexture.ReleaseTemporary(tempTex);
		}
		

		private struct Allocations
		{
			public Shader Shad;
			public Material Mat;
			public Allocations(Qualities quality)
			{
				string shaderName = null;
				switch (quality)
				{
					case Qualities.Five:
						shaderName = "Poly/Gaussian5";
						break;
					case Qualities.Nine:
						shaderName = "Poly/Gaussian9";
						break;
					default:
						throw new NotImplementedException("No shader for quality: " + quality);
				}

				Shad = Shader.Find(shaderName);
				Mat = new Material(Shad);
			}
		}
		private static Dictionary<Qualities, Allocations> allocs =
			new Dictionary<Qualities, Allocations>();
	}
}
