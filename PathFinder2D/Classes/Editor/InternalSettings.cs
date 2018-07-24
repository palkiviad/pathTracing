using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using PathFinder.Mathematics;
using System.Globalization;
using System.Linq;

namespace PathFinder.Editor {

    public class InternalSettings {

        public const string SettingsDir = "ReleaseData";
        private const string SettingsFile = SettingsDir+ "/Settings.dat";

        private Size size;

        public Size Size {
            get { return size; }
            set {
                if (value.Height == size.Height && value.Width == size.Width)
                    return;

                size = value;
                Save();
            }
        }

        private Point location;

        public Point Location {
            get { return location; }
            set {
                if (value.X == location.X && value.Y == location.Y)
                    return;

                location = value;
                Save();
            }
        }

        private string currentFile;

        public string CurrentFile {
            get { return currentFile; }
            set {
                currentFile = value;
                Save();
            }
        }

        private readonly object savingThreadLock = new object();
        private Thread savingThread;

        private Vector2[] startAndEnd;

        public Vector2[] StartAndEnd {
            get { return startAndEnd; }
            set {
                startAndEnd = value;

                lock (savingThreadLock) {
                    if (savingThread != null)
                        return;

                    savingThread = new Thread(() => {
                        Thread.Sleep(1000);
                        Save();
                        lock (savingThreadLock) {
                            savingThread = null;
                        }
                    });
                    savingThread.Start();
                }
            }
        }

        public InternalSettings() {
            if (!Load())
                SetDefaults();
        }

        private void SetDefaults() {
            Size = new Size(800, 600);
            Location = new Point(int.MinValue, int.MinValue);
            CurrentFile = NextFile;
        }

        public string NextFile {
            get { return ShiftFile(true); }
        }

        public string PreviousFile {
            get { return ShiftFile(false); }
        }

        private string ShiftFile(bool forward) {
            string[] files = Directory.EnumerateFiles(SettingsDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".txt") || s.EndsWith(".svg")).ToArray(); 

            if (files.Length == 0)
                return null;

            if (files.Length == 1 || CurrentFile == null)
                return files[0];

            int currentIndex = Array.IndexOf(files, CurrentFile);

            if (currentIndex == -1)
                return files[0];

            int index = currentIndex + (forward ? 1 : -1);
            if (index >= files.Length)
                index = 0;
            else if (index < 0)
                index = files.Length - 1;

            return currentIndex == -1 ? files[0] : files[index];
        }

        private void Save() {
            List<string> lines = new List<string>();

            lines.Add("Size=" + (Size.Width == 0 || Size.Height == 0 ? "800,600" : Size.Width + "," + Size.Height));

            if (location.X != int.MinValue && location.Y != int.MinValue)
                lines.Add("Location=" + Location.X + "," + Location.Y);

            if (CurrentFile != null)
                lines.Add("CurrentFile=" + CurrentFile);

            if (StartAndEnd != null)
                lines.Add("StartAndEnd=" +
                          StartAndEnd[0].x.ToString(CultureInfo.InvariantCulture) + "," +
                          StartAndEnd[0].y.ToString(CultureInfo.InvariantCulture) + "," +
                          StartAndEnd[1].x.ToString(CultureInfo.InvariantCulture) + "," +
                          StartAndEnd[1].y.ToString(CultureInfo.InvariantCulture));

            File.WriteAllLines(SettingsFile, lines);
        }

        private static string GetValue(string line, string prefix) {
            return line.Substring(prefix.Length, line.Length - prefix.Length);
        }

        private bool Load() {
            if (!File.Exists(SettingsFile))
                return false;

            try {
                string[] lines = File.ReadAllLines(SettingsFile);
                foreach (string line in lines) {
                    if (line.StartsWith("Size=")) {
                        string[] values = GetValue(line, "Size=").Split(',');
                        if (values.Length != 2)
                            throw new Exception("Broken ");
                        size = new Size(int.Parse(values[0]), int.Parse(values[1]));
                    } else if (line.StartsWith("Location=")) {
                        string[] values = GetValue(line, "Location=").Split(',');
                        if (values.Length != 2)
                            throw new Exception("Broken ");
                        location = new Point(int.Parse(values[0]), int.Parse(values[1]));
                    } else if (line.StartsWith("CurrentFile=")) {
                        CurrentFile = GetValue(line, "CurrentFile=");
                        if (!File.Exists(CurrentFile))
                            currentFile = null;
                    } else if (line.StartsWith("StartAndEnd=")) {
                        string[] pathStr = GetValue(line, "StartAndEnd=").Split(',');

                        startAndEnd = new[] {
                            new Vector2(float.Parse(pathStr[0], CultureInfo.InvariantCulture), float.Parse(pathStr[1], CultureInfo.InvariantCulture)),
                            new Vector2(float.Parse(pathStr[2], CultureInfo.InvariantCulture), float.Parse(pathStr[3], CultureInfo.InvariantCulture))
                        };
                    }
                }
            } catch (Exception) {
                return false;
            }

            return true;
        }
    }
}