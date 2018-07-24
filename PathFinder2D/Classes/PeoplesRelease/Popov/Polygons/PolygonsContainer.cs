using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Popov {
    public class PolygonsContainer : IPolygon {
        private Polygon rectBounds;
        public PolygonsContainer[] ChildContainers { get; private set; }
        public IPolygon[] Polygons { get; private set; }
        public PolygonsContainer Parent { get; private set; }

        public bool HasChildren {
            get { return ChildContainers != null && ChildContainers.Length > 0; }
        }

        public bool HasParent {
            get { return Parent != null; }
        }

        public PathTracer _tracer;


        public PolygonsContainer(Vector2 bottomLeft, Vector2 topRight, PolygonsContainer parent) {
            var vertices = new Vector2 [4];
            vertices[0] = bottomLeft;
            vertices[1] = new Vector2(bottomLeft.x, topRight.y);
            vertices[2] = topRight;
            vertices[3] = new Vector2(topRight.x, bottomLeft.y);
            rectBounds = new Polygon(vertices);
            Parent = parent;
        }

        public bool Contains(Vector2 point) {
            return rectBounds.Contains(point);
        }

        public bool ContainsPolygon(Polygon polygon) {
            for (int i = 0; i < polygon.SegmentsCount(); i++) {
                var segment = polygon.GetSegment(i);
                var x = segment.StartPoint.x;
                var y = segment.StartPoint.y;
                if (
                    x <= rectBounds.MaxX && x >= rectBounds.MinX
                                         && y <= rectBounds.MaxY && y >= rectBounds.MinY
                ) {
                    return true;
                }
            }

            return false;
        }

        public Vector2? GetNearestIntersection(Segment segment) {
            return rectBounds.GetNearestIntersection(segment);
        }

        public bool PointOnEdge(Vector2 point) {
            return rectBounds.PointOnEdge(point);
        }

        public int GetNearestSegmentIndex(Vector2 currentPoint) {
            return rectBounds.GetNearestSegmentIndex(currentPoint);
        }

        public int GetId() {
            return rectBounds.GetId();
        }

        public Segment GetSegment(int index) {
            return rectBounds.GetSegment(index);
        }

        public int SegmentsCount() {
            return rectBounds.SegmentsCount();
        }

        public bool SegmentIntersectsVertex(Segment segment) {
            return rectBounds.SegmentIntersectsVertex(segment);
        }

        public bool LayOnSameHalfPlane(Segment segment) {
            return rectBounds.LayOnSameHalfPlane(segment);
        }

        public bool LayInSegmentBounds(Segment segment) {
            return rectBounds.LayInSegmentBounds(segment);
        }

        public void CreateChildren() {
            ChildContainers = new PolygonsContainer[4];
            var minX = rectBounds.MinX;
            var minY = rectBounds.MinY;
            var maxX = rectBounds.MaxX;
            var maxY = rectBounds.MaxY;
            var halfX = minX + (maxX - minX) / 2;
            var halfY = minY + (maxY - minY) / 2;

            ChildContainers[0] = new PolygonsContainer(new Vector2(minX, minY), new Vector2(halfX, halfY), this);
            ChildContainers[1] = new PolygonsContainer(new Vector2(minX, halfY), new Vector2(halfX, maxY), this);
            ChildContainers[2] = new PolygonsContainer(new Vector2(halfX, minY), new Vector2(maxX, halfY), this);
            ChildContainers[3] = new PolygonsContainer(new Vector2(halfX, halfY), new Vector2(maxX, maxY), this);
        }

        private void AddPolygonsToChildren(Polygon[] polygons) {
            Dictionary<int, List<Polygon>> map = new Dictionary<int, List<Polygon>>();
            foreach (var polygon in polygons) {
                for (var i = 0; i < ChildContainers.Length; i++) {
                    var child = ChildContainers[i];
                    if (child.ContainsPolygon(polygon)) {
                        List<Polygon> list;
                        if (map.ContainsKey(i)) {
                            list = map[i];
                        } else {
                            list = new List<Polygon>();
                            map.Add(i, list);
                        }

                        list.Add(polygon);
                    }
                }
            }

            foreach (var pair in map) {
                var child = ChildContainers[pair.Key];
                var childPolygons = pair.Value.ToArray();
                child.InitializeChildren(childPolygons);
            }
        }

        public void InitializeChildren(Polygon[] polygons) {
            if (polygons.Length > 8) {
                CreateChildren();
                AddPolygonsToChildren(polygons);
            } else {
                Polygons = polygons;
            }
        }

        public bool HasPolygons() {
            return Polygons != null && Polygons.Length > 0;
        }

        public bool IsEmpty() {
            if (ChildContainers != null) {
                foreach (var container in ChildContainers) {
                    if (!container.IsEmpty()) {
                        return false;
                    }
                }
            }

            return !HasPolygons();
        }

        public PolygonsContainer GetChildContainedSegment(Vector2 start, Vector2 end) {
            if (!HasChildren) {
                throw new Exception("container has not any child!");
            }
            foreach (var container in ChildContainers) {
                if (container.Contains(start) && container.Contains(end)) {
                    return container;
                }
            }
            return null;
        }

        public PolygonsContainer GetChildContainedPoint(Vector2 start) {
            if (!HasChildren) {
                throw new Exception("container has not any child!");
            }
            foreach (var container in ChildContainers) {
                if (container.Contains(start)) {
                    return container;
                }
            }
            return null;
        }

       /* public override string ToString() {
            return "Bounds is " + rectBounds.BottomLeft + "; " + rectBounds.TopRight;
        }*/
       
    }
}