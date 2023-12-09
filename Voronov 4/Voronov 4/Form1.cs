using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Voronov_4
{
    public partial class Form1 : Form
    {
        private Point[] points = new Point[0];
        private Random random = new Random();
        private Color[] colors = new Color[0];
        static Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        Graphics g = Graphics.FromImage(bitmap);
        private bool multithreadingEnabled = false;
        Stopwatch stopwatch = new Stopwatch();
        public Form1()
        {
            InitializeComponent();    
        }

        private void CreatePoints()
        {
            int count = (int)numericUpDown1.Value;
            points = new Point[count];
            colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = new Point(random.Next(0, pictureBox1.Width), random.Next(0, pictureBox1.Height));
                colors[i] = Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
            }
        }

        private void DrawVoronoi(object threadRange)
        {
            int[] range = (int[])threadRange;
            int startY = range[0];
            int endY = range[1];

            for (int x = 0; x < this.ClientSize.Width; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    int minDistance = int.MaxValue;
                    for (int i = 0; i < points.Length; i++)
                    {
                        int distance = (x - points[i].X) * (x - points[i].X) + (y - points[i].Y) * (y - points[i].Y);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            lock (g)
                            {
                                g.FillRectangle(new SolidBrush(colors[i]), x, y, 1, 1);
                            }
                        }
                    }
                }
            }
        }
        private void DrawVoronoiOneThread()
        {
            stopwatch.Restart();
            stopwatch.Start();
            TimeSpan timeSpan = stopwatch.Elapsed;
            g.Clear(Color.White);
            CreatePoints();
            if (points != null && points.Length > 0)
            {

                for (int x = 0; x < this.ClientSize.Width; x++)
                {
                    for (int y = 0; y < this.ClientSize.Height; y++)
                    {
                        int minDistance = int.MaxValue;
                        for (int i = 0; i < points.Length; i++)
                        {
                            int distance = (x - points[i].X) * (x - points[i].X) + (y - points[i].Y) * (y - points[i].Y);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                g.FillRectangle(new SolidBrush(colors[i]), x, y, 1, 1);
                            }
                        }
                    }
                }
            }
            foreach (Point p in points)
            {
                g.FillEllipse(Brushes.Black, p.X - 3, p.Y - 3, 6, 6);
            }
            pictureBox1.Image = bitmap;
            stopwatch.Stop();
            label1.Text = timeSpan.TotalMilliseconds.ToString() + " mc";
        }
        private void DrawVoronoiMultiThreads()
        {
            stopwatch.Restart();
            stopwatch.Start();
            TimeSpan timeSpan = stopwatch.Elapsed;
            g.Clear(Color.White);
            CreatePoints();
            if (points != null && points.Length > 0)
            {
                int numThreads = Environment.ProcessorCount;
                int sliceHeight = this.ClientSize.Height / numThreads;
                Thread[] threads = new Thread[numThreads];

                for (int i = 0; i < numThreads; i++)
                {
                    int startY = i * sliceHeight;
                    int endY = (i + 1) * sliceHeight;
                    if (i == numThreads - 1) // For the last thread, process the remaining rows
                    {
                        endY = this.ClientSize.Height;
                    }
                    int[] range = new int[] { startY, endY };
                    threads[i] = new Thread(new ParameterizedThreadStart(DrawVoronoi));
                    threads[i].Start(range);
                }
                for (int i = 0; i < numThreads; i++)
                {
                    threads[i].Join(); // Wait for all threads to finish
                }
            }
            foreach (Point p in points)
            {
                g.FillEllipse(Brushes.Black, p.X - 3, p.Y - 3, 6, 6);
            }
            pictureBox1.Image = bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stopwatch.Start();
            if (multithreadingEnabled) DrawVoronoiMultiThreads();
            else DrawVoronoiOneThread();
            TimeSpan timeSpan = stopwatch.Elapsed;
            stopwatch.Stop();
            label1.Text = timeSpan.TotalMilliseconds.ToString() + " mc";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            multithreadingEnabled = checkBox1.Checked;
        }
    }
}
