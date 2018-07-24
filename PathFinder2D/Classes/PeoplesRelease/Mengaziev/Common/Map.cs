using System.Collections.Generic;
using PathFinder.Mathematics;


namespace PathFinder.Release.Mengaziev
{
    class Map
    {
        private Figure[] figures;
        private Node[] nodes;
        private Node[] graph;
        List<Vector2> debug = new List<Vector2>();
        private bool useAStar;
        private bool skipIntrude;

        public Map(bool useAStar, bool skipIntrude)
        {
            this.useAStar = useAStar;
            this.skipIntrude = skipIntrude;
        }


        public void Init(Vector2[][] obstacles)
        {
            PrepareFigures(obstacles);
            CreateFigurePaths();
        }

        private void PrepareFigures(Vector2[][] obstacles)
        {
            figures = new Figure[obstacles.Length];
            int count = 0;
            foreach (var figure in obstacles)
            {
                count += figure.Length;
            }
            nodes = new Node[count];
            graph = new Node[count + 2];

            count = 0;
            for (int figIndex = 0; figIndex < obstacles.Length; figIndex++)
            {
                Node[] verticies = new Node[obstacles[figIndex].Length];
                for (int vertIndex = 0; vertIndex < obstacles[figIndex].Length; vertIndex++)
                {
                    var node = new Node(count, obstacles[figIndex][vertIndex]);
                    nodes[count] = node;
                    verticies[vertIndex] = node;
                    ++count;
                }
                figures[figIndex] = new Figure(verticies);
            }
        }

        private void CreateFigurePaths()
        {
            for (int figIndex = 0; figIndex < figures.Length; figIndex++)
            {
                int maxVertIndex = figures[figIndex].Vertices.Length;
                for (int vertIndex = 0; vertIndex < maxVertIndex; vertIndex++)
                {
                    CreatePathsFromVertex(figIndex, vertIndex);
                }
            }
        }

        private void CreatePathsFromVertex(int fromFigIndex, int fromVertIndex)
        {
            Figure fromFigure = figures[fromFigIndex];
            Node fromVertex = figures[fromFigIndex].Vertices[fromVertIndex];
            if (skipIntrude && fromVertex.Intruded)
            {
                return;
            }
            int prevIndex = fromVertIndex == 0 ? fromFigure.Vertices.Length - 1 : fromVertIndex - 1;
            Vector2 normal1 = fromFigure.UnnormalNormales[prevIndex];
            Vector2 normal2 = fromFigure.UnnormalNormales[fromVertIndex];

            for (int figIndex = fromFigIndex; figIndex < figures.Length; figIndex++)
            {
                Figure figure = figures[figIndex];
                for (int vertIndex = figIndex == fromFigIndex ? fromVertIndex : 0; vertIndex < figure.Vertices.Length; vertIndex++)
                {
                    Node vertex = figure.Vertices[vertIndex];
                    if (vertex == fromVertex)
                    {
                        continue;
                    }
                    if (skipIntrude && vertex.Intruded)
                    {
                        continue;
                    }

                    Vector2 vector = vertex.Point - fromVertex.Point;
                    if (fromVertex.Intruded)
                    {
                        if (Vector2.Angle(vector, normal1) <= 90 && Vector2.Angle(vector, normal2) <= 90)
                        {
                            if (!IsIntersected(fromVertex.Point, vertex.Point))
                            {
                                float weight = AddEdgeWithWeight(fromVertex, vertex);
                                vertex.AddEdge(fromVertex.Index, weight);
                            }
                        }
                    }
                    else
                    {
                        if (Vector2.Angle(vector, normal1) <= 90 || Vector2.Angle(vector, normal2) <= 90)
                        {
                            if (!IsIntersected(fromVertex.Point, vertex.Point))
                            {
                                float weight = AddEdgeWithWeight(fromVertex, vertex);
                                vertex.AddEdge(fromVertex.Index, weight);
                            }
                        }
                    }
                }
            }
        }


        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            CreateGraph(start, end);

            //DebugPoints();

            List<Vector2> result = new List<Vector2>();
            List<Node> path;

            if (useAStar)
            {
                if (AStar.Algorithm.find(graph, graph[graph.Length - 2], graph[graph.Length - 1], out path))
                {
                    foreach (var node in path)
                    {
                        result.Add(node.Point);
                    }
                }
            }
            else
            {
                if (Dijkstra.Algorithm.find(graph, graph[graph.Length - 2], graph[graph.Length - 1], out path))
                {
                    foreach (var node in path)
                    {
                        result.Add(node.Point);
                    }
                }
            }

            return result;
        }

        private void CreateGraph(Vector2 start, Vector2 end)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                graph[i] = nodes[i].Copy();
            }

            Node startNode = new Node(nodes.Length, start);
            Node endNode = new Node(nodes.Length + 1, end);
            graph[graph.Length - 2] = startNode;
            graph[graph.Length - 1] = endNode;
            AddEdgesToPointNode(startNode);
            AddEdgesToPointNode(endNode);
            if (!IsIntersected(startNode.Point, endNode.Point))
            {
                float weight = AddEdgeWithWeight(startNode, endNode);
                endNode.AddEdge(startNode.Index, weight);
            }
        }


        private void AddEdgesToPointNode(Node pointNode)
        {
            foreach (var figure in figures)
            {
                foreach (var vertex in figure.Vertices)
                {
                    if (skipIntrude && vertex.Intruded)
                    {
                        continue;
                    }

                    if (!IsIntersected(pointNode.Point, vertex.Point))
                    {
                        Node to = graph[vertex.Index];
                        float weight = AddEdgeWithWeight(pointNode, to);
                        to.AddEdge(pointNode.Index, weight);
                    }
                }
            }
        }

        private bool IsIntersected(Vector2 from, Vector2 to)
        {
            Vector2 useless = Vector2.zero;
            foreach (var figure in figures)
            {
                for (int i = 0; i < figure.Vertices.Length; i++)
                {
                    if (Vector2.SegmentToSegmentIntersection(from, to, figure.Vertices[i].Point, figure.getNextVertexByIndex(i).Point, ref useless))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private float AddEdgeWithWeight(Node from, Node to)
        {
            float weight = Vector2.Distance(from.Point, to.Point);
            from.AddEdge(to.Index, weight);
            return weight;
        }



        private void DebugPoints()
        {
            debug.Clear();
            foreach (var node in graph)
            {
                foreach (var edge in node.Edges)
                {
                    debug.Add(node.Point);
                    debug.Add(graph[edge.Key].Point);
                }
            }
        }

        public IEnumerable<Vector2> GetDebug()
        {
            return debug;
        }


    }
}