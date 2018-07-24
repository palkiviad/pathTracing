using System.Collections.Generic;
using PathFinder.Mathematics;

namespace PathFinder.Release.Pavlenko {

    // Нужны следующие оптимизации
    // 1. BoundingBox должен получить метод BehindLine, который будет использоваться для отбасывания тех объектов, которые оказались целиком за плоскостью ближайшего пересечения
    // 2. Map должен научиться работать с аналогом Octree и строиь его на старте
    // 3. Map должен обходить препятствия и слева и справа в поисках более короткого пути, возможно, строить граф и искать кратчайший путь
    // 4. Надо строить выпуклые оболочки и если целевая точка не лежит внутри оболочки/или AABB, обходить по контуру оболочки!
    // 5. Возможно, оболочки надо триангулировать, чтобы было проще проверять факт того что точка внутри
    // 6. Разумеется, надо переходить на иерархические NavMesh-ы, ибо для целого ряда задач они будут рулить, однако наша реализация о другом, мы хотим доказать
    //    что на большинстве областей мы получим решение по скорости незначительно хуже чем навмещ, но при этом не потратим время на препроцессинг области.
    public sealed class Map : IMap {

        private Contour[] contours;

        private TreeNode root;

        public void Init(Vector2[][] obstacles) {

            BoundingBox box = new BoundingBox();

            contours = new Contour[obstacles.Length];
            for (int i = 0, count = obstacles.Length; i < count; i++) {
                contours[i] = new Contour(obstacles[i]);
                box.Add(contours[i].bounds);
            }

            box.UpdateSecondary();

            // Дерево сцены
            root = new TreeNode(0, box);
            root.Add(contours);
            root.RecalcBounds();
        }
        
        private readonly ListPool pool = new ListPool();

        private List<Vector2> InnerGetPath(Vector2 start, Vector2 end, Contour previousContour) {
            
            List<Vector2> result = pool.Get();
            
            Vector2 last = start;
            while (true) {

                Intersection nearest = root.GetNearestIntersection(last, end);

                if (nearest == null) {
                    result.Add(end);
                    return result;
                }

                // но если пересечение нашлось, мы должны пройти по контуру до открытой грани, а она должна быть 100%
                
                // TODO
                // 1. Надо двигаться от last к nearest.edge.end, не вставляя промежуточный point, это даст нам более короткий путь
                // 2. Надо контур обходить с двух сторон, оставляя лишь короткий путь
                // 3. Выпуклый контур надо обходить по Convex граням и не искать пересесения e.start, end 
                //    а искать только открытую к end грань то же и с контуром, для которого end не внутри!

                List<Vector2> cvTry;
                {
                    // TODO Если здесь на пути (last, nearest.edge.end) есть рёбра моего контура, что может выйти как в случае выпуклого, так и в случае невыпуклого, 
                    // надо обходить контур дальше
//                    Edge e = nearest.edge;
//                    Vector2 lastLast = last;
//                    if (previousContour != null) {
//                       
//                        while (previousContour.GetIntersection(last, e.end, false) != null) {
//                            cvTry.Add(e.end);
//                            e = e.next;
//                            lastLast = e.end;
//                        } 
//                    }
                    
                    cvTry = InnerGetPath(last, nearest.edge.end, nearest.contour);
                    //cvTry.AddRange(tmp);
                    
                    Vector2 back = end - nearest.edge.end;

                    Edge e = nearest.edge.next;

                    while (!e.VisibleTo(back) || !nearest.contour.convex && nearest.contour.GetIntersection(e.start, end, false) != null) {
                        cvTry.Add(e.end);
                        back = end - e.end;
                        e = e.next;
                    }
                }

                List<Vector2> ccvTry;
                {
                    ccvTry = InnerGetPath(last, nearest.edge.start, nearest.contour);
   
                    // Походу здесь нам придётся ещё походить и по нашему контуру на предмет пересечений
                    Vector2 back = end - nearest.edge.start;
                 
                    Edge e = nearest.edge.previous;

                    while (!e.VisibleTo(back) || !nearest.contour.convex && nearest.contour.GetIntersection(e.end, end, false) != null) {
                        ccvTry.Add(e.start);
                        back = end - e.start;
                        e = e.previous;
                    }
                }
                
                // TODO надо не забыть что в ccvTry и cvTry я не добавляю last!, надо в PathMeasure добавить её отдельным параметром
                if (PathMeasure(ccvTry, end) < PathMeasure(cvTry, end)) {
                    result.AddRange(ccvTry);
                    last = ccvTry[ccvTry.Count-1];
                } else {
                    result.AddRange(cvTry);
                    last = cvTry[cvTry.Count-1];
                }
                
                pool.Put(ccvTry);
                pool.Put(cvTry);
            }
        }

        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 end) {
            List<Vector2> result = InnerGetPath(start, end, null);
            List<Vector2> fullRes = new List<Vector2>(result.Count + 1);
            fullRes.Add(start);
            fullRes.AddRange(result);
            pool.Put(result);
            return fullRes;
        }

        // Абстрактная в вакууме мера пути
        private static float PathMeasure(List<Vector2> path, Vector2 end) {
            float res = 0;
            float dx, dy;
            int count = path.Count - 1;
            for (int i = 0;i < count; i++) {
                dx = path[i + 1].x - path[i].x;
                if (dx < 0)
                    dx = -dx;
                dy = path[i + 1].y - path[i].y;
                if (dy < 0)
                    dy = -dy;

                res += dx + dy;
            }
            
            dx = path[count].x - end.x;
            if (dx < 0)
                dx = -dx;
            dy = path[count].y - end.y;
            if (dy < 0)
                dy = -dy;

            res += dx + dy;

            return res;
        }
    }
}