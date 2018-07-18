namespace PathFinder.Arkhipov
{
    using Vector2 = Mathematics.Vector2;
    
    internal static class MathHelper
    {
        internal static bool AlmostEquals(this Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).sqrMagnitude < 0.01f;
        }

        internal static bool SegmentToSegmentIntersection(Segment s1, Segment s2, ref Vector2 i)
        {
            return Vector2.SegmentToSegmentIntersection(s1.Point1, s1.Point2, s2.Point1, s2.Point2, ref i);
        }
    }
}