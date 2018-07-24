using System;
using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {

    public class BoundingBox {

        public Vector2 min;
        public Vector2 max;
        
        public Vector2 halfSize;
        private Vector2 center;
     //   private float radius;
        
        public BoundingBox() {
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);
        }

        public BoundingBox(Vector2 min, Vector2 max) {
            this.min = min;
            this.max = max;
            UpdateSecondary();
        }

        // В случае выпуклой оболочки мы не храним точки массиивом, а храним 
        public BoundingBox(Edge[] edges) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = 0;
            float maxY = 0;

            for (int i = 0; i < edges.Length; i++) {
                minX = Math.Min(edges[i].start.x, minX);
                minY = Math.Min(edges[i].start.y, minY);
                maxX = Math.Max(edges[i].start.x, maxX);
                maxY = Math.Max(edges[i].start.y, maxY);
            }

            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
            
            UpdateSecondary();
        }

        public BoundingBox(Vector2[] points) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = 0;
            float maxY = 0;

            for (int i = 0; i < points.Length; i++) {
                minX = Math.Min(points[i].x, minX);
                minY = Math.Min(points[i].y, minY);
                maxX = Math.Max(points[i].x, maxX);
                maxY = Math.Max(points[i].y, maxY);
            }

            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
            
            UpdateSecondary();
        }

        public void UpdateSecondary() {
            halfSize = new Vector2((max.x - min.x) * 0.5f, (max.y - min.y) * 0.5f);
            center = new Vector2((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f);
          //  radius = Math.Max(halfSize.x, halfSize.y);
        }

        public bool Intersects(BoundingBox box) {
            float dx  = box.center.x - center.x;
            float px  = box.halfSize.x + halfSize.x - Math.Abs(dx);
            if (px <= 0) {
                return false;
            }

            float dy  = box.center.y - center.y;
            float py  = box.halfSize.y + halfSize.y - Math.Abs(dy);
            return py > 0;
        }

        public bool BehindLine(Vector2 point, Vector2 normal) {
            // все 4 точки бокса находятся по normal стороне от линии

            if (Vector2.Dot(min - point, normal) < 0)
                return false;
            
            if(Vector2.Dot(max - point, normal) < 0)
                return false;
            
            if(Vector2.Dot(new Vector2(max.x, min.y) - point, normal) < 0)
                return false;
            
            if(Vector2.Dot(new Vector2(min.x, max.y) - point, normal) < 0)
                return false;

            return true;
        }

        public bool Contains(Vector2 point) {
            return min.x <= point.x && max.x >= point.x &&
                   min.y <= point.y && max.y >= point.y;
        }

        public void Add(BoundingBox other) {
            min = new Vector2(Math.Min(other.min.x, min.x), Math.Min(other.min.y, min.y));
            max = new Vector2(Math.Max(other.max.x, max.x), Math.Max(other.max.y, max.y));
        }

        // https://stackoverflow.com/questions/99353/how-to-test-if-a-line-segment-intersects-an-axis-aligned-rectange-in-2d
        public bool SegmentIntersectRectangle(Vector2 p1, Vector2 p2) {
            // Find min and max X for the segment

            float minX;
            float maxX;

            if (p1.x > p2.x) {
                minX = p2.x;
                maxX = p1.x;
            } else {
                minX = p1.x;
                maxX = p2.x;
            }

            // Find the intersection of the segment's and rectangle's x-projections
            if (maxX > max.x)
                maxX = max.x;

            if (minX < min.x)
                minX = min.x;

            if (minX > maxX) // If their projections do not intersect return false
                return false;

            // Find corresponding min and max Y for min and max X we found before

            float minY = p1.y;
            float maxY = p2.y;

            float dx = p2.x - p1.x;

            if (Math.Abs(dx) > Vector2.kEpsilon) {
                float a = (p2.y - p1.y) / dx;
                float b = p1.y - a * p1.x;
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