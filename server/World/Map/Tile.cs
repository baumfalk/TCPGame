﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.Control.Output;
using TCPGameServer.World.Creatures;
using TCPGameServer.World.Map.Tiles;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map
{
    public abstract class Tile
    {
        // area and world the tile is part of
        private int ID;
        private Area area;
        private World world;

        // the neighbors that are already linked up
        private Tile[] neighbors;

        // list containing lists of tiles at a specific range (tilesAtRange[0] = all tiles at range 1, [1] -> all at range 2, etc)
        private List<List<Tile>> tilesAtRange = new List<List<Tile>>();

        // list of all creatures that can see this tile
        private HashSet<Creature> creaturesInVisionRange;

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

        // it's handy to know if a tile links to another area or not
        private bool hasAreaLink;

        // representation is a string key for a specific image
        private TileRepresentation representation;

        // the occupant of the tile. Null means noone is there.
        private Creature occupant = null;

        // coordinates which can be used to calculate distances
        private Location location;

        // used in BFS to find neighboring tiles to a certain depth
        private int color;

        // fills fields and initializes arrays
        protected Tile(TileRepresentation representation, Location location, int ID, Area area, World world)
        {
            this.representation = representation;

            this.location = location;

            this.ID = ID;
            this.area = area;
            this.world = world;

            tilesAtRange.Add(new List<Tile>(new Tile[] { this }));

            creaturesInVisionRange = new HashSet<Creature>();

            // create the neighbor-arrays (6 directions)
            hasNeighbor = new bool[6];
            neighbors = new Tile[6];
            areaLinks = new AreaLink[6];
        }

        // creatures can register as viewing this tile or not viewing this tile
        public void RegisterAsViewing(Creature viewingCreature)
        {
            creaturesInVisionRange.Add(viewingCreature);
        }
        public void DeregisterAsViewing(Creature viewingCreature)
        {
            creaturesInVisionRange.Remove(viewingCreature);
        }
        private void SendVisionEvents()
        {
            foreach (Creature viewingCreature in creaturesInVisionRange)
            {
                viewingCreature.VisionEvent(this);
            }
        }

        // creates a tile of the right kind
        public static Tile CreateTileOfType(TileType type, TileRepresentation representation, Location location, int ID, Area area, World world)
        {
            switch (type)
            {
                case TileType.Floor:
                    return new Floor(representation, location, ID, area, world);
                case TileType.Wall:
                    return new Wall(representation, location, ID, area, world);
                case TileType.Stairs:
                    return new Stairs(representation, location, ID, area, world);
                case TileType.Campfire:
                    return new Campfire(representation, location, ID, area, world);
            }
            return null;
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

        // ID can be requested and set. Setting it is sometimes necessary when more
        // entrances or exits are generated in a file and tiles have to be moved.
        // Be extremely careful altering IDs though.
        public int GetID()
        {
            return ID;
        }
        public void SetID(int ID)
        {
            this.ID = ID;
        }

        // world and area can be requested
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
            hasAreaLink = true;
        }

        // check if a tile is linked to another area
        public bool HasAreaLink()
        {
            return hasAreaLink;
        }

        // create a link to a tile. If it has a link but that link is not this tile,
        // output messages to show linking is incorrect if needed, but don't force links
        // to follow the standard rule. Teleporter tiles, special tiles, etcetera should
        // not be forced to follow the rules for standard tiles.
        public void Link(int direction, Tile neighbor)
        {
            // set the neighbor tile to be the supplied tile
            neighbors[direction] = neighbor;

            // check if, if that tile is linked as well, it's linked to this tile.
            if (neighbor.HasNeighbor(Directions.Inverse(direction)))
            {
                if (neighbor.GetNeighbor(Directions.Inverse(direction)) != this)
                {
                    Log.Print("[" + area.GetName() + "(" + ID + ")] no linkback (" + Directions.ToString(direction) + ")");
                }
            }

            // check that the tile linked to is in the correct relation to this one x/y/z-wise
            Location properNeighbor = Directions.GetNeighboring(direction, location);
            Location neighborLocation = neighbor.GetLocation();

            if (!properNeighbor.Equals(neighborLocation))
            {
                Log.Print("[" + area.GetName() + "(" + ID + ")] link to tile that's not adjacent. Possible error (" + Directions.ToString(direction) + ")");
            }

            // set the flag to show this tile has a neighbor in the given direction
            hasNeighbor[direction] = true;
        }

        // unlink a tile
        public void Unlink(int direction)
        {
            // remove the link, and the flag showing there is a link. We don't have to
            // remove an area link struct, since it won't be checked when the flag is false
            // or an actual link exists.
            neighbors[direction] = null;
            hasNeighbor[direction] = false;
        }

        // unlink an area link when an area is unloaded
        public void UnlinkAreaOnUnload(int direction)
        {
            // if this link has been used, it's been "proper" linked. Remove that
            // link, but keep areaLinks and hasNeighbor as they are.
            neighbors[direction] = null;
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

            SendVisionEvents();

            // this area is active at this point
            area.SetActive();
        }

        // clears the occupant from this tile
        public void Vacate()
        {
            this.occupant = null;

            SendVisionEvents();
        }

        // the x/y/z location in the world
        public Location GetLocation()
        {
            return location;
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

                // link this tile
                Link(direction, neighbor);
            }
            return neighbors[direction];
        }

        // get the integer value of a link, return -2 when it's an area link
        public int GetLink(int direction)
        {
            // check if there's a neighbor
            if (hasNeighbor[direction])
            {
                if (neighbors[direction] != null && neighbors[direction].GetArea().Equals(area))
                {
                    // if it's in the same area, return the ID of the target tile.
                    return neighbors[direction].ID;
                }
                else
                {
                    // if it's in a different area, return -2
                    return -2;
                }
            }
            else
            {
                // if there isn't, return -1
                return -1;
            }
        }

        // Get the text of a link, used when saving an area
        public String GetLinkText(int direction)
        {
            // check if there's a neighbor
            if (hasNeighbor[direction])
            {
                // if there is, check if it's in the same area (either it's not linked yet if another
                // area hasn't been loaded, or the area name is different.
                if (neighbors[direction] != null && neighbors[direction].GetArea().Equals(area))
                {
                    // if it's in the same area, return the ID of the target tile.
                    return neighbors[direction].ID.ToString();
                }
                else
                {
                    // if it's in a different area, return the area name and the target ID separated
                    // by a semicolon
                    return areaLinks[direction].areaName + ";" + areaLinks[direction].targetID;
                }
            }
            else
            {
                // if there's no link, return -1
                return "-1";
            }
        }

        // gets the tiles at a certain range of this one (0 = this tile, 5 = all tiles at a distance of 5).
        public List<Tile> GetTilesAtRange(int range)
        {
            // ensure the tilesAtRange list has been loaded to this range and fill it to this range if needed
            FillTilesAtRangeTo(range);
            
            // return the correct set of tiles
            return tilesAtRange[range];
        }

        // gets all the tiles within a certain range of this one (0 = this tile, 5 = all tiles at a distance of 5 or less).
        public List<Tile> GetTilesInRange(int range)
        {
            // ensure the tilesAtRange list has been loaded to this range and fill it to this range if needed
            FillTilesAtRangeTo(range);

            // a list to aggregate the tiles at all the different ranges
            List<Tile> aggregateTiles = new List<Tile>();

            // get the tiles at each range up to and including the range asked and add them to the result
            for (int n = 0; n <= range; n++)
            {
                aggregateTiles.AddRange(tilesAtRange[n]);
            }

            // return the result
            return aggregateTiles;
        }

        // checks if the tiles at this range are already in memory, and if not, adds them.
        private void FillTilesAtRangeTo(int range)
        {
            // range 0 is just this tile, at that point the count of tilesAtRange is 1. For each layer added
            // the count goes up by one, so if the count is not at least equal to the range we need to add more layers
            if (tilesAtRange.Count < range) Geography.FillTilesAtRange(range, ref tilesAtRange);
        }

        // the type of tile
        public abstract TileType GetTileType();

        // get the image key for this tile
        public TileRepresentation GetRepresentation()
        {
            return representation;
        }

        // check if this tile is passable
        public abstract bool IsPassable();
    }
}
