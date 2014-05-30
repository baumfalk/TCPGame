using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;

namespace TCPGameClient.View
{
    class ImageBuffer
    {
        Dictionary<String, Image> images;

        private Image defaultImage;

        public ImageBuffer(int defaultWidth, int defaultHeight)
        {
            setDefault(defaultWidth, defaultHeight);

            images = new Dictionary<String,Image>();

            String path = Directory.GetCurrentDirectory() + @"\images";

            try
            {
                images.Add("wall", Image.FromFile(path + @"\wall.png"));
                images.Add("floor", Image.FromFile(path + @"\floor.png"));
                images.Add("player", Image.FromFile(path + @"\player.png"));
            }
            catch (System.IO.FileNotFoundException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        private void setDefault(int defaultWidth, int defaultHeight)
        {
            defaultImage = new Bitmap(defaultWidth, defaultHeight);
            Graphics g = Graphics.FromImage(defaultImage);

            g.FillRectangle(new SolidBrush(Color.Red), 0, 0, defaultWidth, defaultHeight);

            g.Dispose();
        }

        public Image getImage(String imageName)
        {
            Image imReturn;

            if (images.TryGetValue(imageName, out imReturn)) return imReturn;
            return defaultImage;
        }
    }
}
