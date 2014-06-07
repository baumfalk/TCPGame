using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TCPGameServer.World.Map.IO
{
    class AreaWriter
    {
        // path, based on where the files are in the git repository
        private static String gitPath = @"../../../map/";

        // save an area to file, based on its name
        public static void Save(Tile[] tiles, String name)
        {
            // the number of tiles
            int numTiles = tiles.Length;

            // create a stream writer at the proper path (overwrites files with the same name
            StreamWriter fileWriter = new StreamWriter(gitPath + name + ".are");

            // first line is the number of tiles
            fileWriter.WriteLine(numTiles);

            // loop over all the tiles, writing down their data
            for (int n = 0; n < numTiles; n++)
            {
                // the index
                fileWriter.WriteLine(n);
                // tile type
                fileWriter.WriteLine(tiles[n].GetType());
                // tile representation
                fileWriter.WriteLine(tiles[n].GetRepresentation());
                // x/y/z coordinates
                fileWriter.WriteLine(tiles[n].GetX());
                fileWriter.WriteLine(tiles[n].GetY());
                fileWriter.WriteLine(tiles[n].GetZ());
                // link info
                fileWriter.WriteLine(ParseDirections(tiles[n]));
            }
            // close the streamwriter
            fileWriter.Close();
        }

        // parses the links to strings
        private static String ParseDirections(Tile tile)
        {
            String linkData = "";

            // Use the GetLinkText method in Tile to get direct links and area
            // links, separate the six directions with commas
            for (int n = 0; n < 5; n++)
            {
                linkData += tile.GetLinkText(n) + ",";
            }
            linkData += tile.GetLinkText(6);

            // return the completed string
            return linkData;
        }
    }
}
