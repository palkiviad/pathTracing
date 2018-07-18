using System;
using System.Collections.Generic;
using PathFinder.Mathematics;


namespace PathFinder.Pavlenko {

    // Вот это плохой алгоитм.
    public class BoundingCircle {
        
        private readonly Vector2 center;
        private readonly float radius;

        public BoundingCircle(IList<Vector2> points) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = 0;
            float maxY = 0;

            for (int i = 0; i < points.Count; i++) {
                minX = Math.Min(points[i].x, minX);
                minY = Math.Min(points[i].y, minX);
                maxX = Math.Max(points[i].x, maxX);
                maxY = Math.Max(points[i].y, maxY);
            }

            center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            radius = Math.Max(maxX - minX, maxY - minY);
        }

        public bool Contains(Vector2 point) {
            return Vector2.Distance(center, point) <= radius;
        }
    }
}