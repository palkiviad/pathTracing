using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Arkhipov
{
    // решение задачи в минимальном варианте на сегодня ожидается методом трейсинга.
    // while(Смотрим в сторону цели, и если есть препятствие)
    // обойдём по контуру
    public class DumbRayPathfinder : IMap
    {
        private List<Obstacle> _obstacles;
        private Vector2 _endPoint;
        private LinkedList<Vector2> _result = new LinkedList<Vector2>();

        public void Init(Vector2[][] obstacles)
        {
            _obstacles = new List<Obstacle>();
            foreach (var obs in obstacles)
            {
                _obstacles.Add(new Obstacle(obs));
            }
        }
        
        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            _result = new LinkedList<Vector2>();
            _result.AddFirst(start);
            _endPoint = end;
            
            if (_endPoint.AlmostEquals(start))
            {
                _result.AddLast(_endPoint);
                return _result;
            }
            
            Vector2 p = start;
            bool endReached;
            do
            {
                AppendPath(p, _result);
                p = _result.Last.Value;
                endReached = p.AlmostEquals(_endPoint);
            } while (!endReached);
            return _result;
        }

        private void AppendPath(Vector2 from, LinkedList<Vector2> result)
        {
            Segment ray = new Segment(from, _endPoint);
            List<KeyValuePair<SegmentIntersection, Obstacle>> obstacleIntersections = new List<KeyValuePair<SegmentIntersection, Obstacle>>();
            foreach (var obstacle in _obstacles)
            {
                SegmentIntersection i;
                if (obstacle.GetClosestIntersection(ray, out i))
                {
                    obstacleIntersections.Add(new KeyValuePair<SegmentIntersection, Obstacle>(i, obstacle));
                }
            }
            if (obstacleIntersections.Count == 0)
            {
                result.AddLast(_endPoint);
                return;
            }
            var nearestObstacleIntersection = obstacleIntersections
                .Select(kvp => new {Dist = (from - kvp.Key.Point).sqrMagnitude, Obstacle = kvp.Value, Intersection = kvp.Key})
                .OrderBy(o => o.Dist)
                .First();
            result.AddLast(nearestObstacleIntersection.Intersection.Point);
            var pathAround = nearestObstacleIntersection.Obstacle.GetPathAroundObstacle(nearestObstacleIntersection.Intersection, _endPoint);
            foreach (var point in pathAround)
            {
                result.AddLast(point);
            }
        }
    }
}