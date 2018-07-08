using System.Collections.Generic;
using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Peoples.Popov.Clasters {
    public class PolygonsCluster  : IPolygon {
        private static int divisionFactor = 4;
        private IList<IPolygon> children;

        public void AddPolygons(List<IPolygon> polygons, IPolygon bounds) {
            this.bounds = bounds;
            children = new List<IPolygon>();
            if (polygons.Count > divisionFactor) {
                CreateSubClusters(polygons);
            } else {
                polygons.ForEach(item => children.Add(item));
            }
        }

        private void CreateSubClusters(List<IPolygon> polygons) {
            
        }


        private IPolygon bounds;
        
        
        
        
        
        
        public bool Contains(Vector2 point) {
            return bounds.Contains(point);
        }

        public Vector2? GetNearestIntersection(Segment segment) {
            return bounds.GetNearestIntersection(segment);
        }

        public bool PointOnEdge(Vector2 point) {
            return bounds.PointOnEdge(point);
        }

        public int GetNearestSegmentIndex(Vector2 currentPoint) {
            return bounds.GetNearestSegmentIndex(currentPoint);
        }

        public int GetId() {
            return bounds.GetId();
        }

        public Segment GetSegment(int index) {
            return bounds.GetSegment(index);
        }

        public int SegmentsCount() {
            return bounds.SegmentsCount();
        }

        public bool SegmentIntersectsVertex(Segment segment) {
            return bounds.SegmentIntersectsVertex(segment);
        }

        public bool LayOnSameHalfPlane(Segment segment) {
            return bounds.LayOnSameHalfPlane(segment);
        }
    }
}