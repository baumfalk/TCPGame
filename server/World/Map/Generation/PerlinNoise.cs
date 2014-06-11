using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;
using System.Drawing;

namespace TCPGameServer.World.Map.Generation
{
    class PerlinNoise
    {
        public static Bitmap GetNoisyBitmapRGB(int seed, int width, int height, int octaves, double ampvar, bool smoothInbetween, bool smoothAfter, bool normalize)
        {
            Random rnd = new Random(seed);

            int[][] returnmapR = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, ampvar, smoothInbetween, smoothAfter, normalize);
            int[][] returnmapG = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, ampvar, smoothInbetween, smoothAfter, normalize);
            int[][] returnmapB = PerlinNoise.Noise(rnd.Next(int.MaxValue), width, height, octaves, ampvar, smoothInbetween, smoothAfter, normalize);

            Bitmap bmpNoise = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(bmpNoise);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int valueR = (int) (returnmapR[x][y]);
                    int valueG = (int) (returnmapG[x][y]);
                    int valueB = (int) (returnmapB[x][y]);

                    Color c = Color.FromArgb(valueR, valueG, valueB);

                    g.FillRectangle(new SolidBrush(c), x, y, 1, 1);
                }
            }

            g.Dispose();

            return bmpNoise;
        }

        public static Bitmap GetNoisyBitmap(int seed, int width, int height, int octaves, double ampvar, bool smoothInbetween, bool smoothAfter, bool normalize)
        {
            int[][] returnmap = PerlinNoise.Noise(seed, width, height, octaves, ampvar, smoothInbetween, smoothAfter, normalize);

            Bitmap bmpNoise = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(bmpNoise);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = (int) (returnmap[x][y]);

                    Color c = Color.FromArgb(value, value, value);

                    g.FillRectangle(new SolidBrush(c), x, y, 1, 1);
                }
            }

            g.Dispose();

            return bmpNoise;
        }

        public static int[][] Noise(int seed, int width, int height, int octaves, double ampvar, bool smoothInbetween, bool smoothAfter, bool normalize)
        {
            Random rnd = new Random(seed);

            int resolutie = 1;

            if (octaves < 1) octaves = 1;
            if (octaves > 16) octaves = 16;

            if (ampvar < 0) ampvar = -1 / ampvar;

            double varTotal = 0.0d;
            for (int n = 0; n < octaves; n++)
            {
                varTotal += Math.Pow(ampvar, n);
            }

            int[][][] bytemap = new int[octaves][][];

            double amplitude = 255 / varTotal;

            // octaves
            for (int n = 0; n < octaves; n++)
            {
                int vakjesX = width / resolutie + 2;
                int vakjesY = height / resolutie + 2;

                bytemap[n] = new int[vakjesX][];

                for (int x = 0; x < vakjesX; x++)
                {
                    bytemap[n][x] = new int[vakjesY];

                    for (int y = 0; y < vakjesY; y++)
                    {
                        int rand = (int)(rnd.NextDouble() * amplitude);

                        bytemap[n][x][y] = rand;
                    }
                }

                if (smoothInbetween) bytemap[n] = Smooth(bytemap[n], vakjesX, vakjesY, amplitude);

                amplitude *= ampvar;
                resolutie *= 4;
            }

            int[][] returnmap = SumInterpolatedMaps(bytemap, octaves);

            if (smoothAfter) returnmap = Smooth(returnmap, width, height, 255);

            if (normalize) returnmap = Normalize(returnmap, width, height, 255);

            return returnmap;
        }

        private static int[][] Smooth(int[][] bytemap, int vakjesX, int vakjesY, double ampMax)
        {
            int[][] returnmap = new int[vakjesX][];

            for (int x = 0; x < vakjesX; x++)
            {
                returnmap[x] = new int[vakjesY];

                for (int y = 0; y < vakjesY; y++)
                {
                    if (x == 0 || y == 0 || x == vakjesX - 1 || y == vakjesY - 1)
                    {
                        returnmap[x][y] = (int)(ampMax / 2);
                    }
                    else
                    {
                        int corners = bytemap[x + 1][y + 1] + bytemap[x + 1][y - 1] + bytemap[x - 1][y + 1] + bytemap[x - 1][y - 1];
                        int sides = bytemap[x + 1][y] + bytemap[x - 1][y] + bytemap[x][y + 1] + bytemap[x][y - 1];
                        int center = bytemap[x][y];

                        returnmap[x][y] = (corners + sides * 2 + center * 4) / 16;
                    }
                }
            }

            return returnmap;
        }

        private static int[][] SumInterpolatedMaps(int[][][] bytemap, int octaves)
        {
            int width = bytemap[0].Length;
            int height = bytemap[0][0].Length;

            int[][] returnmap = new int[width - 2][];

            for (int x = 1; x < width - 1; x++)
            {
                returnmap[x - 1] = new int[height - 2];

                for (int y = 1; y < height - 1; y++)
                {
                    returnmap[x - 1][y - 1] = 0;

                    int resolutie = 1;

                    for (int n = 0; n < octaves; n++)
                    {
                        double xPart = x % resolutie / ((double)resolutie);
                        double yPart = y % resolutie / ((double)resolutie);

                        int topleft = bytemap[n][x / resolutie][y / resolutie];
                        int topright = bytemap[n][x / resolutie + 1][y / resolutie];
                        int bottomleft = bytemap[n][x / resolutie][y / resolutie + 1];
                        int bottomright = bytemap[n][x / resolutie + 1][y / resolutie + 1];

                        int above = Interpolate(topleft, topright, xPart, false);
                        int below = Interpolate(bottomleft, bottomright, xPart, false);

                        returnmap[x - 1][y - 1] += Interpolate(above, below, yPart, false);

                        resolutie *= 4;
                    }
                }
            }

            return returnmap;
        }

        private static int Interpolate(int first, int second, double distancebetween, bool linear)
        {
            int returnval;

            if (linear)
            {
                double difference = second - first;

                double result = difference * distancebetween + first;

                returnval = ((int) result);
            }
            else
            {

                double dPI = Math.PI * distancebetween;

                double f = (1 - Math.Cos(dPI)) * 0.5;

                returnval = (int)(first * (1 - f) + second * f);
            }
            return returnval;
        }

        private static int[][] Normalize(int[][] bytemap, int width, int height, int maxval)
        {
            int minInMap = int.MaxValue;
            int maxInMap = int.MinValue;

            

            for (int x = 0; x < width; x++)
            {
                

                for (int y = 0; y < height; y++)
                {
                    if (bytemap[x][y] < minInMap) minInMap = bytemap[x][y];
                    if (bytemap[x][y] > maxInMap) maxInMap = bytemap[x][y];
                }
            }

            int[][] returnmap = new int[width][];

            double multVal = maxval / (double) (maxInMap - minInMap);

            for (int x = 0; x < width; x++)
            {
                returnmap[x] = new int[height];

                for (int y = 0; y < height; y++)
                {
                    returnmap[x][y] = (int) ((bytemap[x][y] - minInMap) * multVal);
                }
            }

            return returnmap;
        }
    }
}
