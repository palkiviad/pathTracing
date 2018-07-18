using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Pavlenko {
    
    public class BoundingBox {

        private Vector2 min;
        private Vector2 max;
        
        public BoundingBox(IList<Vector2> points) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = 0;
            float maxY = 0;

            for (int i = 0; i < points.Count; i++) {
                minX = Math.Min(points[i].x, minX);
                minY = Math.Min(points[i].y, minY);
                maxX = Math.Max(points[i].x, maxX);
                maxY = Math.Max(points[i].y, maxY);
            }
            
            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
            
//            center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
//            radius = Math.Max(maxX - minX, maxY - minY);
        }
        
        public bool Contains(Vector2 point) {
            return min.x <= point.x && max.x >= point.x &&
                   min.y <= point.y && max.y >= point.y;
        }

        public bool SegmentIntersectRectangle(Vector2 a_p1, Vector2 a_p2) {
            // Find min and max X for the segment

            float minX;
            float maxX;

            if (a_p1.x > a_p2.x) {
                minX = a_p2.x;
                maxX = a_p1.x;
            } else {
                minX = a_p1.x;
                maxX = a_p2.x;
            }

            // Find the intersection of the segment's and rectangle's x-projections
            if (maxX > max.x)
                maxX = max.x;

            if (minX < min.x)
                minX = min.x;

            if (minX > maxX) // If their projections do not intersect return false
                return false;

            // Find corresponding min and max Y for min and max X we found before

            float minY = a_p1.y;
            float maxY = a_p2.y;

            float dx = a_p2.x - a_p1.x;

            if (Math.Abs(dx) > Vector2.kEpsilon) {
                float a = (a_p2.y - a_p1.y) / dx;
                float b = a_p1.y - a * a_p1.x;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY) {
                float tmp = maxY;
                maxY = minY;
                minY = tmp;
            }
            
            // Find the intersection of the segment's and rectangle's y-projections

            if (maxY > max.y)
                maxY = max.y;

            if (minY < min.y)
                minY = min.y;

            // If Y-projections do not intersect return false
            return minY <= maxY;
        }
    }
}