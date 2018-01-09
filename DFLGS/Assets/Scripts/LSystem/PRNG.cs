using System;
using UnityEngine;


[Serializable]
public struct PRNG
{
	/// <summary>
	/// Creates a new instance using the current time.
	/// </summary>
	public static PRNG Make() { return new PRNG (DateTime.Now.Millisecond); }


	public int Seed;

	public PRNG(int seed) { Seed = seed; }


	/// <summary>
	/// Returns a random positive integer.
	/// </summary>
	public int NextInt()
	{
		Seed = (Seed ^ 61) ^ (Seed >> 16);
		Seed += (Seed << 3);
		Seed ^= (Seed >> 4);
		Seed *= 0x27d4eb2d;
		Seed ^= (Seed >> 15);
		return Seed;
	}
	/// <summary>
	/// Returns a random float from 0 to 1.
	/// </summary>
	public float NextFloat()
	{
		const int max = 16777216;
		return (float)(NextInt() % max) / (float)max;
	}
}