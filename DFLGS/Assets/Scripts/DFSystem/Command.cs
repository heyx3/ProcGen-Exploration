using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
	public abstract class Command
	{
		/// <summary>
		/// Applies this command from the given node.
		/// </summary>
		public abstract bool Apply(ref Node currentPos);
	}
}
