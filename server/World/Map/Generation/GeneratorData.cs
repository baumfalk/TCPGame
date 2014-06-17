using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation
{
    class GeneratorData
    {
        public Area area;
        public World world;

        public bool generateExits;
        public String areaType;
        public int seed;
        public Location bottomLeft;

        public Tile[] entrances;
        public Tile[][] fixedTiles;
    }
}
