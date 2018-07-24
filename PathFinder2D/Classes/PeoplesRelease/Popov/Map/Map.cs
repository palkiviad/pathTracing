using System;
using System.Collections.Generic;
using System.Diagnostics;
using PathFinder.Mathematics;

namespace PathFinder.Release.Popov {
    public class Map : IMap {
        private PolygonsContainer map;
        private Stopwatch stopwatch;
        private Finder pathFinder;
        
        
        public void Init(Vector2[][] obstacles) {
            stopwatch= new Stopwatch();
            stopwatch.Start();
            map = MapBuilder.Build(obstacles);
            stopwatch.Stop();
//            Console.WriteLine("total init time is "+ stopwatch.Elapsed.TotalMilliseconds);
            pathFinder = new Finder(map);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            return pathFinder.GetPath(start, end);
        }
    }
}