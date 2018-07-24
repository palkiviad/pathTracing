using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathFinder.Release.Mengaziev.Dijkstra
{
    class Algorithm
    {
        public static bool find(Node[] graph, Node start, Node end, out List<Node> result)
        {
            result = new List<Node>();
            List<Node> observed = new List<Node>();
            List<Node> front = new List<Node> { start };

            while (true)
            {
                Node closedNode = null;
                {
                    float minVal = float.MaxValue;
                    foreach (var node in front)
                    {
                        if (node.Value < minVal)
                        {
                            minVal = node.Value;
                            closedNode = node;
                        }
                    }
                    if (closedNode == null)
                    {
                        return false;
                    }
                }

                observed.Add(closedNode);
                front.Remove(closedNode);
                foreach (var edge in closedNode.Edges)
                {
                    Node node = graph[edge.Key];
                    if (observed.Contains(node))
                    {
                        continue;
                    }

                    float value = closedNode.Value + edge.Value;
                    if (!front.Contains(node))
                    {
                        node.Value = value;
                        node.From = closedNode;
                        front.Add(node);
                    }
                    else if (node.Value > value)
                    {
                        node.Value = value;
                        node.From = closedNode;
                    }
                }

                if (closedNode == end)
                {
                    break;
                }
            }

            Node current = end;
            while (current != null)
            {
                result.Add(current);
                current = current.From;
            }
            result.Reverse();
            return true;
        }
    }
}
