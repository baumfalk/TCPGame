using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;
using TCPGameServer.World.Map.IO;

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

        // the world and area everything is based in
        private Area area;
        private World world;

        public EnvironmentManager(int width, int height, Location mapGridLocation, TileBlockData entrances, TileBlockData[] fixedTiles, Area area, World world)
        {
            this.width = width;
            this.height = height;

            this.mapGridLocation = mapGridLocation;

            this.entrances = entrances;
            this.fixedTiles = fixedTiles;

            this.area = area;
            this.world = world;
        }

        public void SaveStaticTiles()
        {
            AreaWriter.SaveStatic(area.GetName(), entrances, fixedTiles);
        }

        public TileBlockData GenerateExits(int seed, double exitChance)
        {
            List<TileData> exits = new List<TileData>();

            List<int> PossibleDirections = GetPossibleDirections(mapGridLocation, entrances, fixedTiles);

            Random rnd = new Random(seed);

            int exitNum = entrances.numberOfTiles;

            Location bottomLeft = MapGridHelper.MapGridLocationToBottomLeft(mapGridLocation);
            Location zeroLoc = new Location(0, 0, 0);

            int upX = 0;
            int upY = 0;
            foreach (int direction in PossibleDirections)
            {
                if (rnd.NextDouble() < exitChance)
                {
                    int locX = 0;
                    int locY = 0;

                    Location shift = Directions.GetNeighboring(direction, zeroLoc);

                    if (shift.x == 0)
                    {
                        locX = rnd.Next(80) + 10;

                        if (shift.z == 0)
                        {
                            locY = (shift.y == -1) ? 0 : 99;
                        }
                    }

                    if (shift.y == 0)
                    {
                        locY = rnd.Next(80) + 10;

                        if (shift.z == 0)
                        {
                            locX = (shift.x == -1) ? 0 : 99;
                        }
                    }

                    String type = (shift.z == 0) ? "floor" : "stairs";

                    TileData exit = new TileData();
                    exit.type = type;
                    exit.representation = type;
                    exit.location = new Location(locX + bottomLeft.x, locY + bottomLeft.y, bottomLeft.z);
                    exit.ID = exitNum;
                    
                    String neighbor = "x" + (shift.x + mapGridLocation.x) + "y" + (shift.y + mapGridLocation.y) + "z" + (shift.z + mapGridLocation.z);

                    int otherEnd = AreaWriter.AddEntrance(exit, direction, area.GetName(), neighbor, world);

                    if (otherEnd != -1 && !(locX == upX && locY == upY))
                    {
                        if (direction == Directions.UP)
                        {
                            upX = locX;
                            upY = locY;
                        }

                        for (int exitDirection = 0; exitDirection < 6; exitDirection++)
                        {
                            if (direction == exitDirection)
                            {
                                exit.links[exitDirection] = neighbor + ";" + otherEnd;
                            }
                            else
                            {
                                exit.links[exitDirection] = "-1";
                            }
                        }

                        exits.Add(exit);

                        exitNum++;
                    }
                }
            }

            TileBlockData toReturn = new TileBlockData();
            toReturn.numberOfTiles = exits.Count;
            toReturn.tileData = new TileData[exits.Count];

            for (int n = 0; n < exits.Count; n++)
            {
                toReturn.tileData[n] = exits[n];
            }

            return toReturn;
        }

        private List<int> GetPossibleDirections(Location mapGridLocation, TileBlockData entrances, TileBlockData[] fixedTiles)
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

                    for (int i = 0; i < fixedTiles[n].numberOfTiles; i++)
                    {
                        alreadyLinked = alreadyLinked || CheckLinkedTo(fixedTiles[n].tileData[i], neighbor[direction]);
                    }
                }

                if (!alreadyLinked && (!AreaFile.Exists(neighbor[direction]) || AreaFile.IsStub(neighbor[direction])))
                {
                    toReturn.Add(direction);
                }
            }

            return toReturn;
        }

        private bool CheckLinkedTo(TileData tile, String neighbor)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                if (tile.links[direction].Contains(neighbor)) return true;
            }

            return false;
        }
    }
}
