using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.Generation
{
    public class MidLevelData
    {
        public String areaType;
        public Tile[] tiles;

        public MidLevelData(LowLevelData lowLevelData)
        {
            this.areaType = lowLevelData.areaType;
            this.tiles = lowLevelData.tilemap.GetTiles();
        }
    }
}
