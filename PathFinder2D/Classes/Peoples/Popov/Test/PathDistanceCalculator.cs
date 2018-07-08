using System;
using System.Linq;
using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Peoples.Popov.Test {
    public static class PathDistanceCalculator {

        private static string controlPoints = "StartAndEnd=-118.6163,52.898,272.55,95.57269";

        public static void Calculate(IMap map, Vector2 start, Vector2 end) {
            var path = map.GetPath(start, end).ToList();
            var fullPath = Utils.CalculatePathDistance(path);
            Console.WriteLine(@"Full path distance is " + (int) fullPath);
        }
    }
}