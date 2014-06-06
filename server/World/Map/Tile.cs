using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World.Map
{
    public class Tile
    {
        private Area area;
        private World world;

        private Tile[] neighbors;
        private struct AreaLink
        {
            public String areaName;
            public int targetID;

            public AreaLink(String areaName, int targetID)
            {
                this.areaName = areaName;
                this.targetID = targetID;
            }
        }
        private AreaLink[] areaLinks;

        private bool[] hasNeighbor;

        private String type;
        private String representation;

        private Creature occupant = null;

        private int x;
        private int y;
        private int z;

        private int color;

        public Tile(String type, String representation, int x, int y, int z, Area area, World world)
        {
            this.type = type;
            this.representation = representation;

            this.x = x;
            this.y = y;
            this.z = z;

            this.area = area;
            this.world = world;

            hasNeighbor = new bool[6];
            neighbors = new Tile[6];
            areaLinks = new AreaLink[6];
        }

        public void setColor(int color)
        {
            this.color = color;
        }

        public int getColor()
        {
            return color;
        }

        public void CreateAreaLink(int direction, String areaName, int id)
        {
            areaLinks[direction] = new AreaLink(areaName, id);

            hasNeighbor[direction] = true;
        }

        public void Link(int direction, Tile neighbor)
        {
            neighbors[direction] = neighbor;

            hasNeighbor[direction] = true;
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
            occupant.SetPosition(this);
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
        public bool HasNeighbor(int direction)
        {
            return hasNeighbor[direction];
        }

        public Tile getNeighbor(int direction)
        {
            if (neighbors[direction] == null)
            {            
                AreaLink areaLink = areaLinks[direction];

                Tile neighbor = world.GetTile(areaLink.areaName, areaLink.targetID);

                Link(direction, neighbor);
                neighbor.Link(Directions.inverse(direction), this);
            }
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
