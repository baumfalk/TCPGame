using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Tiles
{
    class TileLinker
    {
        // Links up areas directly.
        public static void SetAreaLinks(Tile tile, String[] linkData)
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

        public static int[][] GetLinks(int tileCount, Tile[,] tiles, List<TileAndLocation> tileList)
        {
            int[][] linkData = new int[tileCount][];

            for (int ID = 0; ID < tileList.Count; ID++)
            {
                linkData[ID] = new int[6];

                for (int direction = 0; direction < 6; direction++)
                {
                    Tile tile = tileList[ID].tile;
                    Location location = tileList[ID].location;

                    if (tile.HasNeighbor(direction))
                    {
                        linkData[ID][direction] = -1;
                    }
                    else
                    {
                        Location neighbor = Directions.GetNeighboring(direction, location);

                        if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.z == location.z &&
                            neighbor.x <= 99 && neighbor.y <= 99 &&
                            tiles[neighbor.x, neighbor.y] != null)
                        {
                            linkData[ID][direction] = tiles[neighbor.x, neighbor.y].GetID();
                        }
                        else
                        {
                            linkData[ID][direction] = -1;
                        }
                    }
                }
            }

            return linkData;
        }
    }
}
