using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Help;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Popov {
    public class Map : IMap {
        private Contour[] _contours;
        private Vector2 _currentPoint;
        private Vector2 _goal;
        private List<int> _excludedContours;
        private PathTracer _tracer;
        

        public void Init(Vector2[][] obstacles) {
            _contours = new Contour[obstacles.Length];
            Contour.ResetId();
            for (int i = 0; i < obstacles.Length; i++) {
                _contours[i] = new Contour(obstacles[i]);
            }
        }
        

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            _excludedContours = new List<int>();
            var result = new List<Vector2>();
            _currentPoint = start;
            _goal = end;
            _tracer = new PathTracer(_contours, _goal, _excludedContours);
            do {
                result.Add(_currentPoint);
                List<IntersectedContour> intersectedContours = Utils.GetIntersectedContours(_currentPoint, _goal, _contours);
                if (intersectedContours.Count == 0) {
                    break;
                }
                intersectedContours.Sort(SortByDistance);
                var contour = intersectedContours.FirstOrDefault(item => _excludedContours.IndexOf(item.Contour.Id) < 0);
                if (contour == null) {
                    throw new Exception("Can't find path because all contoures are excluded!");
                }
                _currentPoint = contour.IntersectionPoint;
                result.Add(_currentPoint);
                _currentPoint = _tracer.Trace(_currentPoint, result, contour.Contour);
            } while (true);

            result.Add(_goal);
            return result;
        }
        
        private int SortByDistance(IntersectedContour x, IntersectedContour y) {
            float distance1 = Vector2.Distance(_currentPoint, x.IntersectionPoint);
            float distance2 = Vector2.Distance(_currentPoint, y.IntersectionPoint);
            return (int) (distance1 - distance2);
        }
    }

    public class IntersectedContour {
        private Contour _contour;

        public Contour Contour => _contour;

        public Vector2 IntersectionPoint => _intersectionPoint;

        private Vector2 _intersectionPoint;


        public IntersectedContour(Contour contour, Vector2 intersectionPoint) {
            _contour = contour;
            _intersectionPoint = intersectionPoint;
        }
    }

}