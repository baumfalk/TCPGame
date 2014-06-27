using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;

using TCPGameSharedInfo;

namespace TCPGameClient.View
{
    class ImageBuffer
    {
        // dictionary to translate tilerepresentation enum to images
        Dictionary<TileRepresentation, Image> tileImages;

        // dictionary to translate creaturerepresentation enum to images
        Dictionary<CreatureRepresentation, Image> creatureImages;

        // a default image to return when a key isn't matched
        private Image defaultImage;

        // constructor takes a default height and width for a tile, so he knows what
        // size to make the default
        public ImageBuffer(int defaultWidth, int defaultHeight)
        {
            // create the default image
            SetDefault(defaultWidth, defaultHeight);

            // initialize the tile dictionary
            tileImages = new Dictionary<TileRepresentation, Image>();

            // initialize the creature dictionary
            creatureImages = new Dictionary<CreatureRepresentation, Image>();

            // images folder
            String path = TCPGameSharedInfo.FileLocations.imagePath;

            // load the tile images from the path specified. Should be "path", but "gitPath" is handier
            // for now.
            loadTiles(path);

            // same for the creatures
            loadCreatures(path);
        }

        // loads the tiles into the dictionary
        private void loadTiles(String path)
        {
            Array representations = Enum.GetValues(typeof(TileRepresentation));

            foreach (TileRepresentation representation in representations)
            {
                String fileName = path + @"\tiles\" + Representation.toString(representation) + ".png";

                Image toAdd = loadImage(fileName);

                tileImages.Add(representation, toAdd);
            }
        }

        // loads the creatures into the dictionary
        private void loadCreatures(String path)
        {
            Array representations = Enum.GetValues(typeof(CreatureRepresentation));

            foreach (CreatureRepresentation representation in representations)
            {
                String fileName = path + @"\creatures\" + Representation.toString(representation) + ".png";

                Image toAdd = loadImage(fileName);

                creatureImages.Add(representation, toAdd);
            }
        }

        private Image loadImage(String fileName)
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
                return Image.FromFile(fileName);
            }
            catch (System.IO.FileNotFoundException e)
            {
                System.Diagnostics.Debug.Print(e.Message);

                return defaultImage;
            }
        }

        //creates a default image
        public void SetDefault(int defaultWidth, int defaultHeight)
        {
            // the image is just a red square
            defaultImage = new Bitmap(defaultWidth, defaultHeight);
            Graphics g = Graphics.FromImage(defaultImage);

            g.FillRectangle(new SolidBrush(Color.Red), 0, 0, defaultWidth, defaultHeight);

            g.Dispose();
        }

        // getImage returns an image from the dictionary if it's there, and otherwise the default
        public Image GetImage(TileRepresentation imageName)
        {
            Image imReturn;

            if (tileImages.TryGetValue(imageName, out imReturn)) return imReturn;
            return defaultImage;
        }

        // getImage returns an image from the dictionary if it's there, and otherwise the default
        public Image GetImage(CreatureRepresentation imageName)
        {
            Image imReturn;

            if (creatureImages.TryGetValue(imageName, out imReturn)) return imReturn;
            return defaultImage;
        }
    }
}
