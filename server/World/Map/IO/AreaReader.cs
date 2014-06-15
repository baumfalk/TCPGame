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

        // load an area from file, based on its name
        public static AreaData Load(String name, Area area, World world)
        {
            AreaReader.area = area;
            AreaReader.world = world;

            AreaData toReturn = new AreaData();

            // the filename is the area name plus an .are extension
            StreamReader fileReader = new StreamReader(gitPath + name + ".are");

            // the number of tiles is on the first line of the file
            int numTiles = int.Parse(fileReader.ReadLine());

            // initialize the tiles array
            toReturn.tiles = new Tile[numTiles];

            // check if this is a complete area or a stub
            String fileType = fileReader.ReadLine();

            // get the type of Area
            String areaType = fileReader.ReadLine();

            // get the seed for the random number generator for this area
            int seed;
            int.TryParse(fileReader.ReadLine(), out seed);

            if (fileType.Equals("Stub"))
            {
                Output.Print("First generate on area " + name);

                toReturn = GenerateFromStub(numTiles, seed, areaType, fileReader, true);

                LinkTiles(toReturn);

                fileReader.Close();

                //AreaWriter.Save(toReturn, name, areaType, "Generated");
            }
            else if (fileType.Equals("Generated"))
            {
                Output.Print("Generating area " + name);

                toReturn = GenerateFromStub(numTiles, seed, areaType, fileReader, false);

                LinkTiles(toReturn);

                fileReader.Close();
            }
            else if (fileType.Equals("Complete"))
            {
                Output.Print("Loading area " + name);

                // parse the tiles from the file, returning a string array with links.
                // These need to be added after creating all the tiles, so the objects
                // to link all exist.
                toReturn = ParseTiles(numTiles, fileReader);

                // link up the tiles, based on the link array we received from the
                // tile parser or generator.
                LinkTiles(toReturn);

                fileReader.Close();
            }
            else
            {
                Output.Print("invalid area file");

                fileReader.Close();
            }

            return toReturn;
        }

        private static AreaData GenerateFromStub(int numTiles, int seed, String areaType, StreamReader fileReader, bool generateExits)
        {
            AreaData entrances = ParseTiles(numTiles, fileReader);

            LinkTiles(entrances);

            AreaGenerator areaGenerator = new AreaGenerator();

            return areaGenerator.Generate(seed, areaType, entrances.tiles, generateExits, -50, -50, 0, area, world);
        }

        // parse the tiles from file, creating the objects and adding them to the
        // array. Returns an array which contains the links for each tile.
        private static AreaData ParseTiles(int numTiles, StreamReader fileReader)
        {
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
                tiles[n] = ParseTile(n, tileData);

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
                if (int.TryParse(splitLinks[direction], out value)) {
                    // if it can be, it's the ID of the target tile
                    toReturn[direction] = value;
                }
                else {
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
