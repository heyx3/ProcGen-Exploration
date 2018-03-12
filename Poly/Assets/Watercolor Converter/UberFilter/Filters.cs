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

		public static RenderTexture PackVectors(Texture inputTex, Vector4 packChannelMask,
												RenderTextureFormat outFormat = RenderTextureFormat.ARGB32)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0, outFormat,
										   RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Point;

			PackVectors(inputTex, outTex, packChannelMask);

			return outTex;
		}

		public static void PackVectors(Texture input, RenderTexture output, Vector4 packChannelMask)
		{
			UberFilterMat.SetVector("_PackMask", packChannelMask);
			Graphics.Blit(input, output,
						  UberFilterMat,
						  (int)Passes.PackVectors);
		}

		#endregion

		#region Canny Edge Detection
		
		public static RenderTexture CannyEdgeFilter(Texture inputTex,
												    float weakEdgeStart, float weakEdgeEnd,
													TextureWrapMode wrapMode = TextureWrapMode.Clamp,
													RenderTextureFormat outFormat = RenderTextureFormat.ARGBFloat)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0, outFormat,
										   RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Point;

			CannyEdgeFilter(inputTex, outTex, weakEdgeStart, weakEdgeEnd, wrapMode);

			return outTex;
		}

		public static void CannyEdgeFilter(Texture input, RenderTexture output,
										   float weakEdgeStart, float weakEdgeEnd,
										   TextureWrapMode wrapMode = TextureWrapMode.Clamp)
		{
			var tempTex = RenderTexture.GetTemporary(output.width, output.height, 0,
													 RenderTextureFormat.ARGBFloat,
													 RenderTextureReadWrite.Linear);
			var tempTex2 = RenderTexture.GetTemporary(output.width, output.height, 0,
													  RenderTextureFormat.ARGBFloat,
													  RenderTextureReadWrite.Linear);
			tempTex.wrapMode = wrapMode;
			tempTex.filterMode = FilterMode.Bilinear;
			tempTex2.wrapMode = wrapMode;
			tempTex2.filterMode = FilterMode.Point;

			var mat = UberFilterMat;
			mat.SetFloat("_WeakEdgeStart", weakEdgeStart);
			mat.SetFloat("_WeakEdgeEnd", weakEdgeEnd);
			Graphics.Blit(input, tempTex, mat, (int)Passes.Canny_P1);
			Graphics.Blit(tempTex, tempTex2, mat, (int)Passes.Canny_P2);
			Graphics.Blit(tempTex2, output, mat, (int)Passes.Canny_P3);

			RenderTexture.ReleaseTemporary(tempTex);
			RenderTexture.ReleaseTemporary(tempTex2);
		}

		#endregion

		#region Full Canny Edge Detection

		/// <summary>
		/// Performs the Canny edge detection algorithm given a color image.
		/// </summary>
		public static RenderTexture FullCannyEdgeFilter(
			Texture inputTex,
			GaussianBlurQuality blurQuality, GreyscaleTypes greyscaleMode, EdgeFilters edgeFilter,
			float weakEdgeThreshold, float strongEdgeThreshold,
			TextureWrapMode samplingWrapMode = TextureWrapMode.Clamp,
			RenderTextureFormat outFormat = RenderTextureFormat.ARGBFloat)
		{
			var outTex = new RenderTexture(inputTex.width, inputTex.height, 0,
										   outFormat, RenderTextureReadWrite.Linear);
			outTex.filterMode = FilterMode.Point;
			outTex.wrapMode = samplingWrapMode;

			FullCannyEdgeFilter(inputTex, outTex,
								blurQuality, greyscaleMode, edgeFilter,
								weakEdgeThreshold, strongEdgeThreshold,
								samplingWrapMode);

			return outTex;
		}

		/// <summary>
		/// Performs the Canny edge detection algorithm given a color image.
		/// The output is a texture that stores edge gradients in Red-Green
		///     and edge strengths in Blue.
		/// Note that edge strength is equal to the length of the edge gradient.
		/// </summary>
		public static void FullCannyEdgeFilter(
			Texture inputTex, RenderTexture outputTex,
			GaussianBlurQuality blurQuality, GreyscaleTypes greyscaleMode, EdgeFilters edgeFilter,
			float weakEdgeThreshold, float strongEdgeThreshold,
			TextureWrapMode samplingWrapMode = TextureWrapMode.Clamp)
		{
			RenderTexture out_gaussian = RenderTexture.GetTemporary(
							  inputTex.width, inputTex.height, 0,
							  RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear),
					      out_greyscale = RenderTexture.GetTemporary(
							  outputTex.width, outputTex.height, 0,
							  RenderTextureFormat.RInt, RenderTextureReadWrite.Linear),
						  out_edgeDetect = RenderTexture.GetTemporary(
							  outputTex.width, outputTex.height, 0,
							  RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
			out_gaussian.filterMode = FilterMode.Point;
			out_gaussian.wrapMode = samplingWrapMode;
			out_greyscale.filterMode = FilterMode.Point;
			out_greyscale.wrapMode = samplingWrapMode;
			out_edgeDetect.filterMode = FilterMode.Point;
			out_edgeDetect.wrapMode = samplingWrapMode;

			Blur(blurQuality, inputTex, out_gaussian, samplingWrapMode);
			Greyscale(greyscaleMode, out_gaussian, out_greyscale);
			EdgeFilter(edgeFilter, out_greyscale, out_edgeDetect, samplingWrapMode);
			CannyEdgeFilter(out_edgeDetect, outputTex,
							weakEdgeThreshold, strongEdgeThreshold,
							samplingWrapMode);

			RenderTexture.ReleaseTemporary(out_edgeDetect);
			RenderTexture.ReleaseTemporary(out_greyscale);
			RenderTexture.ReleaseTemporary(out_gaussian);
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

			Canny_P1,
			Canny_P2,
			Canny_P3,
		}

		#endregion
	}
}
