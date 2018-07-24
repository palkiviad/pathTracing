using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Release.Matusevich {
    public class Map : IMap {
        private class DataForRecursion : IComparable<DataForRecursion> {
            public Node currentNode;
            public Obstacle foundObstacle;
            public float Heuristic;

            public int CompareTo(DataForRecursion other) {
                return Heuristic < other.Heuristic ? -1 : 1;
            }
        }


        private readonly ObstaclesCollection _obstacles;
        private readonly SortedNodesHeap _openNodes = new SortedNodesHeap();
        private readonly HashSet<Node> _closedNodes = new HashSet<Node>();
        private readonly Heap<DataForRecursion> _dataForRecursion = new Heap<DataForRecursion>(HeapType.MinHeap);
        private Vector2 _end;


        public Map() {
            _obstacles = new ObstaclesCollection();
        }

        public void Init(Vector2[][] obstacles) {
            _obstacles.Load(obstacles);
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            var temp = start;
            start = end;
            end = temp;
            _openNodes.Clear();
            _closedNodes.Clear();
            _dataForRecursion.Clear();
            _obstacles.Prepare();
            _end = end;

            Node currentNode = Node.CreateStartNode(start, end);
            int safeRecursionCount = 0;
            while (!currentNode.Point.Equals(end)) {
                if (safeRecursionCount > 100000) {
                    //TODO а это защита от вечного цикла, который у меня есть
                    //Console.WriteLine("Aborting long cycle");
                    return new List<Vector2> {start, end};
                }
                FindNextNodes(currentNode);

                int anotherSafeRecursionCount = 0;
                while (_openNodes.Empty() && !_dataForRecursion.Empty()) {
                    anotherSafeRecursionCount++;
                    var nextData = _dataForRecursion.PopRoot();
                    AddReachableVerticesFromObstacle(nextData);
                    if (anotherSafeRecursionCount > 100000) {
                        //TODO а это защита от вечного цикла, который у меня есть
                       // Console.WriteLine("Aborting another long cycle");
                        return new List<Vector2> {start, end};
                    }
                }
                if (_openNodes.Empty()) {
                  //  Console.WriteLine("no path");
                    return new List<Vector2> {start, end};
                }
                _closedNodes.Add(currentNode);
                currentNode = _openNodes.Shift();
                safeRecursionCount++;
            }
            return currentNode.GetPath();
        }

        private void FindNextNodes(Node currentNode) {
            var foundObstacle = FindFirstIntersection(currentNode.Point, _end);
            if (foundObstacle == null) {
                AddNode(currentNode.CreateChild(_end, _end));
                return;
            }

            if (foundObstacle.WasThrowedInThisPathSearch(currentNode.Point)) {
                return;
            }
            AddDataForRecursion(currentNode, foundObstacle);
        }

        private void AddDataForRecursion(Node currentNode, Obstacle foundObstacle) {
            foundObstacle.AddThrowedInThisPathSearch(currentNode.Point);
            var data = new DataForRecursion();
            data.currentNode = currentNode;
            data.foundObstacle = foundObstacle;
            var center = foundObstacle.GetCenter();
            data.Heuristic = currentNode.TravelledDistance + Node.Distance(currentNode.Point, center) + MapConstants.NearestDistanceHeuristicValue * Node.Distance(center, _end);
            //data.Heuristic = currentNode.Heuristic - currentNode.TravelledDistance;
            //data.Heuristic = currentNode.Heuristic;
            _dataForRecursion.Insert(data);
        }

        private void AddReachableVerticesFromObstacle(DataForRecursion data) {
            var currentNode = data.currentNode;
            var end = _end;
            var foundObstacle = data.foundObstacle;

            for (var i = 0; i < foundObstacle.VerticesCount; i++) {
                var obstacleVertice = foundObstacle.GetVerticeAt(i);
                //if (_openNodes.Find(obstacleVertice) != null)
                //{
                //    continue; // а это хак?
                //}

                if (obstacleVertice.Equals(currentNode.Point)) {
                    continue; //TODO: и вот это хак
                }
                //var existingClosedNode = _closedNodes.FirstOrDefault(anotherNode => anotherNode.Point.Equals(obstacleVertice));
                //if (existingClosedNode != null)
                //{
                //    _debug.ClosedNode();
                //    continue; //TODO: и это хак, но это хороший хак, без него сломается
                //}

                Obstacle anotherIntersectedObstacle = FindFirstIntersection(currentNode.Point, obstacleVertice);
                if (anotherIntersectedObstacle == null) {
                    AddNode(currentNode.CreateChild(obstacleVertice, end));
                    continue;
                }
                if (anotherIntersectedObstacle == foundObstacle) {
                    continue; //TODO: и это хак
                }

                if (anotherIntersectedObstacle.WasThrowedInThisPathSearch(currentNode.Point)) {
                    continue;
                }
                AddDataForRecursion(currentNode, anotherIntersectedObstacle);
            }
        }

        private Obstacle FindFirstIntersection(Vector2 start, Vector2 end) {
            return _obstacles.FindFirstIntersection(start, end);
        }


        private void AddNode(Node node) {
            //TODO: и closedNodes - это в текущем виде хак, который мешает находить правильный путь
            var existingClosedNode = _closedNodes.FirstOrDefault(anotherNode => anotherNode.Point.Equals(node.Point));
            if (existingClosedNode != null) {
                return;
            }
            var existingNode = _openNodes.Find(node.Point);
            if (existingNode != null) {
                if (node.TravelledDistance > existingNode.TravelledDistance) {
                    return;
                }
                _openNodes.UpdateNode(existingNode, node);
            } else {
                _openNodes.Add(node);
            }
        }
    }
}