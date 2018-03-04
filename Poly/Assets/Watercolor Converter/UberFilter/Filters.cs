using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace WatercolorConverter
{
	/// <summary>
	/// Provides easy access to a number of image filters.
	/// </summary>
	public static class Filters
	{
		#region Gaussian blur

		public enum GaussianBlurQuality
		{
			/// <summary>
			/// A 5x5 blur filter, using 6 texture samples per pixel.
			/// </summary>
			Low,
			/// <summary>
			/// A 9x9 blur filter, using 10 texture samples per pixel.
			/// </summary>
			High,
		}

		public static RenderTexture Blur(GaussianBlurQuality quality, Texture inputTex,
										 RenderTextureFormat outFormat = RenderTextureFormat.ARGB32,
										 TextureWrapMode wrapMode = TextureWrapMode.Clamp)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0, outFormat,
										   RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Bilinear;
			outTex.wrapMode = wrapMode;

			Blur(quality, inputTex, outTex, wrapMode);

			return outTex;
		}

		public static void Blur(GaussianBlurQuality quality,
								Texture input, RenderTexture output,
								TextureWrapMode wrapMode = TextureWrapMode.Clamp)
		{
			//Allocate an intermediate texture for the two-pass blur.
			var tempTex = RenderTexture.GetTemporary(output.width, output.height, 0, output.format,
													 RenderTextureReadWrite.Linear);
			tempTex.filterMode = FilterMode.Bilinear;
			tempTex.wrapMode = wrapMode;
			
			//Do the blur.
			Passes pass1, pass2;
			switch (quality)
			{
				case GaussianBlurQuality.Low:
					pass1 = Passes.Gaussian5_P1;
					pass2 = Passes.Gaussian5_P2;
					break;
				case GaussianBlurQuality.High:
					pass1 = Passes.Gaussian9_P1;
					pass2 = Passes.Gaussian9_P2;
					break;

				default: throw new NotImplementedException(quality.ToString());
			}
			var mat = UberFilterMat;
			Graphics.Blit(input, tempTex, mat, (int)pass1);
			Graphics.Blit(tempTex, output, mat, (int)pass2);

			//Clean up.
			RenderTexture.ReleaseTemporary(tempTex);
		}

		#endregion

		#region Greyscale

		public enum GreyscaleTypes
		{
			PreserveLuminance,
		}

		public static RenderTexture Greyscale(GreyscaleTypes type, Texture inputTex,
											  RenderTextureFormat outFormat = RenderTextureFormat.RInt)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0, outFormat,
										   RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Point;

			Greyscale(type, inputTex, outTex);

			return outTex;
		}

		public static void Greyscale(GreyscaleTypes type, Texture input, RenderTexture output)
		{
			//Get the correct pass to use in the uber filter shader.
			Passes pass;
			switch (type)
			{
				case GreyscaleTypes.PreserveLuminance:
					pass = Passes.LuminanceGreyscale;
					break;

				default: throw new NotImplementedException(type.ToString());
			}

			//Do the filter.
			var mat = UberFilterMat;
			Graphics.Blit(input, output, mat, (int)pass);
		}

		#endregion

		#region Edge detection

		public enum EdgeFilters
		{
			Sobel,
			Prewitt,
		}

		public static RenderTexture EdgeFilter(EdgeFilters type, Texture inputTex,
											   RenderTextureFormat outFormat = RenderTextureFormat.RGFloat,
											   TextureWrapMode wrapMode = TextureWrapMode.Clamp)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0, outFormat,
										   RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Point;
			outTex.wrapMode = wrapMode;

			EdgeFilter(type, inputTex, outTex, wrapMode);

			return outTex;
		}

		public static void EdgeFilter(EdgeFilters type,
									  Texture input, RenderTexture output,
									  TextureWrapMode wrapMode = TextureWrapMode.Clamp)
		{
			//Allocate an intermediate texture for the two-pass blur.
			var tempTex = RenderTexture.GetTemporary(output.width, output.height, 0,
													 RenderTextureFormat.RGFloat,
													 RenderTextureReadWrite.Linear);
			tempTex.filterMode = FilterMode.Point;
			tempTex.wrapMode = wrapMode;
			
			//Do the filter.
			Passes pass1, pass2;
			switch (type)
			{
				case EdgeFilters.Sobel:
					pass1 = Passes.Sobel_P1;
					pass2 = Passes.Sobel_P2;
					break;
				case EdgeFilters.Prewitt:
					pass1 = Passes.Prewitt_P1;
					pass2 = Passes.Prewitt_P2;
					break;

				default: throw new NotImplementedException(type.ToString());
			}
			var mat = UberFilterMat;
			Graphics.Blit(input, tempTex, mat, (int)pass1);
			Graphics.Blit(tempTex, output, mat, (int)pass2);

			//Clean up.
			RenderTexture.ReleaseTemporary(tempTex);
		}

		#endregion

		#region Pack Vectors

		public static RenderTexture PackVectors(Texture inputTex,
												RenderTextureFormat outFormat = RenderTextureFormat.ARGB32)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0, outFormat,
										   RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Point;

			PackVectors(inputTex, outTex);

			return outTex;
		}

		public static void PackVectors(Texture input, RenderTexture output)
		{
			Graphics.Blit(input, output,
						  UberFilterMat,
						  (int)Passes.PackVectors);
		}

		#endregion


		#region Private definitions

		private static Material uberFilterMat = null;
		private static Material UberFilterMat
		{
			get
			{
				if (uberFilterMat == null)
					uberFilterMat = new Material(Shader.Find("Hidden/UberFilter"));

				return uberFilterMat;
			}
		}

		/// <summary>
		/// The different passes in the uber filter shader, in order.
		/// Note that pass indices are 0-based.
		/// </summary>
		private enum Passes
		{
			Gaussian5_P1,
			Gaussian5_P2,
			Gaussian9_P1,
			Gaussian9_P2,

			LuminanceGreyscale,

			Sobel_P1,
			Sobel_P2,
			Prewitt_P1,
			Prewitt_P2,

			PackVectors,
		}

		#endregion
	}
}
