using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map.Tiles
{
    class Floor : Tile
    {
        public Floor(TileRepresentation representation, Location location, int ID, Area area, World world)
            : base(representation, location, ID, area, world)
        {

        }

        public override TileType GetTileType()
        {
            return TileType.Stairs;
        }

        public override bool IsPassable()
        {
            return true;
        }
    }
}
