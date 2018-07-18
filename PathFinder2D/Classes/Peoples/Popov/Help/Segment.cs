using System;
using PathFinder.Mathematics;

namespace PathFinder2D.Classes.Peoples.Popov.Help {
    public class Segment {

        public Vector2 StartPoint { get; }

        public Vector2 EndPoint { get; }

        private float _distance;

        public Segment(Vector2 startPoint, Vector2 endPoint) {
            StartPoint = startPoint;
            EndPoint = endPoint;
            _distance = Vector2.Distance(StartPoint, EndPoint);
        }

        /// <summary>
        /// Приблизительное вычисление основывающеесе на сумме подотрезков
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point) {
            float distance1 = Vector2.Distance(StartPoint, point);
            float distance2 = Vector2.Distance(EndPoint, point);
            return Math.Abs(distance1 + distance2 - _distance) < 0.00003;
        }

        public bool EndPointCloser(Vector2 goal) {
            float distance1 = Vector2.Distance(StartPoint, goal);
            float distance2 = Vector2.Distance(EndPoint, goal);
            return distance2 < distance1;
        }
    }
}