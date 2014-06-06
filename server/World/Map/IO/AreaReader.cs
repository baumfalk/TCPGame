using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.IO
{
    class AreaReader
    {
        // the tiles in the area, to be returned on load completion
        private static Tile[] tiles;

        // the area being loaded, and the world it's in
        private static Area area;
        private static World world;

        // path, based on where the files are in the git repository
        private static String gitPath = @"../../../map/";

        // load an area from file, based on it's name
        public static Tile[] Load(String name, Area area, World world)
        {
            Output.Print("Loading area " + name);

            AreaReader.area = area;
            AreaReader.world = world;

            // the filename is the area name plus a .are extension
            StreamReader fileReader = new StreamReader(gitPath + name + ".are");

            // the number of tiles is on the first line of the file
            int numTiles = int.Parse(fileReader.ReadLine());

            // initialize the tiles array
            tiles = new Tile[numTiles];

            // parse the tiles from the file, returning a string array with links.
            // These need to be added after creating all the tiles, so the objects
            // to link all exist.
            String[] links = ParseTiles(numTiles, fileReader);

            // close the streamreader
            fileReader.Close();

            // link up the tiles, based on the link array we received from the
            // tile parser.
            LinkTiles(numTiles, links);

            return tiles;
        }

        // parse the tiles from file, creating the objects and adding them to the
        // array. Returns an array which contains the links for each tile.
        private static String[] ParseTiles(int numTiles, StreamReader fileReader)
        {
            // initialize the array
            String[] links = new String[numTiles];

            // loop through every tile in the file
            for (int n = 0; n < numTiles; n++)
            {
                // we have seven lines per tile, but the last is read separately
                String[] tileData = new String[6];

                // loop through them, and add them to the tileData array
                for (int i = 0; i < 6; i++)
                {
                    tileData[i] = fileReader.ReadLine();
                }

                // create the tile and add it to the tiles array
                ParseTile(n, tileData);

                // the links are on the next line, add them to the array
                links[n] = fileReader.ReadLine();
            }

            // return the links
            return links;
        }

        // parse an individual tile from the data in the file
        private static void ParseTile(int index, String[] tileData)
        {
            // the tile is represented as follows in the file:
            /* ID                   0
             * type                 floor
             * representation       2floor
             * x                    12
             * y                    10
             * z                    1
             * links n/e/u/s/w/d    -1,1,-1,5,12,-1
             */
            // links aren't parsed here, the rest is read as follows:
            int id = int.Parse(tileData[0]);
            String type = tileData[1];
            String representation = tileData[2];
            int x = int.Parse(tileData[3]);
            int y = int.Parse(tileData[4]);
            int z = int.Parse(tileData[5]);

            // the the index in the loop and the index in the file don't match, someone made a mistake
            // writing the map file. It's likely errors will occur.
            if (index != id) Output.Print("ID mismatch (ID = " + index + ", file ID = " + id + "), map file incorrect. Bad links expected.");

            // create the tile. Although we do read the ID from the file, we only use it as a check on
            // the mapfile. Actual tile ID is the index in the array.
            tiles[index] = new Tile(type, representation, x, y, z, index, area, world);
        }

        // links the tiles in the area together
        private static void LinkTiles(int numTiles, String[] links)
        {
            // loop through all the tiles
            for (int n = 0; n < numTiles; n++)
            {
                // split into the six different directions. -1 indicates no link,
                // a link into another area is denoted with the area name, a semicolon
                // and the tile ID in the other area.
                String[] link = links[n].Split(',');

                // we check for each direction
                for (int direction = 0; direction < 6; direction++)
                {
                    // area links contain a semicolon
                    if (link[direction].Contains(';'))
                    {
                        // split into name and ID
                        String[] areaLink = link[direction].Split(';');

                        String areaName = areaLink[0];
                        int ID = int.Parse(areaLink[1]);

                        // create an area link using the data read
                        tiles[n].CreateAreaLink(direction, areaName, ID);
                    }
                    else
                    {
                        // get the link direction
                        int linkTo = int.Parse(link[direction]);

                        // if there is a link, tell the Tile to hook it up
                        if (linkTo > -1) tiles[n].Link(direction, tiles[linkTo]);
                    }
                }
            }
        }
    }
}
