using System;
using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Mathematics;
using PathFinder.Peoples.Popov.Clusters;
using PathFinder2D.Classes.Peoples.Popov.Help;

namespace PathFinder.Popov {
    public class MapV2 : IMap {
        private PolygonsContainer map;
        private Stopwatch stopwatch;
        private Finder pathFinder;
        
        
        public void Init(Vector2[][] obstacles) {
            stopwatch= new Stopwatch();
            stopwatch.Start();
            map = MapBuilder.Build(obstacles);
            stopwatch.Stop();
            Console.WriteLine("total init time is "+ stopwatch.Elapsed.TotalMilliseconds);
            pathFinder = new Finder(map);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            return pathFinder.GetPath(start, end);
        }
    }
}