using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.Tiles
{
    class Campfire : Tile
    {
        public Campfire(String representation, Location location, int ID, Area area, World world)
            : base(representation, location, ID, area, world)
        {

        }

        public override TileType GetTileType()
        {
            return TileType.Campfire;
        }

        public override bool IsPassable()
        {
            return true;
        }
    }
}
