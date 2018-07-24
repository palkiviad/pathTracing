using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Peoples.Popov.Test {
    public static class PathDistanceCalculator {

        private static string controlPoints = "StartAndEnd=-118.6163,52.898,276.4717,96.89355";

        public static void Calculate(IMap map, Vector2 start, Vector2 end) {
            var path = map.GetPath(start, end).ToList();
            CalculatePathDistance(path);
        }
        
        private static void CalculatePathDistance(IList<Vector2> path) {
            float fullPath = 0;
            for (int i = 0; i < path.Count - 1; i++) {
                fullPath += Vector2.Distance(path[i], path[i + 1]);
            }
          //  Console.WriteLine(@"Full path distance is " + (int)fullPath);
        }
    }
}