using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A fully-GPU-accelerated version of a PolyShape.
/// </summary>
[Serializable]
public class PolyShapeGPU
{
	public int NInitialPoints = 16;
	public float InitialRadius = 0.1f;
	
	/// <summary>
	/// The number of iterations to create the base shape and/or its variations.
	/// </summary>
	public int NBaseShapeIterations = 3,
			   NSpecialIterations = 4;
	
	/// <summary>
	/// The "Variance Scale" is used to decrease the "variance" of points after each iteration,
	///     so that points are more well-behaved as the edges get smaller and smaller.
	/// This field describes the average variance scale for each level of iteration.
	/// The value at 0 represents the variance of the initial points.
	/// </summary>
	public AnimationCurve VarianceScaleByIteration = new AnimationCurve(new Keyframe(0.0f, 0.25f),
																	    new Keyframe(6.0f, 0.00625f));
	/// <summary>
	/// This value is used to randomize the "Variance Scale" for each point.
	/// Given the average Variance Scale x, and VarianceScaleRandomness y,
	///     the min Variance Scale is x/y and the max Variance scale is x*y.
	/// </summary>
	public float VarianceScaleRandomness = 1.1f;

	public float Seed = 0.349523498f;


	/// <summary>
	/// The base shape that variations can be made from.
	/// </summary>
	public RenderTexture BaseShapeTex
	{
		get
		{
			if (baseShapeTex == null)
				GenerateBaseShape();

			return baseShapeTex;
		}
	}
	private RenderTexture baseShapeTex;
	private static Material pointGenerateMat;

	/// <summary>
	/// The number of vertices in the base shape.
	/// </summary>
	public int BaseShapeSize
	{
		get
		{
			return NInitialPoints *
				   Mathf.RoundToInt(Mathf.Pow(2.0f, NBaseShapeIterations));
		}
	}
	/// <summary>
	/// The number of vertices in a variation on the base shape.
	/// </summary>
	public int VariationShapeSize
	{
		get
		{
			int nTotalIterations = NBaseShapeIterations + NSpecialIterations;
			return NInitialPoints *
				   Mathf.RoundToInt(Mathf.Pow(2.0f, nTotalIterations));
		}
	}


	/// <summary>
	/// Should be called after any parameters have been changed.
	/// Regenerates the "base" shape.
	/// </summary>
	public void Reset()
	{
		baseShapeTex = null;
	}
	/// <summary>
	/// Generates a new seed.
	/// </summary>
	public void ChangeSeed()
	{
		Seed = UnityEngine.Random.value + UnityEngine.Random.Range(0, 5);
	}

	/// <summary>
	/// Generates a CPU PolyShape based on this one.
	/// Intended for debugging, not efficient runtime use.
	/// </summary>
	/// <param name="variation">
	/// If true, a random variation will be output.
	/// Otherwise, the base shape will be output.
	/// </param>
	/// <param name="texBuffer">
	/// A buffer to get the GPU shape data.
	/// If null, a new one will be temporarily allocated.
	/// If given a texture of the wrong size, it will be resized.
	/// </param>
	public PolyShape MakeCPUVersion(bool variation, Texture2D texBuffer = null)
	{
		//Get the shape's texture.
		RenderTexture srcTex = BaseShapeTex;
		if (variation)
		{
			var newSrc = RenderTexture.GetTemporary(VariationShapeSize, 1, 0,
													RenderTextureFormat.ARGBFloat,
													RenderTextureReadWrite.Linear);
			GenerateVariation(newSrc);

			srcTex = newSrc;
		}

		//Read the pixels.
		if (texBuffer == null)
		{
			texBuffer = new Texture2D(srcTex.width, srcTex.height,
									  TextureFormat.RGBAFloat, false, true);
		}
		if (texBuffer.width != srcTex.width || texBuffer.height != srcTex.height)
			texBuffer.Resize(srcTex.width, srcTex.height);
		RenderTexture.active = srcTex;
		texBuffer.ReadPixels(new Rect(0.0f, 0.0f, texBuffer.width, texBuffer.height), 0, 0);
		var pixels = texBuffer.GetPixels();

		//Convert.
		PolyShape outShape = new PolyShape(pixels.Select(p => new PolyShape.Point(new Vector2(p.r, p.g), p.b))
												 .ToArray());

		//Clean up.
		if (variation)
			RenderTexture.ReleaseTemporary(srcTex);

		return outShape;
	}

	/// <summary>
	/// Generates a variation on the base shape.
	/// </summary>
	/// <param name="output">
	/// The texture that will store the final shape.
	/// Its width must be equal to VariationShapeSize.
	/// Its height must be 1.
	/// </param>
	/// <param name="tempSeed">
	/// The seed to use for this variation, or NaN to automatically generate one.
	/// </param>
	public void GenerateVariation(RenderTexture output, float tempSeed = float.NaN)
	{
		float currentSeed = Seed;
		if (float.IsNaN(tempSeed))
			ChangeSeed();
		else
			Seed = tempSeed;

		GenerateShape(BaseShapeTex, output,
					  NBaseShapeIterations,
					  NBaseShapeIterations + NSpecialIterations - 1);

		Seed = currentSeed;
	}

	private void GenerateBaseShape()
	{
		//Create the Material if it doesn't exist yet.
		if (pointGenerateMat == null)
		{
			var shader = Shader.Find("PolyShape/Generator");
			if (shader == null)
				throw new Exception("Can't find generator shader!");

			pointGenerateMat = new Material(shader);
		}

		//Run a "generator" pass to create the initial shape.
		RenderTexture startShape;
		if (NBaseShapeIterations < 1)
		{
			startShape = new RenderTexture(NInitialPoints, 1, 0,
										   RenderTextureFormat.ARGBFloat,
										   RenderTextureReadWrite.Linear);
		}
		else
		{
			startShape = RenderTexture.GetTemporary(NInitialPoints, 1, 0,													RenderTextureFormat.ARGBFloat,
													RenderTextureReadWrite.Linear);
		}
		pointGenerateMat.SetFloat("_Seed", Seed);
		pointGenerateMat.SetFloat("_Radius", InitialRadius);
		float avgVariance = VarianceScaleByIteration.Evaluate(0.0f);
		pointGenerateMat.SetVector("_InitialVariance",
								   new Vector2(avgVariance / VarianceScaleRandomness,
											   avgVariance * VarianceScaleRandomness));
		pointGenerateMat.SetFloat("_OutputSize", startShape.width);
		Graphics.Blit(Texture2D.whiteTexture, startShape, pointGenerateMat, 0);

		//Allocate the "base shape" texture.
		if (NBaseShapeIterations < 1)
			baseShapeTex = startShape;
		else
		{
			baseShapeTex = new RenderTexture(BaseShapeSize, 1, 0,
											 RenderTextureFormat.ARGBFloat,
											 RenderTextureReadWrite.Linear);
		}

		//Finally, generate the shape!
		GenerateShape(startShape, baseShapeTex, 0, NBaseShapeIterations - 1);

		//Clean up.
		if (NBaseShapeIterations > 0)
			RenderTexture.ReleaseTemporary(startShape);
	}
	private void GenerateShape(RenderTexture startShape, RenderTexture outputShape,
							   int firstIterationI, int lastIterationI)
	{
		RenderTexture src = startShape,
					  dest = null;
		for (int i = firstIterationI; i <= lastIterationI; ++i)
		{
			//Get or make the destination render target.
			if (i == lastIterationI)
			{
				dest = outputShape;

				if (dest.width != src.width * 2)
					throw new ArgumentException("Output width must be " + (src.width * 2));
				if (dest.height != 1)
					throw new ArgumentException("Output height must be 1");
			}
			else
			{
				dest = RenderTexture.GetTemporary(src.width * 2, 1, 0,
												  RenderTextureFormat.ARGBFloat,
												  RenderTextureReadWrite.Linear);
			}

			//Set up the src texture.
			src.filterMode = FilterMode.Point;
			src.wrapMode = TextureWrapMode.Repeat;

			//Calculate the variance scale for this iteration.
			float variance = VarianceScaleByIteration.Evaluate(i + 1);
			float minVariance = variance + (1.0f / VarianceScaleRandomness),
				  maxVariance = variance + VarianceScaleRandomness;

			//Run the refinement pass.
			pointGenerateMat.SetFloat("_Seed", Seed * (i + 2.12375f));
			pointGenerateMat.SetVector("_VarianceScale", new Vector2(minVariance, maxVariance));
			Graphics.Blit(src, dest, pointGenerateMat, 1);

			//Clean up and prepare for the next iteration.
			if (i > firstIterationI)
				RenderTexture.ReleaseTemporary(src);
			src = dest;
		}
	}
}