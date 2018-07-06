using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Peoples.Popov.Test {
    public static class PathDistanceCalculator {
        

        public static void CalculatePathDistance(IList<Vector2> path) {
            float fullPath = 0;
            for (int i = 0; i < path.Count - 1; i++) {
                fullPath += Vector2.Distance(path[i], path[i + 1]);
            }
            Console.WriteLine(@"Full path distance is " + (int)fullPath);
        }
    }
}