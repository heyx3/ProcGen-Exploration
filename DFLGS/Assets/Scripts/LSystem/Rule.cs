using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LSystem
{
    /// <summary>
    /// A single rule in an L-system.
    /// Replaces a "trigger" character with a string.
    /// </summary>
	[Serializable]
    public struct Rule
    {
        public char Trigger;
        public string ReplaceWith;

        public Rule(char trigger, string replaceWith)
        {
            Trigger = trigger;
            ReplaceWith = replaceWith;
        }
    }
}
