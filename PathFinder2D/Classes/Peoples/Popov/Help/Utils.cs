using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Clusters;
using PathFinder.Popov;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
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

        public static List<PolygonIntersection> GetIntersectedPolygons(Vector2 start, Vector2 goal, List<IPolygon> polygons) {
            var result = new List<PolygonIntersection>();
            foreach (var contour in polygons) {
                var point = contour.GetNearestIntersection(new Segment(start, goal));
                if (point.HasValue) {
                    result.Add(new PolygonIntersection(contour, point.Value));
                }
            }

            return result;
        }


        public class PolygonIntersection {
            public IPolygon Polygon { get; }

            public Vector2 Intersection { get; }

            public PolygonIntersection(IPolygon polygon, Vector2 intersection) {
                Polygon = polygon;
                Intersection = intersection;
            }
        }
    }
}