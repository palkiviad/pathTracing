using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Matusevich
{
    public class Map : IMap
    {
        private readonly List<Obstacle> _obstacles = new List<Obstacle>();
        private readonly List<Node> _openNodes = new List<Node>();
        private readonly List<Node> _closedNodes = new List<Node>();
        private readonly HashSet<Obstacle> _visitedInRecursiveObstacles = new HashSet<Obstacle>();
        
        public void Init(Vector2[][] obstacles)
        {
            _obstacles.Clear();
            for (var i = 0; i < obstacles.Length; i++)
            {
                _obstacles.Add(new Obstacle(obstacles[i]));
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            _openNodes.Clear();
            _closedNodes.Clear();
            _visitedInRecursiveObstacles.Clear(); //TODO: вот это хак всё с _visitedInRecursiveObstacles
            
            AddNode(Node.CreateStartNode(start, end));

            Node currentNode = GetNextNode();
            int safeRecursionCount = 0;
            while (!currentNode.Point.Equals(end))
            {
                if (safeRecursionCount > 1000)
                {
                    //TODO а это защита от вечного цикла, который у меня есть
                    //Console.WriteLine("Aborting long cycle");
                    return new List<Vector2>{start,end};
                }
                _closedNodes.Add(currentNode);
                FindNextNodes(currentNode, end);
                if (_openNodes.Count == 0)
                {
                    throw new Exception("no path");
                }
                currentNode = GetNextNode();
                safeRecursionCount++;
            }
            return currentNode.GetPath();
        }

        private void FindNextNodes(Node currentNode, Vector2 end)
        {
            //тут надо найти ближайший обстакл, который пересекается с путём до финиша
            //если такого не найдено - вернуть новую ноду, которая будет совпадать с финишем
            //если найдено - то найти на нём 2 ближайшие к точке пересечения вершины (в случае, если точка пересечения лежит на вершине они будут одинаковые) 
            //а потом найти все вершины этого обстакла, до которых можем добраться из текущей ноды и добавить в наш список
            //это тормозит, но должно работать

            IntersectionDetails nearestIntersection = FindNearestIntersection(currentNode.Point, end);
            if (nearestIntersection.Obstacle == null)
            {
                AddNode(currentNode.CreateChild(end, end));
                return;
            }

            var foundObstacle = nearestIntersection.Obstacle;
            RecursivelyAddReachableVerticesFromObstacle(currentNode, end, foundObstacle);
        }

        private void RecursivelyAddReachableVerticesFromObstacle(Node currentNode, Vector2 end, Obstacle foundObstacle)
        {
            _visitedInRecursiveObstacles.Add(foundObstacle);
            for (var i = 0; i < foundObstacle.VerticesCount; i++)
            {
                var obstacleVertice = foundObstacle.GetVerticeAt(i);
                if (obstacleVertice.Equals(currentNode.Point))
                {
                    continue; //TODO: и вот это хак
                }
                Obstacle anotherIntersectedObstacle;
                if (IsPathClear(currentNode.Point, obstacleVertice, out anotherIntersectedObstacle))
                {
                    AddNode(currentNode.CreateChild(obstacleVertice, end));
                    continue;
                }
                if (_visitedInRecursiveObstacles.Contains(anotherIntersectedObstacle))
                {
                    continue; //TODO: и это хак
                }
                RecursivelyAddReachableVerticesFromObstacle(currentNode, end, anotherIntersectedObstacle);
            }
        }

        private bool IsPathClear(Vector2 start, Vector2 end, out Obstacle anotherIntersectedObstacle)
        {
            var intersectionDetails = FindNearestIntersection(start, end);
            anotherIntersectedObstacle = intersectionDetails.Obstacle;
            return intersectionDetails.Obstacle == null;
        }

        private IntersectionDetails FindNearestIntersection(Vector2 start, Vector2 end)
        {
            IntersectionDetails nearestIntersection = IntersectionDetails.CreateEmpty();
            var segmentToEnd = Segment.Create(start, end);
            for (var i = 0; i < _obstacles.Count;i++)
            {
                var obstacle = _obstacles[i];
                IntersectionDetails thisObstactleIntersection;
                if (!obstacle.Inersects(segmentToEnd, out thisObstactleIntersection))
                {
                    continue;
                }
                if (nearestIntersection.Obstacle == null || thisObstactleIntersection.SquaredDistanceFromStart < nearestIntersection.SquaredDistanceFromStart)
                {
                    nearestIntersection = thisObstactleIntersection;
                }
            }
            return nearestIntersection;
        }

        private void AddNode(Node node)
        {
            if (_closedNodes.Any(node1 => node.Point.Equals(node1.Point)))
            {
                return; //TODO: и это хак
            }
            if (_openNodes.Any(node1 => node.Point.Equals(node1.Point)))
            {
                return; //TODO: и это хак
            }
            _openNodes.Add(node);
            _openNodes.Sort(SortNodes);
        }

        private int SortNodes(Node x, Node y)
        {
            return x.Heuristic < y.Heuristic ? -1 : 1;
        }

        private Node GetNextNode()
        {
            var result = _openNodes.FirstOrDefault();
            _openNodes.RemoveAt(0);
            return result;
        }
    }
}