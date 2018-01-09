using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LSystem
{
    /// <summary>
    /// The state of an L-system. Can compute the next state.
    /// </summary>
	[Serializable]
    public struct LState
    {
        public string Value;
        public List<Rule> Rules;
        public PrioritySystem RulePriorities;

        public PRNG R;

		
		public LState(string seed, List<Rule> rules, PrioritySystem rulePriorities, PRNG r)
        {
            Value = seed;
            Rules = rules;
            RulePriorities = rulePriorities;

            R = r;
        }
		public LState(string seed, List<Rule> rules, PrioritySystem rulePriorities)
			: this(seed, rules, rulePriorities, PRNG.Make()) { }


		public LState Evaluate()
        {
            StringBuilder newStr = new StringBuilder(Value.Length * 2);
            List<int> applicableRules = new List<int>(Rules.Count);

            //Try to apply a rule to each character.
            for (int i = 0; i < Value.Length; ++i)
            {
                //Get all rules that apply to this character.
                applicableRules.Clear();
                for (int j = 0; j < Rules.Count; ++j)
                {
                    if (Rules[j].Trigger == Value[i])
                    {
                        applicableRules.Add(j);
                    }
                }

                //Choose and apply a rule if one exists, otherwise just leave the character alone.
                if (applicableRules.Count > 0)
                {
                    int j = RulePriorities.ChooseRule(R, applicableRules.Count);
                    newStr.Append(Rules[applicableRules[j]].ReplaceWith);
                }
                else
                {
                    newStr.Append(Value[i]);
                }
            }

			return new LState(newStr.ToString(), Rules, RulePriorities, R);
        }
    }
}
