using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO;

namespace TCPGameServer.World.Map
{
    public class Area
    {
        // the world this area is part of
        private World world;

        // the name of the area
        private String name;

        // list of tiles
        private Tile[] tiles;

        // on creation, an area should load itself from file
        public Area(World world, String name)
        {
            this.world = world;
            this.name = name;

            // load the tiles form the area reader
            tiles = AreaReader.Load(name, this, world);
        }

        // get the area's name
        public String GetName()
        {
            return name;
        }

        // get a tile from the area by ID
        public Tile GetTile(int ID)
        {
            return tiles[ID];
        }
    }
}
