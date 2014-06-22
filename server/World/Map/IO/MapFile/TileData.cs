using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.IO.MapFile
{
    public class TileData
    {
        public int ID;
        public TileType type;
        public String representation;
        public Location location;
        public String[] links;

        public TileData()
        {
            location = new Location();
            links = new String[] { "-1", "-1", "-1", "-1", "-1", "-1" };
        }

        public static TileData FromTile(Tile input)
        {
            TileData toReturn = new TileData();

            toReturn.ID = input.GetID();
            toReturn.type = input.GetTileType();
            toReturn.representation = input.GetRepresentation();
            toReturn.location = input.GetLocation();
            toReturn.links = new String[] {
                input.GetLinkText(Directions.NORTH),
                input.GetLinkText(Directions.EAST),
                input.GetLinkText(Directions.UP),
                input.GetLinkText(Directions.SOUTH),
                input.GetLinkText(Directions.WEST),
                input.GetLinkText(Directions.DOWN)};

            return toReturn;
        }
    }
}
