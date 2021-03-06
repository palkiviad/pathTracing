﻿using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Editor {

    // AABB rectangle
    public sealed class InternalBox {

        public Vector2 LeftBottom { get; private set; }
        public Vector2 RightTop { get; private set; }

        public float W {
            get { return RightTop.x - LeftBottom.x; }
        }

        public float H {
            get { return RightTop.y - LeftBottom.y; }
        }

        public Vector2 Center {
            get { return (RightTop + LeftBottom) / 2.0f; }
        }

        public InternalBox() {
            LeftBottom = new Vector2(float.MaxValue, float.MaxValue);
            RightTop = new Vector2(float.MinValue, float.MinValue);
        }

        public InternalBox(Vector2 leftBottom, Vector2 rightTop) {
            LeftBottom = leftBottom;
            RightTop = rightTop;
        }

        // Проверка на то что сегмент [a,b] пересекает, или лежит внутри AABB. Границу не будем учитывать.
        public bool ContainOrIntersects(Vector2 a, Vector2 b) {
            if (ContainsWithoutBounds(a) || ContainsWithoutBounds(b))
                return true;

            // А дальше посмотрим пересечение 4х сегментов c указанным
            Vector2 temp = Vector2.zero;

            if (Vector2.SegmentToSegmentIntersection(a, b, new Vector2(LeftBottom.x, LeftBottom.y), new Vector2(LeftBottom.x, RightTop.y), ref temp))
                return true;

            if (Vector2.SegmentToSegmentIntersection(a, b, new Vector2(LeftBottom.x, LeftBottom.y), new Vector2(RightTop.x, RightTop.y), ref temp))
                return true;

            if (Vector2.SegmentToSegmentIntersection(a, b, new Vector2(RightTop.x, RightTop.y), new Vector2(RightTop.x, LeftBottom.y), ref temp))
                return true;

            if (Vector2.SegmentToSegmentIntersection(a, b, new Vector2(RightTop.x, LeftBottom.y), new Vector2(LeftBottom.x, LeftBottom.y), ref temp))
                return true;

            return false;
        }

        public bool ContainsWithoutBounds(Vector2 point) {
            return LeftBottom.x < point.x && RightTop.x > point.x &&
                   LeftBottom.y < point.y && RightTop.y > point.y;
        }

        public bool Contains(Vector2 point) {
            return LeftBottom.x <= point.x && RightTop.x >= point.x &&
                   LeftBottom.y <= point.y && RightTop.y >= point.y;
        }

        public InternalBox(IList<Vector2> vertices) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < vertices.Count; i++) {
                minX = Math.Min(vertices[i].x, minX);
                minY = Math.Min(vertices[i].y, minY);
                maxX = Math.Max(vertices[i].x, maxX);
                maxY = Math.Max(vertices[i].y, maxY);
            }

            LeftBottom = new Vector2(minX, minY);
            RightTop = new Vector2(maxX, maxY);
        }

        public void Add(InternalBox other) {
            LeftBottom = new Vector2(Math.Min(other.LeftBottom.x, LeftBottom.x), Math.Min(other.LeftBottom.y, LeftBottom.y));
            RightTop = new Vector2(Math.Max(other.RightTop.x, RightTop.x), Math.Max(other.RightTop.y, RightTop.y));
        }
    }
}