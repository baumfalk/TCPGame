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
        // controller, handles communication with server and model
        private Controller control;

        // image buffer containing preloaded images
        private ImageBuffer imageBuffer;

        // list of input put into the textbox between updates.
        private List<String> inputList = new List<String>();

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

        // input request by the controller, has to return a list of all input given since last request
        public List<String> getInput()
        {
            // can't clear the list after returning it, so I worked around that
            List<String> returnList = new List<String>(inputList);

            inputList.Clear();
            return returnList;
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
            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(10, 10, pictureBox1.Width, pictureBox1.Height));

            // centerx/centery indicate the center of the drawing area
            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            // draws an 11x11 grid around the player
            // TODO: make this are more sensible, matching the size of the picturebox
            for (int x = -5; x < 6; x++)
            {
                for (int y = -5; y < 6; y++)
                {
                    // check if a field exists to draw
                    if (theModel.hasFieldAtPosition(11 + x, 11 + y))
                    {
                        // get that field
                        Field fieldToDraw = theModel.getFieldAtPosition(11 + x, 11 + y);

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

                inputList.Add(input);
                textBox1.Clear();
            }
        }
    }
}
