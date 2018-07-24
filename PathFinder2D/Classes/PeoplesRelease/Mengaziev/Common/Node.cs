using PathFinder.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathFinder.Release.Mengaziev
{
    class Node
    {
        public int Index { get; set; }
        public Vector2 Point { get; set; }
        public bool Intruded { get; set; }
        public Dictionary<int, float> Edges { get; set; }

        public float Value { get; set; }
        public float StraightDistanceToEndPoint { get; set; }
        public Node From { get; set; }

        public Node(int index, Vector2 point)
        {
            Index = index;
            Point = point;
            Intruded = false;
            Edges = new Dictionary<int, float>();
            Value = 0;
            StraightDistanceToEndPoint = 0;
            From = null;
        }


        public void AddEdge(int toIndex, float weight)
        {
            Edges.Add(toIndex, weight);
        }


        public Node Copy()
        {
            return new Node(Index, Point, Intruded, Edges);
        }

        private Node(int index, Vector2 point, bool intruded, Dictionary<int, float> edges)
        {
            Index = index;
            Point = point;
            Intruded = intruded;
            Edges = new Dictionary<int, float>(edges);
            Value = 0;
            StraightDistanceToEndPoint = 0;
            From = null;
        }

    }
}
