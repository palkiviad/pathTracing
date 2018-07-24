using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Release.Popov {
    public class Finder {
        private PolygonsContainer map;
        private HashSet<int> excludedPolygons;
        private PathTracer tracer;
        private Vector2 currentPoint;
        private Vector2 goal;
        private List<Vector2> currentPath;


        public Finder(PolygonsContainer map) {
            this.map = map;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            excludedPolygons = new HashSet<int>();
            currentPoint = start;
            goal = end;
            currentPath = new List<Vector2>();
            currentPath.Add(currentPoint);
            TraceContainer(map);
            currentPath.Add(goal);
            return currentPath;
        }

        private void TraceContainer(PolygonsContainer container) {
            var currentContainer = container;
            var segment = new Segment(currentPoint, goal);
            if (currentContainer.IsEmpty()) {
                return;
            }

            if (!currentContainer.LayInSegmentBounds(segment)) {
                return;
            }
            do {
                if (currentContainer.HasChildren) {
                    if (currentContainer.Contains(currentPoint) ) {
                        var child = currentContainer.GetChildContainedPoint(currentPoint);
                        if (child != null && !excludedPolygons.Contains(child.GetId())) {
                            currentContainer = child;
                            continue;
                        }
                    }
                    var intersection = FindNearestPolygonIntersection(currentContainer.ChildContainers);
                    if (intersection == null) {
                        excludedPolygons.Add(currentContainer.GetId());
                        currentContainer = currentContainer.Parent;
                        if (currentContainer == null) {
                            break;
                        }
                        continue;
                    }
                    currentContainer = intersection.Polygon as PolygonsContainer;
                } else {
                    excludedPolygons.Add(currentContainer.GetId());
                    if (currentContainer.HasPolygons()) {
                       TracePolygons(currentContainer.Polygons);
                    }
                    if (currentContainer.HasParent) {
                        currentContainer = currentContainer.Parent;
                    } else {
                        return;
                    }
                }
            } while (true);
        }

        private void TracePolygons(IPolygon[] polygons) {
            tracer = new PathTracer(polygons, goal, excludedPolygons);
            do {
                var intersection = FindNearestPolygonIntersection(polygons);
                if (intersection == null) {
                    AddNearestreaBoundForContainer(polygons);
                    return;
                }

                currentPoint = intersection.Intersection;
                currentPath.Add(currentPoint);
                currentPoint = tracer.Trace(currentPoint, currentPath, intersection.Polygon);
                currentPath.Add(currentPoint);
            } while (true);
        }

        private void AddNearestreaBoundForContainer(IPolygon[] polygons) {
            //
        }
        


        private Utils.PolygonIntersection FindNearestPolygonIntersection(IList<IPolygon> polygons) {
            var polygonsInBounds = Utils.RemovePolygonsOutsideBounds(currentPoint, goal, polygons);
            if (polygonsInBounds.Length == 0) {
                return null;
            }

            var intersectedPolygons = Utils.GetIntersectedPolygons(currentPoint, goal, polygonsInBounds);
            if (intersectedPolygons.Count == 0) {
                return null;
            }

            intersectedPolygons.Sort(SortByDistance);
            var container = intersectedPolygons.FirstOrDefault(item => !excludedPolygons.Contains(item.Polygon.GetId()));
            /*if (container == null) {
                throw new Exception("Can't find path because all containers are excluded!");
            }*/
            return container;
        }

        private int SortByDistance(Utils.PolygonIntersection x, Utils.PolygonIntersection y) {
            var distance1 = Vector2.Distance(currentPoint, x.Intersection);
            var distance2 = Vector2.Distance(currentPoint, y.Intersection);
            return (int) (distance1 - distance2);
        }
    }
}