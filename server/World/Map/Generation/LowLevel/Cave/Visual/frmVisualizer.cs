﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;

using TCPGameServer.World.Map.Generation.LowLevel.Connections;
using TCPGameServer.World.Map.Generation.LowLevel.Values;
using TCPGameServer.World.Map.Generation.LowLevel.Tiles;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave.Visual
{
    public partial class frmVisualizer : Form
    {
        private Visualizer visualizer;

        private Valuemap valuemap;
        private Connectionmap connectionmap;
        private Tilemap tilemap;

        private Bitmap bmpDraw;
        private Location lastLocation;

        bool running;
        int speed;

        private static Color[] partitionColors = new Color[] { Color.Blue, Color.Green, Color.Aqua, Color.Magenta, Color.Yellow, Color.Pink };

        public frmVisualizer(Visualizer visualizer)
        {
            this.visualizer = visualizer;

            InitializeComponent();

            valuemap = visualizer.getValuemap();
            connectionmap = visualizer.getConnectionmap();
            tilemap = visualizer.getTilemap();

            FirstDraw();

            speed = trackBar1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            visualizer.takeStep();
        }

        private void FirstDraw()
        {
            bmpDraw = new Bitmap(1000, 1000);

            Graphics g = Graphics.FromImage(bmpDraw);

            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    DrawCell(new Location(x, y, 0), g, false);
                }
            }

            g.Dispose();

            pictureBox1.Image = bmpDraw;
        }

        private void DrawCell(Location location, Graphics g, bool updated)
        {
            if (location == null) return;

            int x = location.x;
            int y = location.y;

            SolidBrush background;
            if (updated)
            {
                background = new SolidBrush(Color.Red);
            }
            else
            {
                int value = valuemap.GetValue(location);

                background = new SolidBrush(Color.FromArgb(value, value, value));
            }

            g.FillRectangle(background, x * 10, 1000 - y * 10, 10, 10);

            Partition partition = connectionmap.CheckPlacement(location);

            if (partition != null)
            {
                int index = partition.GetIndex();

                SolidBrush partitionBrush = new SolidBrush(partitionColors[index]);

                g.FillRectangle(partitionBrush, x * 10 + 2, 1000 - y * 10 + 2, 6, 6);
            }

            pictureBox1.Invalidate();
        }

        public void DoUpdate(Location updated)
        {

            Graphics g = Graphics.FromImage(bmpDraw);

            DrawCell(lastLocation, g, false);
            DrawCell(updated, g, true);

            g.Dispose();

            lastLocation = updated;

            if (running)
            {
                Thread.Sleep(speed);
                visualizer.takeStep();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            visualizer.takeStep();

            running = !running;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            speed = trackBar1.Value;
        }

        private void frmVisualizer_Load(object sender, EventArgs e)
        {
            visualizer.indicatedLoaded();
        }
    }
}
