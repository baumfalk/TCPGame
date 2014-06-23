using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.Generation.LowLevel.Tiles;
using TCPGameServer.World.Map.Generation.LowLevel.Connections;
using TCPGameServer.World.Map.Generation.LowLevel.Values;

namespace TCPGameServer.World.Map.Generation
{
    public class LowLevelData
    {
        public String areaType;

        public Valuemap valuemap;
        public Connectionmap connectionmap;
        public Tilemap tilemap;
    }
}
