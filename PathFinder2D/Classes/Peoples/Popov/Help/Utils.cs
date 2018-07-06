using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using PathFinder.Mathematics;
using PathFinder.Popov;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
    public static class Utils {
        
        
        public static List<IntersectedContour> GetIntersectedContours(Vector2 start, Vector2 goal, Contour[] contours) {
            if (contours == null) throw new ArgumentNullException(nameof(contours));
            var result = new List<IntersectedContour>();
            foreach (var contour in contours) {
                Vector2? point = contour.GetNearestIntersection(start, goal);
                if (point.HasValue) {
                    result.Add(new IntersectedContour(contour, point.Value));
                }
            }
            return result;
        }


        public static bool ContourLayInSameHalfPlane(Contour contour, Segment segment) {
            int sign = 0;
            Vector2 halfPlaneVector = segment.EndPoint - segment.StartPoint;
            foreach (var contourSegment in contour.Segments) {
                int newSign = GetHalfPlaneSign(halfPlaneVector, contourSegment.EndPoint - segment.StartPoint);
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

        public static int GetHalfPlaneSign(Vector2 halfPlaneVector, Vector2 contourVector) {
            float value = halfPlaneVector.x * contourVector.y - contourVector.x * halfPlaneVector.y;
            if (Math.Abs(value) < float.Epsilon) {
                return 0;
            }
            return Math.Sign(value);
        }
    }
}