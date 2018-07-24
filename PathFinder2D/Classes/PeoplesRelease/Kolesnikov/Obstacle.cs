using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Release.Kolesnikov {
    public class Obstacle {
       
       
        public readonly Vector2[] vectors;
        public Obstacle(Vector2[] vectors)
        {
            this.vectors = vectors;
        }
        
        // ближайшее пересечение с фигурой
        public bool GetNeaerestIntersectionWithObstacle(Vector2 startPoint, Vector2 endPoint, ref Vector2 minDistanceIntersectionPoint, ref Vector2 partOfObstacle)
        {
            List<Vector2> intersectionList = new List<Vector2>();
            List<Vector2> parts = new List<Vector2>();

            // ближайшая к стартовой точка пересечения
            Vector2 prePoint = startPoint;
            bool firstRun = true;
            var intersectionPoint = new Vector2(0, 0);

            foreach (var point in this.vectors)
            {
                bool intersection = false;
                if (firstRun)
                {
                    // точки фигуры обстакла
                    var firstPoint = this.vectors.First<Vector2>();
                    var lastPoint = this.vectors.Last<Vector2>();
                    // первый с последним
                    intersection = Vector2.SegmentToSegmentIntersectionWithBounds(startPoint, endPoint, lastPoint, firstPoint, ref intersectionPoint);
                    firstRun = false;
                }
                else
                {
                    intersection = Vector2.SegmentToSegmentIntersectionWithBounds(startPoint, endPoint, prePoint, point, ref intersectionPoint);
                }

                if (intersection)
                {
                    intersectionList.Add(intersectionPoint); // тут пересечение
                    parts.Add(point);                        // с какой частью
                }

                prePoint = point;
            }
            
            if (intersectionList.Count == 1) {
                minDistanceIntersectionPoint = intersectionList[0];
                partOfObstacle = parts[0];
                return true;
                
            } else if (intersectionList.Count > 1) // ищем среди пересеченных ближайшую точку к стартовой 
            {
                float distance = 1000000; // min distance
                int index = 0;
                int indexMin = 0;

                foreach (var iPoint in intersectionList)
                {
                    float d = Vector2.Distance(startPoint, iPoint);
                    if (distance > d)
                    {
                        distance = d;
                        indexMin = index;
                    }
                    index++;
                }

                minDistanceIntersectionPoint = intersectionList[indexMin];
                partOfObstacle = parts[indexMin];

                return true;
            }

            return false;
        }
        
        
        public int GetNextVertex(Vector2 point) { //  left hand path
            int index = 0;
            foreach (var p in this.vectors) {
                if (point.Equals(p)) {
                    if (index < (this.vectors.Length - 2)) {
                        return index + 1;
                    }
                }

                index++;
            }

            return 0;
        }
        
        public int GetPreVertex(Vector2 point) { //  right hand path
            int index = 0;
            foreach (var p in this.vectors) {
                if (point.Equals(p)) {
                    if (index !=0) {
                        return index - 1;
                    } else if (index == 0) {
                        return vectors.Length - 1;
                    }
                }

                index++;
            }

            return 0;
        }
        
        public Vector2 GetBeginPartPoint(Vector2 point)
        {
            Vector2 prev = this.vectors.Last<Vector2>();
            foreach (var vector in this.vectors)
            {
                if (vector.Equals(point))
                {
                    return prev;
                }
                prev = vector;
            }

            return prev;
        }

        // пересекаемся с точкой
        public bool IsIntesectionWithVertex(Vector2 point) {
            return vectors.Any(p => p.Equals(point));
        }
    }
}