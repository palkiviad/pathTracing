using System.Collections.Generic;
using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Peoples.Popov.Clusters {
    public class PolygonsContainer : IPolygon {
        private Polygon _rectBounds;
        public PolygonsContainer[] ChildContainers { get; private set; }
        public Polygon[] Polygons { get; private set; }

        public PolygonsContainer(Vector2 bottomLeft, Vector2 topRight) {
            var vertices = new Vector2 [4];
            vertices[0] = bottomLeft;
            vertices[1] = new Vector2(bottomLeft.x, topRight.y);
            vertices[2] = topRight;
            vertices[3] = new Vector2(topRight.x, bottomLeft.y);
            _rectBounds = new Polygon(vertices);
        }

        public bool Contains(Vector2 point) {
            return _rectBounds.Contains(point);
        }

        public bool ContainsPolygon(Polygon polygon) {
            for (int i = 0; i < polygon.SegmentsCount(); i++) {
                var segment = polygon.GetSegment(i);
                var x = segment.StartPoint.x;
                var y = segment.StartPoint.y;
                if (
                    x <= _rectBounds.MaxX && x >= _rectBounds.MinX
                                          && y <= _rectBounds.MaxY && y >= _rectBounds.MinY
                ) {
                    return true;
                }
            }

            return false;
        }

        public Vector2? GetNearestIntersection(Segment segment) {
            return _rectBounds.GetNearestIntersection(segment);
        }

        public bool PointOnEdge(Vector2 point) {
            return _rectBounds.PointOnEdge(point);
        }

        public int GetNearestSegmentIndex(Vector2 currentPoint) {
            return _rectBounds.GetNearestSegmentIndex(currentPoint);
        }

        public int GetId() {
            return _rectBounds.GetId();
        }

        public Segment GetSegment(int index) {
            return _rectBounds.GetSegment(index);
        }

        public int SegmentsCount() {
            return _rectBounds.SegmentsCount();
        }

        public bool SegmentIntersectsVertex(Segment segment) {
            return _rectBounds.SegmentIntersectsVertex(segment);
        }

        public bool LayOnSameHalfPlane(Segment segment) {
            return _rectBounds.LayOnSameHalfPlane(segment);
        }

        public void CreateChildren() {
            ChildContainers = new PolygonsContainer[4];
            var minX = _rectBounds.BottomLeft.x;
            var minY = _rectBounds.BottomLeft.y;
            var maxX = _rectBounds.TopRight.x;
            var maxY = _rectBounds.TopRight.y;
            var halfX = minX + (maxX - minX) / 2;
            var halfY = minY + (maxY - minY) / 2;

            ChildContainers[0] = new PolygonsContainer(new Vector2(minX, minY), new Vector2(halfX, halfY));
            ChildContainers[1] = new PolygonsContainer(new Vector2(minX, halfY), new Vector2(halfX, maxY));
            ChildContainers[2] = new PolygonsContainer(new Vector2(halfX, minY), new Vector2(maxX, halfY));
            ChildContainers[3] = new PolygonsContainer(new Vector2(halfX, halfY), new Vector2(maxX, maxY));
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

        public void FindPath(Vector2 start, Vector2 end, List<Vector2> result) {
            
        }
    }
}