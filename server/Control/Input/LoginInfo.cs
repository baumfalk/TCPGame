using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.Control.Input
{
    public class LoginInfo
    {
        public bool newUser;
        public byte[] salt;
        public string name;
        public string areaName;
        public int tileIndex;
    }
}
