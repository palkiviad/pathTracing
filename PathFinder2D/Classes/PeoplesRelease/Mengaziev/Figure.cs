using PathFinder.Mathematics;
using PathFinder.Release.Mengaziev.Dijkstra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathFinder.Release.Mengaziev
{

    class Figure
    {
        public Node[] Vertices { get; set; }
        public Vector2[] UnnormalNormales { get; set; }

        public Figure(Node[] vertices)
        {
            Vertices = vertices;
            UnnormalNormales = new Vector2[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                Node nextVertex = getNextVertexByIndex(i);
                Node curVertex = Vertices[i];
                Node prevVertex = getPrevVertexByIndex(i);

                UnnormalNormales[i] = Vector2.Perpendicular((nextVertex.Point - curVertex.Point));
                curVertex.Intruded = Vector2.SignedAngle(curVertex.Point - prevVertex.Point, nextVertex.Point - prevVertex.Point) > 0;
            }
        }

        public Node getNextVertexByIndex(int index)
        {
            if (index < 0 || index >= Vertices.Length)
            {
                throw new IndexOutOfRangeException("Index is " + index);
            }
            return Vertices[index == Vertices.Length - 1 ? 0 : index + 1];
        }

        public Node getPrevVertexByIndex(int index)
        {
            if (index < 0 || index >= Vertices.Length)
            {
                throw new IndexOutOfRangeException("Index is " + index);
            }
            return Vertices[index == 0 ? Vertices.Length - 1 : index - 1];
        }

    }
}
