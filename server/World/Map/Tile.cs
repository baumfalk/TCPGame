using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map
{
    public class Tile
    {
        // area and world the tile is part of
        private int ID;
        private Area area;
        private World world;

        // the neighbors that are already linked up
        private Tile[] neighbors;

        // struct designating a link to another area
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
        // neighbors in other areas will be put in here
        private AreaLink[] areaLinks;

        // a tile has a neighbor when it's directly linked or when a tile
        // in another area will be linked on load
        private bool[] hasNeighbor;

        // types are things like "floor" or "wall", which may have different
        // representations but the same behavior
        private String type;
        // representation is a string key for a specific image
        private String representation;

        // the occupant of the tile. Null means noone is there.
        private Creature occupant = null;

        // coordinates which can be used to calculate distances
        private int x;
        private int y;
        private int z;

        // used in BFS to find neighboring tiles to a certain depth
        private int color;

        // fills fields and initializes arrays
        public Tile(String type, String representation, int x, int y, int z, int ID, Area area, World world)
        {
            // fill fields from arguments
            this.type = type;
            this.representation = representation;

            this.x = x;
            this.y = y;
            this.z = z;

            this.ID = ID;
            this.area = area;
            this.world = world;

            // create the neighbor-arrays (6 directions)
            hasNeighbor = new bool[6];
            neighbors = new Tile[6];
            areaLinks = new AreaLink[6];
        }

        // the "color" of the tile can be used to find neighbors to a certain depth
        public void SetColor(int color)
        {
            this.color = color;
        }
        public int GetColor()
        {
            return color;
        }

        // world, area and ID can be requested
        public int GetID()
        {
            return ID;
        }
        public Area GetArea()
        {
            return area;
        }
        public World GetWorld()
        {
            return world;
        }

        // create a link to a tile in a different area
        public void CreateAreaLink(int direction, String areaName, int id)
        {
            areaLinks[direction] = new AreaLink(areaName, id);

            hasNeighbor[direction] = true;
        }

        // create a link to a tile. If it has a link but that link is not this tile,
        // output a message
        public void Link(int direction, Tile neighbor)
        {
            neighbors[direction] = neighbor;

            if (neighbor.HasNeighbor(Directions.Inverse(direction)))
            {
                if (neighbor.GetNeighbor(Directions.Inverse(direction)) != this)
                {
                    Output.Print("[" + area.GetName() + "(" + ID + ")] no linkback (" + Directions.ToString(direction) + ")");
                }
            }

            int[] properNeighbor = Directions.GetNeighboring(direction, x, y, z);

            if (properNeighbor[0] != neighbor.GetX() || properNeighbor[1] != neighbor.GetY() || properNeighbor[2] != neighbor.GetZ())
            {
                Output.Print("[" + area.GetName() + "(" + ID + ")] link to tile that's not adjacent. Possible error (" + Directions.ToString(direction) + ")");
            }

            hasNeighbor[direction] = true;
        }

        // checks if this tile has an occupant
        public bool HasOccupant()
        {
            return (occupant != null);
        }

        // gets the occupant
        public Creature GetOccupant()
        {
            return occupant;
        }

        // sets the occupant and sets this tile as it's position
        public void SetOccupant(Creature occupant)
        {
            this.occupant = occupant;
            occupant.SetPosition(this);
        }

        // clears the occupant from this tile
        public void Vacate()
        {
            this.occupant = null;
        }

        // the x/y/z location in the world
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

        // check if this tile has a neighbor
        public bool HasNeighbor(int direction)
        {
            return hasNeighbor[direction];
        }

        // get the neighbor if it's linked, if it's in another area, get that tile from
        // that area and link it up (loading the area if it's not in memory).
        public Tile GetNeighbor(int direction)
        {
            // if there is a neighbor but it's not linked, it's an area link
            if (neighbors[direction] == null && hasNeighbor[direction])
            {
                // get the right area link
                AreaLink areaLink = areaLinks[direction];

                // get the neighboring tile by requesting it from the world object
                Tile neighbor = world.GetTile(areaLink.areaName, areaLink.targetID);

                // link this tile, and link back right away so this code won't have
                // to run twice.
                Link(direction, neighbor);
                neighbor.Link(Directions.Inverse(direction), this);
            }
            return neighbors[direction];
        }

        // get the image key for this tile
        public String GetRepresentation()
        {
            return representation;
        }

        // check if this tile is passable
        public bool IsPassable()
        {
            if (type.Equals("wall")) return false;
            return true;
        }
    }
}
