using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Arkhipov
{
    public class RayPathfinder : IMap
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
            bool isLast = false;
            do
            {
                var pathSection = new LinkedList<Vector2>(); 
                isLast = CalculatePathSegment(p, _endPoint, ref pathSection);
                foreach (var sectionSegment in pathSection)
                {
                    _result.AddLast(sectionSegment);
                }
                p = _result.Last.Value;
            } while (!isLast);
            return _result;
        }

        private bool CalculatePathSegment(Vector2 from, Vector2 to, ref LinkedList<Vector2> pathSection)
        {
            Segment ray = new Segment(from, to);
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
                pathSection.AddLast(to);
                return true;
            }
            var nearestObstacleIntersection = obstacleIntersections
                .Select(kvp => new {Dist = (from - kvp.Key.Point).sqrMagnitude, Obstacle = kvp.Value, Intersection = kvp.Key})
                .OrderBy(o => o.Dist)
                .First();
            
            var pathAround = nearestObstacleIntersection.Obstacle.GetPathAroundObstacle(nearestObstacleIntersection.Intersection, to);
            var maybeOptimizedSection = new LinkedList<Vector2>();
            bool finalized = CalculatePathSegment(from, pathAround.First(), ref maybeOptimizedSection);
            if (finalized)
            {
                foreach (var point in maybeOptimizedSection)
                {
                    pathSection.AddLast(point);
                }
            } 
            else
            {
                pathSection.AddLast(nearestObstacleIntersection.Intersection.Point);
            }
            foreach (var point in pathAround)
            {
                pathSection.AddLast(point);
            }
            return false;
        }
    }
}