using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using TCPGameServer.World;
using TCPGameServer.World.Map.IO;
using TCPGameServer.World.Map.IO.MapFile;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map
{
    class MapReset
    {
        public static World ResetMap(Model model)
        {
            foreach (Player player in model.getCopyOfPlayerList())
            {
                player.AddMessage("QUIT", int.MinValue);
            }
            World world = new World();

            DeleteAllMapFiles();

            WriteDefaultArea(world);

            return world;
        }

        private static void DeleteAllMapFiles()
        {
            String mapPath = TCPGameSharedInfo.FileLocations.mapPath;

            if (Directory.Exists(mapPath))
            {
                foreach (String fileName in Directory.EnumerateFiles(mapPath))
                {
                    if (fileName.EndsWith(".are")) File.Delete(fileName);
                }
            }
            else Directory.CreateDirectory(mapPath);
        }

        private static void WriteDefaultArea(World world)
        {
            AreaFileData defaultArea = GetDefaultArea(world);

            AreaFile.Write(defaultArea, "x0y0z0");
        }

        private static AreaFileData GetDefaultArea(World world)
        {
            AreaFileData fileData = new AreaFileData();

            fileData.header = GetDefaultHeader(world);

            fileData.entrances = GetDefaultEntrance();

            fileData.fixedTiles = GetDefaultFixedTiles();

            return fileData;
        }

        private static HeaderData GetDefaultHeader(World world)
        {
            HeaderData header = new HeaderData();

            Location zeroLoc = new Location(0, 0, 0);

            header.areaType = world.GetAreaType(zeroLoc);
            header.fileType = "Stub";
            header.mapGridLocation = zeroLoc;
            header.seed = world.GetAreaSeed(zeroLoc);

            return header;
        }

        private static TileBlockData GetDefaultEntrance()
        {
            TileBlockData entrances = new TileBlockData();

            entrances.numberOfTiles = 1;

            TileData entrance = new TileData();
            entrance.ID = 0;
            entrance.location = new Location(50, 50, 0);
            entrance.type = TileType.Floor;
            entrance.representation = TileRepresentation.Floor;
            entrance.links = new String[] { "-1", "-1", "-1", "-1", "-1", "-1" };

            entrances.tileData = new TileData[] { entrance };

            return entrances;
        }

        private static TileBlockData[] GetDefaultFixedTiles()
        {
            TileBlockData fixedTiles = new TileBlockData();

            fixedTiles.numberOfTiles = 24;
            fixedTiles.tileData = new TileData[24];

            int ID = 1;
            for (int x = 46; x <= 50; x++)
            {
                for (int y = 48; y <= 52; y++)
                {
                    if (x == 50 && y == 50) continue;

                    TileData tile = new TileData();
                    tile.ID = ID++;
                    tile.location = new Location(x, y, 0);
                    tile.type = (x == 46 || x == 50 || y == 48 || y == 52) ? TileType.Wall :
                        ((x == 48 && y == 50) ? TileType.Campfire : TileType.Floor);
                    tile.representation = (tile.type == TileType.Wall) ? TileRepresentation.Wall :
                        ((tile.type == TileType.Campfire) ? TileRepresentation.Campfire : TileRepresentation.Floor);
                    tile.links = new String[] { "-1", "-1", "-1", "-1", "-1", "-1" };

                    fixedTiles.tileData[ID - 2] = tile;
                }
            }

            return new TileBlockData[] { fixedTiles };
        }
    }
}
