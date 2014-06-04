using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameClient.Model
{
    public class Creature
    {
        private int x;
        private int y;
        private int z;

        private String representation;

        public Creature(int x, int y, int z, String representation)
        {
            this.representation = representation;

            this.x = x;
            this.y = y;
            this.z = z;
        }

        public String GetRepresentation()
        {
            return representation;
        }

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public int GetZ()
        {
            return z;
        }
    }
}
