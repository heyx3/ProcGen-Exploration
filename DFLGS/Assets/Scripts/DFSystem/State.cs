using System;
using UnityEngine;


namespace DFSystem
{
    /// <summary>
    /// A 3D transform. Affects all subsequent shapes that are created.
    /// </summary>
	public class State123456789
	{
		private Vector3 pos;
        private float scale;
		private Quaternion rot;

		private bool modified = true;
		private Vector3 forward, right, up;
		private Matrix4x4 toWorld, toLocal;


		public Vector3 Pos { get { return pos; } set { pos = value; modified = true; } }
		public float Scale { get { return scale; } set { scale = value; modified = true; } }
		public Quaternion Rot { get { return rot; } set { rot = value; modified = true; } }

		public Vector3 Forward { get { Recompute(); return forward; } }
		public Vector3 Right { get { Recompute(); return right; } }
		public Vector3 Up { get { Recompute(); return up; } }
		public Matrix4x4 ToWorld { get { Recompute(); return toWorld; } }
		public Matrix4x4 ToLocal { get { Recompute(); return toLocal; } }

		public State Parent { get; private set; }


		public State(State parent = null)
			: this(Vector3.zero, 1.0f, Quaternion.identity, parent) { }
		public State(Vector3 _pos, float _scale, Quaternion _rot, State parent = null)
		{
			pos = _pos;
			scale = _scale;
			rot = _rot;

			forward = rot * Vector3.forward;
			right = rot * Vector3.right;
			up = rot * Vector3.up;

			toWorld = Matrix4x4.TRS(pos, rot, new Vector3(scale, scale, scale));
			toLocal = toWorld.inverse;

			Parent = parent;
		}


		private void Recompute()
		{
			if (!modified)
				return;
			modified = false;

			forward = Rot * Vector3.forward;
			right = rot * Vector3.right;
			up = rot * Vector3.up;

			toWorld = Matrix4x4.TRS(pos, rot, new Vector3(scale, scale, scale));
			toLocal = toWorld.inverse;
		}
	}
}
