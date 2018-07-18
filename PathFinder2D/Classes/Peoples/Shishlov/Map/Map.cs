using System;
using System.Collections.Generic;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder.Shishlov {

    public sealed class Map : IMap {

        Vector2[][] obstacles;

        public void Init(Vector2[][] obstacles) {
            this.obstacles = obstacles;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            var result = new List<Vector2>();
            result.Add(start);

            //printObstacle();

            Vector2 nearestPeak1 = step1(start, end, result, false);
            Vector2 nearestPeak2 = step1(nearestPeak1, end, result, false);
            Vector2 nearestPeak3 = step1(nearestPeak2, end, result, true);
            Vector2 nearestPeak4 = step1(nearestPeak3, end, result, true);
            Vector2 nearestPeak5 = step1(nearestPeak4, end, result, true);
//            Vector2 nearestPeak6 = step1(nearestPeak5, end, result, true);
//            Vector2 nearestPeak7 = step1(nearestPeak6, end, result, true);
//            Vector2 nearestPeak8 = step1(nearestPeak7, end, result, true);

            return result;
        }

        private Vector2 step1(Vector2 start, Vector2 end, List<Vector2> result, bool v) {
            var intersectPoints = findIntersectPoints(start, end);

            Vector2 nearestPoint = findNearestPoint(start, intersectPoints);
            result.Add(nearestPoint);

            Vector2 nearestPeak = findNearestPeak(nearestPoint, v);
            result.Add(nearestPeak);
            return nearestPeak;
        }

        private Vector2 findNearestPeak(Vector2 nearestPoint, bool v) {
            Vector2 res = new Vector2();

            for (int i = 0; i < obstacles.Length; i++) {
                for (int j = 0; j < obstacles[i].Length; j++) {
                    Vector2 v1;
                    Vector2 v2;

                    if (j == obstacles[i].Length - 1) {
                        v1 = new Vector2(obstacles[i][j].x, obstacles[i][j].y);
                        v2 = new Vector2(obstacles[i][0].x, obstacles[i][0].y);
                    } else {
                        v1 = new Vector2(obstacles[i][j].x, obstacles[i][j].y);
                        v2 = new Vector2(obstacles[i][j + 1].x, obstacles[i][j + 1].y);
                    }

                    Vector2 part1 = new Vector2();
                    Vector2 part2 = new Vector2();
                    float diff = Vector2.Distance(v1, nearestPoint) + Vector2.Distance(v2, nearestPoint) - Vector2.Distance(v1, v2);

                    Vector2 peak = v ? v1 : v2;

                    if (diff < 0.1) {
                        res = peak;
                        break;
                    }
                }
            }


            return res;
        }

        private Vector2 findNearestPoint(Vector2 startPath, IEnumerable<Vector2> intersectPoints) {
            Vector2 res = new Vector2();
            float minDistance = 0;
            foreach (Vector2 point in intersectPoints) {
                var distance = Vector2.Distance(startPath, point);
                if (minDistance == 0) {
                    minDistance = distance;
                    res = point;
                }

                if (distance < minDistance) {
                    res = point;
                }
            }

           // Console.WriteLine("nearestPoint = " + res);
            return res;
        }

        private IEnumerable<Vector2> findIntersectPoints(Vector2 startPath, Vector2 endPath) {
            var result = new List<Vector2>();
            for (int i = 0; i < obstacles.Length; i++) {
                for (int j = 0; j < obstacles[i].Length; j++) {
                    Vector2 v1;
                    Vector2 v2;

                    if (j == obstacles[i].Length - 1) {
                        v1 = new Vector2(obstacles[i][j].x, obstacles[i][j].y);
                        v2 = new Vector2(obstacles[i][0].x, obstacles[i][0].y);
                    } else {
                        v1 = new Vector2(obstacles[i][j].x, obstacles[i][j].y);
                        v2 = new Vector2(obstacles[i][j + 1].x, obstacles[i][j + 1].y);
                    }

                    Vector2 v3 = new Vector2(startPath.x, startPath.y);
                    Vector2 v4 = new Vector2(endPath.x, endPath.y);
                    Vector2 res = new Vector2();

                  //  Console.WriteLine("obstacle segment = " + v1 + " - " + v2);
                 //   Console.WriteLine("path segment = " + v3 + " - " + v4);

                    var segmentToSegmentIntersection = Vector2.SegmentToSegmentIntersection(v1, v2, v3, v4, ref res);
                 //   Console.WriteLine("intersect res = " + segmentToSegmentIntersection);
                  //  Console.WriteLine("intersect point = " + v4);
                  //  Console.WriteLine();
                    if (segmentToSegmentIntersection) {
                        result.Add(res);
                    }
                }
            }

            return result;
        }

        public void printObstacle() {
            for (int i = 0; i < obstacles.Length; i++) {
                Console.WriteLine("obstacle" + i);
                for (int j = 0; j < obstacles[i].Length; j++) {
                    Console.WriteLine("point = " + obstacles[i][j].x + " - " + obstacles[i][j].y);
                }

                Console.WriteLine();
            }
        }
    }
}