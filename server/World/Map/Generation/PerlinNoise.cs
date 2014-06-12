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
        public static int[][] Noise(int seed, int width, int height, int octaves, double frequencyIncrease, double persistence, bool smoothInbetween, bool smoothAfter, bool bowl, bool normalize)
        {
            Random rnd = new Random(seed);

            // first pass, we always handle single pixels
            int frequency = 1;

            // don't let octaves get below 1 (in which case no layers would be created) or above 16 (which is already absurdly high).
            if (octaves < 1) octaves = 1;
            if (octaves > 16) octaves = 16;

            // don't let persistence be 0 or negative, don't allow it above 20 (in which case you'll pretty much only see the top layer).
            if (persistence <= 0.0d) persistence = 0.01d;
            if (persistence > 20.0d) persistence = 20.0d;

            // the amplitude of the noise wave function is persistence^i, where i is the octave being handled. When we add all waves
            // together, the maximum value should be 255, so we divide 255 by the total maximum of all waves.
            double maximumWaveHeight;
            if (persistence == 1.0d)
            {
                maximumWaveHeight = octaves;
            }
            else
            {
                maximumWaveHeight = (Math.Pow(persistence, octaves) - 1) / (persistence - 1);
            }

            double amplitude = 255 / maximumWaveHeight;

            // valuemap[octave][x][y]
            int[][][] valuemap = new int[octaves][][];
            
            for (int n = 0; n < octaves; n++)
            {
                // we only need a certain number of values for each layer, depending on the size of the map and the frequency. For example,
                // if the frequency is 16, that means a "pixel" on that layer is 16x16 pixels on layer 0. The image is only a certain size,
                // so we can use fewer fields on higher frequencies. We do need 1 extra field to account for possible issues with integer
                // division and 1 more because we're dealing with gradients
                int numValuesOnX = width / frequency + 2;
                int numValuesOnY = height / frequency + 2;

                valuemap[n] = new int[numValuesOnX][];

                for (int x = 0; x < numValuesOnX; x++)
                {
                    valuemap[n][x] = new int[numValuesOnY];

                    for (int y = 0; y < numValuesOnY; y++)
                    {
                        // we fill all the fields with random values from 0 to the amplitude

                        int rand = (int)(rnd.NextDouble() * amplitude);

                        valuemap[n][x][y] = rand;
                    }
                }

                // once all values are filled in, we have the option to smoothe out each layer
                if (smoothInbetween) valuemap[n] = Smooth(valuemap[n], numValuesOnX, numValuesOnY, amplitude);

                amplitude *= persistence;
                frequency = (int) (frequency * frequencyIncrease);
            }

            // sum all interpolated gradient maps
            int[][] returnmap = SumInterpolatedMaps(valuemap, width, height, octaves, frequencyIncrease);

            // optionally, smoothe out the map after adding everything together
            if (smoothAfter) returnmap = Smooth(returnmap, width, height, 255);

            // optionally, make the edges higher and the center shallower
            if (bowl) returnmap = Bowl(returnmap, width, height, 255);

            // optionally, make sure the values range from 0 to 255.
            if (normalize) returnmap = Normalize(returnmap, width, height, 255);

            return returnmap;
        }

        // smoothes out an intmap, making values lie closer to each other
        private static int[][] Smooth(int[][] valuemap, int numValuesOnX, int numValuesOnY, double amplitude)
        {
            int[][] returnmap = new int[numValuesOnX][];

            for (int x = 0; x < numValuesOnX; x++)
            {
                returnmap[x] = new int[numValuesOnY];

                for (int y = 0; y < numValuesOnY; y++)
                {
                    // smoothe out each value using the ones near it. Uses a helper function to make sure we're not querying values
                    // outside the array. Squares with the average value are "simulated" outside the array.

                    int corners =
                        GetSmoothingValue(x + 1, y + 1, valuemap, amplitude) +
                        GetSmoothingValue(x + 1, y - 1, valuemap, amplitude) +
                        GetSmoothingValue(x - 1, y + 1, valuemap, amplitude) +
                        GetSmoothingValue(x - 1, y - 1, valuemap, amplitude);

                    int sides =
                        GetSmoothingValue(x + 1, y, valuemap, amplitude) +
                        GetSmoothingValue(x - 1, y, valuemap, amplitude) +
                        GetSmoothingValue(x, y + 1, valuemap, amplitude) +
                        GetSmoothingValue(x, y - 1, valuemap, amplitude);

                    int center = GetSmoothingValue(x, y, valuemap, amplitude);

                    // each corner counts for 1/16th of the value, each side for 1/8th, and the center for 1/4th.
                    returnmap[x][y] = (corners + sides * 2 + center * 4) / 16;
                }
            }

            return returnmap;
        }

        // makes sure values outside the array aren't queried, returns average amplitude for the "tiles" outside the map
        private static int GetSmoothingValue(int x, int y, int[][] valuemap, double amplitude)
        {
            if (x < 0 || y < 0 || x == valuemap.Length || y == valuemap[0].Length) return (int) (amplitude / 2);

            else return valuemap[x][y];
        }

        
        private static int[][] SumInterpolatedMaps(int[][][] bytemap, int width, int height, int octaves, double frequencyIncrease)
        {
            int[][] returnmap = new int[width][];

            for (int x = 0; x < width; x++)
            {
                returnmap[x] = new int[height];

                for (int y = 0; y < height; y++)
                {
                    returnmap[x][y] = 0;

                    int frequency = 1;

                    for (int n = 0; n < octaves; n++)
                    {
                        double xPart = x % frequency / ((double)frequency);
                        double yPart = y % frequency / ((double)frequency);

                        int topleft = bytemap[n][x / frequency][y / frequency];
                        int topright = bytemap[n][x / frequency + 1][y / frequency];
                        int bottomleft = bytemap[n][x / frequency][y / frequency + 1];
                        int bottomright = bytemap[n][x / frequency + 1][y / frequency + 1];

                        int above = Interpolate(topleft, topright, xPart, false);
                        int below = Interpolate(bottomleft, bottomright, xPart, false);

                        returnmap[x][y] += Interpolate(above, below, yPart, false);

                        frequency = (int) (frequency * frequencyIncrease);
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

        private static int[][] Bowl(int[][] valuemap, int width, int height, int maxval)
        {
            int[][] returnmap = new int[width][];

            for (int x = 0; x < width; x++)
            {
                returnmap[x] = new int[height];

                for (int y = 0; y < height; y++)
                {
                    double xDist = Math.Abs(width / 2 - x);
                    double yDist = Math.Abs(height / 2 - y);

                    double distanceToCenter = Math.Pow(xDist, 2) + Math.Pow(yDist, 2);

                    if (distanceToCenter == 0.0d)
                    {
                        returnmap[x][y] = (int) (0.5d * valuemap[x][y]);
                        continue;
                    }

                    double yFromEdge = height / 2 - yDist;
                    double xFromEdge = width / 2 - xDist;

                    double distanceToEdge;
                    if (yFromEdge < xFromEdge)
                    {
                         distanceToEdge = yFromEdge / (height / 2) * distanceToCenter;
                    }
                    else
                    {
                        distanceToEdge = xFromEdge / (width / 2) * distanceToCenter;
                    }

                    double fractionEdgeToCenter = distanceToEdge / distanceToCenter;

                    double interpolation = Interpolate(255, 128, fractionEdgeToCenter, false);

                    double multiplier = interpolation / 255.0d;

                    returnmap[x][y] = (int) (valuemap[x][y] * multiplier);
                }
            }

            return returnmap;
        }
    }
}
