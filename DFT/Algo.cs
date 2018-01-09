using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFT
{
    public static class Algo1D
    {
        /// <summary>
        /// Performs the DFT -- converting a time-domain signal into a frequency-domain signal.
        /// </summary>
        /// <param name="samples">
        /// The samples from an input signal.
        /// </param>
        /// <returns>The sine/cosine wave amplitudes for each frequency in the signal.</returns>
        public static Complex[] Forward(Complex[] samples)
        {
            Complex[] results = new Complex[samples.Length];
            Forward(samples, ref results);
            return results;
        }
        /// <summary>
        /// Performs the DFT -- converting a time-domain signal into a frequency-domain signal.
        /// </summary>
        /// <param name="samples">
        /// The samples from an input signal.
        /// </param>
        /// <param name="outValues">
        /// The output array.
        /// If it's null or the wrong size, it will be automatically created.
        /// Contains the sine/cosine wave amplitudes for each frequency in the signal.
        /// </param>
        public static void Forward(Complex[] samples, ref Complex[] outResult)
        {
            //Make sure the output array is the right size.
            if (outResult == null || outResult.Length != samples.Length)
                outResult = new Complex[samples.Length];

            float scale = 1.0f / samples.Length,
                  _frequencyMultiplier = 2.0f * Mathf.PI * scale;
            for (int frequencyI = 0; frequencyI < outResult.Length; ++frequencyI)
            {
                float frequencyMultiplier = _frequencyMultiplier * frequencyI;

                //Dot the samples with the sine/cosine waves of this frequency.
                Complex dotResult = new Complex(0.0f, 0.0f);
                for (int sampleI = 0; sampleI < samples.Length; ++sampleI)
                {
                    float trigInput = frequencyMultiplier * sampleI;
                    dotResult += samples[sampleI] * new Complex(Mathf.Cos(trigInput),
                                                                -Mathf.Sin(trigInput));
                }

                outResult[frequencyI] = dotResult * scale;
            }
        }

        /// <summary>
        /// Performs the inverse DFT -- converting a frequency-domain signal
        ///     back into a time-domain signal.
        /// </summary>
        /// <param name="dftResults">
        /// The frequency-domain signal.
        /// </param>
        public static Complex[] Inverse(Complex[] dftResults)
        {
            Complex[] samples = new Complex[dftResults.Length];
            Inverse(dftResults, ref samples);
            return samples;
        }
        /// <summary>
        /// Performs the inverse DFT -- converting a frequency-domain signal
        ///     back into a time-domain signal.
        /// </summary>
        /// <param name="dftResults">
        /// The frequency-domain signal.
        /// </param>
        /// <param name="outSamples">
        /// The time-domain signal samples.
        /// If the array is null or the wrong size, it will be automatically created.
        /// </param>
        public static void Inverse(Complex[] dftResults, ref Complex[] outSamples)
        {
            //Make sure the output array is the right size.
            if (outSamples == null || outSamples.Length != dftResults.Length)
                outSamples = new Complex[dftResults.Length];

            float _trigMultiplier = 2.0f * Mathf.PI / outSamples.Length;
            for (int sampleI = 0; sampleI < outSamples.Length; ++sampleI)
            {
                float trigMultiplier = _trigMultiplier * sampleI;

                Complex sum = new Complex(0.0f, 0.0f);
                for (int frequencyI = 0; frequencyI < dftResults.Length; ++frequencyI)
                {
                    float trigContents = trigMultiplier * frequencyI;
                    float cos = Mathf.Cos(trigContents),
                          sin = Mathf.Sin(trigContents);
                    sum += new Complex(cos, sin) * dftResults[frequencyI].R;
                    sum -= new Complex(sin, -cos) * dftResults[frequencyI].I;
                }

                outSamples[sampleI] = sum;
            }
        }
    }
    
    public static class Algo2D
    {
        public static int NThreads = 8;


        /// <summary>
        /// Performs the DFT -- converting a 2D position-domain signal into
        ///     a 2D frequency-domain signal.
        /// </summary>
        /// <param name="samples">
        /// The samples from an input signal.
        /// </param>
        /// <returns>The sine/cosine wave amplitudes for each frequency in the signal.</returns>
        public static Complex[,] Forward(Complex[,] samples)
        {
            Complex[,] result = new Complex[samples.GetLength(0), samples.GetLength(1)];
            Forward(samples, ref result);
            return result;
        }
        /// <summary>
        /// Performs the DFT -- converting a 2D position-domain signal into a 2D frequency-domain signal.
        /// </summary>
        /// <param name="samples">
        /// The samples from an input signal.
        /// </param>
        /// <param name="outResult">
        /// The output array.
        /// If it's null or the wrong size, it will be automatically created.
        /// Contains the sine/cosine wave amplitudes for each frequency in the signal.
        /// </param>
        public static void Forward(Complex[,] samples, ref Complex[,] outResult)
        {
            //Make sure the output array is the right size.
            if (outResult == null ||
                outResult.GetLength(0) != samples.GetLength(0) ||
                outResult.GetLength(1) != samples.GetLength(1))
            {
                outResult = new Complex[samples.GetLength(0), samples.GetLength(1)];
            }


            //Perform the 1D DFT along every row and column.

            //Row:
            Complex[][] dftByRow = new Complex[samples.GetLength(1)][];
            ThreadedRunner.Run(NThreads, samples.GetLength(1),
                (startY, endY) =>
                {
                    Complex[] sampleLine = new Complex[samples.GetLength(0)];
                    for (int sampleY = startY; sampleY <= endY; ++sampleY)
                    {
                        //Populate the sample array.
                        for (int sampleX = 0; sampleX < samples.GetLength(0); ++sampleX)
                            sampleLine[sampleX] = samples[sampleX, sampleY];
                        //Run the dft.
                        dftByRow[sampleY] = Algo1D.Forward(sampleLine);
                    }
                });

            //Column:
            var _outResult = outResult; //ref variables can't be used inside a lambda.
            ThreadedRunner.Run(NThreads, samples.GetLength(0),
                (startX, endX) =>
                {
                    Complex[] sampleLine = new Complex[samples.GetLength(1)];
                    for (int sampleX = startX; sampleX <= endX; ++sampleX)
                    {
                        //Populate the sample array.
                        for (int sampleY = 0; sampleY < samples.GetLength(1); ++sampleY)
                            sampleLine[sampleY] = dftByRow[sampleY][sampleX];
                        //Run the dft.
                        var results = Algo1D.Forward(sampleLine);
                        for (int sampleY = 0; sampleY < samples.GetLength(1); ++sampleY)
                            _outResult[sampleX, sampleY] = results[sampleY];
                    }
                });
        }

        /// <summary>
        /// Performs the inverse DFT -- converting a frequency-domain signal
        ///     back into a 2D position-domain signal.
        /// </summary>
        /// <param name="dftResults">
        /// The frequency-domain signal.
        /// </param>
        public static Complex[,] Inverse(Complex[,] dftResults)
        {
            Complex[,] result = new Complex[dftResults.GetLength(0), dftResults.GetLength(1)];
            Inverse(dftResults, ref result);
            return result;
        }
        /// <summary>
        /// Performs the inverse DFT -- converting a frequency-domain signal
        ///     back into a 2D position-domain signal.
        /// </summary>
        /// <param name="dftResults">
        /// The frequency-domain signal.
        /// </param>
        /// <param name="outSamples">
        /// The position-domain signal samples.
        /// If the array is null or the wrong size, it will be automatically created.
        /// </param>
        public static void Inverse(Complex[,] dftResults, ref Complex[,] outSamples)
        {
            //Make sure the output array is the right size.
            if (outSamples == null ||
                outSamples.GetLength(0) != dftResults.GetLength(0) ||
                outSamples.GetLength(1) != dftResults.GetLength(1))
            {
                outSamples = new Complex[dftResults.GetLength(0), dftResults.GetLength(1)];
            }

            //Perform the 1D inverse DFT along each row/column.
            //Column:
            Complex[][] inverseDFTByCol = new Complex[dftResults.GetLength(0)][];
            ThreadedRunner.Run(NThreads, dftResults.GetLength(0),
                (startX, endX) =>
                {
                    Complex[] sampleLine = new Complex[dftResults.GetLength(1)];
                    for (int sampleX = startX; sampleX <= endX; ++sampleX)
                    {
                        //Populate the sample array.
                        for (int sampleY = 0; sampleY < dftResults.GetLength(1); ++sampleY)
                            sampleLine[sampleY] = dftResults[sampleX, sampleY];
                        //Run the dft.
                        inverseDFTByCol[sampleX] = Algo1D.Inverse(sampleLine);
                    }
                });
            //Row:
            var _outSamples = outSamples; //Can't use a ref variable in a lambda.
            ThreadedRunner.Run(NThreads, dftResults.GetLength(1),
                (startY, endY) =>
                {
                    Complex[] sampleLine = new Complex[dftResults.GetLength(0)];
                    for (int sampleY = startY; sampleY <= endY; ++sampleY)
                    {
                        //Populate the sample array.
                        for (int sampleX = 0; sampleX < dftResults.GetLength(0); ++sampleX)
                            sampleLine[sampleX] = inverseDFTByCol[sampleX][sampleY];
                        //Run the dft.
                        var results = Algo1D.Inverse(sampleLine);
                        for (int sampleX = 0; sampleX < dftResults.GetLength(0); ++sampleX)
                            _outSamples[sampleX, sampleY] = results[sampleX];
                    }
                });
        }


        /// <summary>
        /// Converts the given texture to an array of complex number samples.
        /// Assumes the texture uses a floating-point component type.
        /// </summary>
        public static Complex[,] ToSamples(Texture2D tex)
        {
            Complex[,] comp = null;
            ToSamples(tex, ref comp);
            return comp;
        }
        /// <summary>
        /// Converts the given texture to an array of complex number samples.
        /// Assumes the texture uses a floating-point component type.
        /// If the sample array is null or the wrong size, it will be reallocated automatically.
        /// </summary>
        public static void ToSamples(Texture2D tex, ref Complex[,] outSamples)
        {
            //Make sure the array is the right size.
            if (outSamples == null ||
                outSamples.GetLength(0) != tex.width || outSamples.GetLength(1) != tex.height)
            {
                outSamples = new Complex[tex.width, tex.height];
            }

            //Copy the pixel data into the array.
            var pixelData = tex.GetPixels();
            for (int y = 0; y < tex.height; ++y)
                for (int x = 0; x < tex.width; ++x)
                {
                    var pixel = pixelData[x + (y * tex.width)];
                    outSamples[x, y] = new Complex(pixel.r, pixel.g);
                }
        }

        /// <summary>
        /// Puts the given complex number samples into the Red/Green channels of the given texture.
        /// Automatically resizes the texture if it's the wrong size.
        /// </summary>
        public static void ToTex(Complex[,] samples, Texture2D tex)
        {
            //Make sure the texture is the right size.
            if (tex.width != samples.GetLength(0) || tex.height != samples.GetLength(1))
                tex.Resize(samples.GetLength(0), samples.GetLength(1));

            //Create the pixel data.
            Color[] pixels = new Color[samples.GetLength(0) * samples.GetLength(1)];
            for (int y = 0; y < tex.height; ++y)
                for (int x = 0; x < tex.width; ++x)
                    pixels[x + (y * tex.width)] = new Color(samples[x, y].R, samples[x, y].I, 0.0f, 1.0f);

            //Update the texture.
            tex.SetPixels(pixels);
            tex.Apply(true, false);
        }
        /// <summary>
        /// Puts the given complex number samples into the Red channels of the given textures.
        /// Automatically resizes the textures if they're the wrong size.
        /// </summary>
        public static void ToTex(Complex[,] samples, Texture2D realTex, Texture2D imaginaryTex)
        {
            //Make sure the textures are the right size.
            if (realTex.width != samples.GetLength(0) || realTex.height != samples.GetLength(1))
                realTex.Resize(samples.GetLength(0), samples.GetLength(1));
            if (imaginaryTex.width != samples.GetLength(0) || imaginaryTex.height != samples.GetLength(1))
                imaginaryTex.Resize(samples.GetLength(0), samples.GetLength(1));

            //Create the pixel data.
            Color[] realPixels = new Color[samples.GetLength(0) * samples.GetLength(1)],
                    imaginaryPixels = new Color[samples.GetLength(0) * samples.GetLength(1)];
            for (int y = 0; y < realTex.height; ++y)
                for (int x = 0; x < realTex.width; ++x)
                {
                    int i = x + (y * realTex.width);
                    realPixels[i] = new Color(samples[x, y].R, 0.0f, 0.0f, 1.0f);
                    imaginaryPixels[i] = new Color(samples[x, y].I, 0.0f, 0.0f, 1.0f);
                }

            //Update the textures.
            realTex.SetPixels(realPixels);
            imaginaryTex.SetPixels(imaginaryPixels);
            realTex.Apply(true, false);
            imaginaryTex.Apply(true, false);
        }
    }
}