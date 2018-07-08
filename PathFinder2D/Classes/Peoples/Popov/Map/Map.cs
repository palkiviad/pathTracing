using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Clasters;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Popov {
    public class MapV2 : IMap {
        private List<IPolygon> polygons;
        private Vector2 currentPoint;
        private Vector2 goal;
        private List<int> excludedPolygons;
        private PathTracerV2 tracer;
        
        private readonly Stopwatch stopwatch = new Stopwatch();
        private int calculateCount;
        
        public void Init(Vector2[][] obstacles) {
            polygons = new List<IPolygon>();
            Polygon.ResetId();
            for (int i = 0; i < obstacles.Length; i++) {
                polygons.Add(new Polygon(obstacles[i]));
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            stopwatch.Start();
            calculateCount++;
            excludedPolygons = new List<int>();
            var result = new List<Vector2>();
            currentPoint = start;
            goal = end;
            tracer = new PathTracerV2(polygons, goal, excludedPolygons);
            do {
                result.Add(currentPoint);
                var intersectedContours = Utils.GetIntersectedPolygons(currentPoint, goal, polygons);
                if (intersectedContours.Count == 0) {
                    break;
                }
                intersectedContours.Sort(SortByDistance);
                var contour = intersectedContours.FirstOrDefault(item => excludedPolygons.IndexOf(item.Polygon.GetId()) < 0);
                if (contour == null) {
                    throw new Exception("Can't find path because all contoures are excluded!");
                }
                currentPoint = contour.Intersection;
                result.Add(currentPoint);
                currentPoint = tracer.Trace(currentPoint, result, contour.Polygon);
            } while (true);

            result.Add(goal);
            stopwatch.Stop();
            if (calculateCount % 50 == 0) {
                Console.WriteLine("calculate count is {0} ", calculateCount);
                Console.WriteLine("average time is {0}", stopwatch.Elapsed.TotalMilliseconds /calculateCount);
            }
            return result;
        }
        
        private int SortByDistance(Utils.PolygonIntersection x, Utils.PolygonIntersection y) {
            float distance1 = Vector2.Distance(currentPoint, x.Intersection);
            float distance2 = Vector2.Distance(currentPoint, y.Intersection);
            return (int) (distance1 - distance2);
        }
    }
}