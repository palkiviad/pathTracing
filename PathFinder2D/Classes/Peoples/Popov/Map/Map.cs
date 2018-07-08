using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Help;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Popov {
    public class Map : IMap {
        private Contour[] contours;
        private Vector2 currentPoint;
        private Vector2 goal;
        private List<int> excludedContours;
        private PathTracer tracer;
        
        private readonly Stopwatch stopwatch = new Stopwatch();
        private int calculateCount;
        

        public void Init(Vector2[][] obstacles) {
            contours = new Contour[obstacles.Length];
            Contour.ResetId();
            for (int i = 0; i < obstacles.Length; i++) {
                contours[i] = new Contour(obstacles[i]);
            }
        }
        

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            stopwatch.Start();
            calculateCount++;
            excludedContours = new List<int>();
            var result = new List<Vector2>();
            currentPoint = start;
            goal = end;
            tracer = new PathTracer(contours, goal, excludedContours);
            do {
                result.Add(currentPoint);
                List<IntersectedContour> intersectedContours = Utils.GetIntersectedContours(currentPoint, goal, contours);
                if (intersectedContours.Count == 0) {
                    break;
                }
                intersectedContours.Sort(SortByDistance);
                var contour = intersectedContours.FirstOrDefault(item => excludedContours.IndexOf(item.Contour.Id) < 0);
                if (contour == null) {
                    throw new Exception("Can't find path because all contoures are excluded!");
                }
                currentPoint = contour.IntersectionPoint;
                result.Add(currentPoint);
                currentPoint = tracer.Trace(currentPoint, result, contour.Contour);
            } while (true);

            result.Add(goal);
            stopwatch.Stop();
            if (calculateCount % 2 == 0) {
                Console.WriteLine("calculate count is {0} ", calculateCount);
                Console.WriteLine("average time is {0}", stopwatch.Elapsed.TotalMilliseconds /calculateCount);
            }
            return result;
        }
        
        private int SortByDistance(IntersectedContour x, IntersectedContour y) {
            float distance1 = Vector2.Distance(currentPoint, x.IntersectionPoint);
            float distance2 = Vector2.Distance(currentPoint, y.IntersectionPoint);
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