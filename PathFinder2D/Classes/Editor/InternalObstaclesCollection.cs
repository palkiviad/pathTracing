using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        
        public InternalObstaclesCollection(string fileName) {
            
            IEnumerable<string> lines = File.ReadLines(fileName);
            foreach (string line in lines) {
                if (string.IsNullOrEmpty(line.Trim()))
                    continue;

                if (line.StartsWith("translate=")) {
                    string[] values = line.Substring("translate=".Length, line.Length - "translate=".Length).Split(',');
                    Vector2 delta = new Vector2(float.Parse(values[0], CultureInfo.InvariantCulture),
                                                float.Parse(values[1], CultureInfo.InvariantCulture));

                    InternalObstacle obstacle = obstacles.Last();
                    Vector2[] verts = obstacle.Data;
                    for (int i = 0; i < verts.Length; i++)
                        verts[i] = verts[i] + delta;
                    
                    obstacles.Add(new InternalObstacle(verts));
                    
                    continue;
                }

                if (line.StartsWith("grid=")) {
                    string[] values = line.Substring("grid=".Length, line.Length - "grid=".Length).Split(',');
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

                string[] vectorsStrings = line.Split('|');
                if(vectorsStrings.Length < 3)
                    throw new Exception("In line " + line + " less than 3 vectors.");
                
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

        public void Draw() {
            obstacles.ForEach(item => item.Draw()); 
        }
    }
    
}