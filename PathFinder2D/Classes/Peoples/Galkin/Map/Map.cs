using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.ES11;
using PathFinder.Mathematics;
using PathFinder2D.Classes.Peoples.Galkin.Map;

namespace PathFinder.Galkin {

    public sealed class Map : IMap {
        private Vector2 _direction;
        private Vector2 _point;
        private Vector2 _nextPoint;
        private Vector2 _targetPoint;
        private List<Obstacle> _obstacles;
        private const float _step = 1f;

        public void Init(Vector2[][] obstacles)
        {
            _obstacles = new List<Obstacle>();
            for (int i = 0, count = obstacles.Length; i < count; ++i)
            {
                _obstacles.Add(new Obstacle(obstacles[i]));
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end)
        {
            _point = start;
            _targetPoint = end;
            _nextPoint = end;
            _direction = (_targetPoint - _point).normalized;

            List<Vector2> way = new List<Vector2>();
            way.Add(_point);

            while ((_point - _targetPoint).magnitude > _step)
            {
                Obstacle nearestObstacle = null;
                float obstacleDistance = float.PositiveInfinity;

                bool intersection = false;
                // среди обстаклов находим ближайший с которым пересекаемся
                for (int i = 0, count = _obstacles.Count; i < count; ++i)
                {
                    Obstacle curObstacle = _obstacles[i];
                    Vector2 intersectionPoint = Vector2.zero;
                    
                    if (curObstacle.Contains(_point, _nextPoint, ref intersectionPoint))
                    {
                        intersection = true;
                        if ((_point - intersectionPoint).magnitude < obstacleDistance)
                        {
                            nearestObstacle = curObstacle;
                            obstacleDistance = (_point - intersectionPoint).magnitude;
                        }
                    }
                }

                if (intersection)
                {
                    // TODO есть 1 известный мне баг:
                    // точка пересечения смотрится, а надо старт-энд, из-за этого бывают такие штуки https://c2n.me/3VidLKi
                    // а так же есть где поотимайзить
                    List<Vector2> obstacleWayPoints = nearestObstacle.GetDirectionalPoints(_point, _nextPoint);
                    // для каждой точки обстакла мы должны проверить что нет пересечения с самим собой в случае движения от этой точки к цели
                    // при этом надо найти кротчайший путь
                    List<Vector2> tempWay1 = new List<Vector2>();
                    Vector2 tempPoint = Vector2.zero;
                    for (int i1 = 0, count1 = obstacleWayPoints.Count; i1 < count1; ++i1)
                    {
                        var obstaclePoint = obstacleWayPoints[i1];
                        tempWay1.Add(obstaclePoint);
                        if (!nearestObstacle.Contains(obstaclePoint, _nextPoint, ref tempPoint))
                        {
                            break;
                        }
                    }

                    // с нулевого, но задом-наперед
                    List<Vector2> tempWay2 = new List<Vector2>();
                    for (int i2 = 0, count2 = obstacleWayPoints.Count; i2 < count2; ++i2)
                    {
                        // сделал убого, 
                        int curIndex = i2 == 0 ? 0 : count2 - i2;
                        var obstaclePoint = obstacleWayPoints[curIndex];
                        tempWay2.Add(obstaclePoint);
                        if (!nearestObstacle.Contains(obstaclePoint, _nextPoint, ref tempPoint))
                        {
                            break;
                        }
                    }

                    // выбираем кротчайший
                    if (tempWay1.Count < tempWay2.Count)
                    {
                        way.AddRange(tempWay1);
                        _nextPoint = tempWay1[tempWay1.Count - 1];
                    }
                    else
                    {
                        way.AddRange(tempWay2);
                        _nextPoint = tempWay2[tempWay2.Count - 1];
                    }
                }

                if (!intersection)
                {
                    way.Add(_nextPoint);
                }
                _point = _nextPoint;
                _nextPoint = _targetPoint;
            }

           return way;
        }
    }
}
