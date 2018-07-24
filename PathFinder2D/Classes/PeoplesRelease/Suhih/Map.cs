using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathFinder.Mathematics;

namespace PathFinder.Release.Suhih
{
    public sealed class Map : IMap
    {
        private Vector2[][] obstacles;

        public void Init(Vector2[][] obstacles)
        {
            this.obstacles = obstacles;
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            List<Vector2> path = new List<Vector2>();

            Vector2 current = start;
            
            bool intersectOccured;
            do
            {
                intersectOccured = false;
                path.Add(current);
                List<Tuple<Vector2, int, int>> intersects = new List<Tuple<Vector2, int, int>>();
                for (int obstacleId = 0; obstacleId < obstacles.Length; obstacleId++) 
                {
                    Vector2[] obstacle = obstacles[obstacleId];
                    for (int i = 0; i < obstacle.Length; i++)
                    {
                        int edgeStartIndex = i % obstacle.Length;
                        int edgeEndIndex = (i + 1) % obstacle.Length;
                        Vector2 intersectPoint = new Vector2();
                        if (Vector2.SegmentToSegmentIntersection(current, end, obstacle[edgeStartIndex], obstacle[edgeEndIndex], ref intersectPoint) && isPointUp(current, obstacle[edgeStartIndex], obstacle[edgeEndIndex]))
                        {
                            intersects.Add(new Tuple<Vector2, int, int>(intersectPoint, obstacleId, edgeEndIndex));
                        }
                    }
                }

                Tuple<Vector2, int, int> nearestIntersect = null;
                foreach (Tuple<Vector2, int, int> intersectCandidate in intersects)
                {
                    if (nearestIntersect == null || Vector2.SqrDistance(current, intersectCandidate.Item1) < Vector2.SqrDistance(current, nearestIntersect.Item1))
                    {
                        nearestIntersect = intersectCandidate;
                    }
                }

                if (nearestIntersect != null)
                {
                    path.Add(nearestIntersect.Item1);
                    current = turnAround(obstacles[nearestIntersect.Item2], nearestIntersect.Item3, end, path);
                    intersectOccured = true;
                }

            } while (intersectOccured);

            path.Add(end);

            return path;
        }

        private bool isPointUp(Vector2 p, Vector2 vp1, Vector2 vp2)
        {
            // [(vp1, vp2), (vp1, p)] > 0
            return pseudoScalar(vp2 - vp1, p - vp1) >= 0;
        }

        private bool isPointDown(Vector2 p, Vector2 vp1, Vector2 vp2)
        {
            // [(vp1, vp2), (vp1, p)] < 0
            return pseudoScalar(vp2 - vp1, p - vp1) < 0;
        }

        private float pseudoScalar(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v2.x * v1.y;
        }

        private Vector2 turnAround(Vector2[] obstacle, int startEdge, Vector2 end, List<Vector2> path)
        {
            Vector2 lastPoint = obstacle[startEdge];
            for (int i = 0; i < obstacle.Length; i++)
            {
                path.Add(lastPoint);
                Vector2 edgeStart = obstacle[(startEdge + i) % obstacle.Length];
                Vector2 edgeEnd = obstacle[(startEdge + i + 1) % obstacle.Length];
                Vector2 edge = edgeEnd - edgeStart;

                Vector2 prevEdgeStart = obstacle[(startEdge + i + (obstacle.Length - 1)) % obstacle.Length];
                Vector2 prevEdgeEnd = edgeStart;
                Vector2 prevEdge = prevEdgeEnd - prevEdgeStart;
                Vector2 nextEdge = obstacle[(startEdge + i + 2) % obstacle.Length] - obstacle[(startEdge + i + 1) % obstacle.Length];
                //Console.WriteLine("pseudoScalar(prevEdge, edge)=" + pseudoScalar(prevEdge, edge));
                /*if (isPointDown(end, edgeStart, edgeEnd)) {
                    lastPoint = edgeEnd;
                } else */if (intersect(obstacle, edgeStart, end))
                {
                    lastPoint = edgeEnd;
                }
                else
                {
                    break;
                }
            }
            return lastPoint;
        } 

        private bool intersect(Vector2[] obstacle, Vector2 start, Vector2 end)
        {
            for (int i = 0; i < obstacle.Length; i++)
            {
                int edgeStartIndex = i % obstacle.Length;
                int edgeEndIndex = (i + 1) % obstacle.Length;
                Vector2 intersect = new Vector2();
                if (Vector2.SegmentToSegmentIntersection(start, end, obstacle[edgeStartIndex], obstacle[edgeEndIndex], ref intersect))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
