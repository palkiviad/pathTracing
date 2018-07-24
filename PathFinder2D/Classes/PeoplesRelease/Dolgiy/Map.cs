using System.Collections.Generic;
using System.Linq;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder.Release.Dolgii {

    public sealed class Map : IMap {

        private Obstacle[] Obstacles;

        public void Init(Vector2[][] obstacles) {
            Obstacles = new Obstacle[obstacles.Length];
            for (var i = 0; i < obstacles.Length; i++) {
                Obstacles[i] = new Obstacle(obstacles[i]);
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            var path = new List<Vector2> {start};
            var startEnd = new Segment(start, end);

            foreach (var obstacle in Obstacles) {
                obstacle.WayAround(startEnd).ForEach(p => path.Add(p));
            }

            path.Add(end);
            return path;
        }
    }

    class Obstacle {
        private Segment[] Segments;

        public Obstacle(Vector2[] corners) {
            Segments = new Segment[corners.Length];
            for (var i = 0; i < corners.Length; i++) {
                var p1 = corners[i];
                var p2 = i == corners.Length - 1 ? corners[0] : corners[i + 1];
                Segments[i] = new Segment(p1, p2);
            }
        }

        public List<Vector2> WayAround(Segment l) {
            var td = new TracingData();
            if (!FindTrasingData(l, ref td)) {
                return new List<Vector2>();
            }

            var toEndPath = new List<Vector2>();
            var toEndPathLen = 0.0; 
            toEndPath.Add(td.InPoint);
            for (var i = td.InSegmentIndex; i != td.OutSegmentIndex; i = (i + 1) % Segments.Length) {
                var newP = Segments[i].End;
                toEndPathLen += Vector2.Distance(toEndPath.Last(), newP);
                toEndPath.Add(newP);    
            }

            toEndPathLen += Vector2.Distance(toEndPath.Last(), td.OutPoint);
            toEndPath.Add(td.OutPoint);      
            
            var toStartPath = new List<Vector2>();
            var toStartPathLen = 0.0; 
            toStartPath.Add(td.InPoint);
            for (var i = td.InSegmentIndex; i != td.OutSegmentIndex; i = i == 0 ? Segments.Length - 1 : i - 1) {
                var newP = Segments[i].Start;
                toStartPathLen += Vector2.Distance(toStartPath.Last(), newP);
                toStartPath.Add(newP);    
            }

            toStartPathLen += Vector2.Distance(toStartPath.Last(), td.OutPoint);
            toStartPath.Add(td.OutPoint);

            return toStartPathLen < toEndPathLen ? toStartPath : toEndPath;
        }

        private bool FindTrasingData(Segment l, ref TracingData data) {
            var intersections = new Dictionary<Vector2, int>();
            var inDistances = new Dictionary<float, Vector2>();
            var outDistances = new Dictionary<float, Vector2>();
            for (var i = 0; i < Segments.Length; i++) {
                var segment = Segments[i];
                var intersectPoint = new Vector2();
                var isIntersects = Vector2.SegmentToSegmentIntersection(l.Start, l.End, segment.Start, segment.End, ref intersectPoint);
                if (!isIntersects) {
                    continue;
                }

                intersections[intersectPoint] = i;
                inDistances[Vector2.Distance(intersectPoint, l.Start)] = intersectPoint;
                outDistances[Vector2.Distance(intersectPoint, l.End)] = intersectPoint;
            }

            if (intersections.Count == 0) {
                return false;
            }

            data.InPoint = inDistances[inDistances.Keys.Min()];
            data.OutPoint = outDistances[outDistances.Keys.Min()];
            data.InSegmentIndex = intersections[data.InPoint];
            data.OutSegmentIndex = intersections[data.OutPoint];
            return true;
        }
    }

    class TracingData {
        public int InSegmentIndex;
        public int OutSegmentIndex;
        public Vector2 InPoint;
        public Vector2 OutPoint;
    }

    class Segment {
        public Vector2 Start;
        public Vector2 End;

        public Segment(Vector2 start, Vector2 end) {
            Start = start;
            End = end;
        }
    }
}