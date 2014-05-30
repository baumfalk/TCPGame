using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            control = new Controller(this);

            imageBuffer = new ImageBuffer(64, 64);
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
            Tile centerTile = theModel.thePlayer.getPosition();

            Graphics g = this.CreateGraphics();

            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(10, 10, this.Width - 40, this.Height - 100));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.EndsWith("\n")) inputList.Add(textBox1.Text.Replace("\n", ""));
        }
    }
}
