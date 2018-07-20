using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Peoples.Popov.Clusters {
    public interface IPolygon {
        bool Contains(Vector2 point);
        Vector2? GetNearestIntersection(Segment segment);
        bool PointOnEdge(Vector2 point);
        int GetNearestSegmentIndex(Vector2 currentPoint);
        int GetId();
        Segment GetSegment(int index);
        int SegmentsCount();
        bool SegmentIntersectsVertex(Segment segment);
        bool LayOnSameHalfPlane(Segment segment);
        bool LayInSegmentBounds(Segment segment);
    }
}