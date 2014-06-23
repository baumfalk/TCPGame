using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.Generation
{
    public class AreaData
    {
        public String areaType;
        public Tile[] tiles;

        public AreaData(MidLevelData midLevelData)
        {
            areaType = midLevelData.areaType;
            tiles = midLevelData.tiles;
        }
    }
}
