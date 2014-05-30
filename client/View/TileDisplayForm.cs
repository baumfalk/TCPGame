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
        private Controller control;
        private ImageBuffer imageBuffer;
        private List<String> inputList = new List<String>();

        public TileDisplayForm()
        {
            InitializeComponent();
        }

        private void TileDisplayForm_Load(object sender, EventArgs e)
        {
            imageBuffer = new ImageBuffer(64, 64);

            control = new Controller(this);
        }

        // beetje lelijk, maar werkt wel
        public List<String> getInput()
        {
            List<String> returnList = new List<String>(inputList);

            inputList.Clear();
            return returnList;
        }

        public void drawModel(LocalModel theModel)
        {
            Image drawBuffer = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            Graphics g = Graphics.FromImage(drawBuffer);

            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(10, 10, pictureBox1.Width, pictureBox1.Height));

            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            for (int x = -5; x < 6; x++)
            {
                for (int y = -5; y < 6; y++)
                {
                    if (theModel.hasFieldAtPosition(11 + x, 11 + y))
                    {
                        Debug.Print("printing the field at " + (11 + x) + ", " + (11 + y));

                        Field fieldToDraw = theModel.getFieldAtPosition(11 + x, 11 + y);

                        String fieldRepresentation = fieldToDraw.getRepresentation();

                        Debug.Print("of type " + fieldRepresentation);

                        Image imToDraw = imageBuffer.getImage(fieldRepresentation);

                        g.DrawImage(imToDraw, centerX + x * 64 - 32, centerY + y * 64 - 32, 64, 64);
                    }
                }
            }

            g.DrawImage(imageBuffer.getImage("player"), centerX - 16, centerY - 16, 32, 32);

            g.Dispose();

            pictureBox1.Image = drawBuffer;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.EndsWith("\n"))
            {
                inputList.Add(textBox1.Text.Replace("\n", ""));
                textBox1.Clear();
            }
        }
    }
}
