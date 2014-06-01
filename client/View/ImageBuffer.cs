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
        // dictionary to translate string keys to images
        Dictionary<String, Image> images;

        // a default image to return when a key isn't matched
        private Image defaultImage;

        // constructor takes a default height and width for a tile, so he knows what
        // size to make the default
        public ImageBuffer(int defaultWidth, int defaultHeight)
        {
            // create the default image
            setDefault(defaultWidth, defaultHeight);

            // initialize the dictionary
            images = new Dictionary<String,Image>();

            // the path is the executing directory + \images for now
            String path = Directory.GetCurrentDirectory() + @"\images";

            // try to load these images. Should be dynamic at some point
            try
            {
                images.Add("wall", Image.FromFile(path + @"\wall.png"));
                images.Add("floor", Image.FromFile(path + @"\floor.png"));
                images.Add("player", Image.FromFile(path + @"\player.png"));
            }
            catch (System.IO.FileNotFoundException e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
        }

        //creates a default image
        private void setDefault(int defaultWidth, int defaultHeight)
        {
            // the image is just a red square
            defaultImage = new Bitmap(defaultWidth, defaultHeight);
            Graphics g = Graphics.FromImage(defaultImage);

            g.FillRectangle(new SolidBrush(Color.Red), 0, 0, defaultWidth, defaultHeight);

            g.Dispose();
        }

        // getImage returns an image from the dictionary if it's there, and otherwise the default
        public Image getImage(String imageName)
        {
            Image imReturn;

            if (images.TryGetValue(imageName, out imReturn)) return imReturn;
            return defaultImage;
        }
    }
}
