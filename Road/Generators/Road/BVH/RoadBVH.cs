namespace Generators.Road
{
    public class RoadBVH : BVH.BVH<Road>
    {
        public RoadBVH(int threshold) : base(threshold) { }

		public override UnityEngine.Rect GetBounds(Road d)
        {
            return d.BoundingBox;
        }
    }
}