using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel
{
    class EnvironmentManager
    {
        // should be multiples of 100, determines world map "tiles" the area occupies
        private int width;
        private int height;

        // location on the map of the bottom left world map "tile"
        private Location mapGridLocation;

        // needed because both entrances and fixed tiles may have links to other areas
        private TileBlockData entrances;
        private TileBlockData[] fixedTiles;

        public EnvironmentManager(int width, int height, Location mapGridLocation, TileBlockData entrances, TileBlockData[] fixedTiles)
        {
            this.width = width;
            this.height = height;

            this.mapGridLocation = mapGridLocation;

            this.entrances = entrances;
            this.fixedTiles = fixedTiles;
        }

        public TileBlockData GenerateExits(int seed)
        {
            List<int> PossibleDirections = GetPossibleDirections(mapGridLocation, entrances, fixedTiles);

            Random rnd = new Random(seed);

            int exitNum = entrances.Length;

            int upX = 0;
            int upY = 0;
            foreach (int direction in PossibleDirections)
            {
                if (rnd.NextDouble() < GetExitChance())
                {
                    int locX = 0;
                    int locY = 0;

                    if (nextDoor[direction].x == mapGridPosition.x)
                    {
                        locX = rnd.Next(80) + 10;

                        if (nextDoor[direction].z == mapGridPosition.z)
                        {
                            locY = (nextDoor[direction].y == mapGridPosition.y - 1) ? 0 : 99;
                        }
                    }

                    if (nextDoor[direction].y == mapGridPosition.y)
                    {
                        locY = rnd.Next(80) + 10;
                        if (nextDoor[direction].z == mapGridPosition.z)
                        {
                            locX = (nextDoor[direction].x == mapGridPosition.x - 1) ? 0 : 99;
                        }
                    }

                    String type = (nextDoor[direction].z == mapGridPosition.z) ? "floor" : "stairs";

                    Tile exit = new Tile(type, type, new Location(locX + bottomLeft.x, locY + bottomLeft.y, bottomLeft.z), exitNum, area, world);

                    int otherEnd = AreaWriter.AddEntrance(exit, direction, neighbor[direction]);

                    if (otherEnd != -1 && !(locX == upX && locY == upY))
                    {
                        if (direction == Directions.UP)
                        {
                            upX = locX;
                            upY = locY;
                        }

                        exit.CreateAreaLink(direction, neighbor[direction], otherEnd);

                        toReturn.Add(exit);

                        exitNum++;
                    }
                }
            }

            return toReturn.ToArray();
        }

        private static List<int> GetPossibleDirections(Location mapGridLocation, TileBlockData entrances, TileBlockData fixedTiles)
        {
            List<int> toReturn = new List<int>();

            Location[] nextDoor = new Location[6];
            String[] neighbor = new String[6];

            for (int direction = 0; direction < 6; direction++)
            {
                nextDoor[direction] = Directions.GetNeighboring(direction, mapGridLocation);

                neighbor[direction] = "x" + nextDoor[direction].x + "y" + nextDoor[direction].y + "z" + nextDoor[direction].z;

                bool alreadyLinked = false;
                for (int n = 0; n < entrances.numberOfTiles; n++)
                {
                    alreadyLinked = alreadyLinked || CheckLinkedTo(entrances.tileData[n], neighbor[direction]);

                    for (int i = 0; i < fixedTiles[n].Length; i++)
                    {
                        alreadyLinked = alreadyLinked || CheckLinkedTo(fixedTiles[n][i], neighbor[direction]);
                    }
                }

                if (!alreadyLinked && (!AreaFile.Exists(neighbor[direction]) || AreaFile.IsStub(neighbor[direction])))
                {
                    toReturn.Add(direction);
                }
            }
        }

        private bool CheckLinkedTo(TileData tile, String neighbor)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                if (tile.GetLinkText(direction).Contains(neighbor)) return true;
            }

            return false;
        }
    }
}
