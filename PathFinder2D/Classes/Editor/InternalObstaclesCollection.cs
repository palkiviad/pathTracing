using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PathFinder.Mathematics;

namespace PathFinder.Editor {
    public sealed class InternalObstaclesCollection {

        private readonly List<InternalObstacle> obstacles = new List<InternalObstacle>();

        private InternalBox bounds;
        public InternalBox Bounds {
            get {
                if (bounds == null) {
                    bounds = new InternalBox();
                    obstacles.ForEach(obstacle => bounds.Add(obstacle.Bounds));  
                }

                return bounds;
            }
        }

        public Vector2[][] Data {
            get {
                Vector2[][] result = new Vector2[obstacles.Count][];
                for (int i = 0; i < obstacles.Count; i++)
                    result[i] = obstacles[i].Data;
                return result;
            }
        }

        public InternalObstaclesCollection(Vector2[][] obstacles) {
            Array.ForEach(obstacles, item => this.obstacles.Add(new InternalObstacle(item)));
        }

        public bool Contains(Vector2 point) {
            for(int i=0;i<obstacles.Count; i++)
                if (obstacles[i].Contains(point))
                    return true;
            return false;
        }


        private void LoadTxt(IEnumerable<string> lines) {
             foreach (string line in lines) {

                string trimmed = line.Trim();
                
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
                    continue;

                if (trimmed.StartsWith("translate=")) {
                    string[] values = trimmed.Substring("translate=".Length, trimmed.Length - "translate=".Length).Split(',');
                    Vector2 delta = new Vector2(float.Parse(values[0], CultureInfo.InvariantCulture),
                                                float.Parse(values[1], CultureInfo.InvariantCulture));

                    InternalObstacle obstacle = obstacles.Last();
                    Vector2[] verts = obstacle.Data;
                    for (int i = 0; i < verts.Length; i++)
                        verts[i] = verts[i] + delta;
                    
                    obstacles.Add(new InternalObstacle(verts));
                    
                    continue;
                }

                if (trimmed.StartsWith("grid=")) {
                    string[] values = trimmed.Substring("grid=".Length, trimmed.Length - "grid=".Length).Split(',');
                    int countX = int.Parse(values[0], CultureInfo.InvariantCulture);
                    int countY = int.Parse(values[0], CultureInfo.InvariantCulture);
                    
                    Vector2 delta = new Vector2(float.Parse(values[2], CultureInfo.InvariantCulture),
                        float.Parse(values[3], CultureInfo.InvariantCulture));
                    
                    InternalObstacle reference = obstacles.Last();

                    for (int i = 1; i <= countX; i++) {
                        for (int j = 1; j <= countY; j++) {
                            Vector2[] verts = reference.Data;
                            
                            for (int k = 0; k < verts.Length; k++)
                                verts[k] = verts[k] + delta*new Vector2(i,j);
                    
                            obstacles.Add(new InternalObstacle(verts));
                        }
                    }

                    continue;
                }

                string[] vectorsStrings = trimmed.Split('|');
                if(vectorsStrings.Length < 3)
                    throw new Exception("In line " + trimmed + " less than 3 vectors.");
                
                Vector2[] vertices = new Vector2[vectorsStrings.Length];
                for (int i=0;i<vectorsStrings.Length; i++) {
                    string[] values = vectorsStrings[i].Split(',');
                    if(values.Length != 2)
                        throw new Exception("In vector " + vectorsStrings[i] + " components count != 2.");

                    vertices[i] = new Vector2(float.Parse(values[0], CultureInfo.InvariantCulture), 
                                              float.Parse(values[1], CultureInfo.InvariantCulture));
                }
                
                obstacles.Add(new InternalObstacle(vertices));
            }
        }

        private void LoadSvg(IEnumerable<string> lines) {
             
            // Объединяем в одну строку
            
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines) {
                sb.Append(line);
            }

            string single = sb.ToString();
            
            // Ищем начало - <path d="

            int start=0;
            while ((start = single.IndexOf("<path d=", start, StringComparison.Ordinal)) != -1) {

                int end = single.IndexOf( "\"", start+10, StringComparison.Ordinal);

                string pathString = single.Substring(start + 9, end - start - 9  );
                if(!pathString.StartsWith("M") && pathString.EndsWith("Z"))
                    throw new Exception("Cannot find M and Z markers. Wrong svg contour.");

                pathString = pathString.Substring(1, pathString.Length - 2);
                pathString = pathString.Replace("L", " ");

                string[] splitted = pathString.Split(' ');
                if(splitted.Length%2 != 0)
                    throw new Exception("Wrong count of vertices in contour");
                
                // -1 и -2 потому что последняя точка дублирует первую
                Vector2[] vertices = new Vector2[splitted.Length/2-1];
                for(int i=0, j=0;i<splitted.Length-2; i+=2,j++)
                    vertices[j] = new Vector2(float.Parse(splitted[i]), float.Parse(splitted[i+1]));
                
                obstacles.Add(new InternalObstacle(vertices));
                    
                start = end+1;
            }
        }

        public InternalObstaclesCollection(string fileName) {
            
            Console.WriteLine("Loading " + fileName);
            
            IEnumerable<string> lines = File.ReadLines(fileName);
            
            if(lines.Count() == 0)
                throw new Exception("File " + fileName + " has no lines");

            
            if (lines.First().StartsWith("<?xml")) {
                LoadSvg(lines);
            } else {
                LoadTxt(lines);
            }
        }

        public void Draw() {
            obstacles.ForEach(item => item.Draw()); 
        }
    }
}