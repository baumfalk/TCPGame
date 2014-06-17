using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

using TCPGameServer.General;

using TCPGameServer.World.Map.IO;
using TCPGameServer.World.Map.IO.MapFile;
using TCPGameServer.World.Map.Generation.Perlin;

namespace TCPGameServer.World.Map.Generation
{
    class CaveGenerator
    {
        private Tile[] entrances;
        private Tile[] exits;
        private Tile[][] fixedTiles;
        private bool generateExits;

        protected List<int> toConnect;
        private List<int>[] connected;
        protected int[,] connectedBy;

        protected int seed;
        protected int[][] valuemap;

        private int tileCount;
        private Tile[] tileArray;
        private Tile[,] tiles;
        private PriorityQueue<Location>[] tileFront;
        private bool[][,] isInFront;

        private int minX = 100;
        private int maxX = 0;
        private int minY = 100;
        private int maxY = 0;

        private Location bottomLeft;

        private Area area;
        private World world;

        public CaveGenerator(GeneratorData generatorData)
        {
            seed = generatorData.seed;

            entrances = generatorData.entrances;
            fixedTiles = generatorData.fixedTiles;

            generateExits = generatorData.generateExits;

            bottomLeft = generatorData.bottomLeft;

            area = generatorData.area;
            world = generatorData.world;

            tileCount = 0;
            tiles = new Tile[100, 100];

            toConnect = new List<int>();
            connectedBy = new int[100, 100];
        }

        public AreaData Generate()
        {
            InitializeGeneration();

            while (GetContinueCondition())
            {
                for (int n = 0; n < toConnect.Count; n++)
                {
                    Expand(toConnect[n], "floor", "floor", true);
                }
            }

            int remainingColor = toConnect[0];
            while (tileFront[remainingColor].Count() > 0)
            {
                Expand(remainingColor, "wall", "wall", false);
            }

            int[][] linkData = LinkTiles();

            AreaData toReturn = new AreaData();

            toReturn.tiles = tileArray;
            toReturn.linkData = linkData;

            return toReturn;
        }

        private void InitializeGeneration()
        {
            valuemap = GetValuemap();

            AddOuterWalls();

            // decide on exits, also add them to toConnect
            if (generateExits) exits = AddExits();
            else exits = new Tile[0];

            AddTilesToConnect();

            SetInitialFront();

            AddFixedTiles();

            if (generateExits) SaveStaticTiles();
        }

        private void SaveStaticTiles()
        {
            AreaWriter.SaveStatic(area.GetName(), entrances, exits, fixedTiles);
        }

        protected virtual int[][] GetValuemap()
        {
            return PerlinNoise.Noise(seed, 100, 100, 3, 2.9299d, 1.50d, false, false, true, false);
        }

        protected virtual bool GetContinueCondition()
        {
            return (toConnect.Count > 1 || tileCount < 300);
        }

        private void AddOuterWalls()
        {
            for (int x = 0; x < 100; x++)
            {
                valuemap[x][0] = 255;
                valuemap[x][99] = 255;
            }

            for (int y = 0; y < 100; y++)
            {
                valuemap[0][y] = 255;
                valuemap[99][y] = 255;
            }
        }

        protected void AddTilesToConnect()
        {
            connected = new List<int>[entrances.Length + exits.Length];
            tileFront = new PriorityQueue<Location>[entrances.Length + exits.Length];
            isInFront = new bool[entrances.Length + exits.Length][,];

            for (int n = 0; n < entrances.Length; n++)
            {
                AddTileToConnect(entrances[n], n);
            }

            int offset = entrances.Length;

            for (int n = 0; n < exits.Length; n++)
            {
                AddTileToConnect(exits[n], n + offset);
            }
        }

        private void SetInitialFront()
        {
            for (int n = 0; n < entrances.Length; n++)
            {
                Location entranceLocation = MapGridHelper.TileLocationToCurrentMapLocation(entrances[n].GetLocation());

                AddNeighbors(entranceLocation, entrances[n].GetID());
            }

            int offset = entrances.Length;

            for (int n = 0; n < exits.Length; n++)
            {
                Location exitLocation = MapGridHelper.TileLocationToCurrentMapLocation(exits[n].GetLocation());

                AddNeighbors(exitLocation, exits[n].GetID());
            }
        }

        private Tile[] AddExits()
        {
            List<int> PossibleDirections = new List<int>();
            List<Tile> toReturn = new List<Tile>();

            Location mapGridPosition = MapGridHelper.TileLocationToMapGridLocation(bottomLeft);

            Location[] nextDoor = new Location[6];
            String[] neighbor = new String[6];

            for (int direction = 0; direction < 6; direction++)
            {
                nextDoor[direction] =  Directions.GetNeighboring(direction, mapGridPosition);

                neighbor[direction] = "x" + nextDoor[direction].x + "y" + nextDoor[direction].y + "z" + nextDoor[direction].z;

                bool alreadyLinked = false;
                for (int n = 0; n < entrances.Length; n++)
                {
                    alreadyLinked = alreadyLinked || CheckLinkedTo(entrances[n], neighbor[direction]);

                    for (int i = 0; i < fixedTiles[n].Length; i++)
                    {
                        alreadyLinked = alreadyLinked || CheckLinkedTo(fixedTiles[n][i], neighbor[direction]);
                    }
                }

                if (!alreadyLinked && (!AreaFile.Exists(neighbor[direction]) || AreaFile.IsStub(neighbor[direction])))
                {
                    PossibleDirections.Add(direction);
                }
            }

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

        private void AddTileToConnect(Tile tileToConnect, int entranceIndex)
        {
            connected[entranceIndex] = new List<int>();

            Location mapPosition = MapGridHelper.TileLocationToCurrentMapLocation(tileToConnect.GetLocation());

            toConnect.Add(entranceIndex);
            connected[entranceIndex].Add(entranceIndex);

            tileFront[entranceIndex] = new PriorityQueue<Location>();
            isInFront[entranceIndex] = new bool[100, 100];

            AddTile(mapPosition, entranceIndex, tileToConnect);
        }

        private void AddFixedTiles()
        {
            for (int n = 0; n < fixedTiles.Length; n++)
            {
                for (int i = 0; i < fixedTiles[n].Length; i++)
                {
                    Location mapPosition = MapGridHelper.TileLocationToCurrentMapLocation(fixedTiles[n][i].GetLocation());

                    AddTile(mapPosition, n, fixedTiles[n][i]);
                }
            }
        }

        private bool CheckLinkedTo(Tile tile, String neighbor)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                if (tile.GetLinkText(direction).Contains(neighbor)) return true;
            }

            return false;
        }

        protected virtual double GetExitChance()
        {
            // betere values voor bedenken!
            return 0.67d;
        }

        private void AddTile(Location location, int color, Tile toAdd)
        {
            int connector = connectedBy[location.x, location.y] - 1;

            if (connector != -1)
            {
                if (!connected[color].Contains(connector))
                {
                    tileFront[color].Merge(tileFront[connector]);

                    toConnect.Remove(connector);
                    connected[color].Add(connector);

                    if (toConnect.Count > 1) RecalculateWeights(color);
                }
            }
            else
            {
                connectedBy[location.x, location.y] = color + 1;

                if (location.x < minX) minX = location.x;
                if (location.x > maxX) maxX = location.x;
                if (location.y < minY) minY = location.y;
                if (location.y > maxY) maxY = location.y;

                tiles[location.x, location.y] = toAdd;

                if (tiles[location.x, location.y].GetID() != tileCount) tiles[location.x, location.y].SetID(tileCount);

                tileCount++;
            }
        }

        protected void AddToFront(Location location, int color)
        {
            int x = location.x;
            int y = location.y;
            int z = location.z;

            int connector = connectedBy[x, y] - 1;

            if (connector != color && !isInFront[color][x,y])
            {
                isInFront[color][x, y] = true;

                int weight = GetWeight(location, color);

                tileFront[color].Add(weight, location);
            }
        }

        protected virtual int GetWeight(Location location, int color)
        {
            int x = location.x;
            int y = location.y;

            return valuemap[x][y];
        }

        private void RecalculateWeights(int queueIndex)
        {
            PriorityQueue<Location> bufferQueue = new PriorityQueue<Location>();

            isInFront[queueIndex] = new bool[100,100];

            while (tileFront[queueIndex].Count() > 0)
            {
                Location location = tileFront[queueIndex].RemoveMin();

                int x = location.x;
                int y = location.y;

                if (!isInFront[queueIndex][x,y])
                {
                    bufferQueue.Add(GetWeight(location, queueIndex), location);
                    isInFront[queueIndex][x, y] = true;
                }
            }

            tileFront[queueIndex] = bufferQueue;
        }

        protected void Expand(int color, String type, String representation, bool expand)
        {
            Location pointToAdd = tileFront[color].RemoveMin();

            Location realLocation = MapGridHelper.CurrentMapLocationToTileLocation(pointToAdd, bottomLeft);

            AddTile(pointToAdd, color, new Tile(type, representation, realLocation, tileCount, area, world));

            if (expand) AddNeighbors(pointToAdd, color);
        }

        private void AddNeighbors(Location location, int color)
        {
            List<Location> neighbors = GetNeighbors(location);

            foreach (Location neighbor in neighbors)
            {
                AddToFront(neighbor, color);
            }
        }

        private int[][] LinkTiles()
        {
            int[][] linkData = new int[tileCount][];
            tileArray = new Tile[tileCount];

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile tile = tiles[x,y];

                    if (tile != null)
                    {
                        linkData[tile.GetID()] = new int[6];
                        tileArray[tile.GetID()] = tile;

                        for (int direction = 0; direction < 6; direction++)
                        {
                            linkData[tile.GetID()][direction] = GetLink(direction, tile, new Location(x, y, tile.GetLocation().z));
                        }
                    }
                }
            }

            return linkData;
        }

        private int GetLink(int direction, Tile tile, Location location)
        {
            if (tile.HasNeighbor(direction))
            {
                return -1;
            }
            else
            {
                Location neighbor = Directions.GetNeighboring(direction, location);

                if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.z == location.z &&
                    neighbor.x <= 99 && neighbor.y <= 99 &&
                    tiles[neighbor.x, neighbor.y] != null)
                {
                    return tiles[neighbor.x, neighbor.y].GetID();
                }
                else
                {
                    return -1;
                }
            }
        }

        private List<Location> GetNeighbors(Location mapLocation)
        {
            List<Location> neighbors = new List<Location>();

            if (mapLocation.x > 0) neighbors.Add(new Location(mapLocation.x - 1, mapLocation.y, mapLocation.z));
            if (mapLocation.x < 99) neighbors.Add(new Location( mapLocation.x + 1, mapLocation.y, mapLocation.z));
            if (mapLocation.y > 0) neighbors.Add(new Location(mapLocation.x, mapLocation.y - 1, mapLocation.z));
            if (mapLocation.y < 99) neighbors.Add(new Location(mapLocation.x, mapLocation.y + 1, mapLocation.z));

            return neighbors;
        }

        protected virtual String GetAreaType()
        {
            return "Cave";
        }
    }
}
