using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using OpenTK.Graphics.ES11;
using PathFinder.Mathematics;

namespace PathFinder2D.Classes.Peoples.Galkin.Map
{
    class Line {
        public Vector2 start;
        public Vector2 end;
    }

    class Obstacle {
        private Bound _bound;
        private List<Line> _lines;

        public Obstacle(Vector2[] points)
        {
            _bound = new Bound(points);
            _lines = new List<Line>();

            for (int i = 0, count = points.Length; i < count; i++)
            {
                Line line = new Line();
                line.start = points[i];
                line.end = (i + 1) < count ? points[i + 1] : points[0];
                _lines.Add(line);
            }
        }

        /// <summary>
        /// проверка находится ли точка внутри обстакла
        /// </summary>
        public bool Contains(Vector2 startPoint, Vector2 endPoint, ref Vector2 intersectionPoint)
        {
            /*
            // сначала проверяем по баунду для скорости
            if (!_bound.Contains(point)) {
                return null;
            }
            */

            foreach (var line in _lines) {
                if (Vector2.SegmentToSegmentIntersection(line.start, line.end, startPoint, endPoint, ref intersectionPoint)) {
                    return true;
                }
            }
            return false;
        }


        public List<Vector2> GetDirectionalPoints(Vector2 startPoint, Vector2 endPoint) {

            List<Vector2> directionalPoints = new List<Vector2>();
            Vector2 intersectionPoint = Vector2.zero;
            float minDistanceToPoint = float.PositiveInfinity;

            int startIndex = 0;
            int linesCount = _lines.Count;

            bool start = true;
            for (int i = 0; i < linesCount; ++i)
            {
                var line = _lines[i];
                if (Vector2.SegmentToSegmentIntersection(line.start, line.end, startPoint, endPoint, ref intersectionPoint))
                {
                    float distanceToPoint = (startPoint - intersectionPoint).magnitude;
                    if (distanceToPoint < minDistanceToPoint)
                    {
                        startIndex = i;
                        minDistanceToPoint = distanceToPoint;
                        start = ((intersectionPoint - line.start).magnitude < (intersectionPoint - line.end).magnitude);
                    }
                }
            }

            for (int i = 0; i < linesCount; ++i)
            {
                int curIndex = startIndex + i;
                if (curIndex >= linesCount)
                {
                    curIndex = curIndex - linesCount;
                }

                if (start)
                {
                    directionalPoints.Add(_lines[curIndex].start);
                }
                else
                {
                    directionalPoints.Add(_lines[curIndex].end);
                }
                
            }
            return directionalPoints;
        }
    }
}
