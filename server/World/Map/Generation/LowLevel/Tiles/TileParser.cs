using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Tiles
{
    class TileParser
    {
        // parse the tiles from file, creating the objects and adding them to the
        // array. Returns an array which contains the links for each tile.
        public static Tile[] ParseTileBlock(TileBlockData tileBlockData, Area area, World world)
        {
            int numTiles = tileBlockData.numberOfTiles;

            // initialize the arrays
            Tile[] tiles = new Tile[numTiles];

            // loop through every tile in the block
            for (int n = 0; n < numTiles; n++)
            {
                TileData tileData = tileBlockData.tileData[n];

                // create the tile and add it to the tiles array
                tiles[n] = new Tile(tileData.type,
                                    tileData.representation,
                                    tileData.location,
                                    tileData.ID,
                                    area,
                                    world);

                // set the area links
                SetAreaLinks(tiles[n], tileData.links);
            }

            // return the tiles
            return tiles;
        }

        // parses link strings to ints. Links up areas directly.
        public static void SetAreaLinks(Tile tile, String[] linkData)
        {
            if (tile.HasAreaLink())
            {
                for (int direction = 0; direction < 6; direction++)
                {
                    if (linkData[direction].Contains(';'))
                    {
                        // directly link up area links, don't further link this tile in
                        // this direction
                        AddAreaLink(direction, tile, linkData[direction]);
                    }
                }
            }
        }

        // creates area links
        private static void AddAreaLink(int direction, Tile toLink, String areaLink)
        {
            // split into name and ID
            String[] splitAreaLink = areaLink.Split(';');

            String areaName = splitAreaLink[0];
            int ID = int.Parse(splitAreaLink[1]);

            // create an area link using the data read
            toLink.CreateAreaLink(direction, areaName, ID);
        }
    }
}
