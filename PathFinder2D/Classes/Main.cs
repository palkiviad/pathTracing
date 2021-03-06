﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PathFinder.Editor;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder {
    internal sealed class Game : GameWindow {

//        public static readonly IMap[] pathFinders = {
//            new Arkhipov.Map(),
//            new Chermashentsev.Map(),
//            new Dolgii_2018_07_11.Map(),
//           new Galkin.Map(),
//            new Kolesnikov.Map(),
//            new Leyko.Map(),
//            new Matusevich.Map(),
//            new Mengaziev.Map(),
//            new Minaev.Map(),
//           new Pavlenko.Map(),
//            new Popov.Map(),
//            new Shishlov.Map()
//        };
        
        public static readonly IMap[] pathFinders = {
          /* new Release.Dolgii.Map(),
            
           new Release.Kolesnikov.Map(),          
           
            
           new Release.Mengaziev_1.Map(),
           new Release.Mengaziev_2.Map(),
           new Release.Mengaziev_3.Map(),
            new Release.Pavlenko.Map(),
            new Release.Matusevich.Map(),*/
            new Release.Popov.Map(),
            
        /*  new Release.Shishlov.Map(),
            
          new Release.Suhih.Map()*/
        };

        private int currentPathFinder;

        private IMap map;// = new Pavlenko.Map();

        private InternalObstaclesCollection obstaclesCollection;
        private readonly InternalSettings settings = new InternalSettings();

        private float selectionRadius;

        private enum DragMode {
            None,
            Start,
            End
        }

        private bool pathSet;
        private Vector2 start;
        private Vector2 end;

        private DragMode dragMode = DragMode.None;
        private readonly Stopwatch stopwatch = new Stopwatch();

        private Game() : base(800, 600, GraphicsMode.Default, "Path Finder") {
            VSync = VSyncMode.On;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }
        
        private void LoadFile(string file) {
            if (file == null || !File.Exists(file))
                throw new Exception("Cannot load data file");

            obstaclesCollection = new InternalObstaclesCollection(file);
            
            settings.CurrentFile = file;
            
            SwithPathFinder(currentPathFinder);

            OnResize();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            Size = settings.Size;
            if (settings.Location.X != int.MinValue && settings.Location.Y != int.MinValue)
                Location = new Point(settings.Location.X, settings.Location.Y);

            if (settings.StartAndEnd != null) {
                start = settings.StartAndEnd[0];
                end = settings.StartAndEnd[1];
                pathSet = true;
            }

            GL.ClearColor(0.7f, 0.7f, 0.7f, 0.0f);
            GL.Disable(EnableCap.DepthTest);

            if (!File.Exists(settings.CurrentFile)) {
                settings.CurrentFile = null;
                LoadFile(settings.NextFile);
            } else
                LoadFile(settings.CurrentFile);
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            OnResize();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            settings.Location = Location;
        }

        private void OnResize() {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            InternalBox box = obstaclesCollection.Bounds;

            settings.Size = new Size(Bounds.Width, Bounds.Height);

            // теперь отскейлим  box так, чтобы сохранить пропорции
            // также добавим ещё процентов 20 по сторонам

            float coeffW = (float) Width / Height;
            float coeffM = box.W / box.H;

            float halfW, halfH;
            if (coeffW < coeffM) {
                halfW = box.W / 2;
                halfH = box.W / coeffW / 2;
            } else {
                halfH = box.H / 2;
                halfW = box.H * coeffW / 2;
            }

            Vector2 center = box.Center;

            // если пути нет, проставим его ровно посереднине слева направо
            if (!pathSet) {
                start = new Vector2(center.x - halfW * 1.1f, center.y);
                end = new Vector2(center.x + halfW * 1.1f, center.y);
                pathSet = true;

                settings.StartAndEnd = new[] {start, end};
            }

            // И надбавим 20% чтобы было откуда и куда строить путь
            halfW *= 1.2f;
            halfH *= 1.2f;

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(center.x - halfW, center.x + halfW,
                center.y - halfH, center.y + halfH, -10, 10);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            Vector3 point = new Vector3(0, 0, 0);
            Glu.UnProject(new Vector3(0, 0, 0), ref point);
            Vector2 pointA = new Vector2(point.X, point.Y);

            Glu.UnProject(new Vector3(20, 0, 0), ref point);
            Vector2 pointB = new Vector2(point.X, point.Y);

            selectionRadius = Vector2.Distance(pointA, pointB);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e) {
            base.OnKeyDown(e);

            switch (e.Key) {
                case Key.Right:
                    LoadFile(settings.NextFile);
                    pathSet = false;
                    OnResize();
                    break;
                case Key.Left:
                    LoadFile(settings.PreviousFile);
                    pathSet = false;
                    OnResize();
                    break;
                case Key.Up:
                    SwithPathFinder(currentPathFinder + 1);
                    break;
                case Key.Down:
                    SwithPathFinder(currentPathFinder - 1);
                    break;
            } 
        }

        private void SwithPathFinder(int index) {
            currentPathFinder = index < 0 ? pathFinders.Length+index : index >= pathFinders.Length ? index -pathFinders.Length : index ;
            map = pathFinders[currentPathFinder];
            map.Init(obstaclesCollection.Data);

            UpdateWindowTitle();
        }

        private void UpdateWindowTitle() {
            Title = "PathFinder2D [" +currentPathFinder +"] "+ map.GetType().Namespace.Replace("PathFinder.", "") + " (" + settings.CurrentFile + ")";
        }

        protected override void OnMouseMove(MouseMoveEventArgs e) {
            base.OnMouseMove(e);

            if (dragMode != DragMode.Start && dragMode != DragMode.End)
                return;

            Vector3 point = new Vector3(0, 0, 0);
            Glu.UnProject(new Vector3(e.X, e.Y, 0.0f), ref point);
            Vector2 point2 = new Vector2(point.X, point.Y);

            if (dragMode == DragMode.Start)
                start = point2;
            else
                end = point2;

            settings.StartAndEnd = new[] {start, end};
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);

            Vector3 point = new Vector3(0, 0, 0);
            Glu.UnProject(new Vector3(e.X, e.Y, 0.0f), ref point);
            Vector2 point2 = new Vector2(point.X, point.Y);

            if (e.Button == MouseButton.Left) {
                if (Vector2.Distance(start, point2) < selectionRadius) {
                    dragMode = DragMode.Start;
                    start = point2;
                } else if (Vector2.Distance(end, point2) < selectionRadius) {
                    dragMode = DragMode.End;
                    end = point2;
                }
            } else if (e.Button == MouseButton.Right) {
                pathSet = false;
                OnResize();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            dragMode = DragMode.None;
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
           
        //    tr.Draw();
            
            obstaclesCollection.Draw();

            if (obstaclesCollection.Initialized && !obstaclesCollection.Contains(start) && !obstaclesCollection.Contains(end)) {

                //  stopwatch.Restart();
                IEnumerable<Vector2> path = map.GetPath(start, end);

                //  stopwatch.Stop();    
                //    Console.WriteLine("Time elapsed (ms): {0}", stopwatch.Elapsed.TotalMilliseconds);

                // Сами линии пути
                {
                    GL.Color3(0.5f, 1.0f, 0.5f);
                    GL.LineWidth(3);
                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (Vector2 vertex in path)
                        GL.Vertex3(vertex.x, vertex.y, 0.0f);
                    GL.End();
                }

                // Точки на линии пути
                {
                    GL.Color3(1.0f, 1.0f, 0.5f);
                    GL.PointSize(6);
                    GL.Begin(PrimitiveType.Points);
                    foreach (Vector2 vertex in path)
                        GL.Vertex3(vertex.x, vertex.y, 0.0f);
                    GL.End();
                }
                
                Vector2 badSegmentStart = Vector2.zero;
                Vector2 badSegmentEnd = Vector2.zero;
                if (obstaclesCollection.Intersects(path, ref badSegmentStart, ref badSegmentEnd)) {
              //      Console.WriteLine("Intersection");
                    
                    GL.Color3(1f, 0.0f, 0.0f);
                    GL.LineWidth(3);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(badSegmentStart.x, badSegmentStart.y, 0.0f);
                    GL.Vertex3(badSegmentEnd.x, badSegmentEnd.y, 0.0f);
                    GL.End();
                }
            }

            // Яркими точечками - начало и конец
            GL.PointSize(12);

            GL.Color3(1.0f, 0.5f, 0.5f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(start.x, start.y, 0.0f);
            GL.End();

            GL.Color3(0.5f, 0.5f, 1.0f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(end.x, end.y, 0.0f);
            GL.End();
            
            SwapBuffers();
        }

        [STAThread]
        private static void Main() {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            MainTest mainTest = new MainTest();
            mainTest.TestMain();

            // using (Game game = new Game())
            // game.Run(30.0);
        }

        // Просто тестики массивов и листов
        private static void ArrayListTest() {
                        
            
            // последовательность случайных интов
            const int count = 100000000;
            double[] randoms = new double[count];
            List<double> randomsList = new List<double>(count);
            for (int i = 0; i < count; i++) {
                randoms[i] = Mathematics.Random.Range(0, 10000);
                randomsList.Add(randoms[i]);
            }

            const int repeats = 100;


            double[] precize = new double[repeats];
            double[] loop = new double[repeats];
            double[] foreachvar = new double[repeats];
            double[] foreachdouble = new double[repeats];
            double[] linq = new double[repeats];
           
            double[] precizeList = new double[repeats];
            double[] loopList = new double[repeats];
            double[] foreachvarList = new double[repeats];
            double[] foreachdoubleList = new double[repeats];
            double[] linqList = new double[repeats];
            
            for (int j = 0; j < repeats; j++) {

                Stopwatch stopwatch = new Stopwatch();
                double sum;

                {
                    stopwatch.Restart();
                    sum = 0;
                    for (int i = 0; i < count; i++)
                        sum += randoms[i];
                    stopwatch.Stop();
                    Console.WriteLine("Presize (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);

                    precize[j] = stopwatch.Elapsed.TotalMilliseconds;
                }


                {
                    stopwatch.Restart();
                    sum = 0;
                    for (int i = 0; i < randoms.Length; i++)
                        sum += randoms[i];
                    stopwatch.Stop();
                    Console.WriteLine("Loop (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    loop[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                {
                    stopwatch.Restart();
                    sum = 0;
                    foreach (var i in randoms)
                        sum += i;

                    stopwatch.Stop();
                    Console.WriteLine("Foreach var (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    foreachvar[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                {
                    stopwatch.Restart();
                    sum = 0;
                    foreach (double i in randoms)
                        sum += i;

                    stopwatch.Stop();
                    Console.WriteLine("Foreach int (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    foreachdouble[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                {
                    stopwatch.Restart();
                    sum = randoms.Sum();
                    stopwatch.Stop();
                   Console.WriteLine("Linq (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    linq[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                //===================================================
                // То же с листом
                
                {
                    stopwatch.Restart();
                    sum = 0;
                    for (int i = 0; i < count; i++)
                        sum += randomsList[i];
                    stopwatch.Stop();
                    Console.WriteLine("Presize (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);

                    precizeList[j] = stopwatch.Elapsed.TotalMilliseconds;
                }


                {
                    stopwatch.Restart();
                    sum = 0;
                    for (int i = 0; i < randoms.Length; i++)
                        sum += randomsList[i];
                    stopwatch.Stop();
                    Console.WriteLine("Loop (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    loopList[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                {
                    stopwatch.Restart();
                    sum = 0;
                    foreach (var i in randomsList)
                        sum += i;

                    stopwatch.Stop();
                    Console.WriteLine("Foreach var (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    foreachvarList[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                {
                    stopwatch.Restart();
                    sum = 0;
                    foreach (double i in randomsList)
                        sum += i;

                    stopwatch.Stop();
                    Console.WriteLine("Foreach int (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    foreachdoubleList[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
                
                {
                    stopwatch.Restart();
                    sum = randomsList.Sum();
                    stopwatch.Stop();
                    Console.WriteLine("Linq (ms): {0} | {1}", stopwatch.Elapsed.TotalMilliseconds, sum);
                    
                    linqList[j] = stopwatch.Elapsed.TotalMilliseconds;
                }
            }
            
            Console.WriteLine(
                "precize: " + precize.Average() + "\n" +
                "loop: " + loop.Average() + "\n" +
                "foreachdouble: " + foreachdouble.Average() + "\n" +
                "foreachvar: " + foreachvar.Average() + "\n" +
                "linq: " + linq.Average() + "\n\n" +
                
                "precizeList: " + precizeList.Average() + "\n" +
                "loopList: " + loopList.Average() + "\n" +
                "foreachdoubleList: " + foreachdoubleList.Average() + "\n" +
                "foreachvarList: " + foreachvarList.Average() + "\n" +
                "linqList: " + linqList.Average() + "\n" 
                );

        }
    }
}