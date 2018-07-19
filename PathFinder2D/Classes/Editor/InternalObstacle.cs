using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using PathFinder.Mathematics;

namespace PathFinder.Editor {
    public sealed class InternalObstacle {

        private BeginMode drawMode; // У нас - либо LineLoop, либо  Polygon, либо Triangles

        // Вершины контура + 2 добавочные (в конец добавляем первую и вторую точку)
        private readonly Vector2[] vertices;

        // Результат триангуляции - тройки вершин. Прелесть в том что мы точно знаем что они все лежат на контуре и что ребро между 0 и 2 вершиной - внутреннее
        private InternalTriangle[] triangles;

        // Индексы треугольников после триангуляции.
      //  private int[] indices;

        public InternalBox Bounds { get; private set; }

        public bool Initialized {
            get { return triangles != null; }
        }

        public Vector2[] Data {
            get {
                Vector2[] result = new Vector2[vertices.Length - 2];
                Array.Copy(vertices, 0, result, 0, result.Length);
                return result;
            }
        }

        // Просто восхитительный алгоритм определения направления контура!
        // https://ru.wikipedia.org/wiki/Формула_площади_Гаусса 
        private bool CV() {

            float sum = 0;
            for (int i = 0; i < vertices.Length - 2; i++)
                sum += vertices[i].x * vertices[i + 1].y - vertices[i + 1].x * vertices[i].y;

            if(sum >=0)
                Console.WriteLine("Wrong contour direction! Should be CV!");
            
            return sum < 0;
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

        // Это простенькая проверка и в случае её прохождения, дальше можно сегмент не проверять, он отличный.
        private bool AllContourPointsOnOneSide(Vector2 point, Vector2 normal) {
            bool first = true;
            bool left = false;

            for (int i = 0; i < vertices.Length - 2; i++) {
                Vector2 a = vertices[i] - point;

                if (first) {
                    left = Vector2.Dot(a, normal) >= 0; //a.x * normal.y - a.y * normal.x >= 0;
                    first = false;
                } else {
                    if (left != Vector2.Dot(a, normal) >= 0)
                        return false;
                }
            }

            return true;
        }


        public bool Intersects(IEnumerable<Vector2> path) {
            // Сначала проверим дешёвое пересечение AABB с этим путём

            bool intersectsWithAABB = false;
            bool first = true;
            Vector2 prev = Vector2.zero;


            foreach (Vector2 point in path) {
                if (first) {
                    first = false;
                    prev = point;
                }

                if (Bounds.ContainOrIntersects(prev, point)) {
                    intersectsWithAABB = true;
                    break;
                }
            }

            // Временно выключим
            //     if (!intersectsWithAABB)
            //        return false;

          //  Vector2 temp = Vector2.down;
            // Раз мы здесь, значит нет пересечения с AABB и надо уже проверять умное пересечение с гранями
            first = true;
            prev = Vector2.zero;

            foreach (Vector2 point in path) {
                if (first) {
                    first = false;
                    prev = point;
                    continue;
                }

                // у нас есть сегмент пути [prev, point] 
                // получим его нормаль и посмотрим что все точки контура с одной из сторон от этого сегмента
                if (AllContourPointsOnOneSide(prev, Vector2.Perpendicular(point - prev))) {
                    prev = point;
                    continue;
                }

                // Далее остаётся немного вариантов - и совпадение с внутренним ребром один из них 
                for (int i = 0; i < triangles.Length; i++)
                    if (triangles[i].TriangleSegmentIntersection(prev, point))
                        return true;
                
                prev = point;
            }

            return false;
        }

        public bool Contains(Vector2 point) {

            for (int i = 0; i < triangles.Length; i++) {
                if (triangles[i].TrianglePointIntersection(point) == IntersectionType.Inside)
                    return true;
            }

            return false;
        }

        private bool HasSelfIntersections() {
            Vector2 temp = Vector2.zero;
            // Просто попарно перебираем все сегменты
            for (int i = 0; i < vertices.Length - 1; i++) {
                for (int j = i + 1; j < vertices.Length - 1; j++) {
                    if (Vector2.SegmentToSegmentIntersection(vertices[i], vertices[i + 1], vertices[j], vertices[j + 1], ref temp)) {
                        Console.WriteLine("Error! Contour has self intersections!");
                        return true;
                    }
                }
            }

            return false;
        }

        private bool Convex() {
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
            if (HasSelfIntersections() || !CV()) {
                drawMode = BeginMode.LineLoop;
            } else {
                Triangulate();
                drawMode = BeginMode.Triangles;
            }
        }

//        private int IndexOfFirstNonConvex(IList<int> ind) {
//            for (int i = 0; i < ind.Count; i++) {
//                int ind0 = ind[i - 1 < 0 ? ind.Count - 1 : i - 1];
//                int ind1 = ind[i];
//                int ind2 = ind[i + 1 >= ind.Count ? 0 : i + 1];
//
//                Vector2 a = vertices[ind0] - vertices[ind1];
//                Vector2 b = vertices[ind2] - vertices[ind1];
//
//                float cross = a.x * b.y - a.y * b.x;
//
//                if (cross < 0)
//                    return i;
//            }
//
//            return -1;
//        }
//
//        // Возвращает индекс результата в переданном массиве индексов.
//        private int IndexOfFirstConvex(IList<int> ind) {
//            for (int i = 0; i < ind.Count; i++) {
//                int ind0 = ind[i - 1 < 0 ? ind.Count - 1 : i - 1];
//                int ind1 = ind[i];
//                int ind2 = ind[i + 1 >= ind.Count ? 0 : i + 1];
//
//                Vector2 a = vertices[ind0] - vertices[ind1];
//                Vector2 b = vertices[ind2] - vertices[ind1];
//
//                float cross = a.x * b.y - a.y * b.x;
//
//                if (cross >= 0)
//                    return i;
//            }
//
//            return -1;
//        }

        private bool Convex(int prev, int cur, int next) {
            Vector2 a = vertices[prev] - vertices[cur];
            Vector2 b = vertices[next] - vertices[cur];

            return a.x * b.y - a.y * b.x >= 0;
        }



  



        // Внутри! Не на границе!
//        private bool PointInTriangles(Vector2 point) {
//            for (int i = 0; i < triangles.Length; i += 3) {
//                if (PointInTriangle(point, triangles[i], triangles[i + 1], triangles[i + 2]))
//                    return true;
//            }
//
//            return false;
//        }

        private bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
            var b1 = InternalTriangle.Sign(pt, v1, v2) < 0.0f;
            var b2 = InternalTriangle.Sign(pt, v2, v3) < 0.0f;
            var b3 = InternalTriangle.Sign(pt, v3, v1) < 0.0f;

            return b1 == b2 && b2 == b3;
        }
        
        private void Triangulate() {
            // Начальный список всех индексов
            List<int> ind = new List<int>();
            for (int i = 0; i < vertices.Length - 2; i++)
                ind.Add(i);

            int indicesPosition = 0;
        //    int[] resultIndices = new int[(vertices.Length - 4) * 3];
            InternalTriangle[] resultTriangles = new InternalTriangle[(vertices.Length - 4)];

            while (ind.Count != 3) {
                bool found = false;

                int prevCount = ind.Count;
                // Нам надо выпуклую найти вершину, для которой две её товарки дают ребро не имеющее пересечений с остальными рёбрами 
                for (int j = 0; j < ind.Count; j++) {
                    int curIndex = ind[j];
                    int prevIndex = j - 1 < 0 ? ind[ind.Count - 1] : ind[j - 1];
                    int nextIndex = j + 1 >= ind.Count ? ind[0] : ind[j + 1];

                    if (!Convex(prevIndex, curIndex, nextIndex))
                   //if (!Convex(nextIndex, curIndex, prevIndex))
                        continue;

                    // Проверяем что ни одна из точек не попала внутрь этого треугольника
                    for (int i = 0; i < ind.Count; i++) {
                        if (ind[i] == curIndex || ind[i] == prevIndex || ind[i] == nextIndex)
                            continue;

                        if (PointInTriangle(vertices[ind[i]], vertices[prevIndex], vertices[curIndex], vertices[nextIndex])) {
                            found = true;
                            break;
                        }
                    }

                    if (found) {
                        found = false;
                        continue;
                    }
                   
                    // Вершины нужно добавлять хитро - чтобы первые две грани были внешними, а последняя - 
                    resultTriangles[indicesPosition] = new InternalTriangle(
                        vertices[prevIndex], 
                        vertices[curIndex], 
                        vertices[nextIndex],
                        !IsExternalEdge(prevIndex, curIndex),
                        !IsExternalEdge(curIndex, nextIndex),
                        !IsExternalEdge(nextIndex, prevIndex)
                        );

                    indicesPosition ++;

                    // её удаляем, а двух её товарок записываем в список вместе с ней
                    ind.RemoveAt(j);
                    break;
                }

                if (prevCount == ind.Count)
                    throw new Exception("Something wrong with contour or algorithm.");
            }

            // И последняя порция
//            resultIndices[indicesPosition] = ind[0];
//            resultIndices[indicesPosition + 1] = ind[1];
//            resultIndices[indicesPosition + 2] = ind[2];

            // Здесь важно не сделать огромной ошибки, нам нужно, чтобы 
            resultTriangles[indicesPosition] = new InternalTriangle(
                vertices[ind[0]],
                vertices[ind[1]],
                vertices[ind[2]],
                !IsExternalEdge(ind[0], ind[1]),
                !IsExternalEdge(ind[1], ind[2]),
                !IsExternalEdge(ind[2], ind[0]));

          //  indices = resultIndices;
            triangles = resultTriangles;
        }

        private bool IsExternalEdge(int index1, int index2) {
            if (index1 == 0 && index2 == vertices.Length - 3 ||
                index1 == vertices.Length - 3 && index2 == 0)
                return true;

            return Math.Abs(index1 - index2) == 1;
        }

        public void Draw() {
            if (drawMode == BeginMode.LineLoop) {
                // Этот режим выбирается, если у нас контур самопересекающийся, или вообще какой-то неправильный.
                GL.LineWidth(3);
                GL.Color3(1f, 0.0f, 0.0f);
                GL.Begin(PrimitiveType.LineLoop);
                for (int i = 0; i < vertices.Length - 2; i++)
                    GL.Vertex2(vertices[i].x, vertices[i].y);
                GL.End();
            } else if (drawMode == BeginMode.Polygon) {
              
                GL.Color3(0.5f, 0.5f, 0.5f);
                GL.Begin(PrimitiveType.Polygon);
                for (int i = 0; i < vertices.Length - 2; i++)
                    GL.Vertex2(vertices[i].x, vertices[i].y);
                GL.End();
            } else if (drawMode == BeginMode.Triangles) {
                for (int i = 0; i < triangles.Length; i++)
                    triangles[i].Draw();
            }
        }
    }
}