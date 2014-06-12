using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation
{
    class AreaGenerator
    {
        CaveGenerator caveGenerator = new CaveGenerator();

        public Tile[] Generate(int seed, String areaType, Tile[] entrances, int bottomRightX, int bottomRightY, int bottomRightZ, Area area, World world)
        {
            if (areaType.Equals("Cave"))
            {
                return caveGenerator.Generate(seed, entrances, bottomRightX, bottomRightY, bottomRightZ, area, world);
            }
            else
            {
                Output.Print("nonexistent map type " + areaType + ", returning cave");

                return caveGenerator.Generate(seed, entrances, bottomRightX, bottomRightY, bottomRightZ, area, world);
            }
        }
    }
}
