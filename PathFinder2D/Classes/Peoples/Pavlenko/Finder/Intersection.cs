
using PathFinder.Mathematics;

namespace PathFinder.Pavlenko {
    
    public class Intersection {
        public float distance;
        public Contour contour;
        public Edge edge;
        public Vector2 point;

        public Intersection(float distance, Contour contour, Edge edge, Vector2 point) {
            this.distance = distance;
            this.contour = contour;
            this.edge = edge;
            this.point = point;
        }
    }
}