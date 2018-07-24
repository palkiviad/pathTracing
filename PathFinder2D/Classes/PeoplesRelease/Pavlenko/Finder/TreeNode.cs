using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {

    public sealed class TreeNode {
        private readonly int level;
        private Contour[] contours;
        private TreeNode[] children;
        private BoundingBox aabb;

        public TreeNode(int level, BoundingBox aabb) {
            this.aabb = aabb;
            this.level = level;
        }

        private bool Empty {
            get { return contours == null && children == null; }
        }

        public void RecalcBounds() {
            int i;

            BoundingBox box = new BoundingBox();
            if (contours != null) {
                for (i = 0; i < contours.Length; i++)
                    box.Add(contours[i].bounds);
            }

            if (children != null) {
                for (i = 0; i < children.Length; i++) {
                    children[i].RecalcBounds();
                    box.Add(children[i].aabb);
                }
            }

            aabb = box;
        }

        public Intersection GetNearestIntersection(Vector2 start, Vector2 end) {
            if (!aabb.SegmentIntersectRectangle(start, end))
                return null;

            Intersection nearest = null;

            Vector2 normal = end - start;
            int i;
            if (contours != null) {
                // Ближайшее пересечение
                for (i = 0; i < contours.Length; i++) {
                    // если у нас уже есть nearest мы можем дико оптимизировать, убрав проверку пересечения с контурами, находящимися целиком за плоскость
                    // образованной точкой последнего пересечения и вектором направления
                  //  if (nearest != null) {
                        // Это необъяснимо! Почему более жесткое отбрасывание ненужного приводит к ухудшению производительности?!
                        // if (contours[i].bounds.BehindLine(nearest.point, normal))
                 //       if (contours[i].bounds.BehindLine(nearest.point, normal))
                //            continue;
                //    }

                    Intersection inter = contours[i].GetIntersection(start, end);
                    if (inter == null)
                        continue;

                    if (nearest == null || nearest.distance > inter.distance)
                        nearest = inter;
                }
            }

            // Ну и дальше с потомками
            if (children != null) {
                for (i = 0; i < children.Length; i++) {
//                        if (nearest != null) {
//                            
//                            // Это необъяснимо! Почему более жесткое отбрасывание ненужного приводит к ухудшению производительности?!
//                             if (contours[i].bounds.BehindLine(nearest.point, normal))
//                             // if (children[i].aabb.BehindLine(start, normal))
//                                continue;
//                        }

                    Intersection inter = children[i].GetNearestIntersection(start, end);
                    if (inter == null)
                        continue;

                    if (nearest == null || nearest.distance > inter.distance)
                        nearest = inter;
                }
            }

            return nearest;
        }

        public void Add(Contour[] contours) {

            int complexity = 0;
            for (int i = 0, count= contours.Length; i < count; i++)
                complexity += contours[i].Complexity();
            
          //  Console.WriteLine("Level " + level + " compexity " + complexity + " count " + contours.Length);
            
            if (complexity < 30) {
                this.contours = contours;
                return;
            }

            List<TreeNode> nodes = new List<TreeNode>();
            nodes.Add(new TreeNode(level + 1, new BoundingBox(aabb.min, aabb.min + aabb.halfSize)));
            nodes.Add(new TreeNode(level + 1, new BoundingBox(aabb.min + aabb.halfSize, aabb.max)));
            nodes.Add(new TreeNode(level + 1, new BoundingBox(new Vector2(aabb.min.x, aabb.min.y + aabb.halfSize.y), new Vector2(aabb.max.x - aabb.halfSize.x, aabb.max.y))));
            nodes.Add(new TreeNode(level + 1, new BoundingBox(new Vector2(aabb.min.x + aabb.halfSize.x, aabb.min.y), new Vector2(aabb.max.x, aabb.max.y - aabb.halfSize.y))));

            // Дикая хитрость - дополнительный child, в который попадут контуры, попавшие на границу с несколькими нодами
            // такие контуры останутся в данном узле
            List<Contour>[] children = new List<Contour>[5];
            for (int j = 0; j < 5; j++)
                children[j] = new List<Contour>();

            // граничный отправляется прямо в этот TreeNode, а тот что попадает в какой-то бокс - остаётся в нём
            for (int i = 0, count = contours.Length; i < count; i++) {
                List<Contour> target = null;
                for (int j = 0, countJ = nodes.Count; j < countJ; j++) {
                    if (!nodes[j].aabb.Intersects(contours[i].bounds))
                        continue;

                    if (target != null) {
                        target = children[4];
                        break;
                    }

                    target = children[j];
                }

                target.Add(contours[i]);
            }

            if (children[4].Count != 0) {
                this.contours = children[4].ToArray();
                
              // Console.WriteLine("Lost childs " + level + " compexity " + this.contours.Length);
            }

            for (int j = 0, count=nodes.Count; j < count; j++) {
                if (children[j].Count > 0)
                    nodes[j].Add(children[j].ToArray());
            }
            
            for(int j=nodes.Count-1; j>=0; j--)
                if( nodes[j].Empty)
                    nodes.RemoveAt(j);

            this.children = nodes.ToArray();
        }
    }
}