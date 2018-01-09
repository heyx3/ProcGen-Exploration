using UnityEngine;


namespace Generators.Road
{
    public class VertexBVH : BVH.BVH<Vertex>
    {
        public float MergeRadius;

        public VertexBVH(float mergeRadius, int threshold)
            : base(threshold)
        {
            MergeRadius = mergeRadius;
        }
		
        public override Rect GetBounds(Vertex d)
        {
            return new Rect(d.Pos.x - MergeRadius, d.Pos.y - MergeRadius,
                            MergeRadius * 2.0f, MergeRadius * 2.0f);
        }
    }
}