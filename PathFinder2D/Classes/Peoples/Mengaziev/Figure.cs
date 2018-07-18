using PathFinder.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathFinder.Mengaziev
{
    class Figure
    {
        public Vector2[] Vertices { get; private set; }

        public Figure(Vector2[] data)
        {
            Vertices = data;
        }

        public int GetNextIndex(int index)
        {
            ++index;
            if (index == Vertices.Length)
            {
                return 0;
            }
            else
            {
                return index;
            }
        }

        public int GetPrevIndex(int index)
        {
            --index;
            if (index == -1)
            {
                return Vertices.Length - 1;
            }
            else
            {
                return index;
            }
        }

        public Vector2 GetEdgeNormalByIndex(int index)
        {
            Vector2 edge = Vertices[GetNextIndex(index)] - Vertices[index];
            edge.Normalize();
            return Vector2.Perpendicular(edge);
        }

    }
}
