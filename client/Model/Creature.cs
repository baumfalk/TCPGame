using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace TCPGameClient.Model
{
    public class Creature
    {
        protected String representation;
        protected Tile position;

        public Creature(String representation)
        {
            this.representation = representation;
        }
        public void setPosition(Tile position)
        {
            this.position = position;
        }
        public Tile getPosition()
        {
            return position;
        }

        public String getRepresentation()
        {
            return representation;
        }
    }
}
