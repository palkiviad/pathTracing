using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;
using System;
using System.CodeDom;
using System.ComponentModel;
using System.Windows;
//using System.Windows.Markup.Localizer;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace PathFinder.Release.Kolesnikov {

    public sealed class Map : IMap
    {


        private List<Obstacle> obstacleList;

        public void Init(Vector2[][] obstacles)
        {
            obstacleList = new List<Obstacle>();
            foreach (var oneObstacle in obstacles)
            {
                obstacleList.Add(new Obstacle(oneObstacle));
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            List<Vector2> leftWay = new List<Vector2>();
            GetTheWay(start, end, ref leftWay, true);
            
            
            List<Vector2> rightWay = new List<Vector2>();
            GetTheWay(start, end, ref rightWay, false);

            List<Vector2>  result = leftWay.Count <rightWay.Count ?leftWay : rightWay;
                         
             
            //Optimize(ref result);

            return result;
        }

        private void GetTheWay(Vector2 start, Vector2 end, ref List<Vector2> myWay, bool leftHandPath) {
            myWay.Add(start);

            Vector2 fromPoint = start;
            Vector2 endObstaclePart = new Vector2();
            Vector2 intersectionPoint = new Vector2();

            foreach (var obstacle in obstacleList)
            {
                while (obstacle.GetNeaerestIntersectionWithObstacle(fromPoint, end, ref intersectionPoint, ref endObstaclePart))
                {
                    
                    if (obstacle.IsIntesectionWithVertex(fromPoint)) { // да мы ж на угле прям
                        
                        int nextIndex = leftHandPath ? obstacle.GetNextVertex(fromPoint) : obstacle.GetPreVertex(fromPoint);
                        intersectionPoint = obstacle.vectors[nextIndex];
                        
                        myWay.Add(intersectionPoint);
                        fromPoint = intersectionPoint;
                    } else {
                        myWay.Add(intersectionPoint);
                        Vector2 nextPoint = leftHandPath ? endObstaclePart : obstacle.GetBeginPartPoint(endObstaclePart);
                        myWay.Add(nextPoint);
                        fromPoint = nextPoint; 
                    }
                    

                    if (myWay.Count > 500) // непорядок
                    {
                        break;
                    }
                }
            }

            myWay.Add(end);
        }
        
        private void Optimize(ref List<Vector2> oldWay) {
            for (int i = 0; i<oldWay.Count; i++) { // смотрим весь путь
                if ((i + 2) >= oldWay.Count) break; // bounds
                
                Vector2 one = oldWay[i];
                Vector2 two = oldWay[i + 2]; // next next

                bool intersection = false;
                foreach (var obstacle in obstacleList) {
                 
                    Vector2 intrsctn = new Vector2();
                    Vector2 obstaclePart = new Vector2();
                    
                   
                    intersection = obstacle.GetNeaerestIntersectionWithObstacle(one, two, ref intrsctn, ref obstaclePart);
                    
                    if (intersection) {
                        break;
                    } 
                }

                if (!intersection) {
                    oldWay.RemoveAt(i+1);
                    i = 0; // again
                }
            }
        }
    }
}