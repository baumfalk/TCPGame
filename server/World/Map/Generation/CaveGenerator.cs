using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;
using TCPGameServer.General;

using TCPGameServer.World.Map.IO;

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
        private PriorityQueue<int[]>[] tileFront;
        private bool[][,] isInFront;

        private int minX = 100;
        private int maxX = 0;
        private int minY = 100;
        private int maxY = 0;

        private int bottomLeftX;
        private int bottomLeftY;
        private int bottomLeftZ;

        private Area area;
        private World world;

        public CaveGenerator(int seed, Tile[] entrances, Tile[][] fixedTiles, bool generateExits, int bottomLeftX, int bottomLeftY, int bottomLeftZ, Area area, World world)
        {
            this.seed = seed;

            this.entrances = entrances;
            this.fixedTiles = fixedTiles;

            this.generateExits = generateExits;

            this.bottomLeftX = bottomLeftX * 100;
            this.bottomLeftY = bottomLeftY * 100;
            this.bottomLeftZ = bottomLeftZ;

            this.area = area;
            this.world = world;

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

            toReturn.seed = seed;
            toReturn.areaType = GetAreaType();
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
            AreaWriter.SaveStatic(area.GetName(), seed, GetAreaType(), new int[] { bottomLeftX / 100, bottomLeftY / 100, bottomLeftZ }, entrances, exits, fixedTiles);
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
            tileFront = new PriorityQueue<int[]>[entrances.Length + exits.Length];
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
                int mapX = entrances[n].GetX() - bottomLeftX;
                int mapY = entrances[n].GetY() - bottomLeftY;

                AddNeighbors(mapX, mapY, entrances[n].GetID());
            }

            int offset = entrances.Length;

            for (int n = 0; n < exits.Length; n++)
            {
                int mapX = exits[n].GetX() - bottomLeftX;
                int mapY = exits[n].GetY() - bottomLeftY;

                AddNeighbors(mapX, mapY, exits[n].GetID());
            }
        }

        private Tile[] AddExits()
        {
            List<int> PossibleDirections = new List<int>();
            List<Tile> toReturn = new List<Tile>();

            int mapGridCoordinateX = bottomLeftX / 100;
            int mapGridCoordinateY = bottomLeftY / 100;
            int mapGridCoordinateZ = bottomLeftZ;

            int[][] nextDoor = new int[6][];
            String[] neighbor = new String[6];

            for (int direction = 0; direction < 6; direction++)
            {
                nextDoor[direction] =  Directions.GetNeighboring(direction, mapGridCoordinateX, mapGridCoordinateY, mapGridCoordinateZ);

                neighbor[direction] = "x" + nextDoor[direction][0] + "y" + nextDoor[direction][1] + "z" + nextDoor[direction][2];

                bool alreadyLinked = false;
                for (int n = 0; n < entrances.Length; n++)
                {
                    alreadyLinked = alreadyLinked || CheckLinkedTo(entrances[n], neighbor[direction]);

                    for (int i = 0; i < fixedTiles[n].Length; i++)
                    {
                        alreadyLinked = alreadyLinked || CheckLinkedTo(fixedTiles[n][i], neighbor[direction]);
                    }
                }

                if (!alreadyLinked && (!AreaReader.Exists(neighbor[direction]) || AreaReader.IsStub(neighbor[direction])))
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

                    if (nextDoor[direction][0] == mapGridCoordinateX)
                    {
                        locX = rnd.Next(80) + 10;

                        if (nextDoor[direction][2] == mapGridCoordinateZ)
                        {
                            locY = (nextDoor[direction][1] == mapGridCoordinateY - 1) ? 0 : 99;
                        }
                    }

                    if (nextDoor[direction][1] == mapGridCoordinateY)
                    {
                        locY = rnd.Next(80) + 10;
                        if (nextDoor[direction][2] == mapGridCoordinateZ)
                        {
                            locX = (nextDoor[direction][0] == mapGridCoordinateX - 1) ? 0 : 99;
                        }
                    }

                    String type = (nextDoor[direction][2] == mapGridCoordinateZ) ? "floor" : "stairs";

                    Tile exit = new Tile(type, type, locX + bottomLeftX, locY + bottomLeftY, bottomLeftZ, exitNum, area, world);

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

            int mapX = tileToConnect.GetX() - bottomLeftX;
            int mapY = tileToConnect.GetY() - bottomLeftY;
            int mapZ = tileToConnect.GetZ() - bottomLeftZ;

            toConnect.Add(entranceIndex);
            connected[entranceIndex].Add(entranceIndex);

            tileFront[entranceIndex] = new PriorityQueue<int[]>();
            isInFront[entranceIndex] = new bool[100, 100];

            AddTile(mapX, mapY, entranceIndex, tileToConnect);
        }

        private void AddFixedTiles()
        {
            int offset = entrances.Length + exits.Length;

            for (int n = 0; n < fixedTiles.Length; n++)
            {
                for (int i = 0; i < fixedTiles[n].Length; i++)
                {
                    int mapX = fixedTiles[n][i].GetX() - bottomLeftX;
                    int mapY = fixedTiles[n][i].GetY() - bottomLeftY;
                    int mapZ = fixedTiles[n][i].GetZ() - bottomLeftZ;

                    AddTile(mapX, mapY, n, fixedTiles[n][i]);
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
            return 0.5d;
        }

        private void AddTile(int x, int y, int color, Tile toAdd)
        {
            int connector = connectedBy[x, y] - 1;

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
                connectedBy[x, y] = color + 1;

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;

                tiles[x, y] = toAdd;

                if (tiles[x, y].GetID() != tileCount) tiles[x, y].SetID(tileCount);

                tileCount++;
            }
        }

        protected void AddToFront(int x, int y, int color)
        {
            int connector = connectedBy[x, y] - 1;

            if (connector != color && !isInFront[color][x,y])
            {
                isInFront[color][x, y] = true;

                int weight = GetWeight(x, y, color);

                tileFront[color].Add(weight, new int[] { x, y });
            }
        }

        protected virtual int GetWeight(int x, int y, int color)
        {
            return valuemap[x][y];
        }

        private void RecalculateWeights(int queueIndex)
        {
            PriorityQueue<int[]> bufferQueue = new PriorityQueue<int[]>();

            isInFront[queueIndex] = new bool[100,100];

            while (tileFront[queueIndex].Count() > 0)
            {
                int[] location = tileFront[queueIndex].RemoveMin();

                int x = location[0];
                int y = location[1];

                if (!isInFront[queueIndex][x,y])
                {
                    bufferQueue.Add(GetWeight(x, y, queueIndex), location);
                    isInFront[queueIndex][x, y] = true;
                }
            }

            tileFront[queueIndex] = bufferQueue;
        }

        protected void Expand(int color, String type, String representation, bool expand)
        {
            int[] pointToAdd = tileFront[color].RemoveMin();

            int mapX = pointToAdd[0];
            int mapY = pointToAdd[1];

            int realX = mapX + bottomLeftX;
            int realY = mapY + bottomLeftY;
            int realZ = bottomLeftZ;

            AddTile(mapX, mapY, color, new Tile(type, representation, realX, realY, realZ, tileCount, area, world));

            if (expand) AddNeighbors(mapX, mapY, color);
        }

        private void AddNeighbors(int x, int y, int color)
        {
            List<int[]> neighbors = GetNeighbors(x, y);

            foreach (int[] neighbor in neighbors)
            {
                int xNeighbor = neighbor[0];
                int yNeighbor = neighbor[1];

                AddToFront(xNeighbor, yNeighbor, color);
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
                            linkData[tile.GetID()][direction] = GetLink(direction, tile, x, y, tile.GetZ());
                        }
                    }
                }
            }

            return linkData;
        }

        private int GetLink(int direction, Tile tile, int x, int y, int z)
        {
            if (tile.HasNeighbor(direction))
            {
                return -1;
            }
            else
            {
                int[] neighborPosition = Directions.GetNeighboring(direction, x, y, z);
                int xNeighbor = neighborPosition[0];
                int yNeighbor = neighborPosition[1];
                int zNeighbor = neighborPosition[2];

                if (xNeighbor >= 0 && yNeighbor >= 0 && zNeighbor == z &&
                    xNeighbor <= 99 && yNeighbor <= 99 &&
                    tiles[xNeighbor, yNeighbor] != null)
                {
                    return tiles[xNeighbor, yNeighbor].GetID();
                }
                else
                {
                    return -1;
                }
            }
        }

        private List<int[]> GetNeighbors(int x, int y)
        {
            List<int[]> neighbors = new List<int[]>();

            if (x > 0) neighbors.Add(new int[] {(x - 1) , y});
            if (x < 99) neighbors.Add(new int[] { (x + 1), y });
            if (y > 0) neighbors.Add(new int[] { x, y - 1 });
            if (y < 99) neighbors.Add(new int[] { x, y + 1 });

            return neighbors;
        }

        protected virtual String GetAreaType()
        {
            return "Cave";
        }
    }
}
