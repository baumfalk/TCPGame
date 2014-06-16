using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation
{
    class AreaGenerator
    {
        public AreaData Generate(int seed, String areaType, Tile[] entrances, Tile[][] fixedTiles, bool generateExits, int bottomRightX, int bottomRightY, int bottomRightZ, Area area, World world)
        {
            DateTime start = DateTime.Now;

            AreaData toReturn;

            if (areaType.Equals("Small Cave"))
            {
                toReturn = new Cave_SmallCaveGenerator(seed, entrances, fixedTiles, generateExits, bottomRightX, bottomRightY, bottomRightZ, area, world).Generate();
            }
            else if (areaType.Equals("Tunnel Cave"))
            {
                toReturn = new Cave_TunnelGenerator(seed, entrances, fixedTiles, generateExits, bottomRightX, bottomRightY, bottomRightZ, area, world).Generate();
            }
            else
            {
                Output.Print("nonexistent map type " + areaType + ", returning small cave");

                toReturn = new Cave_SmallCaveGenerator(seed, entrances, fixedTiles, generateExits, bottomRightX, bottomRightY, bottomRightZ, area, world).Generate();
            }

            Output.Print("generation took " + (DateTime.Now - start).TotalMilliseconds + " milliseconds");

            return toReturn;
        }
    }
}
