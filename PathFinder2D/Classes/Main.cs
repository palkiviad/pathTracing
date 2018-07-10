using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PathFinder.Editor;
using Vector2 = PathFinder.Mathematics.Vector2;

namespace PathFinder {
    internal sealed class Game : GameWindow {
        
        private readonly IMap map = new Popov.MapV2();
        
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
        readonly Stopwatch stopwatch = new Stopwatch();

        private Game() : base(800, 600, GraphicsMode.Default, "Path Finder") {
            VSync = VSyncMode.On;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        private void LoadFile(string file) {
            if (file == null || !File.Exists(file))
                throw new Exception("Cannot load data file");

            obstaclesCollection = new InternalObstaclesCollection(file);
            map.Init(obstaclesCollection.Data);

            OnResize();

            settings.CurrentFile = file;
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

            if (e.Key == Key.Right) {
                LoadFile(settings.NextFile);
                pathSet = false;
                OnResize();
            } else if (e.Key == Key.Left) {
                LoadFile(settings.PreviousFile);
                pathSet = false;
                OnResize();
            }
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

            obstaclesCollection.Draw();

            if (!obstaclesCollection.Contains(start) && !obstaclesCollection.Contains(end)) {

                // Обычными линиями - середина
                // Если захотим вдруг изменить что-нибудь в дебаге
                
              //  stopwatch.Restart();
                IEnumerable<Vector2> path = map.GetPath(start, end);

              //  stopwatch.Stop();    
            //    Console.WriteLine("Time elapsed (ms): {0}", stopwatch.Elapsed.TotalMilliseconds);

                // Сами линии пути
                {
                    GL.Color3(0.5f, 1.0f, 0.5f);
                    GL.LineWidth(3);
                    GL.Begin(BeginMode.LineStrip);
                    foreach (Vector2 vertex in path)
                        GL.Vertex3(vertex.x, vertex.y, 0.0f);
                    GL.End();
                }
                
                // Точки на линии пути
                {
                    GL.Color3(1.0f, 1.0f, 0.5f);
                    GL.PointSize(6);
                    GL.Begin(BeginMode.Points);
                    foreach (Vector2 vertex in path)
                        GL.Vertex3(vertex.x, vertex.y, 0.0f);
                    GL.End();
                }

            }
            
            // Яркими точечками - начало и конец
            GL.PointSize(12);
            
            GL.Color3(1.0f, 0.5f, 0.5f);
            GL.Begin(BeginMode.Points);
            GL.Vertex3(start.x, start.y, 0.0f);
            GL.End();
            
            GL.Color3(0.5f, 0.5f, 1.0f);
            GL.Begin(BeginMode.Points);
            GL.Vertex3(end.x, end.y, 0.0f);
            GL.End();

            SwapBuffers();
        }

        [STAThread]
        private static void Main() {
            using (Game game = new Game()) {
                game.Run(30.0);
            }
        }
    }
}