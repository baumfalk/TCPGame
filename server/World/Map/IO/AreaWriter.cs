using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.IO
{
    class AreaWriter
    {
        // path, based on where the files are in the git repository
        private static String gitPath = @"../../../map/";

        // save an area to file, based on its name
        public static void Save(Tile[] tiles, String name, String areaType, int seed, String fileType)
        {
            Output.Print("writing file for " + name);

            // the number of tiles
            int numTiles = tiles.Length;

            Output.Print("area contains " + tiles.Length + " tiles");

            // create a stream writer at the proper path (overwrites files with the same name)
            StreamWriter fileWriter = new StreamWriter(gitPath + name + ".are2");

            // first line is the number of tiles
            fileWriter.WriteLine(numTiles);

            // write the file type
            fileWriter.WriteLine(fileType);

            // write the area type
            fileWriter.WriteLine(areaType);

            // write all tiles when saving a complete area, otherwise save as a stub
            bool writeNormalTiles = fileType.Equals("Complete");

            // if this is not a complete area file, write down the seed
            if (!writeNormalTiles) fileWriter.WriteLine(seed);

            // loop over all the tiles, writing down their data
            for (int n = 0; n < numTiles; n++)
            {
                String links = ParseDirections(tiles[n]);

                //if (!links.Equals("-1,-1,-1,-1,-1,-1")) Output.Print(links);

                if (links.Contains(';') || writeNormalTiles)
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
                    fileWriter.WriteLine(links);
                }
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
            linkData += tile.GetLinkText(5);

            if (tile.GetID() == 0) Output.Print(linkData);

            // return the completed string
            return linkData;
        }
    }
}
