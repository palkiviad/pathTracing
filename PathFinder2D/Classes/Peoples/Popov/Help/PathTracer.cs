using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Peoples.Popov.Help {
    public class PathTracer {
        private readonly Contour[] _contours;
        private Vector2 _currentPoint;
        private readonly Vector2 _goal;
        private readonly List<int> _excludedContours;
        private Contour _currentContour;

        public PathTracer(Contour[] contours, Vector2 goal, List<int> excludedContours) {
            _contours = contours;
            _goal = goal;
            _excludedContours = excludedContours;
        }

        public Vector2 Trace(Vector2 currentPoint, IList<Vector2> path, Contour contour) {
            _currentPoint = currentPoint;
            _currentContour = contour;
            var startIndex = GetNearestSegmentIndex(contour);
            if (startIndex < 0) {
                throw new Exception("Can't find segment for current point!");
            }

            int nextIndex = startIndex;
            Segment currentSegment = contour.Segments[nextIndex];
            bool moveForward = currentSegment.EndPointCloser(_goal);
            do {
                _currentPoint = moveForward ? currentSegment.EndPoint : currentSegment.StartPoint;
                TryRemovePreviousPoint(path);
                path.Add(_currentPoint);
                if (Utils.ContourLayInSameHalfPlane(_currentContour, new Segment(_currentPoint, _goal))) {
                    Vector2? nextIntersection = contour.GetNearestIntersection(_currentPoint, _goal);
                    if (!nextIntersection.HasValue) {
                        var intersectedContours = Utils.GetIntersectedContours(_currentPoint, _goal, _contours);
                        if (intersectedContours.All(item => _excludedContours.IndexOf(item.Contour.Id) < 0)) {
                            break;
                        }
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
            return _currentPoint;
        }

        private void TryRemovePreviousPoint(IList<Vector2> path) {
            if (path.Count > 1) {
                var previous = path[path.Count - 2];
                var last = path[path.Count - 1];
                var intersected = Utils.GetIntersectedContours(previous, _currentPoint, _contours);
                if (intersected.Count == 0) {
                    if (!(_currentContour.HasPoint(previous) && _currentContour.HasPoint(last)) ||
                        (Utils.ContourLayInSameHalfPlane(_currentContour, new Segment(_currentPoint, previous)))) {
                        path.Remove(last);
                    }
                }
            }
        }

        private int GetNearestSegmentIndex(Contour contour) {
            int startIndex = -1;
            for (int i = 0; i < contour.Segments.Count; i++) {
                if (contour.Segments[i].ContainsPoint(_currentPoint)) {
                    startIndex = i;
                    break;
                }
            }

            return startIndex;
        }
    }
}