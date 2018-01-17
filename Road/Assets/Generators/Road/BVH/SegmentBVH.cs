namespace Generators.Road
{
    public class SegmentBVH : BVH.BVH<Segment>
    {
        public SegmentBVH(int threshold) : base(threshold) { }

        public override UnityEngine.Rect GetBounds(Segment d)
        {
            return d.AABB;
        }
    }
}