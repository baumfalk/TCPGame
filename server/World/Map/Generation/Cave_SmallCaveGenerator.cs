using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation
{
    class Cave_SmallCaveGenerator : CaveGenerator
    {
        public Cave_SmallCaveGenerator(int seed, Tile[] entrances, Tile[][] fixedTiles, bool generateExits, int bottomLeftX, int bottomLeftY, int bottomLeftZ, Area area, World world)
            : base(seed, entrances, fixedTiles, generateExits, bottomLeftX, bottomLeftY, bottomLeftZ, area, world)
        {
            
        }

        protected override string GetAreaType()
        {
            return "Small Cave";
        }
    }
}
