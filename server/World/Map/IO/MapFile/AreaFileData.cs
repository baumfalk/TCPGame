using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.IO.MapFile
{
    public class AreaFileData
    {
        public HeaderData header = new HeaderData();
        public TileBlockData entrances = new TileBlockData();
        public TileBlockData[] fixedTiles;
    }
}
