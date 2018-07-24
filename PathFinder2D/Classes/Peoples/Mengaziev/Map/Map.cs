using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Mengaziev
{

    public sealed class Map : IMap
    {

        private readonly float margin = 1.0f;

        List<Figure> allFigures = new List<Figure>();

        public void Init(Vector2[][] obstacles)
        {
            allFigures.Clear();
            foreach (var obstacle in obstacles)
            {
                allFigures.Add(new Figure(obstacle));
            }
        }


        enum Action
        {
            LookingEndPoint, MoveAlongFigureEdge, MoveFigureCorner, DoNothing
        }

        enum Direction
        {
            Forward, Backward
        }


        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            var result = new List<Vector2>();
            result.Add(start);

            int count = 0;
            Action action = Action.LookingEndPoint;
            Vector2 currentPoint = start;
            int intersectedFigureEdgeIndex = 0;
            Figure intersectedFigure = null;
            Direction direction = Direction.Forward;

            System.Random rnd = new System.Random(123);

            while (++count < 1000)
            {
                switch (action)
                {
                    case Action.LookingEndPoint:
                        {
                            Vector2 intersectPoint;
                            Figure newIntersectedFigure;
                            int newIntersectedFigureEdgeIndex;
                            if (calculateClosestEdge(currentPoint, end, out newIntersectedFigure, out newIntersectedFigureEdgeIndex, out intersectPoint))
                            {
                                if (newIntersectedFigure == intersectedFigure)
                                {
                                    action = Action.MoveAlongFigureEdge;
                                }
                                else
                                {
                                    direction = rnd.Next() % 2 == 0 ? Direction.Forward : Direction.Backward;
                                    intersectedFigure = newIntersectedFigure;
                                    intersectedFigureEdgeIndex = newIntersectedFigureEdgeIndex;
                                    action = Action.MoveAlongFigureEdge;
                                    Vector2 normal = intersectedFigure.GetEdgeNormalByIndex(intersectedFigureEdgeIndex);
                                    normal.Scale(new Vector2(margin, margin));
                                    currentPoint = intersectPoint + normal;
                                    result.Add(currentPoint);
                                }
                            }
                            else
                            {
                                result.Add(end);
                                return result;
                            }
                        }
                        break;


                    case Action.MoveAlongFigureEdge:
                        {
                            int vertexIndex = direction == Direction.Forward ? intersectedFigure.GetNextIndex(intersectedFigureEdgeIndex) : intersectedFigureEdgeIndex;
                            Vector2 straightPoint = GetPointMarginedFromFigureEdge(intersectedFigure, intersectedFigureEdgeIndex, vertexIndex);
                            int edgeIndex = direction == Direction.Forward ? intersectedFigure.GetNextIndex(intersectedFigureEdgeIndex) : intersectedFigure.GetPrevIndex(intersectedFigureEdgeIndex);

                            int vertexEdgeIndex1 = edgeIndex;
                            int vertexEdgeIndex2 = intersectedFigure.GetNextIndex(edgeIndex);
                            Vector2 marginEdgePoint1 = GetPointMarginedFromFigureEdge(intersectedFigure, edgeIndex, vertexEdgeIndex1);
                            Vector2 marginEdgePoint2 = GetPointMarginedFromFigureEdge(intersectedFigure, edgeIndex, vertexEdgeIndex2);
                            Vector2 intersectedPoint = Vector2.zero;
                            bool intersected = Vector2.SegmentToSegmentIntersection(currentPoint, straightPoint, marginEdgePoint1, marginEdgePoint2, ref intersectedPoint);
                            if (intersected)
                            {
                                intersectedFigureEdgeIndex = edgeIndex;
                                currentPoint = intersectedPoint;
                                result.Add(currentPoint);
                                action = Action.MoveAlongFigureEdge;
                            }
                            else
                            {
                                currentPoint = straightPoint;
                                result.Add(currentPoint);
                                action = Action.MoveFigureCorner;
                            }
                        }
                        break;


                    case Action.MoveFigureCorner:
                        {
                            int edgeIndex = direction == Direction.Forward ? intersectedFigure.GetNextIndex(intersectedFigureEdgeIndex) : intersectedFigure.GetPrevIndex(intersectedFigureEdgeIndex);
                            int vertexIndex = direction == Direction.Forward ? intersectedFigure.GetNextIndex(intersectedFigureEdgeIndex) : intersectedFigureEdgeIndex;
                            intersectedFigureEdgeIndex = edgeIndex;
                            currentPoint = GetPointMarginedFromFigureEdge(intersectedFigure, edgeIndex, vertexIndex);
                            result.Add(currentPoint);
                            action = Action.LookingEndPoint;
                        }
                        break;
                }
            }

            return result;
        }


        private bool calculateClosestEdge(Vector2 startPoint, Vector2 endPoint, out Figure interfigure, out int interEdgeIndex, out Vector2 interPoint)
        {
            interfigure = null;
            interEdgeIndex = 0;
            interPoint = Vector2.zero;

            float minSqrtDistance = float.PositiveInfinity;

            foreach (var figure in allFigures)
            {
                int figureVerticesCount = figure.Vertices.Length;
                for (int i = 0; i < figureVerticesCount; ++i)
                {
                    Vector2 intersectedPoint = Vector2.zero;
                    bool intersected = Vector2.SegmentToSegmentIntersection(startPoint, endPoint, figure.Vertices[i], figure.Vertices[figure.GetNextIndex(i)], ref intersectedPoint);
                    if (intersected)
                    {
                        float sqrDistance = Vector2.SqrDistance(startPoint, intersectedPoint);
                        if (sqrDistance < minSqrtDistance)
                        {
                            interfigure = figure;
                            interEdgeIndex = i;
                            interPoint = intersectedPoint;
                            minSqrtDistance = sqrDistance;
                        }
                    }
                }
            }

            return minSqrtDistance != float.PositiveInfinity;
        }

        private Vector2 GetPointMarginedFromFigureEdge(Figure figure, int edgeIndex, int vertexIndex)
        {
            Vector2 normal = figure.GetEdgeNormalByIndex(edgeIndex);
            normal.Scale(new Vector2(margin, margin));
            return figure.Vertices[vertexIndex] + normal;
        }
    }
}