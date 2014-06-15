using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map
{
    class AreaData
    {
        public Tile[] tiles;
        public int[][] linkData;
        public int seed;
        public String areaType;
    }
}
