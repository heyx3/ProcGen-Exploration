using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;


namespace LSystem
{
    /// <summary>
    /// Defines how to choose a rule from multiple applicable ones.
    /// </summary>
	[Serializable]
    public abstract class PrioritySystem
    {
        public abstract int ChooseRule(PRNG r, int nRules);
    }

    /// <summary>
    /// Chooses rules completely randomly.
    /// </summary>
	[Serializable]
    public class PrioritySystem_Random : PrioritySystem
    {
		public override int ChooseRule(PRNG r, int nRules)
        {
			return r.NextInt () % nRules;
        }
    }

    /// <summary>
    /// More likely to choose earlier/later rules based on a parameter.
	/// </summary>
	[Serializable]
    public class PrioritySystem_Order : PrioritySystem
    {
        /// <summary>
        /// Values between 0 and 1 bias this instance toward later rules.
        /// Values greater than 1 bias this instance toward earlier rules.
        /// </summary>
        public float Distribution = 1.0f;

        public PrioritySystem_Order(float distribution = 1.0f)
        {
            Distribution = distribution;
        }

		public override int ChooseRule(PRNG r, int nRules)
        {
            //Make a random double with equal distribution between 0 and (nRules - 1).
            float f = r.NextFloat();            
            const float minF = -0.5f;
			float maxF = (float)nRules - 0.5f;
			f = Mathf.Lerp(minF, maxF, f);

            //Bias it.
            f = Mathf.Pow(f, Distribution);

            return Mathf.RoundToInt(f);
        }
    }
}
