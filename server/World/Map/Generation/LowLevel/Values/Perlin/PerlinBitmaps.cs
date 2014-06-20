using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation.LowLevel.Values.Perlin
{
    class PerlinBitmaps
    {
        public static void SaveBitmapFromNoisemap(int[][] noisemap, int width, int height, String filename)
        {
            Bitmap bmpSave = GetBitmapFromNoisemap(noisemap, width, height);

            bmpSave.Save(filename);

            Output.Print("saving to " + filename);
        }

        public static Bitmap GetBitmapFromNoisemap(int[][] noisemap, int width, int height)
        {
            Bitmap bmpNoise = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(bmpNoise);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = (int)(noisemap[x][y]);

                    Color c;
                    if (value == 256) c = Color.Blue;
                    else if (value == 257) c = Color.Red;
                    else if (value > 255) c = Color.Green;
                    else c = Color.FromArgb(value, value, value);

                    g.FillRectangle(new SolidBrush(c), x, 99 - y, 1, 1);
                }
            }

            g.Dispose();

            return bmpNoise;
        }

        public static Bitmap GetNoisyBitmap(int seed, int width, int height, int octaves, double frequencyIncrease, double persistence, bool smoothInbetween, bool smoothAfter, bool bowl, bool normalize)
        {
            Random rnd = new Random(seed);

            int[][] returnmap = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, frequencyIncrease, persistence, smoothInbetween, smoothAfter, bowl, normalize);

            return GetBitmapFromNoisemap(returnmap, width, height);
        }

        // just for fun, practically the same as GetNoisyBitmap, but uses three different noisemaps as RGB channels
        public static Bitmap GetNoisyBitmapRGB(int seed, int width, int height, int octaves, double frequencyIncrease, double persistence, bool smoothInbetween, bool smoothAfter, bool bowl, bool normalize)
        {
            Random rnd = new Random(seed);

            int[][] returnmapR = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, frequencyIncrease, persistence, smoothInbetween, smoothAfter, bowl, normalize);
            int[][] returnmapG = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, frequencyIncrease, persistence, smoothInbetween, smoothAfter, bowl, normalize);
            int[][] returnmapB = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, frequencyIncrease, persistence, smoothInbetween, smoothAfter, bowl, normalize);

            Bitmap bmpNoise = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(bmpNoise);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int valueR = (int)(returnmapR[x][y]);
                    int valueG = (int)(returnmapG[x][y]);
                    int valueB = (int)(returnmapB[x][y]);

                    Color c = Color.FromArgb(valueR, valueG, valueB);

                    g.FillRectangle(new SolidBrush(c), x, y, 1, 1);
                }
            }

            g.Dispose();

            return bmpNoise;
        }
    }
}
