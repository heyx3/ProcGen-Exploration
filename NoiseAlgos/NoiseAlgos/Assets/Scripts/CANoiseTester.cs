using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Tests
{
	/// <summary>
	/// Runs CellAutomataNoise and displays the results.
	/// </summary>
	public class CANoiseTester : MonoBehaviour
	{
		public enum States
		{
			Start,
			Iterating,
			Finished,
			Error,
		}


		public MutatedAverageNoise Noise = new MutatedAverageNoise();

		public States State { get; private set; }


		private void Awake()
		{
			State = States.Start;
		}
		private void OnDestroy()
		{
			Noise.Dispose();
		}

		private void OnGUI()
		{
			RenderTexture resultTex;

			switch (State)
			{
				case States.Start:
					if (GUILayout.Button("Start"))
					{
						try
						{
							Noise.Init();
							State = States.Iterating;
						}
						catch (Exception e)
						{
							Debug.LogError(e.Message + "\n\t" + e.StackTrace);
							State = States.Error;
						}
					}
					break;

				case States.Iterating:
					GUILayout.Label("Running...");
					resultTex = Noise.GetResult();

					var texSpace = GUILayoutUtility.GetRect(resultTex.width, resultTex.height);
					GUI.Box(new Rect(texSpace.min - new Vector2(2, 2),
									 texSpace.size + new Vector2(4, 4)),
							"");
					GUI.DrawTexture(texSpace, resultTex, ScaleMode.ScaleToFit);

					int nIterations = 0;
					if (GUILayout.Button("Run 1 iteration"))
						nIterations = 1;
					if (GUILayout.Button("Run 10 iterations"))
						nIterations = 10;
					if (GUILayout.Button("Run 100 iterations"))
						nIterations = 100;
					if (GUILayout.Button("Run to end"))
						nIterations = int.MaxValue;

					bool isFinished = false;
					while (nIterations > 0 && !isFinished)
					{
						isFinished = Noise.Update();
						nIterations -= 1;
					}

					break;

				case States.Finished:
					GUILayout.Label("Finished!");
					resultTex = Noise.GetResult();
					GUI.DrawTexture(GUILayoutUtility.GetRect(resultTex.width, resultTex.height),
									resultTex, ScaleMode.ScaleToFit);
					break;

				case States.Error:
					GUILayout.Label("ERROR!");
					break;

				default:
					Debug.LogError("Unknown state " + State.ToString());
					break;
			}
		}
	}
}
