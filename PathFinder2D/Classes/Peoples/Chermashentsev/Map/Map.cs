using PathFinder;
using PathFinder.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathFinder.Chermashentsev
{
    public sealed class Map : IMap
    {
        private Vector2[][] obstacles;
        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            List<Vector2> res = new List<Vector2>();
            res.Add(start);
            Vector2 currentPoint = start;
            Vector2 nextPoint = end;

            List<Vector2> intercetion = getNextMove(start, end);
            while (res.Count < 10000 && !res.Last().Equals(end))
            {
                res.AddRange(intercetion);
                intercetion = getNextMove(res.Last(), end);

            }

            return res;
        }


        List<Vector2> getNextMove(Vector2 p1, Vector2 p2)
        {
            List<Vector2> res = new List<Vector2>();
            Vector2[] closestObstacle = getClosestObstacle(p1, p2);
            if (closestObstacle == null)
            {
                res.Add(p2);
                return res;
            } else
            {
                for (int i = 0; i < closestObstacle.Length; i++)
                {
                    int left = i;
                    int right = i + 1;
                    if (right == closestObstacle.Length)
                    {
                        right = 0;
                    }
                    int vectorIndex = getClosestVectorInterception(p1, p2, closestObstacle, res);
                    if (vectorIndex >= 0)
                    {
                        tryBypassObstacle(vectorIndex, p2, closestObstacle, res);

                        return res;

                    }
                }
            }

            return res;
        }

        private void tryBypassObstacle(int from, Vector2 end, Vector2[] obstacle, List<Vector2> res)
        {
            int i = from;
            int count = obstacle.Count();

            do
            {
               
                int l = i;
                int r = i + 1;
                
                
                res.Add(obstacle[l]);
                if (isDirectPoint(obstacle[l], end, obstacle))
                {
                    return;
                }
                if (r == obstacle.Length)
                {
                    i = 0;
                    r = 0;
                } else
                {
                    i++;
                }
                count--;

            } while (count > 0);
           
        }

        Vector2[] getClosestObstacle(Vector2 start, Vector2 end)
        {
            float minDist = float.PositiveInfinity;
            Vector2[] minObstacle = null;
            foreach (Vector2[] obstacle in obstacles)
            {
                int vectorIndex = getClosestVectorInterception(start, end, obstacle, new List<Vector2>());
                if (vectorIndex >= 0)
                {
                    int right = vectorIndex + 1;
                    if (vectorIndex == obstacle.Length - 1)
                    {
                        right = 0;
                    }
                    float distance = Vector2.Distance(start, obstacle[vectorIndex]);
                    if (distance < minDist)
                    {
                        minDist = distance;
                        minObstacle = obstacle;
                    }
                }
            }

            return minObstacle;
        }

        int getClosestVectorInterception(Vector2 start, Vector2 end, Vector2[] obstacle, List<Vector2> resList)
        {
            int res = -1;
            Vector2 resInterception = Vector2.zero;
            Vector2 interception = Vector2.zero;
            float minDistance = float.PositiveInfinity;
            for (int i = 0; i < obstacle.Length; i++)
            {
                int left = i;
                int right = i + 1;
                if (right == obstacle.Length)
                {
                    right = 0;
                }
                if (Vector2.SegmentToSegmentIntersection(start, end, obstacle[left], obstacle[right], ref interception))
                {                    
                    float d = Vector2.Distance(start, interception);
                    if (d < minDistance)
                    {
                        resInterception = interception;
                        minDistance = d;
                        res = left;
                    }
                }
            }
            if (!resInterception.Equals(Vector2.zero))
            {
                resList.Add(resInterception);
            }

            return res;
        }

        bool isDirectPoint(Vector2 point, Vector2 end, Vector2[] obstacle)
        {
            Vector2 v = Vector2.zero;
            for (int i = 0; i < obstacle.Length; i++)
            {
                int left = i;
                int right = i + 1;
                if (right == obstacle.Length)
                {
                    right = 0;
                }
                if (Vector2.SegmentToSegmentIntersection(point, end, obstacle[left], obstacle[right], ref v))
                {
                    return false;
                }
            }

            return true;
        }

        private void addContur(List<Vector2> res, Vector2[] points, Vector2 p2)
        {
            Vector2 intersecton = Vector2.zero;
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (Vector2.SegmentToSegmentIntersection(points[i], p2, points[i], points[i+1], ref intersecton))
                {
                    res.Add(points[i] + Vector2.left);
                } else
                {
                    return;
                }
            }
        }

        private bool interceptObstacles(Vector2 possibleVariant)
        {
            return false;
        }

        public void Init(Vector2[][] obstacles)
        {
            this.obstacles = obstacles;
        }
    }
}
