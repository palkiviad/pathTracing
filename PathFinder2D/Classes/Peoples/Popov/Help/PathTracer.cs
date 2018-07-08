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

        public Vector2 Trace(Vector2 currentPoint, List<Vector2> path, Contour contour) {
            _currentContour = contour;
            var startIndex = GetNearestSegmentIndex(_currentContour, currentPoint);
            if (startIndex < 0) {
                throw new Exception("Can't find segment for current point!");
            }

            List<Vector2> forwardPath = GetTracedSubPath(startIndex, true, path);
            List<Vector2> backPath = GetTracedSubPath(startIndex, false, path);
            _excludedContours.Add(_currentContour.Id);
            float forwardDistance = Utils.CalculatePathDistance(forwardPath);
            float backDistance = Utils.CalculatePathDistance(backPath);
            bool forwardValuableEffective = forwardDistance / backDistance < 0.99f;
            List<Vector2> shortestPath = forwardValuableEffective ? forwardPath : backPath;
            path.RemoveAt(path.Count -1);
            path.AddRange(shortestPath.GetRange(1, shortestPath.Count - 1));
            return path.Last();
        }

        private List<Vector2> GetTracedSubPath(int startIndex, bool moveForward, List<Vector2> path) {
            if (path.Count < 2) {
                throw  new Exception("path has to be longer!");
            }

            var subPath = path.GetRange(path.Count - 2, 2);
            int nextIndex = startIndex;
            Segment currentSegment = _currentContour.Segments[nextIndex];
            do {
                
                Vector2 nextPoint = moveForward ? currentSegment.EndPoint : currentSegment.StartPoint;
                TryRemovePreviousPoint(subPath, nextPoint);
                subPath.Add(nextPoint);
                Vector2? nextIntersection = _currentContour.GetNearestIntersection(nextPoint, _goal);
                if (!nextIntersection.HasValue && !Utils.ContourVerticesLayOnSegment(new Segment(nextPoint, _goal),_currentContour)) {
                    var intersectedContours = Utils.GetIntersectedContours(nextPoint, _goal, _contours);
                    if (intersectedContours.All(item => _excludedContours.IndexOf(item.Contour.Id) < 0)) {
                        break;
                    }
                }
                nextIndex = moveForward ? nextIndex + 1 : nextIndex - 1;
                if (moveForward) {
                    nextIndex = nextIndex > _currentContour.Segments.Count - 1 ? 0 : nextIndex;
                } else {
                    nextIndex = nextIndex < 0 ? _currentContour.Segments.Count - 1 : nextIndex;
                }

                currentSegment = _currentContour.Segments[nextIndex];
            } while (nextIndex != startIndex);
            return subPath;
        }

        private void TryRemovePreviousPoint(IList<Vector2> path, Vector2 currentPoint) {
            if (path.Count > 1) {
                var previous = path[path.Count - 2];
                var last = path[path.Count - 1];
                var intersected = Utils.GetIntersectedContours(previous, currentPoint, _contours);
                if (intersected.Count == 0) {
                    if (!(_currentContour.HasPoint(previous) && _currentContour.HasPoint(last)) ||
                        Utils.ContourLayInSameHalfPlane(_currentContour, new Segment(currentPoint, previous))) {
                        path.Remove(last);
                    }
                }
            }
        }

        private int GetNearestSegmentIndex(Contour contour, Vector2 currentPoint) {
            int startIndex = -1;
            for (int i = 0; i < contour.Segments.Count; i++) {
                if (contour.Segments[i].ContainsPoint(currentPoint)) {
                    startIndex = i;
                    break;
                }
            }

            return startIndex;
        }
    }
}