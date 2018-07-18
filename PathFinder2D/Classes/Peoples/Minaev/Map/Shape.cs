
using System;
using PathFinder.Mathematics;

namespace PathFinder.Minaev {

    class Shape
    {
        private Vector2[] points;
        public Vector2[] Points
        {
            get
            {
                return this.points;
            }
        }

        public Shape(Vector2[] points)
        {
            this.points = points;
        }

        public ShapeIntersection FindNearestIntersection(Vector2 fromPoint, Vector2 toPoint)
        {
            ShapeIntersection result = ShapeIntersection.EMPTY;
            Vector2 intersection = new Vector2(0, 0);
            int fromPointAsVrtexIdx = -1;
            for (int i=0; i < points.Length; i++)
            {
                // отлавливаем ситуацию, когда fromPoint - вершина многоугольника
                if (fromPointAsVrtexIdx < 0 && fromPoint.Equals(points[i]))
                {
                    fromPointAsVrtexIdx = i;
                }

                Vector2 nextRidPoint = GetRidNextPoint(i);
                if (Vector2.SegmentToSegmentIntersection(points[i], nextRidPoint, fromPoint, toPoint, ref intersection))
                {
                    if (result.isEmpty() ||
                        Vector2.SqrDistance(fromPoint, intersection) < Vector2.SqrDistance(fromPoint, result.IntersectionPoint))
                    {
                        result = new ShapeIntersection(this, intersection, i);
                        intersection = new Vector2(0, 0);
                    }
                }
            }

            // отлавливаем, что мы не попадаем внутрь многоугольника через его вершину
            if (!result.isEmpty() && fromPointAsVrtexIdx >=0)
            {
                int nextRidIdx = GetNextRidIdx(fromPointAsVrtexIdx);
                Vector2 ridVector = points[nextRidIdx] - fromPoint;
                Vector2 directionVector = result.IntersectionPoint - fromPoint;
                float angle = Vector2.SignedAngle(ridVector, directionVector);
                if (angle < 0)
                {
                    // заменяем на пересечение со следующим ребром многоугольника
                    result = new ShapeIntersection(this, points[nextRidIdx], nextRidIdx);
                }
                //Console.WriteLine("ridVector angle: " + Vector2.SignedAngle(ridVector, new Vector2(1, 0)));
                //Console.WriteLine("directionVector angle: " + Vector2.SignedAngle(directionVector, new Vector2(1, 0)));
                //Console.WriteLine("Point=" + fromPoint + ", angle=" + angle);
            }

            return result;
        }

        private int GetNextRidIdx(int ridIdx)
        {
            int nextRidIdx = ridIdx + 1;
            if (nextRidIdx == points.Length)
            {
                nextRidIdx = 0;
            }
            return nextRidIdx;
        }

        private Vector2 GetRidNextPoint(int ridIdx)
        {
            return points[GetNextRidIdx(ridIdx)];
        }
    }

    class ShapeIntersection
    {
        public static readonly ShapeIntersection EMPTY = new ShapeIntersection();

        private Shape shape;
        private Vector2 intersectionPoint;
        private int ridIndex;

        public ShapeIntersection()
        {

        }

        public ShapeIntersection(Shape shape, Vector2 intersectionPoint, int ridIndex)
        {
            this.shape = shape;
            this.intersectionPoint = intersectionPoint;
            this.ridIndex = ridIndex;
        }

        public Vector2 IntersectionPoint
        {
            get
            {
                return this.intersectionPoint;
            }
        }


        internal bool isEmpty()
        {
            return shape == null;
        }

        public Vector2 GetShapeRidLeftPoint()
        {
            int leftPointIdx = ridIndex + 1;
            if (shape.Points.Length == leftPointIdx)
            {
                return shape.Points[0];
            }
            else
            {
                return shape.Points[leftPointIdx];
            }
        }
    }
}
