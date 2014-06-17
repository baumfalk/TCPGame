using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

using TCPGameServer.World.Map.Generation;
using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.IO
{
    class AreaReader
    {
        // the area being loaded, and the world it's in
        private static Area area;
        private static World world;

        // load an area from file, based on its name
        public static AreaData Load(String name, Area area, World world)
        {
            AreaReader.area = area;
            AreaReader.world = world;

            AreaData toReturn = new AreaData();

            AreaFileData fileData = AreaFile.Read(name);

            GeneratorData generatorData = CreateGeneratorData(fileData);

            Output.Print("Generating area " + name);

            if (fileData.entrances.numberOfTiles > 0)
            {
                toReturn = AreaGenerator.Generate(generatorData);
                toReturn.areaType = fileData.header.areaType;
            }

            // link up the tiles, based on the link array we received from the
            // tile parser or generator.
            LinkTiles(toReturn);

            return toReturn;
        }

        private static GeneratorData CreateGeneratorData(AreaFileData fileData)
        {
            GeneratorData generatorData = new GeneratorData();

            generatorData.area = area;
            generatorData.world = world;

            generatorData.generateExits = fileData.header.fileType.Equals("Stub");
            generatorData.areaType = fileData.header.areaType;
            generatorData.seed = fileData.header.seed;
            generatorData.bottomLeft = MapGridHelper.MapGridLocationToBottomLeft(fileData.header.mapGridLocation);

            generatorData.entrances = ParseTileBlock(fileData.entrances);

            int numEntrances = fileData.entrances.numberOfTiles;
            generatorData.fixedTiles = new Tile[numEntrances][];
            for (int n = 0; n < numEntrances; n++)
            {
                generatorData.fixedTiles[n] = ParseTileBlock(fileData.fixedTiles[n]);
            }

            return generatorData;
        }

        // parse the tiles from file, creating the objects and adding them to the
        // array. Returns an array which contains the links for each tile.
        private static Tile[] ParseTileBlock(TileBlockData tileBlockData)
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

                // the links are on the next line, add them to the array
                SetAreaLinks(tiles[n], tileData.links);
            }

            // return the tiles
            return tiles;
        }

        // parses link strings to ints. Links up areas directly.
        private static void SetAreaLinks(Tile tile, String[] linkData)
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
