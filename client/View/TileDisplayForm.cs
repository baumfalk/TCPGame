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

namespace TCPGameClient.View
{
    public partial class TileDisplayForm : Form
    {
        // controller running everything
        private Controller control;

        // image buffer containing preloaded images
        private ImageBuffer imageBuffer;

        // bool which indicated the drawing context is free
        private bool canDraw = true;

        // automatic code by Visual Studio
        public TileDisplayForm()
        {
            InitializeComponent();
        }

        // image buffer and controller are created on launch
        private void TileDisplayForm_Load(object sender, EventArgs e)
        {
            // imagebuffer loads images with default size 64x64
            imageBuffer = new ImageBuffer(64, 64);

            // controller is created, this "starts the program"
            control = new Controller(this);
        }

        // draws the model onto the form
        public void drawModel(LocalModel theModel)
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
            int tilesX = pictureBox1.Width / 64 + 2;
            int tilesY = pictureBox1.Height / 64 + 2;

            int positionX = theModel.getGridSizeX() / 2 + 1;
            int positionY = theModel.getGridSizeY() / 2 + 1;
            int positionZ = theModel.getGridSizeZ() / 2 + 1;

            // draws a 2D grid around the player.
            for (int x = -tilesX / 2; x < tilesX / 2; x++)
            {
                for (int y = -tilesY / 2; y < tilesY / 2; y++)
                {
                    // check if a field exists to draw
                    if (theModel.hasFieldAtPosition(positionX + x, positionY + y, positionZ))
                    {
                        // get that field
                        Field fieldToDraw = theModel.getFieldAtPosition(positionX + x, positionY + y, positionZ);

                        // get the internal name of it's image
                        String fieldRepresentation = fieldToDraw.getRepresentation();

                        // get the actual image from the image buffer, with the name as a key
                        Image imToDraw = imageBuffer.getImage(fieldRepresentation);

                        // draw the image onto the bitmap
                        g.DrawImage(imToDraw, centerX + x * 64 - 32, centerY + y * 64 - 32, 64, 64);
                    }
                }
            }

            // draw the player in the center
            g.DrawImage(imageBuffer.getImage("player"), centerX - 16, centerY - 16, 32, 32);

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

                if (input.Equals("connect"))
                {
                    control.Connect();
                }
                else
                {
                    control.sendInput(input);
                }
                textBox1.Clear();
            }
        }
    }
}
