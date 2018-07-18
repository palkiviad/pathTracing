using System;
using PathFinder.Mathematics;

namespace PathFinder.Pavlenko {

    public class Contour {

        private Vector2[] points;
        private BoundingBox bounds;
        private Edge[] edges;

        public Contour(Vector2[] points) {
            this.points = points;
            
            // сначала - последнее ребро

            edges = new Edge[points.Length];
                
            Edge previous =  new Edge(points.Length-1, points[points.Length-1], points[0]);
            edges[points.Length-1] = previous;
            
            int i = 0;
            for (; i < points.Length-1; i++) {
                Edge current = new Edge(i, points[i], points[i+1]);
                current.previous = previous;
                previous.next = current;
                previous = current;

                edges[i] = current;
            } 
            
            // А теперь последняя связка
            edges[points.Length - 1].previous = previous;
            previous.next = edges[points.Length - 1];

            foreach (var edge in edges) {
                if (edge.next == null || edge.previous == null) {
                    throw new Exception("Should not be");
                }
            }
            
            bounds = new BoundingBox(points);
        }


        public Intersection GetIntersection(Vector2 start, Vector2 end, bool visible = true) {

            if (!bounds.SegmentIntersectRectangle(start, end))
                return null;

            Edge probably = null;
            Vector2 probablePoint = Vector2.zero;
            Vector2 point = Vector2.zero;
            float distance = int.MaxValue;
            
            Vector2 dir = start - end;

            
            
            // нас интересуют лишь рёбра, открытые к вектору!
            foreach (var edge in edges) {
                if (visible && !edge.VisibleTo(dir))
                    continue;
                
                if(!Vector2.SegmentToSegmentIntersection(start, end, edge.start, edge.end, ref point))
                    continue;

                float d = Vector2.Distance(start, point);
                
                if (probably == null || d< distance) {
                    probably = edge;
                    probablePoint = point;
                    distance = d;
                } 
            }


            return probably == null ? null : new Intersection(distance, this, probably, probablePoint);
        }
    }
}