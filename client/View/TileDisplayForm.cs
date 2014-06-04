﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

using TCPGameClient.Control;
using TCPGameClient.Model;

namespace TCPGameClient.View
{
    public partial class TileDisplayForm : Form
    {
        // base size
        int baseSize;

        // controller running everything
        private Controller control;

        // image buffer containing preloaded images
        private ImageBuffer imageBuffer;

        // bool which indicated the drawing context is free
        private bool canDraw = true;

        // to close the form safely
        private delegate void CloseCallback();

        // automatic code by Visual Studio
        public TileDisplayForm()
        {
            InitializeComponent();
        }

        // image buffer and controller are created on launch
        private void TileDisplayForm_Load(object sender, EventArgs e)
        {
            // set base size
            baseSize = 16;

            // imagebuffer loads images with default size 64x64
            imageBuffer = new ImageBuffer(baseSize, baseSize);

            // controller is created, this "starts the program"
            control = new Controller(this);
        }

        // draws the model onto the form
        public void DrawModel(LocalModel theModel)
        {
            // check if we can draw. If we can, noone else can until we're done
            if (!canDraw) return;
            canDraw = false;

            // create bitmap to draw on
            Image drawBuffer = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            // create graphics object for buffer
            Graphics g = Graphics.FromImage(drawBuffer);

            // make the buffer solid black
            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));

            // centerx/centery indicate the center of the drawing area
            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            // number of tiles to draw. Intentionally overestimates the amount needed to make sure the space
            // gets filled
            int tilesX = pictureBox1.Width / baseSize + 2;
            int tilesY = pictureBox1.Height / baseSize + 2;

            // position of the player on the grid (always in the center)
            int playerPositionX = theModel.GetGridSizeX() / 2 + 1;
            int playerPositionY = theModel.GetGridSizeY() / 2 + 1;
            int playerPositionZ = theModel.GetGridSizeZ() / 2 + 1;

            // draws a 2D grid around the player.
            int minX = -tilesX / 2;
            int maxX = tilesX / 2 - 1;
            int minY = -tilesY / 2;
            int maxY = tilesY / 2 - 1;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // check if a field exists to draw
                    if (theModel.HasFieldAtPosition(playerPositionX + x, playerPositionY + y, playerPositionZ))
                    {
                        // get that field
                        Field fieldToDraw = theModel.GetFieldAtPosition(playerPositionX + x, playerPositionY + y, playerPositionZ);

                        // get the internal name of it's image
                        String fieldRepresentation = fieldToDraw.GetRepresentation();

                        // get the actual image from the image buffer, with the name as a key
                        Image imToDraw = imageBuffer.GetImage(fieldRepresentation);

                        // draw the image onto the bitmap
                        g.DrawImage(imToDraw, centerX + x * baseSize - (baseSize / 2), centerY + y * baseSize - baseSize / 2, baseSize, baseSize);
                    }
                }
            }

            // draw creatures onto the grid
            foreach (Creature creature in theModel.GetCreatures()) {
                int xPos = creature.GetX();
                int yPos = creature.GetY();
                int zPos = creature.GetZ();

                if (xPos >= minX && xPos <= maxX &&
                    yPos >= minY && yPos <= maxY &&
                    zPos == 0)
                {
                    String creatureRepresentation = creature.GetRepresentation();

                    Image imToDraw = imageBuffer.GetImage(creatureRepresentation);

                    g.DrawImage(imToDraw, centerX + xPos * baseSize - baseSize / 4, centerY + yPos * baseSize - baseSize / 4, baseSize / 2, baseSize / 2);
                }
            }

            // dispose of the graphics object
            g.Dispose();

            // set the image of the picturebox to be the buffer
            pictureBox1.Image = drawBuffer;

            // re-allow drawing
            canDraw = true;
        }

        // checks if input in the textbox is a full line and adds it to the list of inputs
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // a line is marked by a newline character at the end
            if (textBox1.Text.EndsWith("\n"))
            {
                // add it to the input list with the carriage return / line feed replaced
                String input = textBox1.Text.Replace("\r\n", "");

                control.SendInput(input);
                
                textBox1.Clear();
            }
        }

        // Closes the form safely
        public void Stop()
        {
            // check if this is the thread that created the form
            if (this.InvokeRequired)
            {
                // if it's not, create a callback and invoke it
                try
                {
                    CloseCallback d = new CloseCallback(Stop);
                    this.Invoke(d);
                }
                catch (ObjectDisposedException e)
                {
                    System.Diagnostics.Debug.Print("form was disposed on close?");
                    System.Diagnostics.Debug.Print(e.Message);
                }
            }
            else
            {
                // if it is, close the form
                Close();
            }
        }
    }
}
