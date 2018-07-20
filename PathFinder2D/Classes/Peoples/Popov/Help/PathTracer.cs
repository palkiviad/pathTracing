using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Clusters;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
    public class PathTracer {
        private readonly IPolygon [] polygons;
        private Vector2 _currentPoint;
        private readonly Vector2 _goal;
        private readonly HashSet<int> excludedPolygons;
        private IPolygon polygon;
        
        
        public PathTracer(IPolygon[] polygons, Vector2 goal, HashSet<int> excludedPolygons) {
            this.polygons = polygons;
            _goal = goal;
            this.excludedPolygons = excludedPolygons;
        }

        public Vector2 Trace(Vector2 currentPoint, List<Vector2> path, IPolygon polygon) {
            this.polygon = polygon;
            var startIndex = this.polygon.GetNearestSegmentIndex(currentPoint);
            if (startIndex < 0) {
                throw new Exception("Can't find segment for current point!");
            }

            var forwardPath = GetTracedSubPath(startIndex, true, path);
            var backPath = GetTracedSubPath(startIndex, false, path);
            excludedPolygons.Add(this.polygon.GetId());
            var forwardDistance = Utils.CalculatePathDistance(forwardPath);
            var backDistance = Utils.CalculatePathDistance(backPath);
            var forwardValuableEffective = forwardDistance < backDistance /*< 0.99f*/;
            var shortestPath = forwardValuableEffective ? forwardPath : backPath;
            path.RemoveAt(path.Count -1);
            path.AddRange(shortestPath.GetRange(1, shortestPath.Count - 1));
            var last = path.Last();
            path.Remove(last);
            return last;
        }

        private List<Vector2> GetTracedSubPath(int startIndex, bool moveForward, List<Vector2> path) {
            if (path.Count < 2) {
                throw  new Exception("path has to be longer!");
            }

            var subPath = path.GetRange(path.Count - 2, 2);
            int nextIndex = startIndex;
            Segment currentSegment = polygon.GetSegment(nextIndex);
            do {
                
                Vector2 nextPoint = moveForward ? currentSegment.EndPoint : currentSegment.StartPoint;
                TryRemovePreviousPoint(subPath, nextPoint);
                subPath.Add(nextPoint);
                Vector2? nextIntersection = polygon.GetNearestIntersection(new Segment(nextPoint, _goal));
                if (!nextIntersection.HasValue && !polygon.SegmentIntersectsVertex(new Segment(nextPoint, _goal))) {
                    var intersectedContours = Utils.GetIntersectedPolygons(nextPoint, _goal, polygons);
                    if (intersectedContours.All(item => !excludedPolygons.Contains(item.Polygon.GetId()))) {
                        break;
                    }
                }
                nextIndex = moveForward ? nextIndex + 1 : nextIndex - 1;
                if (moveForward) {
                    nextIndex = nextIndex > polygon.SegmentsCount() - 1 ? 0 : nextIndex;
                } else {
                    nextIndex = nextIndex < 0 ? polygon.SegmentsCount() - 1 : nextIndex;
                }

                currentSegment = polygon.GetSegment(nextIndex);
            } while (nextIndex != startIndex);
            return subPath;
        }

        private void TryRemovePreviousPoint(IList<Vector2> path, Vector2 currentPoint) {
            if (path.Count <= 1) {
                return;
            }
            var previous = path[path.Count - 2];
            var last = path[path.Count - 1];
            var intersected = Utils.GetIntersectedPolygons(previous, currentPoint, polygons);
            if (intersected.Count != 0) {
                return;
            }
            if (!(polygon.PointOnEdge(previous) && polygon.PointOnEdge(last)) ||
                polygon.LayOnSameHalfPlane(new Segment(currentPoint, previous))) {
                path.Remove(last);
            }
        }

    }
}