                                                                     
                                                                     
                                                                     
                                             
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder.Shishlov {

    public class Segment {
        public Vector2 start;
        public Vector2 end;
        public Segment() { }

        public Segment(Vector2 start, Vector2 end) {
            this.start = start;
            this.end = end;
        }

        public override string ToString() {
            return start + "->" + end;
        }
    }

    public class Obstacle {
        public Segment[] segments;
        public int index;

        public Obstacle() { }

        public Obstacle(Segment[] segments, int index) {
            this.segments = segments;
            this.index = index;
        }

        public Segment getNextSegmentByClockWise(Segment sourceSegment) {
            int sourceIndex = 0;
            for (int i = 0; i < segments.Length; i++) {
                if (segments[i].start == sourceSegment.start && segments[i].end == sourceSegment.end) {
                    sourceIndex = i;
                    break;
                }
            }

            return sourceIndex == segments.Length - 1 ? segments[0] : segments[sourceIndex + 1];
        }

        public Segment getNextSegmentByCounterClockWise(Segment sourceSegment) {
            int sourceIndex = 0;
            for (int i = 0; i < segments.Length; i++) {
                if (segments[i].start == sourceSegment.start && segments[i].end == sourceSegment.end) {
                    sourceIndex = i;
                    break;
                }
            }

            return sourceIndex == 0 ? segments[segments.Length - 1] : segments[sourceIndex - 1];
        }
    }

    public class PathDistance {
        public float distance;
        public List<Vector2> points = new List<Vector2>();
        public Vector2 obstacleEnd;

        public void addPoint(Vector2 point) {
            points.Add(point);
        }

        public void accDistance(float distance) {
            this.distance += distance;
        }

        public void addPointsToResult(List<Vector2> result) {
            foreach (Vector2 point in points) {
                result.Add(point);
            }
        }

        public override string ToString() {
            return "distance=" + distance + ",points=" + points.Count + ",obstacleEnd=" + obstacleEnd;
        }
    }

    public sealed class Map : IMap {

        private Segment[] segments;
        private Obstacle[] obstacles;
        private List<Segment> movedSegments;

        public void Init(Vector2[][] obstacles) {
            createSegmentsAndObstacles(obstacles);
        }

        public IEnumerable<Vector2> GetPath2(Vector2 start, Vector2 end) {
            var result = new List<Vector2>();
            result.Add(start);
//            result.Add(new Vector2(100f, 100f));
//            result.Add(new Vector2(100f, 90f));
            result.Add(new Vector2(100.0912f, 99.90877f));
            result.Add(new Vector2(100.0166f, 89.98505f));
            result.Add(end);


            return result;
        }

        private List<Vector2> findIntersectPoints2(Vector2 start, Vector2 end, Segment[] segments) {
            var result = new List<Vector2>();
            foreach (Segment segment in segments) {
                Vector2 res = Vector2.zero;
                bool isIntersect = Vector2.SegmentToSegmentIntersection(segment.start, segment.end, start, end, ref res);
                if (isIntersect) {
                    result.Add(res);
                }
            }

            return result;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            var result = new List<Vector2>();
            movedSegments = new List<Segment>();

            // 1. Старт
            result.Add(start);

            Vector2 obstacleEnd = start;
            while (obstacleEnd != Vector2.zero) {
                // 2. Сразу проверяем можем ли построить путь без пересечений
                List<Vector2> intersectPoints = findIntersectPoints2(obstacleEnd, end, segments);
                if (intersectPoints.Count == 0) {
                    result.Add(end);
                    return result;
                }

                // Вычисляем два пути: по часовой и против часовой стрелки
                PathDistance byEnd = calcPathByEnd(obstacleEnd, intersectPoints, start, end);
                PathDistance byStart = calcPathByStart(obstacleEnd, intersectPoints, start, end);

                PathDistance shortestDistance = byEnd.distance < byStart.distance ? byEnd : byStart;

                shortestDistance.addPointsToResult(result);
                obstacleEnd = shortestDistance.obstacleEnd;
            }

            result.Add(end);
            return result;
        }

        public PathDistance calcPathByEnd(Vector2 obstacleEnd, List<Vector2> intersectPoints, Vector2 start, Vector2 end) {
            PathDistance path = new PathDistance();

            // Находим ближайшую точку пересечения и ее сегмент, запоминаем
            Vector2 intersectNearestPoint = findNearestPoint(obstacleEnd, intersectPoints);
            Segment nearestSegment = findNearestSegment(intersectNearestPoint);

            path.addPoint(intersectNearestPoint);
            path.accDistance(Vector2.Distance(start, intersectNearestPoint));

            // Выбираем Конец отрезка и запоминаем 
            path.addPoint(nearestSegment.end);
            path.accDistance(Vector2.Distance(intersectNearestPoint, nearestSegment.end));

            // Проверяем можем ли с вершины обстакла дойти до финиша
            if (findIntersectPoints2(nearestSegment.end, end, segments).Count == 0) {
                path.obstacleEnd = Vector2.zero;
                return path;
            }

            // Если не можем, ищем обстакл на котором располагается ближайший сегмент
            Obstacle obstacle = findObstacleBySegment(nearestSegment);

            // Идем по сегментам обстакла, пока не будем пересекать сами себя и не будем пересекать уже пройденные обстаклы,
            // или не построем путь из вершина до финиша
            while (!(findIntersectPoints2(nearestSegment.end, end, obstacle.segments).Count == 0
                     && findIntersectPoints2(nearestSegment.end, end, movedSegments.ToArray()).Count == 0)) {
                nearestSegment = obstacle.getNextSegmentByClockWise(nearestSegment);

                // Нашли следующий сегмент обстакла, проверяем можем ли дойти до финиша без пересечений
                if (findIntersectPoints2(nearestSegment.end, end, segments).Count == 0) {
                    path.addPoint(nearestSegment.end);
                    path.accDistance(Vector2.Distance(nearestSegment.start, nearestSegment.end));
                    path.obstacleEnd = Vector2.zero;
                    return path;
                }

                // Не можем - запоминаем конец обстакла
                path.addPoint(nearestSegment.end);
                path.accDistance(Vector2.Distance(nearestSegment.start, nearestSegment.end));
            }

            path.obstacleEnd = nearestSegment.end;
            movedSegments.AddRange(obstacle.segments.ToList());
            return path;
        }

        public PathDistance calcPathByStart(Vector2 obstacleEnd, List<Vector2> intersectPoints, Vector2 start, Vector2 end) {
            PathDistance path = new PathDistance();

            // Находим ближайшую точку пересечения и ее сегмент, запоминаем
            Vector2 intersectNearestPoint = findNearestPoint(obstacleEnd, intersectPoints);
            Segment nearestSegment = findNearestSegment(intersectNearestPoint);

            path.addPoint(intersectNearestPoint);
            path.accDistance(Vector2.Distance(start, intersectNearestPoint));

            // Выбираем Начало отрезка и запоминаем 
            path.addPoint(nearestSegment.start);
            path.accDistance(Vector2.Distance(intersectNearestPoint, nearestSegment.start));

            // Проверяем можем ли с вершины обстакла дойти до финиша
            if (findIntersectPoints2(nearestSegment.start, end, segments).Count == 0) {
                path.obstacleEnd = Vector2.zero;
                return path;
            }

            // Если не можем, ищем обстакл на котором располагается ближайший сегмент
            Obstacle obstacle = findObstacleBySegment(nearestSegment);

            // Идем по сегментам обстакла, пока не будем пересекать сами себя и не будем пересекать уже пройденные обстаклы,
            // или не построем путь из вершина до финиша
            while (!(findIntersectPoints2(nearestSegment.start, end, obstacle.segments).Count == 0
                     && findIntersectPoints2(nearestSegment.start, end, movedSegments.ToArray()).Count == 0)) {
                nearestSegment = obstacle.getNextSegmentByCounterClockWise(nearestSegment);

                // Нашли следующий сегмент обстакла, проверяем можем ли дойти до финиша без пересечений
                if (findIntersectPoints2(nearestSegment.start, end, segments).Count == 0) {
                    path.addPoint(nearestSegment.start);
                    path.accDistance(Vector2.Distance(nearestSegment.end, nearestSegment.start));
                    path.obstacleEnd = Vector2.zero;
                    return path;
                }

                // Не можем - запоминаем конец обстакла
                path.addPoint(nearestSegment.start);
                path.accDistance(Vector2.Distance(nearestSegment.end, nearestSegment.start));
            }

            path.obstacleEnd = nearestSegment.start;
            movedSegments.AddRange(obstacle.segments.ToList());
            return path;
        }

        public Vector2 resolveDirection(Segment segment, String direction) {
            return direction.Equals("end") ? segment.end : segment.start;
        }

        /**
         * Обходит обстакл почасовой стрелке.
         * Возвращает конечную точку обхода обстакла, либо говорим что можем дойти до финиша
         */
        public Vector2 moveByObstacle(Segment nearestSegment, Vector2 end, List<Vector2> result) {
            // Если не можем, ищем обстакл на котором располагается ближайший сегмент
            Obstacle obstacle = findObstacleBySegment(nearestSegment);

            // Проверяем можем ли с ближайшего сегмента дойти до финиша
            if (findIntersectPoints2(nearestSegment.end, end, segments).Count == 0) {
                result.Add(nearestSegment.end);
                result.Add(end);
                return Vector2.zero;
            }

            // Идем по сегментам обстакла, пока путь до финиша не будет пересекаться с обстаклом,
            // либо мы не построим до финиша
            while (!(findIntersectPoints2(nearestSegment.end, end, obstacle.segments).Count == 0
                     && findIntersectPoints2(nearestSegment.end, end, movedSegments.ToArray()).Count == 0)) {
                nearestSegment = obstacle.getNextSegmentByClockWise(nearestSegment);

                // Нашли следующий сегмент обстакла, проверяем можем ли дойти до финиша без пересечений
                if (findIntersectPoints2(nearestSegment.end, end, segments).Count == 0) {
                    result.Add(nearestSegment.end);
                    result.Add(end);
                    return Vector2.zero;
                }

                // Не можем - запоминаем конец обстакла
                result.Add(nearestSegment.end);
            }

            movedSegments.AddRange(obstacle.segments.ToList());
            return nearestSegment.end;
        }

        private Obstacle findObstacleBySegment(Segment nearestSegment) {
            Obstacle res = new Obstacle();
            foreach (Obstacle obstacle in obstacles) {
                foreach (Segment segment in obstacle.segments) {
                    if (segment.start == nearestSegment.start && segment.end == nearestSegment.end) {
                        return obstacle;
                    }
                }
            }

            return res;
        }

        private Vector2 findStartPeak(Vector2 start, Vector2 end, Vector2 intersectNearestPoint, Segment nearestSegment) {
            bool intersectToStart = findIntersectPoints2(start, nearestSegment.start, segments).Count != 0;
            bool intersectToEnd = findIntersectPoints2(start, nearestSegment.end, segments).Count != 0;

            if (intersectToStart && intersectToEnd) {
                return intersectNearestPoint;
            }

            if (intersectToStart) {
                return nearestSegment.end;
            }

            if (intersectToEnd) {
                return nearestSegment.start;
            }

            float distanceFromStart = Vector2.Distance(nearestSegment.start, end);
            float distanceFromEnd = Vector2.Distance(nearestSegment.end, end);
            return distanceFromStart > distanceFromEnd ? nearestSegment.end : nearestSegment.start;
        }

        private Segment findNearestSegment(Vector2 intersectNearestPoint) {
            var result = new Segment();
            foreach (Segment segment in segments) {
                float diff = Vector2.Distance(segment.start, intersectNearestPoint)
                             + Vector2.Distance(segment.end, intersectNearestPoint)
                             - Vector2.Distance(segment.start, segment.end);
                if (diff < 0.01) {
                    result = segment;
                    break;
                }
            }

            return result;
        }

        private Vector2 findNearestPoint(Vector2 start, IEnumerable<Vector2> intersectPoints) {
            Vector2 res = new Vector2();
            float minDistance = 0;
            foreach (Vector2 point in intersectPoints) {
                var distance = Vector2.Distance(start, point);
                if (minDistance == 0) {
                    minDistance = distance;
                    res = point;
                }

                if (distance < minDistance) {
                    minDistance = distance;
                    res = point;
                }
            }

            return res;
        }

        private static void drawIntersectPoints(IEnumerable<Vector2> intersectPoints, List<Vector2> result) {
            foreach (Vector2 point in intersectPoints) {
                result.Add(point);
            }
        }

        private void drawSegments(Segment[] segments, List<Vector2> result) {
            for (int i = 0; i < segments.Length; i++) {
                result.Add(segments[i].start);
                result.Add(segments[i].end);
            }
        }

        private void createSegmentsAndObstacles(Vector2[][] sourceObstacles) {
            int segmentsLength = 0;
            for (int i = 0; i < sourceObstacles.Length; i++) {
                segmentsLength += sourceObstacles[i].Length;
            }

            obstacles = new Obstacle[sourceObstacles.Length];
            segments = new Segment[segmentsLength];
            int k = 0;
            for (int i = 0; i < sourceObstacles.Length; i++) {
                Segment[] segmentsForObstacle = new Segment[sourceObstacles[i].Length];
                for (int j = 0; j < sourceObstacles[i].Length; j++) {
                    Vector2 endS = Vector2.zero;
                    Vector2 startS = Vector2.zero;
                    if (j == sourceObstacles[i].Length - 1) {
                        startS = sourceObstacles[i][j];
                        endS = sourceObstacles[i][0];
                    } else {
                        startS = sourceObstacles[i][j];
                        endS = sourceObstacles[i][j + 1];
                    }

                    var segment = new Segment(startS, endS);
                    segments[k] = segment;
                    segmentsForObstacle[j] = segment;
                    k++;
                }

                obstacles[i] = new Obstacle(segmentsForObstacle, i);
            }
        }
    }
}