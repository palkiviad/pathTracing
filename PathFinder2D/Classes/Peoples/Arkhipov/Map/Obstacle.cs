using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Arkhipov 
{
    internal class Obstacle
    {
        private readonly List<Segment> _segments;

        public Obstacle(Vector2[] points)
        {
            _segments = new List<Segment>();
            for (int i = 0; i < points.Length; ++i)
            {
                _segments.Add(i < points.Length - 1 
                    ? new Segment(points[i], points[i + 1]) 
                    : new Segment(points[i], points[0]));
            }
        }

        public bool GetClosestIntersection(Segment ray, out SegmentIntersection intersection)
        {
            List<SegmentIntersection> intersections = new List<SegmentIntersection>();
            foreach (var segment in _segments)
            {
                Vector2 i = new Vector2();
                if (MathHelper.SegmentToSegmentIntersection(segment, ray, ref i))
                {
                    intersections.Add(new SegmentIntersection(segment, i));
                }
            }
            if (intersections.Count == 0)
            {
                intersection = null;
                return false;
            }
            intersection = intersections[0];
            float dist = (intersection.Point - ray.Point1).sqrMagnitude;
            for (int i = 1; i < intersections.Count; ++i)
            {
                float tmp = (intersections[i].Point - ray.Point1).sqrMagnitude;
                if (tmp < dist)
                {
                    intersection = intersections[i];
                    dist = tmp;
                }
            }
            return true;
        }

        public IList<Vector2> GetPathAroundObstacle(SegmentIntersection from, Vector2 closestTo)
        {
            int indexOfSegment = _segments.IndexOf(from.Segment);
            var clockwise = new List<Vector2>();
            bool finished = false;
            for (int i = indexOfSegment; i < _segments.Count; ++i)
            {
                clockwise.Add(_segments[i].Point2);
                SegmentIntersection si;
                if (!GetClosestIntersection(new Segment(_segments[i].Point2, closestTo), out si))
                {
                    finished = true;
                    break;
                }
            }
            if (!finished)
            {
                for (int i = 0; i < indexOfSegment; ++i)
                {
                    clockwise.Add(_segments[i].Point2);
                    SegmentIntersection si;
                    if (!GetClosestIntersection(new Segment(_segments[i].Point2, closestTo), out si))
                    {
                        break;
                    }
                }
            }
            
            var counterClockwise = new List<Vector2>();
            finished = false;
            for (int i = indexOfSegment; i >= 0; --i)
            {
                counterClockwise.Add(_segments[i].Point1);
                SegmentIntersection si;
                if (!GetClosestIntersection(new Segment(_segments[i].Point1, closestTo), out si))
                {
                    finished = true;
                    break;
                }
            }
            if (!finished)
            {
                for (int i = _segments.Count - 1; i > indexOfSegment; --i)
                {
                    counterClockwise.Add(_segments[i].Point1);
                    SegmentIntersection si;
                    if (!GetClosestIntersection(new Segment(_segments[i].Point1, closestTo), out si))
                    {
                        break;
                    }
                }
            }

            float cwLen = GetLen(from.Point, clockwise);
            float ccwLen = GetLen(from.Point, counterClockwise);
            return (cwLen < ccwLen) ? clockwise : counterClockwise;
        }

        private float GetLen(Vector2 start, List<Vector2> path)
        {
            float len = 0;
            Vector2 prevPoint = start;
            foreach (var endPoint in path)
            {
                len += (endPoint - prevPoint).sqrMagnitude;
                prevPoint = endPoint;
            }
            return len;
        }
    }
}