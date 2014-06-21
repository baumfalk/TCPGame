using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.Generation.LowLevel.Values.Perlin;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation.LowLevel.Values
{
    public class Valuemap
    {
        int[][] valuemap;

        public const int GENERATOR_TYPE_RANDOM = 0;
        public const int GENERATOR_TYPE_PERLIN = 1;

        public Valuemap(int GeneratorType, ValuemapData data)
        {
            ValuemapGenerator generator;

            switch (GeneratorType)
            {
                case GENERATOR_TYPE_RANDOM:
                    generator = new ValuemapGenerator();
                    break;
                case GENERATOR_TYPE_PERLIN:
                    generator = new PerlinNoise();
                    break;
                default:
                    Output.Print("no valid generator type, generating random");
                    generator = new ValuemapGenerator();
                    break;
            }

            valuemap = generator.Generate(data);

            AddOuterWalls(data.width, data.height);
        }

        public int GetValue(Location location)
        {
            return valuemap[location.x][location.y];
        }
        public void SetValue(Location location, int value)
        {
            valuemap[location.x][location.y] = value;
        }

        private void AddOuterWalls(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                valuemap[x][0] = 255;
                valuemap[x][height - 1] = 255;
            }

            for (int y = 0; y < height; y++)
            {
                valuemap[0][y] = 255;
                valuemap[width - 1][y] = 255;
            }
        }
    }
}
