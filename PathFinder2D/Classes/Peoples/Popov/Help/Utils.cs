using System;
using System.Collections.Generic;
using PathFinder.Mathematics;
using PathFinder.Popov;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
    public static class Utils {
        
        
        public static List<IntersectedContour> GetIntersectedContours(Vector2 start, Vector2 goal, Contour[] contours) {
            if (contours == null) throw new ArgumentNullException("Contour[]");
            var result = new List<IntersectedContour>();
            foreach (var contour in contours) {
                Vector2? point = contour.GetNearestIntersection(start, goal);
                if (point.HasValue) {
                    result.Add(new IntersectedContour(contour, point.Value));
                }
            }
            return result;
        }
    }
}