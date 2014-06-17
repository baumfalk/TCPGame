using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.IO.MapFile
{
    class HeaderData
    {
        public String fileType;
        public String areaType;
        public int seed;
        public Location mapGridLocation = new Location();
    }
}
