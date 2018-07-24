using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Minaev {

    public sealed class Map : IMap {

        private Shape[] obstacles;

        public void Init(Vector2[][] obstacles) {

            this.obstacles = new Shape[obstacles.Length];
            for (int i=0; i < obstacles.Length; i++)
            {
                this.obstacles[i] = new Shape(obstacles[i]);
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {

            List<Vector2> result = new List<Vector2>();
            GetPathInner(start, end, result);

          //  System.Console.WriteLine(result);
            if (true)
            {
                //throw new System.ArgumentException();
            }

            return result;
        }

        private void GetPathInner(Vector2 start, Vector2 end, List<Vector2> result)
        {
            result.Add(start);
            ShapeIntersection nearestIntersection = ShapeIntersection.EMPTY;
            foreach (Shape obstacle in obstacles)
            {
                ShapeIntersection intersecion = obstacle.FindNearestIntersection(start, end);
                if (intersecion.isEmpty())
                {
                    continue;
                }

                if (nearestIntersection.isEmpty()
                    || Vector2.SqrDistance(start, nearestIntersection.IntersectionPoint) > Vector2.SqrDistance(start, intersecion.IntersectionPoint))
                {
                    nearestIntersection = intersecion;
                }
            }

            if (nearestIntersection.isEmpty())
            {
                result.Add(end);
            }
            else
            {
                result.Add(nearestIntersection.IntersectionPoint);
                GetPathInner(nearestIntersection.GetShapeRidLeftPoint(), end, result);
            }
        }
    }
}