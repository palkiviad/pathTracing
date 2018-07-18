using PathFinder.Mathematics;

namespace PathFinder.Arkhipov 
{
    internal class SegmentIntersection
    {
        public Segment Segment { get; private set; }
        public Vector2 Point { get; private set; }

        public SegmentIntersection(Segment segment, Vector2 point)
        {
            Segment = segment;
            Point = point;
        }
    }

    internal class Segment
    {
        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }
                
        public Segment(Vector2 p1, Vector2 p2)
        {
            Point1 = p1;
            Point2 = p2;
        }
    }
}