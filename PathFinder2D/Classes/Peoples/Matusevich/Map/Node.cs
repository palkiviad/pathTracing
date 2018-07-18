using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Matusevich
{
    public class Node
    {
        private readonly Vector2 _point;
        private readonly float _travelledDistance;
        private readonly float _heuristic;
        private readonly Node _parent;

        private Node(Node parent, Vector2 point, float travelledDistance, float distanceToTarget)
        {
            _parent = parent;
            _point = point;
            _travelledDistance = travelledDistance;
            _heuristic = _travelledDistance + distanceToTarget;
        }

        public float Heuristic {
            get { return _heuristic; }
        }

        public Vector2 Point {
            get { return _point; }
        }

        public IEnumerable<Vector2> GetPath()
        {
            var result = new List<Vector2>();
            var node = this;
            while (node != null)
            {
                result.Add(node._point);
                node = node._parent;
            }
            result.Reverse();
            return result;
        }

        public override string ToString()
        {
            return "(" + _point.x + "; " + _point.y + ")";
        }

        public static Node CreateStartNode(Vector2 start, Vector2 end)
        {
            return new Node(null, start, 0, Distance(start, end));
        }

        public Node CreateChild(Vector2 newPosition, Vector2 target)
        {
            return new Node(this, newPosition, _travelledDistance + Distance(Point, newPosition), Distance(newPosition, target));
        }

        private static float Distance(Vector2 a, Vector2 b)
        {
            //самое правильное и самое долгое - Distance
#if true
            return Vector2.Distance(a, b);
#else
            return Vector2.Distance(a, b);
            return Vector2.SqrDistance(a, b);
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
#endif
        }
    }
}