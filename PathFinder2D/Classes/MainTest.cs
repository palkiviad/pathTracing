using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PathFinder.Editor;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder {

    public class MainTest {

        private IMap[] maps = {
            
            new Pavlenko.Map(),
            new Popov.Map(),
        };

        private class Obstacles {
            public readonly string file;
            public readonly InternalObstaclesCollection collection;
            public readonly Vector2[] points;

            public Obstacles(string file, InternalObstaclesCollection collection) {
                this.file = file;
                this.collection = collection;

                const int numberOfPoints = 5;


                // Для каждого набора точек сгенерим список случайных конечных и начальных точек


                InternalBox box = collection.Bounds;
                Vector2 min = box.LeftBottom - new Vector2(box.W * 0.1f, box.H * 0.1f);
                Vector2 max = box.RightTop + new Vector2(box.W * 0.1f, box.H * 0.1f);

                Vector2[] points = new Vector2[numberOfPoints * 2];
                int pos = 0;
                do {
                    Vector2 randomPoint = new Vector2(Mathematics.Random.Range(min.x, max.x), Mathematics.Random.Range(min.y, max.y));

                    if (collection.Contains(randomPoint))
                        continue;

                    points[pos] = randomPoint;
                    pos++;
                } while (pos != points.Length);

                this.points = points;
            }
        }

        private class Statistics {

            public readonly double successRate;
            public readonly double duration;
            public readonly double length;
            public readonly double points;

            public Statistics(double successRate, double duration, double length, double points) {
                this.successRate = successRate;
                this.duration = duration;
                this.length = length;
                this.points = points;
            }
        }

        private class UserFileStatistics {
            public readonly string user;
            public readonly string file;
            public readonly Statistics statistics;

            public UserFileStatistics(string user, string file, Statistics statistics) {
                this.user = user;
                this.file = file;
                this.statistics = statistics;
            }
        }

        private readonly List<UserFileStatistics> statistics = new List<UserFileStatistics>();

        private Obstacles[] fileToObstacles;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public void TestMain() {
            Console.WriteLine("Started");

            // Чтобы как-то не дай Бог никакой зависимости от порядка запуска не допустить, рандомизируем.
            System.Random rnd = new System.Random();
            maps = maps.OrderBy(x => rnd.Next()).ToArray();

            // Предзагрузим все файлы
            string[] files = Directory.EnumerateFiles("Data", "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".txt") || s.EndsWith(".svg")).ToArray();


            fileToObstacles = new Obstacles[files.Length];
            for (int i = 0; i < files.Length; i++)
                fileToObstacles[i] = new Obstacles(files[i], new InternalObstaclesCollection(files[i]));


            // Ну и запустим банальные тестики, просто на нахождение пути
            for (int i = 0; i < maps.Length; i++) {
                Console.WriteLine("Map: " + maps[i].GetType().Namespace);

                for (int j = 0; j < fileToObstacles.Length; j++) {
                    Obstacles obstacle = fileToObstacles[j];

                    int pairsCount = obstacle.points.Length / 2;

                    List<double> durations = new List<double>();
                    List<double> lengths = new List<double>();
                    List<int> pointCounts = new List<int>();
                    bool[] successes = new bool[pairsCount];

                    IMap map = maps[i];
                    map.Init(obstacle.collection.Data);

                    for (int k = 0, l = 0; k < obstacle.points.Length; k += 2, l++) {
                        
                        bool fail = false;
                        
                        stopwatch.Restart();
                        IEnumerable<Vector2> path = null;
                        try {
                            path = map.GetPath(obstacle.points[k], obstacle.points[k + 1]);
                        } catch (Exception) {
                            fail = true;
                        }

                        stopwatch.Stop();
                        //    Console.WriteLine("Time elapsed (ms): {0}", stopwatch.Elapsed.TotalMilliseconds);


                        // Здесь мы должны отвалидировать адекватность пути и если он невалидный, отметить это
                        if (!fail && (path.First() != obstacle.points[k] || path.Last() != obstacle.points[k + 1]))
                            fail = true;

                        if (!fail && obstacle.collection.Intersects(path))
                            fail = true;

                        if (!fail) {
                            durations.Add(stopwatch.Elapsed.TotalMilliseconds);
                            pointCounts.Add(path.Count());
                            lengths.Add(Length(path));
                        }

                        successes[l] = !fail;
                    }

                    int successCount = successes.Count(p => p);

                    Console.WriteLine("File: " + obstacle.file +
                                      " SuccessRate: " + string.Format("{0:0.00}", successCount == 0 ? 0 : (float)successCount/ successes.Length) +
                                      " Duration: " + string.Format("{0:0.0000}", durations.Count == 0 ? 0 : durations.Average()) +
                                      " Points: " + string.Format("{0:0.00}", pointCounts.Count == 0 ? 0 : pointCounts.Average()) +
                                      " Length: " + string.Format("{0:0.0000}", lengths.Count == 0 ? 0 : lengths.Average()));

                    statistics.Add(new UserFileStatistics(maps[i].GetType().Namespace.Replace("PathFinder.", ""), obstacle.file,
                        new Statistics(
                            successCount == 0 ? 0 :  (float)successCount/ successes.Length,
                            durations.Count == 0 ? 0 : durations.Average(),
                            pointCounts.Count == 0 ? 0 : pointCounts.Average(),
                            lengths.Count == 0 ? 0 : lengths.Average()
                        )));
                }
            }

            SaveStatistics();
            
            Console.WriteLine("Test finsihed.");
        }


        private void SaveStatistics() {

            const float barrierSuccessRate = 0.96f;
            
            StringBuilder sb = new StringBuilder(
                "<!DOCTYPE html><html><head><style>table {    width:100%;}table, th, td {    border: 1px solid black;    border-collapse: collapse;}th, td {    padding: 15px;    text-align: left;}table#t01 tr:nth-child(even) {    background-color: #eee;}table#t01 tr:nth-child(odd) {   background-color: #fff;}table#t01 th {    background-color: black;    color: white;}</style></head><body><h2>Рекорды</h2><table>");

            // тут табличка

            // Сначала хэдера
            sb.Append("<tr>");
            sb.Append("<th>User</th>");
            
            sb.Append("<th>Total</th>");
            sb.Append("<th>Total duration</th>");
            sb.Append("<th>Total length</th>");

            // Имена файлов
            HashSet<string> files = new HashSet<string>();
            foreach (var val in statistics)
                files.Add(val.file);

            HashSet<string> users = new HashSet<string>();
            foreach (var val in statistics)
                users.Add(val.user);


            foreach (var file in files)
                sb.Append("<th colspan=\"4\" style=\"text-align:center\">" + file + "</th>");
            sb.Append("</tr>");

            // Вторая строка - средние значение
            sb.Append("<tr>");
            sb.Append("<td>-</td>");
            
            sb.Append("<td>-</td>");
            sb.Append("<td>-</td>");
            sb.Append("<td>-</td>");

            for (int i=0;i<files.Count; i++) {
                sb.Append("<td>Success</td>");
                sb.Append("<td>Duration</td>");
                sb.Append("<td>Length</td>");
                sb.Append("<td>Points</td>");
            }

            sb.Append("</tr>");

            
            // Построим интервалы, за попадание в которые будем давать баллы. Для этого просто получим min и max значения 
            // Интервалы строим на базе значений людей, которые выдали
            
            // Интервалы раздельны по файлам и по типу - длительность и длина

            Dictionary<string, Vector2> durationIntervals = new Dictionary<string, Vector2>();
            Dictionary<string, Vector2> lengthIntervals = new Dictionary<string, Vector2>();
            
            foreach (var file in files) {
               
                var fileStatistics = statistics.FindAll(p => p.file == file);
                
                float minDuration = (float)fileStatistics.Min(p => p.statistics.successRate <= barrierSuccessRate ? double.MaxValue : p.statistics.duration);
                float maxDuration = (float)fileStatistics.Max(p => p.statistics.successRate <= barrierSuccessRate ? double.MinValue : p.statistics.duration);
                
                float minLength = (float)fileStatistics.Min(p => p.statistics.successRate <= barrierSuccessRate ? double.MaxValue : p.statistics.length);
                float maxLength = (float)fileStatistics.Max(p => p.statistics.successRate <= barrierSuccessRate ? double.MinValue : p.statistics.length);
                
                durationIntervals[file]  = new Vector2(minDuration, maxDuration);
                lengthIntervals[file] = new Vector2(minLength, maxLength);
            }
            
            
            // Теперь по фамилиям и файлам получим очки
            List<Tuple<string, string, int>> durationScore = new List<Tuple<string, string, int>>();
            List<Tuple<string, string, int>> lengthScore = new List<Tuple<string, string, int>>();

            foreach (string user in users) {
                foreach (var file in files) {
                    
                    Statistics stat = statistics.Find(p => p.user == user && p.file == file).statistics;

                    if (stat.successRate <= barrierSuccessRate) {
                        durationScore.Add(new Tuple<string, string, int>(user, file, 0));
                        lengthScore.Add(new Tuple<string, string, int>(user, file, 0));
                    } else {
                        // Интервал по файлу
                        Vector2 durationInterval = durationIntervals[file];

                        float t = (float)((stat.duration - durationInterval.x) / (durationInterval.y - durationInterval.x));
                        durationScore.Add(new Tuple<string, string, int>(user, file, 10-(int)(t*10)));
                        
                        Vector2 lengthInterval = lengthIntervals[file];
                        t = (float)((stat.length - lengthInterval.x) / (lengthInterval.y - lengthInterval.x));
                        
                        lengthScore.Add(new Tuple<string, string, int>(user, file, 10-(int)(t*10)));
                    }
                }
            }

            Dictionary<string, int> totalScores = new Dictionary<string, int>();
            Dictionary<string, int> totalDuration = new Dictionary<string, int>();
            Dictionary<string, int> totalLength = new Dictionary<string, int>();
            
            foreach (string user in users) {
                // Находим все файлы по данному юзеру
                var userDurationScores = durationScore.FindAll(p => p.Item1 == user);
                var userLengthScores = lengthScore.FindAll(p => p.Item1 == user);

                totalDuration[user] = userDurationScores.Sum(p => p.Item3);
                totalLength[user] = userLengthScores.Sum(p => p.Item3);
                
                totalScores[user] = userDurationScores.Sum(p => p.Item3) + userLengthScores.Sum(p => p.Item3);
            }


            // А дальше - по фамилиям
            foreach (string user in users) {
                sb.Append("<tr>");
                sb.Append("<td>" + user + "</td>");

                
                // Тотал
                sb.Append("<td>" + totalScores[user] + "</td>");
                
                // Тотал длительность
                sb.Append("<td>" + totalDuration[user] + "</td>");
                
                // Тотал дистанция
                sb.Append("<td>" + totalLength[user] + "</td>");
                
                
                foreach (var file in files) {
                    Statistics stat = statistics.Find(p => p.user == user && p.file == file).statistics;

                    if (stat.successRate <= barrierSuccessRate)
                        sb.Append("<td style=\"color: red;font-weight: bold;\">" + string.Format("{0:0.00}", stat.successRate) + "</td>");
                    else
                        sb.Append("<td>" + string.Format("{0:0.00}", stat.successRate) + "</td>");

                    // если stat.successRate == 1 мы должны найти среди таких молодцев лучший показатель

                    var fileStatistics = statistics.FindAll(p => p.file == file);

                    if (stat.successRate > barrierSuccessRate) {
                        var minDuration = fileStatistics.Min(p => p.statistics.successRate <= barrierSuccessRate ? double.MaxValue : p.statistics.duration);
                        var minLength = fileStatistics.Min(p => p.statistics.successRate <= barrierSuccessRate ? double.MaxValue : p.statistics.length);
                        var minPoints = fileStatistics.Min(p => p.statistics.successRate <= barrierSuccessRate ? double.MaxValue : p.statistics.points);
                        
                        sb.Append((minDuration == stat.duration?"<td style=\"color: green;font-weight: bold;\">" : "<td>") + string.Format("{0:0.000}", stat.duration) + "</td>");
                        sb.Append((minLength == stat.length?"<td style=\"color: green;font-weight: bold;\">" : "<td>") + string.Format("{0:0.000}", stat.length) + "</td>");
                        sb.Append((minPoints == stat.points?"<td style=\"color: green;font-weight: bold;\">" : "<td>") + string.Format("{0:0.000}", stat.points) + "</td>");
                        
                    }else {
                        sb.Append("<td>" + string.Format("{0:0.000}", stat.duration) + "</td>");
                        sb.Append("<td>" + string.Format("{0:0.000}", stat.length) + "</td>");
                        sb.Append("<td>" + string.Format("{0:0.000}", stat.points) + "</td>");
                    }
                }

                sb.Append("</tr>");
            }

            sb.Append("</table></body></html>");
            File.WriteAllText("records.html", sb.ToString());
        }

        private static double Length(IEnumerable<Vector2> path) {
            double res = 0;

            bool first = true;
            Vector2 prev = Vector2.zero;
            foreach (Vector2 vertex in path) {
                if (first) {
                    first = false;
                    prev = vertex;
                    continue;
                }

                res += Vector2.Distance(prev, vertex);
            }

            return res;
        }

    }
}