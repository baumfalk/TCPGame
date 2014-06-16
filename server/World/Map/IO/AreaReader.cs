using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using TCPGameServer.Control.IO;

using TCPGameServer.World.Map.Generation;

namespace TCPGameServer.World.Map.IO
{
    class AreaReader
    {
        // the area being loaded, and the world it's in
        private static Area area;
        private static World world;

        // path, based on where the files are in the git repository
        private static String gitPath = @"../../../map/";

        public static bool Exists(String name)
        {
            return File.Exists(gitPath + name + ".are");
        }

        public static bool IsStub(String name)
        {
            StreamReader fileReader = new StreamReader(gitPath + name + ".are");

            String fileType = fileReader.ReadLine();

            fileReader.Close();

            return fileType.Equals("Stub");
        }

        // load an area from file, based on its name
        public static AreaData Load(String name, Area area, World world)
        {
            Output.Print("Load");

            AreaReader.area = area;
            AreaReader.world = world;

            AreaData toReturn = new AreaData();

            // the filename is the area name plus an .are extension
            StreamReader fileReader = new StreamReader(gitPath + name + ".are");

            // check if this is a complete area or a stub
            String fileType = fileReader.ReadLine();

            // get the type of Area
            String areaType = fileReader.ReadLine();

            // get the seed for the random number generator for this area
            int seed = int.Parse(fileReader.ReadLine());

            int[] bottomLeft = new int[3];
            bottomLeft[0] = int.Parse(fileReader.ReadLine()); //x
            bottomLeft[1] = int.Parse(fileReader.ReadLine()); //y
            bottomLeft[2] = int.Parse(fileReader.ReadLine()); //z

            int numEntrances = int.Parse(fileReader.ReadLine());

            AreaData entrances = ParseTiles(numEntrances, fileReader, 0);

            Tile[][] fixedTiles = new Tile[entrances.tiles.Length][];

            for (int n = 0; n < entrances.tiles.Length; n++)
            {
                int numFixed = int.Parse(fileReader.ReadLine());

                AreaData fixedData = ParseTiles(numFixed, fileReader, entrances.tiles.Length);

                fixedTiles[n] = fixedData.tiles;
            }

            Output.Print("Generating area " + name);

            if (entrances.tiles.Length > 0)
            {
                toReturn = Generate(seed, areaType, fileType, entrances.tiles, fixedTiles, fileReader, bottomLeft);
            }

            // link up the tiles, based on the link array we received from the
            // tile parser or generator.
            LinkTiles(toReturn);

            fileReader.Close();

            return toReturn;
        }

        private static AreaData Generate(int seed, String areaType, String fileType, Tile[] entrances, Tile[][] fixedTiles, StreamReader fileReader, int[] bottomLeft)
        {
            Output.Print("AreaReader.Generate");

            bool generateExits = fileType.Equals("Stub");

            AreaGenerator areaGenerator = new AreaGenerator();

            AreaData toReturn = areaGenerator.Generate(seed, areaType, entrances, fixedTiles, generateExits, bottomLeft[0], bottomLeft[1], bottomLeft[2], area, world);

            Output.Print("AreaReader.Generate return");

            return toReturn;
        }

        // parse the tiles from file, creating the objects and adding them to the
        // array. Returns an array which contains the links for each tile.
        private static AreaData ParseTiles(int numTiles, StreamReader fileReader, int IDOffset)
        {
            Output.Print("ParseTiles");

            // initialize the arrays
            int[][] links = new int[numTiles][];
            Tile[] tiles = new Tile[numTiles];

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
                tiles[n] = ParseTile(n + IDOffset, tileData);

                // the links are on the next line, add them to the array
                links[n] = ParseLinks(tiles[n], fileReader.ReadLine());
            }

            AreaData toReturn = new AreaData();

            toReturn.tiles = tiles;
            toReturn.linkData = links;

            // return the links
            return toReturn;
        }

        // parse an individual tile from the data in the file
        private static Tile ParseTile(int index, String[] tileData)
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
            return new Tile(type, representation, x, y, z, index, area, world);
        }

        // parses link strings to ints. Links up areas directly.
        private static int[] ParseLinks(Tile tile, String linkData)
        {
            String[] splitLinks = linkData.Split(',');
            int[] toReturn = new int[6];

            for (int direction = 0; direction < 6; direction++)
            {
                // if a link can't be parsed to an int, it's an area link.
                int value;
                if (int.TryParse(splitLinks[direction], out value))
                {
                    // if it can be, it's the ID of the target tile
                    toReturn[direction] = value;
                }
                else
                {
                    // directly link up area links, don't further link this tile in
                    // this direction
                    AddAreaLink(direction, tile, splitLinks[direction]);
                    toReturn[direction] = -1;
                }
            }

            return toReturn;
        }

        // creates area links
        private static void AddAreaLink(int direction, Tile toLink, String areaLink)
        {
            Output.Print("AddAreaLink");

            // split into name and ID
            String[] splitAreaLink = areaLink.Split(';');

            String areaName = splitAreaLink[0];
            int ID = int.Parse(splitAreaLink[1]);

            // create an area link using the data read
            toLink.CreateAreaLink(direction, areaName, ID);
        }

        // links the tiles in the area together
        private static void LinkTiles(AreaData areaData)
        {
            Output.Print("LinkTiles");

            int numTiles = areaData.tiles.Length;

            int[][] link = areaData.linkData;

            // loop through all the tiles
            for (int n = 0; n < numTiles; n++)
            {
                // split into the six different directions. -1 indicates no link,
                // a link into another area is denoted with the area name, a semicolon
                // and the tile ID in the other area.
                
                // we check for each direction
                for (int direction = 0; direction < 6; direction++)
                {
                    // get the link in this direction
                    int linkTo = link[n][direction];

                    // if there is a link, tell the Tile to hook it up
                    if (linkTo > -1) areaData.tiles[n].Link(direction, areaData.tiles[linkTo]);
                }
            }
        }
    }
}
