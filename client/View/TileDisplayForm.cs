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

using TCPGameSharedInfo;

namespace TCPGameClient.View
{
    public partial class TileDisplayForm : Form
    {
        // base size
        int sizeX;
        int sizeY;

        // controller running everything
        private Controller control;
        // model to be drawn
        private LocalModel theModel;

        // image buffer containing preloaded images
        private ImageBuffer imageBuffer;

        // bool which indicated the drawing context is free
        private volatile bool canDraw = true;

        // to close the form safely
        private delegate void CloseCallback();

        // constants to determine corner to draw strings in
        public const int LEFTUP     = 0;
        public const int RIGHTUP    = 1;
        public const int LEFTDOWN   = 2;
        public const int RIGHTDOWN  = 3;

        // automatic code by Visual Studio
        public TileDisplayForm(Controller control, LocalModel theModel)
        {
            this.control = control;
            this.theModel = theModel;

            InitializeComponent();
        }

        // image buffer and controller are created on launch
        private void TileDisplayForm_Load(object sender, EventArgs e)
        {
            // set base size
            sizeX = 64;
            sizeY = 64;

            // imagebuffer loads images with default size 64x64
            imageBuffer = new ImageBuffer(sizeX, sizeY);
        }

        public void SetZoom(int zoomLevelX, int zoomLevelY)
        {
            sizeX = zoomLevelX;
            sizeY = zoomLevelY;

            imageBuffer.SetDefault(sizeX, sizeY);
        }

        // draws the model onto the form
        public void DrawModel()
        {       
            if (WindowState == FormWindowState.Minimized) return;

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
            int tilesX = pictureBox1.Width / sizeX + 2;
            int tilesY = pictureBox1.Height / sizeY + 2;

            // position of the player on the grid (always in the center)
            int playerPositionX = theModel.GetGridSizeX() / 2;
            int playerPositionY = theModel.GetGridSizeY() / 2;
            int playerPositionZ = theModel.GetGridSizeZ() / 2;

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
                        TileRepresentation fieldRepresentation = fieldToDraw.GetRepresentation();

                        // get the actual image from the image buffer, with the name as a key
                        Image imToDraw = imageBuffer.GetImage(fieldRepresentation);

                        // draw the image onto the bitmap
                        g.DrawImage(imToDraw, centerX + x * sizeX - (sizeX / 2), centerY - y * sizeY + sizeY / 2, sizeX, sizeY);

                        // draw a creature if it's on the field
                        Image creatureImage = drawCreature(fieldToDraw);

                        if (creatureImage != null) 
                            g.DrawImage(creatureImage, centerX + x * sizeX - sizeX / 4, centerY - y * sizeY + sizeY / 4 * 3, sizeX / 2, sizeY / 2);
                    }
                }
            }

            List<String> receivedMessages = theModel.GetReceivedMessages();
            List<String> playerList = theModel.GetPlayerList();

            DrawStrings(receivedMessages, g,LEFTDOWN);
            DrawStrings(playerList, g, LEFTUP);

            // dispose of the graphics object
            g.Dispose();
            // set the image of the picturebox to be the buffer
            pictureBox1.Image = drawBuffer;

            // re-allow drawing
            canDraw = true;
        }

        public void DrawMessages()
        {
            return;

            if (WindowState == FormWindowState.Minimized) return;

            // check if we can draw. If we can, noone else can until we're done 
            if (!canDraw) return;
            canDraw = false;

            // create bitmap to draw on
            Image drawBuffer = pictureBox1.Image;
            Graphics g;

            if (drawBuffer == null)
            {
                drawBuffer = new Bitmap(pictureBox1.Width, pictureBox1.Height);

                // create graphics object for buffer
                g = Graphics.FromImage(drawBuffer);

                // make the buffer solid black
                g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            }
            else
            {
                // create graphics object for buffer
                g = Graphics.FromImage(drawBuffer);
            }

            List<String> receivedMessages = theModel.GetReceivedMessages();

            DrawStrings(receivedMessages, g,LEFTDOWN);
            pictureBox1.Image = drawBuffer;

            g.Dispose();

            canDraw = true;
        }

        private void DrawStrings(List<String> stringList, Graphics g, int corner, int maxNumberOfMessages = 10)
        {
            List<string> localCopy = new List<string>();
            foreach (string str in stringList)
            {
                string newstr = string.Copy(str);
                localCopy.Add(newstr);
            }

            SizeF totalStringSize = new SizeF();
            Font drawFont = new System.Drawing.Font("Arial", 8);
            SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Pink);
            StringFormat drawFormat = new System.Drawing.StringFormat();

            for (int i = Math.Max(0, localCopy.Count - maxNumberOfMessages); i < localCopy.Count; i++)
            {
                String[] message = localCopy[i].Split(new char[] { ',' }, 4); // split in 4 parts: time, command, from, and message.
                localCopy[i] = message[2] + ": " + message[3];
                SizeF stringSize = new SizeF();
                stringSize = g.MeasureString(localCopy[i], drawFont);
                totalStringSize.Height += stringSize.Height + 2;
                totalStringSize.Width = Math.Max(stringSize.Width, totalStringSize.Width);
            }
            int x = 0, y = 0, width = 0, height = 0;
            switch (corner)
            {
                case LEFTUP:
                    width = (int)Math.Ceiling(totalStringSize.Width);
                    height = (int)Math.Ceiling(totalStringSize.Height);
                    break;
                case RIGHTUP:
                    x = pictureBox1.Width - (int)Math.Ceiling(totalStringSize.Width);
                    width = (int)Math.Ceiling(totalStringSize.Width);
                    height = (int)Math.Ceiling(totalStringSize.Height);
                    break;
                case LEFTDOWN:
                    y = pictureBox1.Height - (int)Math.Ceiling(totalStringSize.Height);
                    width = (int)Math.Ceiling(totalStringSize.Width);
                    height = pictureBox1.Height;
                    break;
                case RIGHTDOWN:
                    x = pictureBox1.Width - (int)Math.Ceiling(totalStringSize.Width);
                    y = pictureBox1.Height - (int)Math.Ceiling(totalStringSize.Height);
                    width = (int)Math.Ceiling(totalStringSize.Width);
                    height = pictureBox1.Height;
                    break;
                default:
                    break;
            }

            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(x, y, width, height));

            int curHeight = y;
            for (int i = Math.Max(0, localCopy.Count - 10); i < localCopy.Count; i++)
            {
                SizeF stringSize = new SizeF();
                stringSize = g.MeasureString(localCopy[i], drawFont);
                g.DrawString(localCopy[i], drawFont, drawBrush, 0, curHeight, drawFormat);
                curHeight += (int)stringSize.Height + 2;
            }
            drawFont.Dispose();
            drawBrush.Dispose();
        }

        private Image drawCreature(Field fieldToDraw)
        {
            Creature creature = fieldToDraw.GetCreature();

            // get the representation of the creature on the field
            CreatureRepresentation creatureRepresentation = creature.representation;

            if (creatureRepresentation != CreatureRepresentation.None)
            {
                // get the image from the image buffer
                Image baseImage = imageBuffer.GetImage(creatureRepresentation);

                // draw the image onto a bitmap
                Bitmap bmpImage = new Bitmap(sizeX, sizeY);

                Graphics g = Graphics.FromImage(bmpImage);

                g.DrawImage(baseImage, 1, 1, sizeX, sizeY);

                // draw hitpoint and mana bars
                double hpRatio = creature.currentHitpoints / (double)creature.maxHitpoints;
                int hpWidth = (int) (hpRatio * sizeX);
                double manaRatio = creature.currentMana / (double)creature.maxMana;
                int manaWidth = (int) (manaRatio * sizeX);

                g.DrawRectangle(new Pen(new SolidBrush(Color.LightBlue)), new Rectangle(1, sizeY - 4, manaWidth, 2));
                g.DrawRectangle(new Pen(new SolidBrush(Color.Red)), new Rectangle(1, sizeY - 6, hpWidth, 2));

                return (Image)bmpImage;
            }
            else return null;
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

        private void TileDisplayForm_SizeChanged(object sender, EventArgs e)
        {
            control.Redraw();
		}

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && !e.Shift)
            {
                control.SendInput("n");
            }
            else if (e.KeyCode == Keys.Down && !e.Shift)
            {
                control.SendInput("s");
            }
            else if (e.KeyCode == Keys.Left)
            {
                control.SendInput("w");
            } 
            else if (e.KeyCode == Keys.Right)
            {
                control.SendInput("e");
            }
            else if (e.KeyCode == Keys.Up && e.Shift)
            {
                control.SendInput("u");
            }
            else if (e.KeyCode == Keys.Down && e.Shift)
            {
                control.SendInput("d");
            }
        }
    }
}
