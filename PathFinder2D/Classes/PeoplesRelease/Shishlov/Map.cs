using System.Collections.Generic;
using System.Linq;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder.Release.Shishlov {

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
        private Dictionary<Segment, int> segmentToIndex = new Dictionary<Segment, int>();

        public Obstacle(Segment[] segments) {
            this.segments = segments;
            initSegmentToIndex();
        }

        public void initSegmentToIndex() {
            for (int i = 0; i < segments.Length; i++) {
                segmentToIndex.Add(segments[i], i);
            }
        }

        public Segment getNextSegmentByClockWise(Segment sourceSegment) {
            int sourceIndex;
            segmentToIndex.TryGetValue(sourceSegment, out sourceIndex);
            return sourceIndex == segments.Length - 1 ? segments[0] : segments[sourceIndex + 1];
        }

        public Segment getNextSegmentByCounterClockWise(Segment sourceSegment) {
            int sourceIndex;
            segmentToIndex.TryGetValue(sourceSegment, out sourceIndex);
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

        public override string ToString() {
            return "distance=" + distance + ",points=" + points.Count + ",obstacleEnd=" + obstacleEnd;
        }
    }

    public sealed class Map : IMap {

        private Segment[] segments;
        private Obstacle[] obstacles;
        private List<Segment> movedSegments;
        private Dictionary<Segment, int> segmentToObstacleIndex = new Dictionary<Segment, int>();

        public void Init(Vector2[][] obstacles) {
            createSegmentsAndObstacles(obstacles);
        }

        private List<Vector2> findIntersectPoints3(Vector2 start, Vector2 end, Segment[] segments) {
            var result = new List<Vector2>();
            foreach (Segment segment in segments) {
                Vector2 res = Vector2.zero;
                if (Vector2.SegmentToSegmentIntersection(segment.start, segment.end, start, end, ref res)) {
                    result.Add(res);
                }
            }

            return result;
        }

        private int findIntersectPoints2(Vector2 start, Vector2 end, Segment[] segments) {
            int count = 0;
            foreach (Segment segment in segments) {
                Vector2 res = Vector2.zero;
                if (Vector2.SegmentToSegmentIntersection(segment.start, segment.end, start, end, ref res)) {
                    count++;
                }
            }

            return count;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            var result = new List<Vector2>();
            movedSegments = new List<Segment>();

            // 1. Старт
            result.Add(start);

            Vector2 obstacleEnd = start;
            while (obstacleEnd != Vector2.zero) {
                // 2. Сразу проверяем можем ли построить путь без пересечений
                List<Vector2> intersectPoints = findIntersectPoints3(obstacleEnd, end, segments);
                if (intersectPoints.Count == 0) {
                    result.Add(end);
                    return result;
                }

                // Вычисляем два пути: по часовой и против часовой стрелки
                PathDistance byEnd = calcPathByEnd(obstacleEnd, intersectPoints, start, end);
                PathDistance byStart = calcPathByStart(obstacleEnd, intersectPoints, start, end);

                PathDistance shortestDistance = byEnd.distance < byStart.distance ? byEnd : byStart;

                result.AddRange(shortestDistance.points);
                obstacleEnd = shortestDistance.obstacleEnd;
            }

            result.Add(end);
            return result;
        }

        public PathDistance calcPathByEnd(Vector2 obstacleEnd, List<Vector2> intersectPoints, Vector2 start, Vector2 end) {
            // Находим ближайшую точку пересечения и ее сегмент, запоминаем
            Vector2 intersectNearestPoint = findNearestPoint(obstacleEnd, intersectPoints);
            Segment nearestSegment = findNearestSegment(intersectNearestPoint);

            // Проверяем можем ли с вершины обстакла дойти до финиша
            if (findIntersectPoints2(nearestSegment.end, end, segments) == 0) {
                PathDistance path1 = new PathDistance();

                path1.addPoint(intersectNearestPoint);
                path1.accDistance(Vector2.Distance(start, intersectNearestPoint));

                // Выбираем Конец отрезка и запоминаем 
                path1.addPoint(nearestSegment.end);
                path1.accDistance(Vector2.Distance(intersectNearestPoint, nearestSegment.end));

                path1.obstacleEnd = Vector2.zero;
                return path1;
            }

            PathDistance path = new PathDistance();

            path.addPoint(intersectNearestPoint);
            path.accDistance(Vector2.Distance(start, intersectNearestPoint));

            // Выбираем Конец отрезка и запоминаем 
            path.addPoint(nearestSegment.end);
            path.accDistance(Vector2.Distance(intersectNearestPoint, nearestSegment.end));

            // Если не можем, ищем обстакл на котором располагается ближайший сегмент
            Obstacle obstacle = findObstacleBySegment(nearestSegment);

            // Идем по сегментам обстакла, пока не будем пересекать сами себя и не будем пересекать уже пройденные обстаклы,
            // или не построем путь из вершина до финиша
            while (!(findIntersectPoints2(nearestSegment.end, end, obstacle.segments) == 0
                     && findIntersectPoints2(nearestSegment.end, end, movedSegments.ToArray()) == 0)) {
                nearestSegment = obstacle.getNextSegmentByClockWise(nearestSegment);

                // Нашли следующий сегмент обстакла, проверяем можем ли дойти до финиша без пересечений
                if (findIntersectPoints2(nearestSegment.end, end, segments) == 0) {
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
            // Находим ближайшую точку пересечения и ее сегмент, запоминаем
            Vector2 intersectNearestPoint = findNearestPoint(obstacleEnd, intersectPoints);
            Segment nearestSegment = findNearestSegment(intersectNearestPoint);

            // Проверяем можем ли с вершины обстакла дойти до финиша
            if (findIntersectPoints2(nearestSegment.start, end, segments) == 0) {
                PathDistance path1 = new PathDistance();

                path1.addPoint(intersectNearestPoint);
                path1.accDistance(Vector2.Distance(start, intersectNearestPoint));

                // Выбираем Начало отрезка и запоминаем 
                path1.addPoint(nearestSegment.start);
                path1.accDistance(Vector2.Distance(intersectNearestPoint, nearestSegment.start));

                path1.obstacleEnd = Vector2.zero;
                return path1;
            }

            PathDistance path = new PathDistance();

            path.addPoint(intersectNearestPoint);
            path.accDistance(Vector2.Distance(start, intersectNearestPoint));

            // Выбираем Начало отрезка и запоминаем 
            path.addPoint(nearestSegment.start);
            path.accDistance(Vector2.Distance(intersectNearestPoint, nearestSegment.start));

            // Если не можем, ищем обстакл на котором располагается ближайший сегмент
            Obstacle obstacle = findObstacleBySegment(nearestSegment);

            // Идем по сегментам обстакла, пока не будем пересекать сами себя и не будем пересекать уже пройденные обстаклы,
            // или не построем путь из вершина до финиша
            while (!(findIntersectPoints2(nearestSegment.start, end, obstacle.segments) == 0
                     && findIntersectPoints2(nearestSegment.start, end, movedSegments.ToArray()) == 0)) {
                nearestSegment = obstacle.getNextSegmentByCounterClockWise(nearestSegment);

                // Нашли следующий сегмент обстакла, проверяем можем ли дойти до финиша без пересечений
                if (findIntersectPoints2(nearestSegment.start, end, segments) == 0) {
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

        private Obstacle findObstacleBySegment(Segment nearestSegment) {
            int obstacleIndex;
            segmentToObstacleIndex.TryGetValue(nearestSegment, out obstacleIndex);
            return obstacles[obstacleIndex];
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
                    segmentToObstacleIndex.Add(segment, i);
                    segmentsForObstacle[j] = segment;
                    k++;
                }

                obstacles[i] = new Obstacle(segmentsForObstacle);
            }
        }
    }
}