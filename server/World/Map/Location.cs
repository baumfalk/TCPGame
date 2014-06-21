using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map
{
    public class Location
    {
        public int x;
        public int y;
        public int z;

        public Location() { }

        public Location(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            Location location = obj as Location;
            if ((System.Object)location == null) return false;

            return (location.x == x && location.y == y && location.z == z);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }
    }
}
