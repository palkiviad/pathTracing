using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Peoples.Popov.Clusters {
    
    public class Polygon : IPolygon {
        private static int _lastId;
        public Vector2 BottomLeft { get; private set; }
        public Vector2 BottomRight { get; private set; }
        public Vector2 TopLeft { get; private set; }
        public Vector2 TopRight { get; private set; }

        public float MaxX => Math.Max(TopRight.x, BottomRight.x);

        public float MaxY => Math.Max(TopRight.y, TopLeft.y);

        public float MinX => Math.Min(TopLeft.x, BottomLeft.x);

        public float MinY => Math.Min(BottomLeft.y, BottomRight.y);
        
        
        protected List<Vector2> vertices;
        private List<Segment> segments;
        
        private int _id;
        

        public Polygon(Vector2[] vertices) {
            _lastId++;
            _id = _lastId;
            Init(vertices);
        }

        public static void ResetId() {
            _lastId = 0;
        }

        private void Init(Vector2[] points) {
            TopRight = new Vector2(float.MinValue, float.MinValue);
            TopLeft = new Vector2(float.MaxValue, float.MinValue);
            BottomLeft = new Vector2(float.MaxValue, float.MaxValue);
            BottomRight = new Vector2(float.MinValue, float.MaxValue);
            
            vertices = new List<Vector2>();
            segments = new List<Segment>();
            for (int i = 0; i < points.Length; i++) {
                var point = points[i];
                RefreshExtremeVertices(point);
                vertices.Add(point);
                if (i > 0) {
                    segments.Add(new Segment(points[i-1], point));
                }
            }
            segments.Add(new Segment(points [points.Length -1], points[0]));
        }

        private void RefreshExtremeVertices(Vector2 point) {
            if (TopRight.x < point.x && TopRight.y < point.y) {
                TopRight = point;
            }

            if (TopLeft.x > point.x && TopLeft.y < point.y) {
                TopLeft = point;
            }

            if (BottomLeft.x > point.x && BottomLeft.y > point.y) {
                BottomLeft = point;
            }

            if (BottomRight.x < point.x && BottomRight.y > point.y) {
                BottomRight = point;
            }
        }

        public int GetNearestSegmentIndex(Vector2 currentPoint) {
            var startIndex = -1;
            for (var i = 0; i < segments.Count; i++) {
                if (segments[i].ContainsPoint(currentPoint)) {
                    startIndex = i;
                    break;
                }
            }
            return startIndex;
        }

        public int GetId() {
            return _id;
        }

        public Segment GetSegment(int index) {
            if (segments.Count > index) {
                return segments[index];
            }
            throw new ArgumentOutOfRangeException("segments count less than index " + index);
        }

        public int SegmentsCount() {
            return segments.Count;
        }

        public bool Contains(Vector2 point) {
            var rightX = point.x > MaxX ? point.x + 1 : MaxX + 1;
            var segment = new Segment(point, new Vector2(rightX, point.y));
            if (SegmentIntersectsVertex(segment)) {
                return true;
            }
            return GetAllIntersections(segment).Count % 2 == 1;
        }

        public Vector2? GetNearestIntersection(Segment segment) {
            var intersections = GetAllIntersections(segment);
            Vector2? result = null;
            var shortestDistance = float.MaxValue;
            foreach (var point in intersections) {
                    var distance = Vector2.Distance(segment.StartPoint, point);
                if (shortestDistance <= distance) {
                    continue;
                }
                shortestDistance = distance;
                result = point;
            }
            return result;
        }

        private List<Vector2> GetAllIntersections(Segment segment) {
            var result = new List<Vector2>{};
            foreach (var item in segments) {
                Vector2 intersection = Vector2.down;
                if (Vector2.SegmentToSegmentIntersection(segment.StartPoint, segment.EndPoint, item.StartPoint, item.EndPoint, ref intersection)) {
                    result.Add(intersection);
                }
            }
            return result;
        }

        public bool PointOnEdge(Vector2 point) {
            return segments.Any(item => item.ContainsPoint(point));
        }

        public bool SegmentIntersectsVertex(Segment segment) {
            foreach (var vertex in vertices) {
                if (vertex.Equals(segment.EndPoint) || vertex.Equals(segment.StartPoint)) {
                    continue;
                }
                if (segment.ContainsPoint(vertex)) {
                    return true;
                }
            }
            return false;
        }

        public bool LayOnSameHalfPlane(Segment segment) {
            var sign = 0;
            Vector2 halfPlaneVector = segment.EndPoint - segment.StartPoint;
            foreach (var contourSegment in segments) {
                var newSign = Utils.GetHalfPlaneSign(halfPlaneVector, contourSegment.EndPoint - segment.StartPoint);
                if (newSign == 0) {
                    continue;
                }
                if (sign != 0 && newSign != sign) {
                    return false;
                }

                sign = newSign;
            }
            return true;
            
        }

        
    }
}