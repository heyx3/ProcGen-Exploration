using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Runs the DFT/inverse DFT using the GPU for high performance.
/// </summary>
public static class DFT_GPU
{
    //TODO: Go back to Compute Shader.
    //TODO: merge forward and inverse funcs.

    /// <summary>
    /// Performs the forward-dft on the given 2D grid of real-valued samples.
    /// Returns the empty string if everything went well,
    ///     or an error message if something bad happened.
    /// </summary>
    /// <param name="sampleGetter">Gets the sample at the given position.</param>
    /// <param name="resultReader">
    /// Takes in the DFT value samples (one for each sample)
    ///     and presumably stores them somewhere.
    /// </param>
    public static string Forward(uint nSamplesX, uint nSamplesY,
                                 Func<uint, uint, float> sampleGetter,
                                 Action<uint, uint, Complex> resultReader)
    {
        return Forward(nSamplesX, nSamplesY,
                       (x, y) => new Complex(sampleGetter(x, y), 0.0f),
                       resultReader);
    }
    /// <summary>
    /// Performs the forward-dft on the given 2D grid of complex-valued samples.
    /// Returns the empty string if everything went well,
    ///     or an error message if something bad happened.
    /// </summary>
    /// <param name="sampleGetter">Gets the sample at the given position.</param>
    /// <param name="resultReader">
    /// Takes in the DFT value samples (one for each sample)
    ///     and presumably stores them somewhere.
    /// </param>
    public static string Forward(uint nSamplesX, uint nSamplesY,
                                 Func<uint, uint, Complex> sampleGetter,
                                 Action<uint, uint, Complex> resultReader)
    {
        //Make sure the intermediate textures are the correct length.
        string errMsg = Prepare((int)nSamplesX, (int)nSamplesY);
        if (errMsg.Length > 0)
            return errMsg;

        //Initialize the "ping" texture with the inputs.
        SetTexture(tex_Ping, sampleGetter);

        //Run the DFT.
        //Horizontal pass:
        RunDFTPass(false, false, tex_Ping, rtex_Pong);
        CopyTo(rtex_Pong, tex_Ping);
        //Vertical pass:
        RunDFTPass(false, true, tex_Ping, rtex_Pong);
        CopyTo(rtex_Pong, tex_Ping);

        //Read the result into the output array.
        GetTexture(tex_Ping, resultReader);

        return "";
    }


    /// <summary>
    /// Performs the inverse-dft on the given 2D grid of DFT samples.
    /// Returns the empty string if everything went well,
    ///     or an error message if something bad happened.
    /// </summary>
    /// <param name="dftGetter">
    /// Gets the DFT at the given sample position.
    /// </param>
    /// <param name="resultReader">
    /// Takes in the reconstructed real-valued samples and presumably stores them somewhere.
    /// </param>
    public static string Inverse(uint nSamplesX, uint nSamplesY,
                                 Func<uint, uint, Complex> dftGetter,
                                 Action<uint, uint, float> resultReader)
    {
        return Inverse(nSamplesX, nSamplesY, dftGetter,
                       (x, y, c) => resultReader(x, y, c.R));
    }
    /// <summary>
    /// Performs the inverse-dft on the given 2D grid of DFT samples.
    /// Returns the empty string if everything went well,
    ///     or an error message if something bad happened.
    /// </summary>
    /// <param name="dftGetter">
    /// Gets the DFT at the given sample position.
    /// </param>
    /// <param name="resultReader">
    /// Takes in the reconstructed complex-valued samples and presumably stores them somewhere.
    /// </param>
    public static string Inverse(uint nSamplesX, uint nSamplesY,
                                 Func<uint, uint, Complex> dftGetter,
                                 Action<uint, uint, Complex> resultReader)
    {
        //Make sure the intermediate textures are the correct length.
        string errMsg = Prepare((int)nSamplesX, (int)nSamplesY);
        if (errMsg.Length > 0)
            return errMsg;

        //Initialize the "ping" texture with the inputs.
        SetTexture(tex_Ping, dftGetter);

        //Run the DFT.
        //Horizontal pass:
        RunDFTPass(true, false, tex_Ping, rtex_Pong);
        CopyTo(rtex_Pong, tex_Ping);
        //Vertical pass:
        RunDFTPass(true, true, tex_Ping, rtex_Pong);
        CopyTo(rtex_Pong, tex_Ping);

        //Read the result into the output array.
        GetTexture(tex_Ping, resultReader);

        return "";
    }



    //This approach uses separable 1D DFT's to compute the 2D DFT.
    //http://www.cs.umb.edu/~duc/cs447_647/spring13/slides/FourierTransform.pdf

    private static Texture2D tex_Ping = null;
    private static RenderTexture rtex_Pong = null;
    private static Color[] pixelBuffer = null;
    private static Material dftMat = null;

    /// <summary>
    /// Sets up the ping-pong textures, pixel buffer, and compute shader.
    /// Returns an error message, or the empty string.
    /// </summary>
    private static string Prepare(int nSamplesX, int nSamplesY)
    {
        //Ping-poing textures.
        if (tex_Ping == null || tex_Ping.width != nSamplesX || tex_Ping.height != nSamplesY)
        {
            if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
                Debug.LogError("Doesn't support RGBAFloat Texture2Ds!");
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
                Debug.LogError("Doesn't support ARGBFloat RenderTextures!");

            tex_Ping = new Texture2D(nSamplesX, nSamplesY, TextureFormat.RGBAFloat, false, true);
            tex_Ping.filterMode = FilterMode.Point;
            tex_Ping.wrapMode = TextureWrapMode.Repeat;
            
            rtex_Pong = new RenderTexture(nSamplesX, nSamplesY, 16,
                                          RenderTextureFormat.ARGBFloat,
                                          RenderTextureReadWrite.Linear);
            rtex_Pong.filterMode = FilterMode.Point;
            rtex_Pong.wrapMode = TextureWrapMode.Repeat;
            rtex_Pong.enableRandomWrite = true;
            rtex_Pong.Create();
        }

        //Pixel data buffer.
        if (pixelBuffer == null || pixelBuffer.Length != nSamplesX * nSamplesY)
            pixelBuffer = new Color[nSamplesX * nSamplesY];

        //DFT shader.
        if (dftMat == null)
        {
            var shader = Resources.Load<Shader>("DFTShader");
            if (shader == null)
                return "Couldn't find 'DFTComputeShader' in Resources";
            dftMat = new Material(shader);
        }

        return "";
    }

    /// <summary>
    /// Stores the given complex values into the given texture's pixels.
    /// </summary>
    private static void SetTexture(Texture2D tex, Func<uint, uint, Complex> valueGetter)
    {
        for (uint y = 0; y < tex.height; ++y)
        {
            for (uint x = 0; x < tex.width; ++x)
            {
                Complex value = valueGetter(x, y);
                pixelBuffer[x + (y * tex.width)] = new Color(
                    value.R, value.I, 0.0f, 1.0f);
            }
        }
        tex.SetPixels(pixelBuffer);
        tex.Apply(false, false);
    }
    /// <summary>
    /// Reads the stored complex values in the given texture's pixels.
    /// </summary>
    /// <param name="valueSetter">
    /// Acknowledges the given complex value at the given pixel X/Y position.
    /// </param>
    private static void GetTexture(Texture2D tex, Action<uint, uint, Complex> valueSetter)
    {
        var pixels = tex.GetPixels();
        for (uint y = 0; y < tex.height; ++y)
        {
            for (uint x = 0; x < tex.width; ++x)
            {
                Color color = pixels[x + (y * tex.width)];
                valueSetter(x, y, new Complex(color.r, color.g));
            }
        }
    }

    /// <summary>
    /// Runs a 1D DFT pass across rows or columns of the given input texture.
    /// Puts the result into the given output texture.
    /// </summary>
    /// <param name="inverse">
    /// If true, runs the inverse DFT instead of the forward DFT.
    /// </param>
    /// <param name="runOnColumns">
    /// If true, runs the pass across each column in the texture.
    /// If false, runs the pass across each row in the texture.
    /// </param>
    private static void RunDFTPass(bool inverse, bool runOnColumns,
                                   Texture input, RenderTexture output)
    {
        //Set parameters.
        dftMat.SetInt("u_SamplesSizeX", input.width);
        dftMat.SetInt("u_SamplesSizeY", input.height);
        dftMat.SetVector("_MainTex_TexelSize", input.texelSize);
        dftMat.SetTexture("_MainTex", input);

        //Set #define keywords.
        if (inverse)
        {
            dftMat.EnableKeyword("DFT_INVERSE");
            dftMat.DisableKeyword("DFT_FORWARD");
        }
        else
        {
            dftMat.EnableKeyword("DFT_FORWARD");
            dftMat.DisableKeyword("DFT_INVERSE");
        }
        if (runOnColumns)
        {
            dftMat.EnableKeyword("DFT_VERT");
            dftMat.DisableKeyword("DFT_HORZ");
        }
        else
        {
            dftMat.EnableKeyword("DFT_HORZ");
            dftMat.DisableKeyword("DFT_VERT");
        }
        
        //Run the DFT.
        Graphics.Blit(input, output, dftMat);
    }
    /// <summary>
    /// Copies the given RenderTexture's pixels to the given Texture2D (assumed to be the same size).
    /// </summary
    public static void CopyTo(RenderTexture src, Texture2D dest)
    {
        RenderTexture active = RenderTexture.active;

        RenderTexture.active = src;
        dest.ReadPixels(new Rect(0.0f, 0.0f, dest.width, dest.height), 0, 0);
        dest.Apply(false, false);

        RenderTexture.active = active;
    }
}