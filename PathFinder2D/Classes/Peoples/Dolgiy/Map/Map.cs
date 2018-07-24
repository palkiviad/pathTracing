using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Dolgii_2018_07_11 {

    public sealed class Map : IMap {

        private Vector2[][] Obstacles;

        public void Init(Vector2[][] obstacles) {
            Obstacles = obstacles;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            var path = new List<Vector2> {start};
            var point = start;
            while (point != end) {
                // ищем пересечения с обстаклом
                var intersections = new List<Vector2>();
                var intersectionSegments = new List<Vector2[]>();
                var intersects = new Dictionary<Vector2, Vector2[]>();
                foreach (var obstacle in Obstacles) {
                    for (var i = 0; i < obstacle.Length; i++) {
                        var p1 = obstacle[i];
                        Vector2 p2;

                        p2 = i == obstacle.Length - 1 ? obstacle[0] : obstacle[i + 1];

                        var result = new Vector2();
                        if (Vector2.SegmentToSegmentIntersection(point, end, p1, p2, ref result)) {
                            intersections.Add(result);
                            intersectionSegments.Add(new[] {p1, p2});
                            intersects[result] = new[] {p1, p2};
                        }
                    }
                }

                intersections.Sort((v1, v2) => Vector2.Distance(point, v1).CompareTo(Vector2.Distance(point, v2)));

                if (intersections.Count == 0) {
                    path.Add(end);
                    break;
                }

                var minIndex = 0;
                var minDistance = float.MaxValue;
                for (var i = 0; i < intersections.Count; i++) {
                    var distance = Vector2.Distance(point, intersections[i]);
                    if (distance < minDistance) {
                        minIndex = i;
                        minDistance = distance;
                    }
                }

                if (intersections.Count != 0) {
                    var intr = intersections[minIndex];
                    path.Add(intr);
                    var nearestSegment = intersects[intr];
                    var fistDistance = Vector2.Distance(end, nearestSegment[0]);
                    var secondDistance = Vector2.Distance(end, nearestSegment[1]);
                    var nextPoint = fistDistance <= secondDistance ? nearestSegment[0] : nearestSegment[1];
                    path.Add(nextPoint);
                    point = nextPoint;
                }
            }

            return path;
        }

    }
}