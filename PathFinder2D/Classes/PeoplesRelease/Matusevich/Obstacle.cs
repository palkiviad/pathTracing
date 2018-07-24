using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Matusevich {
    public class Obstacle {
        private readonly Vector2[] _vertices;
        private readonly Box _box;
        private readonly HashSet<Vector2> _throwedInThisPathSearch = new HashSet<Vector2>();

        public Box Box {
            get { return _box; }
        }

        public Obstacle(Vector2[] vertices) {
            _vertices = vertices;
            Vector2 min = _vertices[0];
            Vector2 max = _vertices[0];
            for (var i = 1; i < vertices.Length; i++) {
                var vertice = vertices[i];
                if (vertice.x < min.x) {
                    min.x = vertice.x;
                }
                if (vertice.y < min.y) {
                    min.y = vertice.y;
                }
                if (vertice.x > max.x) {
                    max.x = vertice.x;
                }
                if (vertice.y > max.y) {
                    max.y = vertice.y;
                }
            }
            _box = new Box(min, max);
        }

        public int VerticesCount {
            get { return _vertices.Length; }
        }

        private static double AngleBetween(Vector2 vector1, Vector2 vector2) {
            double sin = vector1.x * vector2.y - vector2.x * vector1.y;
            double cos = vector1.x * vector2.x + vector1.y * vector2.y;

            return Math.Atan2(sin, cos);
        }

        public bool Inersects(Segment segment) {
            //коробка
            if (_box.CannotIntersectWithExcluding(segment)) {
                return false;
            }

            //весь обставкл вне линии (TODO: удалить, перенести внутрь сегмента)
            if (OutOfLine(segment)) {
                return false;
            }

#if true

            //узнаем, являются ли начало и конец сегмента вершинами обстакла
            int startVerticeIndex = -1;
            int endVerticeIndex = -1;
            for (var i = 0; i < _vertices.Length; i++) {
                if (_vertices[i] == segment.Start) {
                    startVerticeIndex = i;
                }
                if (_vertices[i] == segment.End) {
                    endVerticeIndex = i;
                }
            }

            //если мы две соседние вершины - можно попытаться выйти раньше
            if (startVerticeIndex != -1 && endVerticeIndex != -1) {
                int delta = startVerticeIndex - endVerticeIndex;
                delta = delta < 0 ? -delta : delta;
                if (delta >= _vertices.Length) {
                    delta -= _vertices.Length;
                }
                if (delta == 1) {
                    return false;
                }
            }

            var segmentTransposed = segment.TransposeToZero();
            //var segmentPerpendicular = Vector2.Perpendicular(segmentTransposed);

            //по каждой из вершин, совпадающих с началом или концом сегмента:
            //идём от первой к старту, дальше если сегмент больше по правую руку, чем следующий поворот - проткнули
            if (startVerticeIndex != -1) {
                var first = _vertices[startVerticeIndex] - _vertices[startVerticeIndex <= 0 ? _vertices.Length - 1 : startVerticeIndex - 1];
                var second = _vertices[startVerticeIndex >= _vertices.Length - 1 ? 0 : startVerticeIndex + 1] - _vertices[startVerticeIndex];
                if (AngleBetween(first, segmentTransposed) < AngleBetween(first, second)) {
                    return true;
                }
            }

            if (endVerticeIndex != -1) {
                var first = _vertices[endVerticeIndex] - _vertices[endVerticeIndex <= 0 ? _vertices.Length - 1 : endVerticeIndex - 1];
                var second = _vertices[endVerticeIndex >= _vertices.Length - 1 ? 0 : endVerticeIndex + 1] - _vertices[endVerticeIndex];
                if (AngleBetween(first, segmentTransposed) < AngleBetween(first, second)) {
                    return true;
                }
            }


            //гоним все грани
            //если пересекаем хотя бы одну - пересеклись с обстаклом.
            //если сегмент пересекает вершину грани (смотрим только правую) и обе соседние вершины 
            //по разные стороны от сегмента - пересеклись с обстаклом

            //TODO: вот тут не дописано как раз про пересечение с вершиной.

            for (var i = 0; i < _vertices.Length; i++) {
                var secondVertice = i == _vertices.Length - 1 ? 0 : i + 1;
                Vector2 intersectionPoint = Vector2.zero;
                if (Vector2.SegmentToSegmentIntersection(segment.Start, segment.End, _vertices[i], _vertices[secondVertice], ref intersectionPoint)) {
                    return true;
                }
            }

            //иначе - не пересеклись
            return false;

#else //intersectionDetails.SquaredDistanceFromStart = 0;

            int firstVerticeIndex = -1;
            int secondVerticeIndex = -1;
            for (var i = 0; i < _vertices.Length; i++)
            {
                if (_vertices[i].Equals(segment.Start))
                {
                    firstVerticeIndex = i;
                }
                if (_vertices[i].Equals(segment.End))
                {
                    secondVerticeIndex = i;
                }
            }
            if (firstVerticeIndex != -1 && secondVerticeIndex != -1)
            {
                int delta = Math.Abs(firstVerticeIndex - secondVerticeIndex);
                if (delta >= _vertices.Length)
                {
                    delta -= _vertices.Length;
                }
                if (delta == 1)
                {
                    return false;
                }
            }

            for (var i = 0; i < _vertices.Length; i++)
            {
                var secondVertice = i == _vertices.Length - 1 ? 0 : i + 1;
                Vector2 intersectionPoint = Vector2.zero;
                if (Vector2.SegmentToSegmentIntersection(segment.Start, segment.End, _vertices[i], _vertices[secondVertice], ref intersectionPoint))
                {
                    return true;
                }
            }

            if (_vertices.Any(vector2 => vector2.Equals(segment.Start)) && _vertices.Any(vector2 => vector2.Equals(segment.End)))
            {
                //TODO: всё это говнохак, направленный на то, чтобы определить, что мы проходим внутри обстакла, когда наши точки находятся на вершинах обстакла
                //это должно быть удалено
                for (var i = 0; i < _vertices.Length; i++)
                {
                    for (var j = i + 1; j < _vertices.Length; j++)
                    {
                        var innerSegment = Segment.Create(_vertices[i], _vertices[j]);
                        innerSegment = Segment.Extend(innerSegment);
                        Vector2 temp = Vector2.down;
                        if (Vector2.SegmentToSegmentIntersection(segment.Start, segment.End, innerSegment.Start, innerSegment.End, ref temp))
                        {
                            return true;
                        }
                    }
                }

                var shrinkedSegment = Segment.Shrink(segment);
                if (Contains(shrinkedSegment.Start) || Contains(shrinkedSegment.End))
                {
                    return true;
                }
            }

            return false;
#endif
        }

        internal void AddThrowedInThisPathSearch(Vector2 point) {
            _throwedInThisPathSearch.Add(point);
        }

        internal bool WasThrowedInThisPathSearch(Vector2 point) {
            return _throwedInThisPathSearch.Contains(point);
        }

        internal void Prepare() {
            _throwedInThisPathSearch.Clear();
        }

        internal Vector2 GetCenter() {
            return _box.GetCenter();
        }

        private bool OutOfLine(Segment segment) {
            bool direction = ToRightSide(segment, _vertices[0]);
            for (var i = 1; i < _vertices.Length; i++) {
                if (direction != ToRightSide(segment, _vertices[i])) {
                    return false;
                }
            }
            return true;
        }

        private bool ToRightSide(Segment segment, Vector2 point) {
            return ((segment.End.x - segment.Start.x) * (point.y - segment.Start.y) - (segment.End.y - segment.Start.y) * (point.x - segment.Start.x)) >= 0;
        }

        public Vector2 GetVerticeAt(int i) {
            return _vertices[i];
        }
    }
}