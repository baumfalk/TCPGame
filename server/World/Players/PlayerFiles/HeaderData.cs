using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.Players.PlayerFiles
{
    class HeaderData
    {
        public String name;
        public byte[] salt;
        public String password;
        public String area;
        public int tileIndex;
    }
}
