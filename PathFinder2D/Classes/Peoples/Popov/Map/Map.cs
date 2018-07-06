using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Popov {
    public class Map : IMap {
        private Contour[] _contours;
        private Vector2 _currentPoint;
        private Vector2 _goal;
        private List<int> _excludedContours;
        

        public void Init(Vector2[][] obstacles) {
            _contours = new Contour[obstacles.Length];
            Contour.ResetId();
            for (int i = 0; i < obstacles.Length; i++) {
                _contours[i] = new Contour(obstacles[i]);
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            _excludedContours = new List<int>();
            var result = new List<Vector2>();
            _currentPoint = start;
            _goal = end;
            do {
                result.Add(_currentPoint);
                List<IntersectedContour> intersectedContours = GetIntersectedContours(_currentPoint);
                if (intersectedContours.Count == 0) {
                    break;
                }

                intersectedContours.Sort(SortByDistance);
                var contour = intersectedContours.FirstOrDefault(item => _excludedContours.IndexOf(item.Contour.Id) < 0);
                if (contour == null) {
                    break;
                    //throw new Exception("Can't find path because all contoures are excluded!");
                }
                _currentPoint = contour.IntersectionPoint;
                result.Add(_currentPoint);
                TracePath(result, contour.Contour);
            } while (true);

            result.Add(_goal);
            return result;
        }

        private void TracePath(IList<Vector2> path, Contour contour) {
            var startIndex = InitStartSegment(contour);

            if (startIndex < 0) {
                throw new Exception("Can't find segment for current point!");
            }

            int nextIndex = startIndex;
            Segment currentSegment = contour.Segments[nextIndex];
            bool moveForward = currentSegment.EndPointCloser(_goal);
            do {
                _currentPoint = moveForward ? currentSegment.EndPoint : currentSegment.StartPoint;
                path.Add(_currentPoint);
                Vector2? nextIntersection = contour.GetNearestIntersection(_currentPoint, _goal);
                if (!nextIntersection.HasValue) {
                    var intersectedContours = GetIntersectedContours(_currentPoint);
                    if (intersectedContours.All(item => _excludedContours.IndexOf(item.Contour.Id) < 0)) {
                         break;
                    }
                }

                nextIndex = moveForward ? nextIndex + 1 : nextIndex - 1;
                if (moveForward) {
                    nextIndex = nextIndex > contour.Segments.Count - 1 ? 0 : nextIndex;
                } else {
                    nextIndex = nextIndex < 0 ? contour.Segments.Count - 1 : nextIndex;
                }
                currentSegment = contour.Segments[nextIndex];
            } while (nextIndex != startIndex);
            _excludedContours.Add(contour.Id);
        }

        private int InitStartSegment(Contour contour) {
            int startIndex = -1;
            for (int i = 0; i < contour.Segments.Count; i++) {
                if (contour.Segments[i].ContainsPoint(_currentPoint)) {
                    startIndex = i;
                    break;
                }
            }

            return startIndex;
        }

        private int SortByDistance(IntersectedContour x, IntersectedContour y) {
            float distance1 = Vector2.Distance(_currentPoint, x.IntersectionPoint);
            float distance2 = Vector2.Distance(_currentPoint, y.IntersectionPoint);
            return (int) (distance1 - distance2);
        }


        private List<IntersectedContour> GetIntersectedContours(Vector2 start) {
            var result = new List<IntersectedContour>();
            foreach (var contour in _contours) {
                Vector2? point = contour.GetNearestIntersection(start, _goal);
                if (point.HasValue) {
                    result.Add(new IntersectedContour(contour, point.Value));
                }
            }

            return result;
        }


    }

    public class IntersectedContour {
        private Contour _contour;

        public Contour Contour => _contour;

        public Vector2 IntersectionPoint => _intersectionPoint;

        private Vector2 _intersectionPoint;


        public IntersectedContour(Contour contour, Vector2 intersectionPoint) {
            _contour = contour;
            _intersectionPoint = intersectionPoint;
        }
    }

}