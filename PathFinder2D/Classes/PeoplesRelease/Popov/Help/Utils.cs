using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Popov {
    public static class Utils {


       public static int GetHalfPlaneSign(Vector2 halfPlaneVector, Vector2 contourVector) {
            float value = halfPlaneVector.x * contourVector.y - contourVector.x * halfPlaneVector.y;
            if (Math.Abs(value) < float.Epsilon) {
                return 0;
            }

            return Math.Sign(value);
        }

       public static float CalculatePathDistance(IList<Vector2> path) {
            float fullPath = 0;
            for (int i = 0; i < path.Count - 1; i++) {
                fullPath += Vector2.Distance(path[i], path[i + 1]);
            }

            return fullPath;
        }

        public static List<PolygonIntersection> GetIntersectedPolygons(Vector2 start, Vector2 goal, IPolygon[] polygons) {
            var result = new List<PolygonIntersection>();
            for (var i = 0; i < polygons.Length; i++) {
                var contour = polygons[i];
                var point = contour.GetNearestIntersection(new Segment(start, goal));
                if (point.HasValue) {
                    result.Add(new PolygonIntersection(contour, point.Value));
                }
            }

            return result;
        }

        public static IPolygon[] RemovePolygonsOutsideBounds(Vector2 start, Vector2 end, IList<IPolygon> polygons) {
            var segment = new Segment(start, end);
            var polygonsToRemove = new List<int>(polygons.Count);
            for (var i = 0; i < polygons.Count; i++) {
                var polygon = polygons[i];
                if (!polygon.LayInSegmentBounds(segment)) {
                    polygonsToRemove.Add(i);
                }
            }

            var restCount = polygons.Count - polygonsToRemove.Count;
            var result = new IPolygon [restCount];
            var startIndex = 0;
            for (var i = 0; i < polygons.Count; i++) {
                if (polygonsToRemove.IndexOf(i) >= 0) continue;
                result[startIndex] = polygons[i];
                startIndex++;
            }
            return result;
        }


        public class PolygonIntersection {
            public IPolygon Polygon { get; private set; }

            public Vector2 Intersection { get; private set; }

            public PolygonIntersection(IPolygon polygon, Vector2 intersection) {
                Polygon = polygon;
                Intersection = intersection;
            }
        }
        
        
    }
}