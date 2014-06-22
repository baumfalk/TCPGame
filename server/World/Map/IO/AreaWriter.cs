using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;
using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.IO
{
    class AreaWriter
    {
        public static int AddEntrance(TileData exit, int direction, String exitAreaName, String targetAreaName, World world)
        {
            TileData target = new TileData();

            target.type = exit.type;
            target.representation = exit.representation;
            target.location = Directions.GetNeighboring(direction, exit.location);
            
            int linkDirection = Directions.Inverse(direction);
            target.links[linkDirection] = exitAreaName + ";" + exit.ID;
                
            Location mapGridPosition = MapGridHelper.TileLocationToMapGridLocation(target.location);

            bool createFile = !AreaFile.Exists(targetAreaName);
            if (createFile) CreateMapFile(targetAreaName, mapGridPosition, target, world);
            else AddEntranceToMapFile(target, targetAreaName);

            return target.ID;
        }

        private static void AddEntranceToMapFile(TileData target, String name)
        {
            // read the file
            AreaFileData fileData = AreaFile.Read(name);

            // check if the file already contains this entrance and returns if so.
            if (CheckForTarget(fileData, target)) return;

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

        // checks if an entrance is already in a file
        private static bool CheckForTarget(AreaFileData fileData, TileData target)
        {
            // check if the location of an entrance is the same as the location of the target.
            for (int n = 0; n < fileData.entrances.numberOfTiles; n++)
            {
                TileData entrance = fileData.entrances.tileData[n];

                if (entrance.location.Equals(target.location)) return true;
            }

            return false;
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

        public static void SaveStatic(String name, TileBlockData entrances, TileBlockData[] fixedTiles)
        {
            Log.Print("Writing static for " + name);

            AreaFileData fileData = new AreaFileData();

            HeaderData header = AreaFile.ReadHeader(name);

            fileData.header = header;

            fileData.header.fileType = "Generated";

            fileData.entrances = entrances;

            if (entrances.numberOfTiles == fixedTiles.Length)
            {
                fileData.fixedTiles = fixedTiles;
            }
            else
            {
                TileBlockData[] paddedFixedTiles = new TileBlockData[entrances.numberOfTiles];

                for (int n = 0; n < entrances.numberOfTiles; n++)
                {
                    if (fixedTiles.Length > n)
                    {
                        paddedFixedTiles[n] = fixedTiles[n];
                    }
                    else
                    {
                        paddedFixedTiles[n] = new TileBlockData();
                        paddedFixedTiles[n].numberOfTiles = 0;
                    }
                }

                fileData.fixedTiles = paddedFixedTiles;
            }

            AreaFile.Write(fileData, name);
        }
    }
}
