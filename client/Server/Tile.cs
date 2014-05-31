using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace TCPGameClient.Server
{
    // 
    public class Tile
    {
        private Tile[] neighbors;
        private String type;
        private String representation;

        private Creature occupant = null;

        private int x;
        private int y;
        private int z;

        private int color;

        public Tile(String type, String representation, int x, int y, int z)
        {
            this.type = type;
            this.representation = representation;

            this.x = x;
            this.y = y;
            this.z = z;

            neighbors = new Tile[6];
        }

        public void setColor(int color)
        {
            this.color = color;
        }

        public int getColor()
        {
            return color;
        }

        public void link(int direction, Tile neighbor)
        {
            neighbors[direction] = neighbor;
            neighbor.backLink(Directions.inverse(direction), this);
        }

        protected void backLink(int direction, Tile neighbor)
        {
            neighbors[direction] = neighbor;
        }

        public bool hasOccupant()
        {
            return (occupant != null);
        }

        public Creature getOccupant()
        {
            return occupant;
        }

        public void setOccupant(Creature occupant)
        {
            this.occupant = occupant;
            occupant.setPosition(this);
        }

        public void vacate()
        {
            this.occupant = null;
        }

        public int getX()
        {
            return x;
        }

        public int getY()
        {
            return y;
        }

        public int getZ()
        {
            return z;
        }

        //0 = north, 1 = east, 2 = up, 3 = south, 4 = west, 5 = down
        public bool hasNeighbor(int direction)
        {
            return (neighbors[direction] != null);
        }

        public Tile getNeighbor(int direction)
        {
            return neighbors[direction];
        }

        public String getRepresentation()
        {
            return representation;
        }

        public bool isPassable()
        {
            if (type.Equals("wall")) return false;
            return true;
        }
    }
}
