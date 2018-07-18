using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Matusevich
{
    public class Obstacle
    {
        private readonly Vector2[] _vertices;

        public Obstacle(Vector2[] vertices)
        {
            _vertices = vertices;
        }

        public int VerticesCount {
            get { return _vertices.Length; }
        }

        public bool Inersects(Segment segment, out IntersectionDetails intersectionDetails)
        {
            intersectionDetails = IntersectionDetails.CreateEmpty();
            intersectionDetails.Obstacle = null;
            intersectionDetails.SquaredDistanceFromStart = 0;

//            int firstVerticeIndex = -1;
//            int secondVerticeIndex = -1;
//            for (var i = 0; i < _vertices.Length; i++)
//            {
//                if (_vertices[i].Equals(segment.Start))
//                {
//                    firstVerticeIndex = i;
//                }
//                if (_vertices[i].Equals(segment.End))
//                {
//                    secondVerticeIndex = i;
//                }
//            }
//            if (firstVerticeIndex != -1 && secondVerticeIndex != -1)
//            {
//                int delta = Math.Abs(firstVerticeIndex - secondVerticeIndex);
//                if (delta >= _vertices.Length)
//                {
//                    delta -= _vertices.Length;
//                }
//                if (delta == 1)
//                {
//                    return false;
//                }
//            }

            for (var i = 0; i < _vertices.Length; i++)
            {
                var secondVertice = i == _vertices.Length - 1 ? 0 : i + 1;
                Segment obstacleSegment = Segment.Create(_vertices[i], _vertices[secondVertice]);
                Vector2 intersectionPoint = Vector2.zero;
                if (Vector2.SegmentToSegmentIntersection(segment.Start, segment.End, obstacleSegment.Start, obstacleSegment.End, ref intersectionPoint))
                {
                    float squaredDistance = Vector2.SqrDistance(segment.Start, intersectionPoint);
                    if (intersectionDetails.Obstacle == null || squaredDistance < intersectionDetails.SquaredDistanceFromStart)
                    {
                        intersectionDetails.SquaredDistanceFromStart = squaredDistance;
                    }
                    intersectionDetails.Obstacle = this;
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
                            intersectionDetails.Obstacle = this;
                            intersectionDetails.SquaredDistanceFromStart = 0;
                            break;
                        }
                    }
                }

                if (intersectionDetails.Obstacle == null)
                {
                    var shrinkedSegment = Segment.Shrink(segment);
                    if (Contains(shrinkedSegment.Start) || Contains(shrinkedSegment.End))
                    {
                        intersectionDetails.Obstacle = this;
                        intersectionDetails.SquaredDistanceFromStart = 0;
                    }
                }
            }

            return intersectionDetails.Obstacle != null;
        }

        private bool Contains(Vector2 point)
        {
            Vector2 temp = Vector2.down;
            int intersectionsCount = 0;
            for (int i = 0; i < _vertices.Length; i++)
            {
                Vector2 edgeStart = _vertices[i];
                Vector2 edgeEnd = _vertices[i + 1 == _vertices.Length ? 0 : i + 1];

                if (Vector2.SegmentToSegmentIntersection(point, new Vector2(100000, 100000), edgeStart, edgeEnd, ref temp))
                {
                    intersectionsCount++;
                }
            }

            return intersectionsCount % 2 == 1;
        }


        public Vector2 GetVerticeAt(int i)
        {
            return _vertices[i];
        }
    }
}