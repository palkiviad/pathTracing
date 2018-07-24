using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using System;

namespace PathFinder.Kolesnikov {

    public sealed class Map : IMap
    {

        private struct Point
        {
            public Point(Vector2 vector)
            {
                this.vector = vector;
                x = vector.x;
                y = vector.y;
            }
            public float x;
            public float y;

            Vector2 vector;
        }

        public struct Obstacle
        {
            public Obstacle(Vector2[] vectors)
            {
                this.vectors = vectors;

                points = new List<Point>();
                foreach (var one in vectors)
                {
                    points.Add(new Point(one));
                }
            }
            public Vector2[] vectors;
            List<Point> points;
        }

        private List<Obstacle> obstacleList;

        public void Init(Vector2[][] obstacles)
        {
            obstacleList = new List<Obstacle>();
            foreach (var oneObstacle in obstacles)
            {
                obstacleList.Add(new Obstacle(oneObstacle));
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            List<Vector2> myWay = new List<Vector2>();
            myWay.Add(start);

            Vector2 fromPoint = start;
            Vector2 prePoint = start;
            Vector2 endObstaclePart = new Vector2();
            Vector2 intersectionPoint = new Vector2();

            int index = 0;
            foreach (var firstObst in obstacleList)
            {
                while (GetAllIntersectionWithObstacle(fromPoint, end, firstObst, ref intersectionPoint, ref endObstaclePart))
                {
                    myWay.Add(intersectionPoint);

                    // left or right ? 
                    Vector2 beginObstaclePart = GetBeginPartPoint(endObstaclePart, firstObst); // начало обстакла
                    fromPoint = Vector2.Distance(intersectionPoint, beginObstaclePart) < Vector2.Distance(intersectionPoint, endObstaclePart) ? beginObstaclePart : endObstaclePart;
                    fromPoint = new Vector2(endObstaclePart.x, endObstaclePart.y);
 
                    myWay.Add(fromPoint);
                    index++;

                    if (index > 10000) // непорядок
                    {
                        break;
                    } 
                }
            }

            myWay.Add(end);

            return myWay;
        }

        // все пересечения с фигурой епта.
        private bool GetAllIntersectionWithObstacle(Vector2 startPoint, Vector2 endPoint, Obstacle obst, ref Vector2 minDistanceIntersectionPoint, ref Vector2 partOfObstacle)
        {
            List<Vector2> intersectionList = new List<Vector2>();
            List<Vector2> parts = new List<Vector2>();

            // ближайшая к стартовой точка пересечения
            Vector2 prePoint = startPoint;
            bool firstRun = true;

            // точки фигуры обстакла
            var firstPoint = obst.vectors.First<Vector2>();
            var lastPoint = obst.vectors.Last<Vector2>();

            var intersectionPoint = new Vector2(0, 0);

            foreach (var point in obst.vectors)
            {
                bool intersection = false;
                if (firstRun)
                {
                    // первый с последним
                    intersection = SegmentToSegmentIntersection(startPoint, endPoint, firstPoint, lastPoint, ref intersectionPoint);
                    firstRun = false;
                }
                else
                {
                    intersection = SegmentToSegmentIntersection(startPoint, endPoint, prePoint, point, ref intersectionPoint);
                }

                if (intersection)
                {
                    intersectionList.Add(intersectionPoint); // тут пересечение
                    parts.Add(point);                        // с какой частью
                }

                prePoint = point;
            }

            // ищем среди пересеченных ближайшую точку к стартовой 
            if (intersectionList.Count > 0)
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

        private Vector2 GetBeginPartPoint(Vector2 point, Obstacle obstacle)
        {
            Vector2 prev = obstacle.vectors.Last<Vector2>();
            foreach (var vector in obstacle.vectors)
            {
                if (vector.Equals(point))
                {
                    return prev;
                }
                prev = vector;
            }

            return prev;
        }

        public static bool SegmentToSegmentIntersection(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 i)
        {
            float s10_x = p1.x - p0.x;
            float s10_y = p1.y - p0.y;
            float s32_x = p3.x - p2.x;
            float s32_y = p3.y - p2.y;

            var denom = s10_x * s32_y - s32_x * s10_y;
            if (denom == 0)
                return false; // Collinear
            bool denomPositive = denom > 0;

            float s02_x = p0.x - p2.x;
            float s02_y = p0.y - p2.y;
            var s_numer = s10_x * s02_y - s10_y * s02_x;
            if (s_numer < 0 == denomPositive)
                return false; // No collision

            var t_numer = s32_x * s02_y - s32_y * s02_x;
            if (t_numer < 0 == denomPositive)
                return false; // No collision

            if (s_numer >= denom == denomPositive || t_numer >= denom == denomPositive)
                return false; // No collision
            // Collision detected
            var t = t_numer / denom;

            i.x = p0.x + t * s10_x;
            i.y = p0.y + t * s10_y;

            return true;
        }
    }
}