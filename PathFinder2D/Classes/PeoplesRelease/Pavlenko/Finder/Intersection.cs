using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {

    public class Intersection {
        public readonly float distance;
        public readonly Contour contour;
        public readonly Edge edge;
        public readonly Vector2 point;

        public Intersection(float distance, Contour contour, Edge edge, Vector2 point) {
            this.distance = distance;
            this.contour = contour;
            this.edge = edge;
            this.point = point;
        }
    }
}