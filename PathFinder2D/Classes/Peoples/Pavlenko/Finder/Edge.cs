using PathFinder.Mathematics;

namespace PathFinder.Pavlenko {

    public class Edge {

        private int index;
        
        public Vector2 start;
        public Vector2 end;

        private Vector2 vector;

        public Edge next;
        public Edge previous;

        private Vector2 perpendicular;
        
        public Edge(int index, Vector2 start, Vector2 end) {
            this.index = index;
            this.start = start;
            this.end = end;

            vector = end - start;
            perpendicular = Vector2.Perpendicular(vector);
        }

        public bool VisibleTo(Vector2 other) {
           // return vector.x * other.y - vector.y * other.x >= 0;
            return perpendicular.x * other.x + perpendicular.y * other.y > 0;
        }
    }
}