using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPGameSharedInfo;
namespace TCPGameServer.World.Map.Generation.LowLevel.Values
{
    class ValuemapGenerator
    {
        public virtual int[][] Generate(ValuemapData mapData)
        {
            CrossPlatformRandom rnd = new CrossPlatformRandom(mapData.seed);

            int[][] generatedMap = new int[mapData.width][];

            for (int x = 0; x < mapData.width; x++)
            {
                generatedMap[x] = new int[mapData.height];

                for (int y = 0; y < mapData.height; y++)
                {
                    generatedMap[x][y] = rnd.Next(256);
                }
            }

            return generatedMap;
        }
    }
}
