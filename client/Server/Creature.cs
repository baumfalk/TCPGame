using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace TCPGameClient.Server
{
    // a creature is any mobile object, including players and such which may extend it
    public class Creature
    {
        // image key
        protected String representation;
        // position in the world
        protected Tile position;

        // created with it's representation
        public Creature(String representation)
        {
            this.representation = representation;
        }

        // position can be changed and requested freely
        public void setPosition(Tile position)
        {
            this.position = position;
        }
        public Tile getPosition()
        {
            return position;
        }

        // image key can be requested and changed freely
        public void setRepresentation(String representation)
        {
            this.representation = representation;
        }
        public String getRepresentation()
        {
            return representation;
        }
    }
}
