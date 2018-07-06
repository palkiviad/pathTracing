using System;
using PathFinder.Mathematics;

namespace PathFinder.Popov {
    public class Segment {
        private Vector2 _startPoint;

        public Vector2 StartPoint => _startPoint;

        public Vector2 EndPoint => _endPoint;

        private Vector2 _endPoint;

        private float _distance;

        public Segment(Vector2 startPoint, Vector2 endPoint) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _distance = Vector2.Distance(_startPoint, _endPoint);
        }

        /// <summary>
        /// Приблизительное вычисление основывающеесе на сумме подотрезков
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point) {
            float distance1 = Vector2.Distance(_startPoint, point);
            float distance2 = Vector2.Distance(_endPoint, point);
            return Math.Abs(distance1 + distance2 - _distance) < 0.00003;
        }

        public bool EndPointCloser(Vector2 goal) {
            float distance1 = Vector2.Distance(_startPoint, goal);
            float distance2 = Vector2.Distance(_endPoint, goal);
            return distance2 < distance1;
        }
    }
}