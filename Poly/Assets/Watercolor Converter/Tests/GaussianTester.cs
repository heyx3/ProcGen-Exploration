using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WatercolorConverter.Tests
{
    /// <summary>
    /// Tests the gaussian filters.
    /// </summary>
    public class GaussianTester : MonoBehaviour
    {
        public Shader GaussianShader5, GaussianShader9;
        public Texture2D ToBlur;
		public Renderer Display_Input, Display_Output5, Display_Output9;
		public int NIterations = 3;

		private RenderTexture outGauss5, outGauss9;


		/// <summary>
		/// Generates the blurred textures.
		/// </summary>
        public void Start()
        {
			Display_Input.material.mainTexture = ToBlur;

			Texture temp5 = ToBlur,
					temp9 = ToBlur;
			for (int i = 0; i < NIterations; ++i)
			{
				temp5 = GaussianBlur.Blur(GaussianBlur.Qualities.Five, temp5);
				temp9 = GaussianBlur.Blur(GaussianBlur.Qualities.Nine, temp9);
			}

			Display_Output5.material.mainTexture = temp5;
			Display_Output9.material.mainTexture = temp9;
        }
	}
}