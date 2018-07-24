using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PathFinder.Mathematics;
using Random = PathFinder.Mathematics.Random;

namespace PathFinder.Editor {
    public sealed class InternalObstaclesCollection {

        private InternalObstacle[] obstacles;

        public bool Initialized {
            get {
                for (int i = 0; i < obstacles.Length; i++)
                    if (!obstacles[i].Initialized)
                        return false;
                return true;
            }
        }

        private InternalBox bounds;

        public InternalBox Bounds {
            get {
                if (bounds == null) {
                    bounds = new InternalBox();
                    for (int i = 0; i < obstacles.Length; i++)
                        bounds.Add(obstacles[i].Bounds);
                }

                return bounds;
            }
        }

        // Метод проверки валидности пути
        public bool Intersects(IEnumerable<Vector2> path, ref Vector2 badSegmentStart, ref Vector2 badSegmentEnd) {
            for (int i = 0; i < obstacles.Length; i++)
                if (obstacles[i].Intersects(path,ref badSegmentStart, ref badSegmentEnd))
                    return true;
            return false;
        }


        // Вспомогательный метод, чтобы передать данные уже в алгоритм нахождения пути
        public Vector2[][] Data {
            get {
                Vector2[][] result = new Vector2[obstacles.Length][];
                for (int i = 0; i < obstacles.Length; i++)
                    result[i] = obstacles[i].Data;
                return result;
            }
        }

//        public InternalObstaclesCollection(Vector2[][] obstacles) {
//            for(int i=0;i<obstacles.Length;i++)
//                this.obstacles[i] = new InternalObstacle(obstacles[i]);
//        }

        public bool Contains(Vector2 point) {
            for (int i = 0; i < obstacles.Length; i++)
                if (obstacles[i].Contains(point))
                    return true;
            return false;
        }

        private void LoadTxt(IEnumerable<string> lines) {
            List<InternalObstacle> result = new List<InternalObstacle>();
            foreach (string line in lines) {
                string trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
                    continue;

                if (trimmed.StartsWith("translate=")) {
                    string[] values = trimmed.Substring("translate=".Length, trimmed.Length - "translate=".Length).Split(',');
                    Vector2 delta = new Vector2(float.Parse(values[0], CultureInfo.InvariantCulture),
                        float.Parse(values[1], CultureInfo.InvariantCulture));

                    InternalObstacle obstacle = result.Last();
                    Vector2[] verts = obstacle.Data;
                    for (int i = 0; i < verts.Length; i++)
                        verts[i] = verts[i] + delta;

                    result.Add(new InternalObstacle(verts));

                    continue;
                }

                if (trimmed.StartsWith("grid=")) {
                    string[] values = trimmed.Substring("grid=".Length, trimmed.Length - "grid=".Length).Split(',');
                    int countX = int.Parse(values[0], CultureInfo.InvariantCulture);
                    int countY = int.Parse(values[1], CultureInfo.InvariantCulture);

                    Vector2 delta = new Vector2(float.Parse(values[2], CultureInfo.InvariantCulture),
                                                float.Parse(values[3], CultureInfo.InvariantCulture));
                    
                    // И наконец, если есть ещё 4 и 5 - это означает что мы разрешаем случайные вращения, а сама точка - центр вращения
                    bool randomRotation = false;
                    Vector2 rotationShift = Vector2.zero;
                    if (values.Length == 6) {
                        randomRotation = true;
                        rotationShift = new Vector2(float.Parse(values[4], CultureInfo.InvariantCulture),
                                                    float.Parse(values[5], CultureInfo.InvariantCulture));
                    }

                    InternalObstacle reference = result.Last();

                    for (int i = 1; i <= countX; i++) {
                        for (int j = 1; j <= countY; j++) {
                            Vector2[] verts = reference.Data;

                            float randomAngle = Random.Range(0, 360);
                            
                            for (int k = 0; k < verts.Length; k++) {

                                if (!randomRotation) {
                                    verts[k] = verts[k] + delta * new Vector2(i, j);
                                } else {
                                    verts[k] = (verts[k] +rotationShift).Rotate(randomAngle) - rotationShift  + delta * new Vector2(i, j);
                                }
                            }

                            result.Add(new InternalObstacle(verts));
                        }
                    }

                    continue;
                }

                string[] vectorsStrings = trimmed.Split('|');
                if (vectorsStrings.Length < 3)
                    throw new Exception("In line " + trimmed + " less than 3 vectors.");

                Vector2[] vertices = new Vector2[vectorsStrings.Length];
                for (int i = 0; i < vectorsStrings.Length; i++) {
                    string[] values = vectorsStrings[i].Split(',');
                    if (values.Length != 2)
                        throw new Exception("In vector " + vectorsStrings[i] + " components count != 2.");

                    vertices[i] = new Vector2(float.Parse(values[0], CultureInfo.InvariantCulture),
                        float.Parse(values[1], CultureInfo.InvariantCulture));
                }

                result.Add(new InternalObstacle(vertices));
            }

            obstacles = result.ToArray();
        }

        private void LoadSvg(IEnumerable<string> lines) {
            // Объединяем в одну строку
            List<InternalObstacle> result = new List<InternalObstacle>();
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines) {
                sb.Append(line);
            }

            string single = sb.ToString();

            // Ищем начало - <path d="

            int start = 0;
            while ((start = single.IndexOf("<path d=", start, StringComparison.Ordinal)) != -1) {
                int end = single.IndexOf("\"", start + 10, StringComparison.Ordinal);

                string pathString = single.Substring(start + 9, end - start - 9);
                if (!pathString.StartsWith("M") && pathString.EndsWith("Z"))
                    throw new Exception("Cannot find M and Z markers. Wrong svg contour.");

                pathString = pathString.Substring(1, pathString.Length - 2);
                pathString = pathString.Replace("L", " ");

                string[] splitted = pathString.Split(' ');
                if (splitted.Length % 2 != 0)
                    throw new Exception("Wrong count of vertices in contour");

                // -1 и -2 потому что последняя точка дублирует первую
                Vector2[] vertices = new Vector2[splitted.Length / 2 - 1];
                for (int i = 0, j = 0; i < splitted.Length - 2; i += 2, j++)
                    vertices[j] = new Vector2(float.Parse(splitted[i]), float.Parse(splitted[i + 1]));

                result.Add(new InternalObstacle(vertices));

                start = end + 1;
            }

            obstacles = result.ToArray();
        }

        public InternalObstaclesCollection(string fileName) {
            Console.WriteLine("Loading " + fileName);

            IEnumerable<string> lines = File.ReadLines(fileName);

            if (!lines.Any())
                throw new Exception("File " + fileName + " has no lines");


            if (lines.First().StartsWith("<?xml")) {
                LoadSvg(lines);
            } else {
                LoadTxt(lines);
            }
            
            // Также попробуем загрузить файл с областями
            TryLoadAreas(fileName);
        }

        public InternalBox[] areasA;
        public InternalBox[] areasB;

        private void TryLoadAreas(string fileName) {
            // Сгенерим реально имя
            string areaFIleName = fileName.Substring(0, fileName.Length - 3) + "areas";
            if (!File.Exists(areaFIleName))
                return;

            List<InternalBox> areaA = new List<InternalBox>();
            List<InternalBox> areaB = new List<InternalBox>();
            
            IEnumerable<string> lines = File.ReadLines(areaFIleName);
            foreach (string line in lines) {
                string trimmed = line.Trim();
                if(trimmed.Length == 0 || trimmed.StartsWith("//"))
                    continue;
                   
                string[] splitted = trimmed.Split('|');
                
                List<InternalBox> target = splitted[0].Trim() == "A" ? areaA : areaB;
                target.Add(new InternalBox(
                    new Vector2(float.Parse(splitted[1]), float.Parse(splitted[2])),
                    new Vector2(float.Parse(splitted[3]), float.Parse(splitted[4]))
                    ));
            }

            areasA = areaA.ToArray();
            areasB = areaB.ToArray();
        }

        public void Draw() {
            for (int i = 0; i < obstacles.Length; i++)
                obstacles[i].Draw();
        }
    }
}