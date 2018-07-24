using PathFinder.Mathematics;

namespace PathFinder.Release.Matusevich {
    public class Box {
        private readonly Vector2 _min;
        private readonly Vector2 _max;

        public Vector2 Min {
            get { return _min; }
        }

        public Vector2 Max {
            get { return _max; }
        }

        public Box(Vector2 min, Vector2 max) {
            _min = min;
            _max = max;
        }

        internal bool CannotIntersectWithExcluding(Segment segment) {
            return (segment.Start.x < _min.x && segment.End.x < _min.x)
                   || (segment.Start.y < _min.y && segment.End.y < _min.y)
                   || (segment.Start.x > _max.x && segment.End.x > _max.x)
                   || (segment.Start.y > _max.y && segment.End.y > _max.y);
        }

        internal bool ContainsLR(Box box) {
            return box.Min.x >= _min.x && box.Min.y >= _min.y && box.Max.x < _max.x && box.Max.y < _max.y;
        }

        internal Vector2 GetCenter() {
            return (_min + _max) / 2;
        }
    }
}