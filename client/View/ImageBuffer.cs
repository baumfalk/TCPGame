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
            SetDefault(defaultWidth, defaultHeight);

            // initialize the dictionary
            images = new Dictionary<String,Image>();

            // the path is the executing directory + \images for now
            String path = Directory.GetCurrentDirectory() + @"\images";

            // images folder in the git..
            String gitPath = @"..\..\..\images";

            // load the images from the path specified. Should be "path", but "gitPath" is handier
            // for now.
            loadImages(gitPath);
        }

        private void loadImages(String path) {
            // try to load the images from the specified path
            foreach (String fileName in Directory.GetFiles(path))
            {
                try
                {
                    // split to extract the name of the file
                    String[] splitFile = fileName.Split('\\');

                    // the last element is the filename
                    String name = splitFile[splitFile.Count() - 1];
                    // take off the extension for the name
                    name = name.Substring(0, name.Length - 4);

                    // add the image to the imagebuffer
                    images.Add(name, Image.FromFile(fileName));
                }
                catch (System.IO.FileNotFoundException e)
                {
                    System.Diagnostics.Debug.Print(e.Message);
                }
            }
        }

        //creates a default image
        private void SetDefault(int defaultWidth, int defaultHeight)
        {
            // the image is just a red square
            defaultImage = new Bitmap(defaultWidth, defaultHeight);
            Graphics g = Graphics.FromImage(defaultImage);

            g.FillRectangle(new SolidBrush(Color.Red), 0, 0, defaultWidth, defaultHeight);

            g.Dispose();
        }

        // getImage returns an image from the dictionary if it's there, and otherwise the default
        public Image GetImage(String imageName)
        {
            Image imReturn;

            if (images.TryGetValue(imageName, out imReturn)) return imReturn;
            return defaultImage;
        }
    }
}
