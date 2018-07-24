using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {

    public class Edge {

    //    private readonly int index;

        public Vector2 start;
        public Vector2 end;

        public  Vector2 vector;
        public  Vector2 backVector;
        
        public Edge next;
        public Edge previous;

        private  Vector2 perpendicular;

        public Edge(Vector2 start, Vector2 end) {
            this.start = start;
            this.end = end;

            UpdateSecondary();
        }

        private void UpdateSecondary() {
            vector = end - start;
            backVector = start - end;
            perpendicular = Vector2.Perpendicular(vector);
        }

        public void Flip() {
            {
                Vector2 tmp = start;
                start = end;
                end = tmp;
            }
            
            {
                Edge tmp = previous;
                previous = next;
                next = tmp;
            }
            
            UpdateSecondary();
        }

        public bool VisibleTo(Vector2 other) {
            return perpendicular.x * other.x + perpendicular.y * other.y > 0;
        }
    }
}