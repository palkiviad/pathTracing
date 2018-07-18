using System;
using OpenTK.Graphics.OpenGL;
using PathFinder.Mathematics;

namespace PathFinder.Editor {

    public enum IntersectionType {
        None,

        Inside,

        FirstVertex,
        SecondVertex,
        ThirdVertex,

        // Это просто значит что точка на прямой лежит, на прямой, не на сегменте!
        FirstStraight,
        SecondStraight,
        ThirdStraight
    }


    public class InternalTriangle {
        private readonly Vector2 v1;
        private readonly Vector2 v2;
        private readonly Vector2 v3;

        // Признак, является ли грань внутренней
        private readonly bool internal1; //[v1, v2]
        private readonly bool internal2; //[v2, v3]
        private readonly bool internal3; //[v3, v1]

        public InternalTriangle(Vector2 v1, Vector2 v2, Vector2 v3, bool internal1, bool internal2, bool internal3) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            this.internal1 = internal1;
            this.internal2 = internal2;
            this.internal3 = internal3;
        }

        public static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) {
            Vector2 a = (p1 - p3).normalized;
            Vector2 b = (p2 - p3).normalized;

            return a.x * b.y - a.y * b.x;
        }

        public bool PointOnSegment(Vector2 a, Vector2 v1, Vector2 v2) {
            // Просто проверим пересечение проекций, выберем наибольшую проекцию
            float minProj;
            float maxProj;

            float posA;


            if (Math.Abs(v1.x - v3.x) < 1e-4) {
                // Мы вырождены по x
                if (v1.y > v3.y) {
                    maxProj = v1.y;
                    minProj = v3.y;
                } else {
                    minProj = v1.y;
                    maxProj = v3.y;
                }

                posA = a.y;

            } else {
                // Мы вырождены по y
                if (v1.x > v3.x) {
                    maxProj = v1.x;
                    minProj = v3.x;
                } else {
                    minProj = v1.x;
                    maxProj = v3.x;
                }

                posA = a.x;
            }

            return maxProj >= posA && minProj <= posA;
        }

        public bool SegmentsOverlap(Vector2 a, Vector2 b, Vector2 v1, Vector2 v2) {
            // Просто проверим пересечение проекций, выберем наибольшую проекцию
            float minProj;
            float maxProj;
            float maxPos;
            float minPos;

            if (Math.Abs(v1.x - v3.x) < 1e-4) {
                // Мы вырождены по x
                if (v1.y > v3.y) {
                    maxProj = v1.y;
                    minProj = v3.y;
                } else {
                    minProj = v1.y;
                    maxProj = v3.y;
                }
                
                if (a.y > b.y) {
                    maxPos = a.y;
                    minPos = b.y;
                } else {
                    minPos = a.y;
                    maxPos = b.y;
                }
            } else {
                // Мы вырождены по y
                if (v1.x > v3.x) {
                    maxProj = v1.x;
                    minProj = v3.x;
                } else {
                    minProj = v1.x;
                    maxProj = v3.x;
                }

                if (a.x > b.x) {
                    maxPos = a.x;
                    minPos = b.x;
                } else {
                    minPos = a.x;
                    maxPos = b.x;
                }
            }

            return maxPos >= minProj && maxProj >= minPos;
        }
        
        // Здесь вариантов немного 
        // Сегмент внутри (как минимум 1 точка полностью внутри)
        // Сегмент частично внутри - начинается/заканчивается в вершине/на грани 
        // Сегмент снаружи
        // Сегмент совпадает с внутренней гранью, включает её, включен в неё, частично перекрывает её
        public bool TriangleSegmentIntersection(Vector2 a, Vector2 b) {
            IntersectionType aType = TrianglePointIntersection(a);
            IntersectionType bType = TrianglePointIntersection(b);

            // Сегмент внутри
            if (aType == IntersectionType.Inside || bType == IntersectionType.Inside)
                return true;

            // Сегмент снаружи
            if (aType == IntersectionType.None && bType == IntersectionType.None)
                return false;
            
            // Есть полное совпадение с гранью
            if (aType == IntersectionType.FirstVertex && bType == IntersectionType.SecondVertex)
                return internal1;
            
            if (aType == IntersectionType.SecondVertex && bType == IntersectionType.ThirdVertex)
                return internal2;
            
            if (aType == IntersectionType.ThirdVertex && bType == IntersectionType.FirstVertex)
                return internal3;
            
            if (aType == IntersectionType.None) {
                if (bType == IntersectionType.FirstStraight)
                    return internal1;

                if (bType == IntersectionType.SecondStraight)
                    return internal2;
                
                if (bType == IntersectionType.ThirdStraight)
                    return internal3;
            } 
            
            if (bType == IntersectionType.None) {
                if (aType == IntersectionType.FirstStraight)
                    return internal1;

                if (aType == IntersectionType.SecondStraight)
                    return internal2;
                
                if (aType == IntersectionType.ThirdStraight)
                    return internal3;
            }
            

            
            // любой степени совпадение с одним ребром допустимо если оно внешнее и недопустимое, если внутреннее
            if (aType == IntersectionType.FirstStraight && bType == IntersectionType.FirstStraight ||
                aType == IntersectionType.FirstVertex && bType == IntersectionType.FirstStraight ||
                aType == IntersectionType.FirstStraight && bType == IntersectionType.FirstVertex ||
                aType == IntersectionType.SecondVertex && bType == IntersectionType.FirstStraight ||
                aType == IntersectionType.FirstStraight && bType == IntersectionType.SecondVertex) {

                if (SegmentsOverlap(a, b, v1, v2))
                    return internal1;
                 return false;
            }
            
            if (aType == IntersectionType.SecondStraight && bType == IntersectionType.SecondStraight ||
                aType == IntersectionType.SecondVertex && bType == IntersectionType.SecondStraight ||
                aType == IntersectionType.SecondStraight && bType == IntersectionType.SecondVertex ||
                aType == IntersectionType.ThirdVertex && bType == IntersectionType.SecondStraight ||
                aType == IntersectionType.SecondStraight && bType == IntersectionType.ThirdVertex) {

                if (SegmentsOverlap(a, b, v2, v3))
                    return internal2;
                return false;
            }
            
            
            if (aType == IntersectionType.ThirdStraight && bType == IntersectionType.ThirdStraight ||
                aType == IntersectionType.FirstVertex && bType == IntersectionType.ThirdStraight ||
                aType == IntersectionType.ThirdStraight && bType == IntersectionType.FirstVertex ||
                aType == IntersectionType.ThirdVertex && bType == IntersectionType.ThirdStraight ||
                aType == IntersectionType.ThirdStraight && bType == IntersectionType.ThirdVertex) {

                if (SegmentsOverlap(a, b, v3, v1))
                    return internal3;
                return false;
            }

            // Самый грубый случай - просто пересечение ребра
            Vector2 temp = Vector2.zero;
            if (Vector2.SegmentToSegmentIntersectionWeak(a, b, v1, v2, ref temp) ||
                Vector2.SegmentToSegmentIntersectionWeak(a, b, v2, v3, ref temp) ||
                Vector2.SegmentToSegmentIntersectionWeak(a, b, v3, v1, ref temp)
            ) {
                return true;
            }

            return false;
        }

        public IntersectionType TrianglePointIntersection(Vector2 pt) {
            const float eps = 1e-4f;
            // Этот сыр бор нам нужен чтобы лишь с определённой точностью определять пересечение границы
            float b1 = Sign(pt, v1, v2);
            if (Math.Abs(b1) < eps)
                b1 = 0;

            float b2 = Sign(pt, v2, v3);
            if (Math.Abs(b2) < eps)
                b2 = 0;

            float b3 = Sign(pt, v3, v1);
            if (Math.Abs(b3) < eps)
                b3 = 0;

            if (b1 == 0 && b2 == 0)
                return IntersectionType.SecondVertex;
            if (b1 == 0 && b3 == 0)
                return IntersectionType.FirstVertex;
            if (b2 == 0 && b3 == 0)
                return IntersectionType.ThirdVertex;
            if (b1 == 0)
                return IntersectionType.FirstStraight;
            if (b2 == 0)
                return IntersectionType.SecondStraight;
            if (b3 == 0) {
                return IntersectionType.ThirdStraight;
            }

            return b1 < 0 == b2 < 0 && b2 < 0 == b3 < 0 ? IntersectionType.Inside : IntersectionType.None;
        }

        public void Draw() {
            GL.Color3(0.5f, 0.5f, 0.5f);
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex2(v1.x, v1.y);
            GL.Vertex2(v2.x, v2.y);
            GL.Vertex2(v3.x, v3.y);
            GL.End();

            GL.LineWidth(3);
            GL.Color3(0f, 0.3f, 0.0f);

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(v1.x, v1.y);
            GL.Vertex2(v2.x, v2.y);
            GL.Vertex2(v2.x, v2.y);
            GL.Vertex2(v3.x, v3.y);
            GL.Vertex2(v3.x, v3.y);
            GL.Vertex2(v1.x, v1.y);
            GL.End();
        }
    }
}