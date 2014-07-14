using System;
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

        // current online list
        private List<String> playersOnline;
        private List<String> receivedMessages;

        // image buffer containing preloaded images
        private ImageBuffer imageBuffer;

        // bool which indicated the drawing context is free
        private volatile bool canDraw = true;

        // to close the form safely
        private delegate void CloseCallback();

        // automatic code by Visual Studio
        public TileDisplayForm()
        {
            InitializeComponent();
        }

        public void AddReceivedMessage(string receivedMessage)
        {
            receivedMessages.Add(receivedMessage);
        }

        // image buffer and controller are created on launch
        private void TileDisplayForm_Load(object sender, EventArgs e)
        {
            // set base size
            sizeX = 64;
            sizeY = 64;

            // imagebuffer loads images with default size 64x64
            imageBuffer = new ImageBuffer(sizeX, sizeY);

            // controller is created, this "starts the program"
            control = new Controller(this);
            receivedMessages = new List<string>();
            playersOnline = new List<string>();
        }


        public void SetZoom(int zoomLevelX, int zoomLevelY)
        {
            sizeX = zoomLevelX;
            sizeY = zoomLevelY;

            imageBuffer.SetDefault(sizeX, sizeY);
        }

        // draws the model onto the form
        public void DrawModel(LocalModel theModel)
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
                    }
                }
            }

            // draw creatures onto the grid
            foreach (Creature creature in theModel.GetCreatures())
            {
                int xPos = creature.GetX();
                int yPos = creature.GetY();
                int zPos = creature.GetZ();

                if (xPos >= minX && xPos <= maxX &&
                    yPos >= minY && yPos <= maxY &&
                    zPos == 0)
                {
                    CreatureRepresentation creatureRepresentation = creature.GetRepresentation();

                    Image imToDraw = imageBuffer.GetImage(creatureRepresentation);

                    g.DrawImage(imToDraw, centerX + xPos * sizeX - sizeX / 4, centerY - yPos * sizeY + sizeY / 4 * 3, sizeX / 2, sizeY / 2);
                }
            }
            DrawStrings(receivedMessages, g);
            // dispose of the graphics object
            g.Dispose();
            // set the image of the picturebox to be the buffer
            pictureBox1.Image = drawBuffer;

            // re-allow drawing
            canDraw = true;
        }

        public void DrawMessages()
        {          
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

            // create graphics object for buffer
            g = Graphics.FromImage(drawBuffer);

            DrawStrings(receivedMessages, g);
            pictureBox1.Image = drawBuffer;
                
            canDraw = true;           
        }

        private void DrawStrings(List<String> stringList, Graphics g)
        {
            List<string> localCopy = new List<string>();
            foreach(string str in stringList) {
                string newstr = string.Copy(str);
                localCopy.Add(newstr);
            }

            SizeF totalStringSize = new SizeF();
            Font drawFont = new System.Drawing.Font("Arial", 8);
            SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Pink);
            StringFormat drawFormat = new System.Drawing.StringFormat();

            for (int i = Math.Max(0, localCopy.Count - 10); i < localCopy.Count; i++)
            {
                String[] message = localCopy[i].Split(new char[] { ',' }, 4); // split in 4 parts: time, command, from, and message.
                localCopy[i] = message[2] + ": " + message[3];
                SizeF stringSize = new SizeF();
                stringSize = g.MeasureString(localCopy[i], drawFont);
                totalStringSize.Height += stringSize.Height + 2;
                totalStringSize.Width =  Math.Max(stringSize.Width,totalStringSize.Width);
            }

            int begin = pictureBox1.Height - (int)Math.Ceiling( totalStringSize.Height);
            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, begin, (int)Math.Ceiling(totalStringSize.Width), pictureBox1.Height));

            int curHeight = begin;
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
