using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map.Tiles
{
    class Wall : Tile
    {
        public Wall(TileRepresentation representation, Location location, int ID, Area area, World world)
            : base(representation, location, ID, area, world)
        {

        }

        public override TileType GetTileType()
        {
            return TileType.Wall;
        }

        public override bool IsPassable()
        {
            return false;
        }
    }
}
