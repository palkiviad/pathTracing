using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder.Popov;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
    public class Contour {
        private List<Segment> _segments;
        private static int _uniqueId = 1;
        private int _id;

        public int Id {
            get { return _id; }
            private set { _id = value; }
        }

        public IList<Segment> Segments {
            get { return _segments; }
        }

        public Contour(Vector2[] vertices) {
            Id = _uniqueId;
            _uniqueId++;
            _segments = new List<Segment>();
            for (int i = 0; i < vertices.Length - 1; i++) {
                _segments.Add(new Segment(vertices[i], vertices[i+1]));
            }
            _segments.Add(new Segment(vertices[vertices.Length-1], vertices[0]));
        }

        public Vector2? GetNearestIntersection(Vector2 start, Vector2 end) {
            Vector2? result = null;
            float shortestDistance = float.MaxValue;
            foreach (var segment in _segments) {
                Vector2 intersection = Vector2.down;
                if (Vector2.SegmentToSegmentIntersection(start, end, segment.StartPoint, segment.EndPoint, ref intersection)) {
                    float distance = Vector2.Distance(start, intersection);
                    if (shortestDistance > distance) {
                        shortestDistance = distance;
                        result = intersection;
                    }
                }
            }
            return result;
        }

        public static void ResetId() {
            _uniqueId = 0;
        }

        public bool HasPoint(Vector2 point) {
            return _segments.Any(item => item.ContainsPoint(point));
        }
    }
}