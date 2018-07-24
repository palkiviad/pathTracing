using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {

    public class Contour {

        private readonly Vector2[] points;
        public readonly BoundingBox bounds;
        private readonly Edge[] edges;
        private readonly Edge[] convexEdges;
        public readonly bool convex;

        // Некоторый показатель сложности контура, нужен нам для более эффективного разбиения деревом
        public int Complexity() {
            return convex ? edges.Length : convexEdges.Length;
        }

        public Contour(Vector2[] points) {
            this.points = points;

            // сначала - последнее ребро
            edges = new Edge[points.Length];

            Edge previous = new Edge(points[points.Length - 1], points[0]);
            edges[points.Length - 1] = previous;

            int i = 0;
            int count;
            for (count = points.Length - 1; i < count; i++) {
                Edge current = new Edge(points[i], points[i + 1]);
                current.previous = previous;
                previous.next = current;
                previous = current;

                edges[i] = current;
            }

            // А теперь последняя связка
            edges[points.Length - 1].previous = previous;
            previous.next = edges[points.Length - 1];

            // А это просто валидация, которую в релизе можно вырубить
            for (i = 0, count = edges.Length; i < count; i++) {
                if (edges[i].next == null || edges[i].previous == null) {
                    throw new Exception("Should not be");
                }
            }

            convex = Convex();

            // построим выпуклую оболочку, она должна нам упростить кучку математики
            if (!convex) {
                HashSet<int> usedIndexes = new HashSet<int>();

                // Пока что это индекс точки с наименьшим x
                Vector2 leftmostPoint = points[0];
                // Сначала выберем индекс опорной точки
                for (i = 1; i < points.Length; i++)
                    if (points[i].x < leftmostPoint.x)
                        leftmostPoint = points[i];

                // А теперь начнём строить рёбра, не забывая что нам суперважно сохранить направление обхода!
                Vector2 prevPoint = leftmostPoint;
                List<Edge> convexEdgesList = new List<Edge>();
                do {
                    bool found = false;
                    for (i = 0, count = points.Length; i < count; i++) {
                        if (points[i] == prevPoint || usedIndexes.Contains(i))
                            continue;

                        if (BehindLine(points, prevPoint, Vector2.Perpendicular(points[i] - prevPoint))) {
                            found = true;

                            usedIndexes.Add(i);
                            convexEdgesList.Add(new Edge(prevPoint, points[i]));

                            prevPoint = points[i];

                            break;
                        }
                    }

                    if (!found)
                        throw new Exception("Failed to create convex hull");
                } while (prevPoint != leftmostPoint);

                convexEdges = convexEdgesList.ToArray();


                Edge pr = convexEdges[convexEdges.Length - 1];
                // Ну и восстановим ссылки
                for (i = 0; i < convexEdges.Length; i++) {
                    Edge cr = convexEdges[i];
                    cr.previous = pr;
                    pr.next = cr;
                    pr = cr;
                }

                if (!CV(convexEdges)) {
                    for (i = 0, count = convexEdges.Length; i < count; i++)
                        convexEdges[i].Flip();

                    if (!CV(convexEdges)) {
                        throw new Exception("Still not cv!");
                    }
                }
            }

            bounds = convex ? new BoundingBox(points) : new BoundingBox(convexEdges);
        }

        private static bool BehindLine(Vector2[] points, Vector2 point, Vector2 normal) {
            // все 4 точки бокса находятся по normal стороне от линии

            for (int i = 0, count = points.Length; i < count; i++)
                if (Vector2.Dot(points[i] - point, normal) < 0)
                    return false;

            return true;
        }

        private static bool CV(Edge[] edges) {
            float sum = 0;
            for (int i = 0, count = edges.Length; i < count; i++)
                sum += edges[i].start.x * edges[i].end.y - edges[i].end.x * edges[i].start.y;

            return sum < 0;
        }

        private bool Convex() {
            // Фантастическая оптимизация
            if (edges.Length < 4)
                return true;

            for (int i = 0, count = edges.Length; i < count; i++) {
                Edge edge = edges[i];
                if (edge.backVector.x * edge.next.vector.y - edge.backVector.y * edge.next.vector.x < 0)
                    return false;
            }

            return true;
        }

        // http://alienryderflex.com/polygon/
        public bool Contains(Vector2 point) {
            bool oddNodes = false;

            for (int i = 0, j = points.Length - 1, count = points.Length; i < count; i++) {
                if ((points[i].y < point.y && points[j].y >= point.y
                     || points[j].y < point.y && points[i].y >= point.y)
                    && (points[i].x <= point.x || points[j].x <= point.x)) {
                    if (points[i].x + (point.y - points[i].y) / (points[j].y - points[i].y) * (points[j].x - points[i].x) < point.x) {
                        oddNodes = !oddNodes;
                    }
                }

                j = i;
            }

            return oddNodes;
        }

        public Intersection GetIntersection(Vector2 start, Vector2 end, bool visible = true) {
            if (!bounds.SegmentIntersectRectangle(start, end))
                return null;

            Edge probableEdge = null;
            Vector2 probablePoint = Vector2.zero;
            Vector2 point = Vector2.zero;
            float distance = int.MaxValue;

            Vector2 dir = start - end;

            Edge[] edges;
            // Если контур невыпуклый и начальная и конечная точки находятся снаружи, мы можем пользоваться выпуклой оболочкой! 
            if (!convex && !bounds.Contains(start) && !bounds.Contains(end)) {
                edges = convexEdges;
            } else {
                edges = this.edges;
            }

            // нас интересуют лишь рёбра, открытые к вектору!
            for (int i = 0, count = edges.Length; i < count; i++) {
                Edge edge = edges[i];

                if (visible && !edge.VisibleTo(dir))
                    continue;

                if (!Vector2.SegmentToSegmentIntersection(start, end, edge.start, edge.end, ref point))
                    continue;

                float d = Vector2.Distance(start, point);

                // Если контур выпуклый, на нём только 1 пересечение с открытой гранью
                if (convex)
                    return new Intersection(d, this, edge, point);

                if (probableEdge == null || d < distance) {
                    probableEdge = edge;
                    probablePoint = point;
                    distance = d;
                }
            }

            return probableEdge == null ? null : new Intersection(distance, this, probableEdge, probablePoint);
        }
    }
}