using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;
using TCPGameServer.General;

namespace TCPGameServer.World.Map.Generation
{
    class CaveGenerator
    {
        private Tile[] entrances;

        protected List<int> toConnect;
        private List<int>[] connected;
        private int[,] connectedBy;

        protected int seed;
        protected int[][] valuemap;

        private Tile[] tileArray;
        private Tile[,] tiles;
        private int tileCount = 0;
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

        public CaveGenerator(int seed, Tile[] entrances, bool generateExits, int bottomLeftX, int bottomLeftY, int bottomLeftZ, Area area, World world)
        {
            this.seed = seed;

            this.bottomLeftX = bottomLeftX;
            this.bottomLeftY = bottomLeftY;
            this.bottomLeftZ = bottomLeftZ;

            this.area = area;
            this.world = world;

            this.entrances = entrances;

            tiles = new Tile[100, 100];
            tileFront = new PriorityQueue<int[]>[entrances.Length];
            isInFront = new bool[entrances.Length][,];

            connected = new List<int>[entrances.Length];
            toConnect = new List<int>();
            connectedBy = new int[100, 100];
        }

        public AreaData Generate()
        {
            valuemap = GetValuemap();

            AddOuterWalls();

            AddEntrances();

            // decide on exits, also add them to toConnect

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

        protected void AddEntrances()
        {
            for (int n = 0; n < entrances.Length; n++)
            {
                connected[n] = new List<int>();

                int mapX = entrances[n].GetX() - bottomLeftX;
                int mapY = entrances[n].GetY() - bottomLeftY;
                int mapZ = entrances[n].GetZ() - bottomLeftZ;

                if (mapZ != 0)
                {
                    Output.Print("entrance Z = " + entrances[n].GetZ());
                    Output.Print("given Z = " + bottomLeftZ);
                    Output.Print("wrong Z-level on map generation");

                    this.bottomLeftZ = entrances[n].GetZ();
                }

                toConnect.Add(n);
                connected[n].Add(n);

                tileFront[n] = new PriorityQueue<int[]>();
                isInFront[n] = new bool[100,100];

                AddTile(mapX, mapY, n, entrances[n]);
            }

            for (int n = 0; n < entrances.Length; n++)
            {
                int mapX = entrances[n].GetX() - bottomLeftX;
                int mapY = entrances[n].GetY() - bottomLeftY;

                AddNeighbors(mapX, mapY, entrances[n].GetID());
            }
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
            tileArray = new Tile[tileCount];
            int[][] linkData = new int[tileCount][];

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile tile = tiles[x,y];

                    if (tile != null)
                    {
                        linkData[tile.GetID()] = new int[6];

                        for (int direction = 0; direction < 6; direction++)
                        {
                            linkData[tile.GetID()][direction] = GetLink(direction, tile, x, y, tile.GetZ());
                        }

                        tileArray[tile.GetID()] = tile;
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
