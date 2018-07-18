using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using PathFinder.Editor;
using PathFinder.Mathematics;

// Тут все плохо, и мне стыдно:)
namespace PathFinder.Leyko {

    public sealed class Map : IMap {

        private Vector2[][] _obstacles;
        private InternalBox[] _obstaclesAABB;

        public void Init(Vector2[][] obstacles) {
            _obstacles = obstacles;
            _obstaclesAABB = new InternalBox[_obstacles.Length];
            for (int i = 0; i < obstacles.Length; i++) {
                _obstaclesAABB[i] = new InternalBox(_obstacles[i]);
            }
        }

        private int GetClosestPoint(Vector2[] obstacle, Vector2 testPoint) {
            int result = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < obstacle.Length; i++) {
                var candidate = (obstacle[i] - testPoint).sqrMagnitude;
                if (candidate < distance) {
                    distance = candidate;
                    result = i;
                }
            }
            return result;
        }

        private List<Vector2> GetContour(Vector2[] obstacle, Vector2 startPoint, Vector2 endPoint) {
            var obstacleStart = GetClosestPoint(obstacle, startPoint);
            var obstacleFinish = GetClosestPoint(obstacle, endPoint);

            if (obstacleFinish < obstacleStart) {
                var tmp = obstacleFinish;
                obstacleFinish = obstacleStart;
                obstacleStart = tmp;
            }

            int cnt1 = obstacleFinish - obstacleStart;
            int cnt2 = obstacleStart + (obstacle.Length - obstacleFinish);

            List<Vector2> result = new List<Vector2>();
            if (cnt1 <= cnt2) {
                for (int i = obstacleStart; i <= obstacleFinish; i++) {
                    result.Add(obstacle[i]);
                }
                if ((result[0] - startPoint).sqrMagnitude > (result[result.Count - 1] - startPoint).sqrMagnitude) {
                    result.Reverse();
                }
                //result = result.OrderBy(a => (a - startPoint).sqrMagnitude).ToList();
                return result;
            }

            for (int i =  obstacleFinish; i < obstacle.Length ; i++) {
                result.Add(obstacle[i]);
            }
            for (int i = 0; i <= obstacleStart; i++) {
                result.Add(obstacle[i]);
            }
            if ((result[0] - startPoint).sqrMagnitude > (result[result.Count - 1] - startPoint).sqrMagnitude) {
                result.Reverse();
            }
            return result;
        }

        private int GetClosestObstacle(List<ObstacleToIndex> obstacles, Vector2 startPoint, Vector2 endPoint) {
            int result = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < obstacles.Count; i++) {
                var candidate1 = (obstacles[i].obstacle.Center - startPoint).sqrMagnitude;
                var candidate2 = (obstacles[i].obstacle.Center - endPoint).sqrMagnitude;
                var c = candidate1 + candidate2;
                if (c < distance) {
                    distance = c;
                    result = i;
                }
            }
            return result;
        }

        private Vector2? Intersect(InternalBox aabb, Vector2 start, Vector2 end) {
            Vector2 tmp = new Vector2();
            if (aabb.Contains(start))
                return start;
            if (aabb.Contains(end))
                return end;

            bool intersect = Vector2.SegmentToSegmentIntersection(start, end, aabb.LeftBottom, new Vector2(aabb.LeftBottom.x, aabb.RightTop.y), ref tmp) ||
                Vector2.SegmentToSegmentIntersection(start, end, aabb.RightTop, new Vector2(aabb.LeftBottom.x, aabb.RightTop.y), ref tmp) ||
                Vector2.SegmentToSegmentIntersection(start, end, aabb.RightTop, new Vector2(aabb.RightTop.x, aabb.LeftBottom.y), ref tmp) ||
                Vector2.SegmentToSegmentIntersection(start, end, aabb.LeftBottom, new Vector2(aabb.RightTop.x, aabb.LeftBottom.y), ref tmp);
            return intersect ? tmp : (Vector2?) null;
        }

        public class ObstacleToIndex {
            public InternalBox obstacle;
            public int index;

            public ObstacleToIndex(InternalBox obstacle, int index) {
                this.obstacle = obstacle;
                this.index = index;
            }
        }

        private IEnumerable<ObstacleToIndex> GetObstaclesPath(Vector2 start, Vector2 end) {
            Vector2 startTmp = start;
            Vector2 endTmp = end;
            List<ObstacleToIndex> obstacles = new List<ObstacleToIndex>();
            for (int index = 0; index < _obstaclesAABB.Length; index++) {
                obstacles.Add(new ObstacleToIndex(_obstaclesAABB[index], index));
            }
            List<ObstacleToIndex> result = new List<ObstacleToIndex>();
            while (obstacles.Count > 0) {
                int index = GetClosestObstacle(obstacles, start, end);
                var intersect = Intersect(obstacles[index].obstacle, start, end);
                if (intersect == null) {
                    if (Intersect(obstacles[index].obstacle, startTmp, endTmp) == null) {
                        obstacles.RemoveAt(index);
                        continue;
                    }
                    
                }
                if (intersect.HasValue) {
                    var testPoint = intersect.Value;
                    if ((start - testPoint).sqrMagnitude < (end - testPoint).sqrMagnitude) {
                        start = testPoint;
                    } else {
                        end = testPoint;
                    }
                }

                result.Add(obstacles[index]);
                obstacles.RemoveAt(index);
                
            }
            return result.OrderBy(o => (o.obstacle.Center - startTmp).sqrMagnitude);
        }

        private List<Vector2> Calculate(Vector2 start, Vector2 end) {
            var result = new List<Vector2>();
            result.Add(start);
            var path = GetObstaclesPath(start, end).ToList();
            for (int i = 0; i < path.Count; i++)
            {
                result.AddRange(GetContour(_obstacles[path[i].index], result[result.Count - 1], end));
            }
            return result;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            
           var result = new List<Vector2>();
            result.AddRange(Calculate(start, end));
            result.Add(end);
           return result;
        }
    }
}
