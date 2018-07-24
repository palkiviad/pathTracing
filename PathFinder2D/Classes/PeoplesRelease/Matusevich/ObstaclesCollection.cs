using System;
using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Matusevich {
    public class ObstaclesCollection {

        private class Node {

            private const int Threshold = 2;

            private readonly Box _box;
            private readonly List<Node> _children = new List<Node>();
            private readonly List<Obstacle> _obstacles = new List<Obstacle>();

            public Node(Box box) {
                _box = box;
            }

            public bool TryAdd(Obstacle obstacle) {
                if (!_box.ContainsLR(obstacle.Box)) {
                    return false;
                }

                _obstacles.Add(obstacle);
                if (_obstacles.Count < Threshold) {
                    return true;
                }
                if (_children.Count == 0) {
                    CreateChildren();
                }
                for (var i = 0; i < _obstacles.Count; i++) {
                    for (var j = 0; j < _children.Count; j++) {
                        if (_children[j].TryAdd(_obstacles[i])) {
                            _obstacles.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }
                return true;
            }

            private void CreateChildren() {
                var center = _box.GetCenter();
                _children.Add(new Node(new Box(new Vector2(_box.Min.x, _box.Min.y), new Vector2(center.x, center.y))));
                _children.Add(new Node(new Box(new Vector2(_box.Min.x, center.y), new Vector2(center.x, _box.Max.y))));
                _children.Add(new Node(new Box(new Vector2(center.x, _box.Min.y), new Vector2(_box.Max.x, center.y))));
                _children.Add(new Node(new Box(new Vector2(center.x, center.y), new Vector2(_box.Max.x, _box.Max.y))));
            }

            internal void Prepare() {
                for (var i = 0; i < _obstacles.Count; i++) {
                    _obstacles[i].Prepare();
                }
                for (var i = 0; i < _children.Count; i++) {
                    _children[i].Prepare();
                }
            }

            internal Obstacle FindFirstIntersection(Segment segment) {
                if (_box.CannotIntersectWithExcluding(segment)) {
                    return null;
                }
                for (var i = 0; i < _obstacles.Count; i++) {
                    var obstacle = _obstacles[i];
                    if (obstacle.Inersects(segment)) {
                        return obstacle;
                    }
                }
                for (var i = 0; i < _children.Count; i++) {
                    var obstacle = _children[i].FindFirstIntersection(segment);
                    if (obstacle != null) {
                        return obstacle;
                    }
                }
                return null;
            }
        }
        
        private Node _node;

        public void Load(Vector2[][] obstacles) {
            if (obstacles.Length == 0) {
                _node = new Node(new Box(new Vector2(0, 0), new Vector2(0, 0)));
                return;
            }
            Vector2 min = obstacles[0][0];
            Vector2 max = obstacles[0][0];

            for (var i = 0; i < obstacles.Length; i++) {
                var obstacle = obstacles[i];
                for (var j = 0; j < obstacle.Length; j++) {
                    var vertice = obstacle[j];
                    min.x = Math.Min(min.x, vertice.x);
                    min.y = Math.Min(min.y, vertice.y);
                    max.x = Math.Max(max.x, vertice.x);
                    max.y = Math.Max(max.y, vertice.y);
                }
            }

            _node = new Node(new Box(min - new Vector2(1, 1), max + new Vector2(1, 1)));

            for (var i = 0; i < obstacles.Length; i++) {
                _node.TryAdd(new Obstacle(obstacles[i]));
            }
        }

        public Obstacle FindFirstIntersection(Vector2 start, Vector2 end) {
            var segmentToEnd = Segment.Create(start, end);
            return _node.FindFirstIntersection(segmentToEnd);
        }

        internal void Prepare() {
            _node.Prepare();
        }
    }
}