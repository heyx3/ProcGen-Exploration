using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class MutatedAverageNoise : IDisposable
{
	[Serializable]
	public struct Point
	{
		public uint x, y;
		public float Value;
		public Point(uint _x, uint _y, float value) { x = _x; y = _y; Value = value; }
		public override string ToString()
		{
			return "{" + x + "," + y + "}";
		}
	}


	//Constants:
	public static readonly uint MAX_POINTS = 16; //This must be the same value as it is in the shader!


	public ComputeShader Shader;
	public uint OutputWidth = 256,
				OutputHeight = 256;

	public List<Point> PixelPoints = new List<Point>() { new Point(0, 0, 0.0f) };

	public AnimationCurve MutationCurve = AnimationCurve.EaseInOut(0.0f, -1.0f, 1.0f, 1.0f);
	public int MutationCurveResolution = 256;

	public int Seed = 2354;


	private System.Random rng;
	private RenderTexture buffer_PullDirs,
						  buffer_Input, buffer_Output;
	private Texture2D mutationCurveTex;
	private ComputeBuffer buffer_FinishedPixelCounter;
	private int kernel_Init, kernel_RunIteration;
	private uint[] data_FinishedPixelCounter = new uint[0];
	private Texture2D resultCopy;


	public void Init()
	{
		rng = new System.Random(Seed);

		//Get shader data.
		if (Shader == null)
			throw new ArgumentException("Shader for MutatedAverageNoise isn't set!");
		kernel_Init = Shader.FindKernel("Init");
		kernel_RunIteration = Shader.FindKernel("RunIteration");

		//Set up buffers.

		buffer_PullDirs = new RenderTexture((int)OutputWidth, (int)OutputHeight, 0,
											RenderTextureFormat.RG16,
											RenderTextureReadWrite.Linear);
		buffer_PullDirs.Create();
		Shader.SetTexture(kernel_Init, "u_PullDirs", buffer_PullDirs);
		Shader.SetTexture(kernel_RunIteration, "u_PullDirs", buffer_PullDirs);

		buffer_Input = new RenderTexture((int)OutputWidth, (int)OutputHeight, 0,
										  RenderTextureFormat.RGHalf,
										  RenderTextureReadWrite.Linear);
		buffer_Input.Create();

		buffer_Output = new RenderTexture((int)OutputWidth, (int)OutputHeight, 0,
										  buffer_Input.format, RenderTextureReadWrite.Linear);
		buffer_Output.Create();

		buffer_FinishedPixelCounter = new ComputeBuffer(1, sizeof(uint),
														ComputeBufferType.Default);
		buffer_FinishedPixelCounter.SetData(new uint[] { 0 });
		Shader.SetBuffer(kernel_RunIteration, "NCompletedPixels", buffer_FinishedPixelCounter);

		//Set up the mutation curve.
		mutationCurveTex = new Texture2D(MutationCurveResolution, 1, TextureFormat.RFloat,
										 false, true);
		var mutationCurvePixels = new Color[mutationCurveTex.width];
		for (int i = 0; i < mutationCurvePixels.Length; ++i)
		{
			float t = i / (float)(mutationCurvePixels.Length - 1);
			float val = MutationCurve.Evaluate(t);
			mutationCurvePixels[i] = new Color(val, val, val, 1.0f);
		}
		mutationCurveTex.SetPixels(mutationCurvePixels);
		mutationCurveTex.Apply();
		Shader.SetTexture(kernel_RunIteration, "u_MutationRange", mutationCurveTex);

		//Run the "Init" pass.
		if (PixelPoints.Count > MAX_POINTS)
		{
			throw new ArgumentException("More than " + MAX_POINTS +
										    " points! The shader doesn't support that");
		}
		Shader.SetTexture(kernel_Init, "u_Output", buffer_Output);
		Shader.SetInt("u_NumbPoints", PixelPoints.Count);
		Shader.SetInts("u_PointPixels",
					   PixelPoints.SelectMany(v => new int[] { (int)v.x, (int)v.y })
								  .ToArray());
		Shader.SetFloats("u_PointValues",
						 PixelPoints.Select(p => p.Value)
									.ToArray());
		Shader.Dispatch(kernel_Init, buffer_PullDirs.width, buffer_PullDirs.height, 1);
	}
	public void Dispose()
	{
		if (buffer_PullDirs != null)
		{
			buffer_PullDirs.Release();
			buffer_Input.Release();
			buffer_Output.Release();
			buffer_FinishedPixelCounter.Dispose();
		}
	}

	/// <summary>
	/// Runs one iteration of this algorithm.
	/// Returns whether the algorithm is finished.
	/// </summary>
	public bool Update()
	{
		//Swap the input and output buffers.
		var oldInp = buffer_Input;
		buffer_Input = buffer_Output;
		buffer_Output = oldInp;
		Shader.SetTexture(kernel_RunIteration, "u_Input", buffer_Input);
		Shader.SetTexture(kernel_RunIteration, "u_Output", buffer_Output);

		//Initialize the counter to 0.
		data_FinishedPixelCounter[0] = 0;
		buffer_FinishedPixelCounter.SetData(data_FinishedPixelCounter);

		//Use a new seed.
		Shader.SetVector("u_Seed",
						 new Vector2((float)(1000.0 * rng.NextDouble()),
							 		 (float)(1000.0 * rng.NextDouble())));

		//Run the shader.
		Shader.Dispatch(kernel_RunIteration, buffer_Input.width, buffer_Input.height, 1);

		//Count the number of pixels left to be done.
		buffer_FinishedPixelCounter.GetData(data_FinishedPixelCounter);
		return data_FinishedPixelCounter[0] == (buffer_Input.width * buffer_Input.height);
	}

	/// <summary>
	/// Gets the render texture that contains the current state of the algorithm.
	/// The value at each pixel is stored in the Red component.
	/// The Green component is 1 if the pixel is set, or 0 if it is not set.
	/// </summary>
	public RenderTexture GetResult()
	{
		return buffer_Output;
	}
	/// <summary>
	/// Gets an array of the current values of this noise algorithm.
	/// If a pixel hasn't been set yet, its value will be 0.
	/// Note that some pixels have a value of 0 AFTER being set.
	/// </summary>
	public float[,] GetResultArray()
	{
		if (resultCopy == null)
		{
			resultCopy = new Texture2D(buffer_Output.width, buffer_Output.height,
									   TextureFormat.RFloat, false, true);
		}

		RenderTexture.active = buffer_Output;
		resultCopy.ReadPixels(new Rect(0.0f, 0.0f, resultCopy.width, resultCopy.height), 0, 0);

		var result = new float[resultCopy.width, resultCopy.height];
		var resultPixels = resultCopy.GetPixels();
		for (int y = 0; y < result.GetLength(1); ++y)
			for (int x = 0; x < result.GetLength(0); ++x)
				result[x, y] = resultPixels[x + (y * resultCopy.height)].r;

		return result;
	}

	/// <summary>
	/// Gets the texture containing the "pull direction" of each pixel.
	/// Mainly for debugging/testing.
	/// </summary>
	public RenderTexture GetPullDirs() { return buffer_PullDirs; }
}