using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;
using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.IO
{
    class AreaWriter
    {
        public static int AddEntrance(Tile exit, int direction, String name)
        {
            TileData target = new TileData();

            target.type = exit.GetTileType();
            target.representation = exit.GetRepresentation();
            target.location = Directions.GetNeighboring(direction, exit.GetLocation());
            
            int linkDirection = Directions.Inverse(direction);
            target.links[linkDirection] = exit.GetArea().GetName() + ";" + exit.GetID();
                
            Location mapGridPosition = MapGridHelper.TileLocationToMapGridLocation(target.location);

            if (!name.Equals("x" + mapGridPosition.x + "y" + mapGridPosition.y + "z" + mapGridPosition.z)) {
                Output.Print("name (" + name + ") does not match area (" + "x" + mapGridPosition.x + "y" + mapGridPosition.y + "z" + mapGridPosition.z + "), aborting AddEntrance");
                
                return -1;
            }

            bool createFile = !AreaFile.Exists(name);
            if (createFile) CreateMapFile(name, mapGridPosition, target, exit.GetWorld());
            else AddEntranceToMapFile(target, name);

            return target.ID;
        }

        private static void AddEntranceToMapFile(TileData target, String name)
        {
            // read the file
            AreaFileData fileData = AreaFile.Read(name);

            // the ID of our tile will be the current number of entrances
            target.ID = fileData.entrances.numberOfTiles;

            // increase the number of entrances by one
            fileData.entrances.numberOfTiles += 1;

            // get the entrance data
            TileData[] entrances = fileData.entrances.tileData;

            // create a new entrance array one larger than current, and add our target
            // tile at the end, then set the new array to be the entrance array
            TileData[] newEntrances = new TileData[entrances.Length + 1];
            Array.Copy(entrances, newEntrances, entrances.Length);
            newEntrances[entrances.Length] = target;
            fileData.entrances.tileData = newEntrances;

            // get the fixed tile block array
            TileBlockData[] fixedTiles = fileData.fixedTiles;

            // loop through all the tiles in all the blocks and increase their ID
            // by one.
            for (int n = 0; n < fixedTiles.Length; n++)
            {
                for (int i = 0; i < fixedTiles[n].tileData.Length; i++)
                {
                    fixedTiles[n].tileData[i].ID++;
                }
            }

            // copy the fixed tile block array into an array one bigger, and add an
            // empty tile block at the end (no fixed tiles are associated with this
            // entrance)
            TileBlockData[] newFixedTiles = new TileBlockData[fixedTiles.Length + 1];
            Array.Copy(fixedTiles, newFixedTiles, fixedTiles.Length);
            newFixedTiles[fixedTiles.Length] = new TileBlockData();
            fileData.fixedTiles = newFixedTiles;

            // write the updated file back to disk
            AreaFile.Write(fileData, name);

            // return the ID of the new entrance
        }

        private static void CreateMapFile(String name, Location mapGridPosition, TileData target, World world)
        {
            AreaFileData fileData = new AreaFileData();

            target.ID = 0;

            fileData.header.fileType = "Stub";
            fileData.header.areaType = world.GetAreaType(mapGridPosition);
            fileData.header.seed = world.GetAreaSeed(mapGridPosition);
            fileData.header.mapGridLocation = mapGridPosition;

            fileData.entrances.numberOfTiles = 1;
            fileData.entrances.tileData = new TileData[] { target };

            fileData.fixedTiles = new TileBlockData[] {new TileBlockData()};

            AreaFile.Write(fileData, name);
        }

        public static void SaveStatic(String name, Tile[] entrances, Tile[] exits, Tile[][] fixedTiles)
        {
            Output.Print("Writing static for " + name);

            AreaFileData fileData = new AreaFileData();

            HeaderData header = AreaFile.ReadHeader(name);

            fileData.header = header;

            fileData.header.fileType = "Generated";

            int numberOfEntrances = entrances.Length + exits.Length;

            fileData.entrances.numberOfTiles = numberOfEntrances;
            fileData.entrances.tileData = new TileData[numberOfEntrances];

            for (int n = 0; n < entrances.Length; n++) {
                fileData.entrances.tileData[n] = TileData.FromTile(entrances[n]);
            }

            for (int n = 0; n < exits.Length; n++)
            {
                fileData.entrances.tileData[n + entrances.Length] = TileData.FromTile(exits[n]);
            }

            fileData.fixedTiles = new TileBlockData[numberOfEntrances];
            for (int n = 0; n < fixedTiles.Length; n++)
            {
                fileData.fixedTiles[n] = new TileBlockData();

                int numberOfFixedTiles = fixedTiles[n].Length;

                fileData.fixedTiles[n].numberOfTiles = numberOfFixedTiles;
                fileData.fixedTiles[n].tileData = new TileData[numberOfFixedTiles];

                for (int i = 0; i < numberOfFixedTiles; i++)
                {
                    fileData.fixedTiles[n].tileData[i] = TileData.FromTile(fixedTiles[n][i]);
                }
            }

            for (int n = 0; n < exits.Length; n++)
            {
                fileData.fixedTiles[n + entrances.Length] = new TileBlockData();
            }

            AreaFile.Write(fileData, name);
        }
    }
}
