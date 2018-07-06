using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using PathFinder.Mathematics;

namespace PathFinder.Editor {
    public sealed class InternalObstacle {

        private BeginMode drawMode; // У нас - либо LineLoop, либо  Polygon, либо Triangles
        private readonly Vector2[] vertices;
        private List<List<int>> indices;
        public InternalBox Bounds { get;}

        public Vector2[] Data {
            get {
                Vector2[] result = new Vector2[vertices.Length - 2];
                Array.Copy(vertices, 0, result, 0, result.Length);
                return result;
            }
        }

        public InternalObstacle(Vector2[] vertices) {
            if (vertices.Length < 3)
                throw new Exception("Vertices count of polygonal obstacle should be >2");

            // содомитство для удобства алгоритмов - повторим первую и вторую точку в конец вектора
            this.vertices = new Vector2[vertices.Length + 2];
            Array.Copy(vertices, 0, this.vertices, 0, vertices.Length);
            this.vertices[this.vertices.Length - 2] = vertices[0];
            this.vertices[this.vertices.Length - 1] = vertices[1];

            UpdateGeometry();
            Bounds = new InternalBox(vertices);
        }

        public bool Contains(Vector2 point) {

            Vector2 temp = Vector2.down;
            int intersectionsCount = 0;
            for (int i = 0; i < vertices.Length; i++) {
                Vector2 edgeStart = vertices[i];
                Vector2 edgeEnd = vertices[i+1 == vertices.Length  ? 0 : i+1];

               if(Vector2.SegmentToSegmentIntersection(point, new Vector2(1000,1000), edgeStart, edgeEnd, ref temp))
                intersectionsCount++;
            }

            return intersectionsCount % 2 == 1;
        }
        
        private bool HasSelfIntersections() {
            Vector2 temp = Vector2.zero;
            // Просто попарно перебираем все сегменты
            for (int i = 0; i < vertices.Length - 1; i++) {
                for (int j = i + 1; j < vertices.Length - 1; j++) {
                    if (Vector2.SegmentToSegmentIntersection(vertices[i], vertices[i + 1], vertices[j], vertices[j + 1], ref temp))
                        return true;
                }
            }

            return false;
        }

        private bool IsConvex() {
            // Фантастическая оптимизация
            if (vertices.Length < 4)
                return true;

            for (int i = 0; i < vertices.Length - 2; i++) {
                Vector2 a = vertices[i] - vertices[i + 1];
                Vector2 b = vertices[i + 2] - vertices[i + 1];

                float cross = a.x * b.y - a.y * b.x;

                if (cross < 0)
                    return false;
            }

            return true;
        }

        private void UpdateGeometry() {
            // Проверяем наличие самопересечений
            if (HasSelfIntersections())
                drawMode = BeginMode.LineLoop;
            else if (IsConvex())
                drawMode = BeginMode.Polygon;
            else {
                drawMode = BeginMode.Triangles;
                Triangulate();
            }
        }

        private int IndexOfFirstNonConvex(IList<int> ind) {
            for (int i = 0; i < ind.Count; i++) {
                int ind0 = ind[i - 1 < 0 ? ind.Count - 1 : i - 1];
                int ind1 = ind[i];
                int ind2 = ind[i + 1 >= ind.Count ? 0 : i + 1];

                Vector2 a = vertices[ind0] - vertices[ind1];
                Vector2 b = vertices[ind2] - vertices[ind1];

                float cross = a.x * b.y - a.y * b.x;

                if (cross < 0)
                    return i;
            }

            return -1;
        }

        private void Triangulate() {
            Vector2 temp = Vector2.zero;

            List<List<int>> tempIndices = new List<List<int>>();

            List<int> ind = new List<int>();
            for (int i = 0; i < vertices.Length - 2; i++)
                ind.Add(i);

            tempIndices.Add(ind);

            List<List<int>> resultIndices = new List<List<int>>();

            while (tempIndices.Count != 0) {
                ind = tempIndices.First();
                tempIndices.Remove(ind);

                int convexIndex = IndexOfFirstNonConvex(ind);
                if (convexIndex == -1)
                    continue;

                // из вешины строим ребро, не являющее существующим и не пересекающее существующие
                // перебираем вершины кроме convexIndex, convexIndex+1, convexIndex-1

                int curIndex = ind[convexIndex];

                int prevIndex = ind[convexIndex - 1 < 0 ? ind.Count - 1 : convexIndex - 1];
                int nextIndex = ind[convexIndex + 1 >= ind.Count ? 0 : convexIndex + 1];

                int indexOfOpposite = -1;
                foreach (var opIndex in ind) {
                    if (opIndex == curIndex || opIndex == prevIndex || opIndex == nextIndex)
                        continue;

                    bool hasIntersections = false;
                    
                    // Строим ребро и смотрим, есть ли пересечения с каким-либо из существующих рёбер контура.
                    for (int j = 0; j < ind.Count; j++) {
                        int nex = j + 1 >= ind.Count ? 0 : j + 1;
                        if (Vector2.SegmentToSegmentIntersection(vertices[ind[j]], vertices[ind[nex]], vertices[curIndex], vertices[opIndex], ref temp)) {
                            hasIntersections = true;
                            break;
                        }
                    }

                    if (hasIntersections)
                        continue;

                    indexOfOpposite = opIndex;
                    break;
                }

                if (indexOfOpposite == -1)
                    throw new Exception("Failed to triangulate");

                // разделяем tempVertices на два контура, первый идёт 
                List<int> firstContour = CreateContour(ind, curIndex, indexOfOpposite);

                if (IndexOfFirstNonConvex(firstContour) == -1) {
                    resultIndices.Add(firstContour);
                } else {
                    tempIndices.Add(firstContour);
                }

                List<int> secondContour = CreateContour(ind, indexOfOpposite, curIndex);
                if (IndexOfFirstNonConvex(secondContour) == -1) {
                    resultIndices.Add(secondContour);
                } else {
                    tempIndices.Add(secondContour);
                }
            }

            indices = resultIndices;
        }

        private static List<int> CreateContour(IList<int> contour, int startNodeIndex, int endNodeIndex) {
            
            List<int> result = new List<int>();
            int startIndex = contour.IndexOf(startNodeIndex);
            int endIndex = contour.IndexOf(endNodeIndex);
            int len = endIndex > startIndex ? endIndex - startIndex - 1 : endIndex + (contour.Count - startIndex) - 1;
            result.Add(startNodeIndex);
            for (int i = 0; i < len; i++) {
                startIndex++;
                int ind = startIndex >= contour.Count ? startIndex - contour.Count : startIndex;
                result.Add(contour[ind]);
            }

            result.Add(endNodeIndex);
            return result;
        }

        public void Draw() {
            if (drawMode == BeginMode.LineLoop) {
                GL.LineWidth(3);
                GL.Color3(1f, 0.0f, 0.0f);
                GL.Begin(BeginMode.LineLoop);
                for (int i = 0; i < vertices.Length - 2; i++)
                    GL.Vertex2(vertices[i].x, vertices[i].y);
                GL.End();
            } else if (drawMode == BeginMode.Polygon) {
                GL.Color3(0.5f, 0.5f, 0.5f);
                GL.Begin(BeginMode.Polygon);
                for (int i = 0; i < vertices.Length - 2; i++)
                    GL.Vertex2(vertices[i].x, vertices[i].y);
                GL.End();
            } else if (drawMode == BeginMode.Triangles) {
                GL.Color3(0.5f, 0.5f, 0.5f);

                foreach (var ind in indices) {
                    GL.Begin(BeginMode.Polygon);
                    foreach (var t in ind)
                        GL.Vertex2(vertices[t].x, vertices[t].y);
                    GL.End();
                }
            }
        }
    }
}