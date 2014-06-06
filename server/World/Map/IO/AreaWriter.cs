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

        public static void Save(Tile[] tiles, String name)
        {
            int numTiles = tiles.Length;

            StreamWriter fileWriter = new StreamWriter(gitPath + name + ".are");

            fileWriter.WriteLine(numTiles);

            for (int n = 0; n < numTiles; n++)
            {
                fileWriter.WriteLine(n);
                fileWriter.WriteLine(tiles[n].GetType());
                fileWriter.WriteLine(tiles[n].GetRepresentation());
                fileWriter.WriteLine(tiles[n].GetX());
                fileWriter.WriteLine(tiles[n].GetY());
                fileWriter.WriteLine(tiles[n].GetZ());
                fileWriter.WriteLine(ParseDirections(tiles[n]));
            }

            fileWriter.Close();
        }

        private static String ParseDirections(Tile tile)
        {
            String linkData = "";

            for (int n = 0; n < 5; n++)
            {
                linkData += tile.GetLinkText(n) + ",";
            }
            linkData += tile.GetLinkText(6);

            return linkData;
        }
    }
}
