using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Pavlenko {

    public sealed class Map : IMap {

        private Contour[] contours;
        
        public void Init(Vector2[][] obstacles) {
            
            contours = new Contour[obstacles.Length];
            
            for(int i=0;i<obstacles.Length; i++)
                contours[i] = new Contour(obstacles[i]);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {

           List<Vector2> result = new List<Vector2> {start};

            while (true) {
                
                Intersection nearest = null;
                // Ближайшее пересечение
                for (int i = 0; i < contours.Length; i++) {
                    Intersection inter = contours[i].GetIntersection(result.Last(), end);
                    if (inter == null)
                        continue;

                    if (nearest == null || nearest.distance > inter.distance)
                        nearest = inter;
                }

                if (nearest == null) {
                    result.Add(end);
                    return result;
                }

                // но если пересечение нашллось, мы должны пройти по контуру до открытой грани, а она должна быть 100%
                result.Add(nearest.point);
                result.Add(nearest.edge.end);

                Vector2 back = end - nearest.edge.end;

                Edge e = nearest.edge.next;
                while (!e.VisibleTo(back) || nearest.contour.GetIntersection(e.start, end, false) != null) {
                    result.Add(e.end);
                    //innerStart = e.end;
                    back = end - e.end;
                    e = e.next;
                }
            }
        }
    }
}