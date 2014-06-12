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
        private List<Tile> toConnect;

        private int[][] valuemap;
        private Tile[,] tiles;
        private int tileCount = 0;
        private PriorityQueue<int> tileFront;

        private int ID;

        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

        private int bottomLeftX;
        private int bottomLeftY;
        private int bottomLeftZ;

        private Area area;
        private World world;

        public Tile[] Generate(int seed, Tile[] entrances, int bottomLeftX, int bottomLeftY, int bottomLeftZ, Area area, World world)
        {
            this.bottomLeftX = bottomLeftX;
            this.bottomLeftY = bottomLeftY;
            this.bottomLeftZ = bottomLeftZ;

            this.area = area;
            this.world = world;

            //valuemap = PerlinNoise.Noise(seed, 100, 100, 4, 2.5d, 0.75d, true, true, true, true);
            valuemap = PerlinNoise.Noise(seed, 100, 100, 3, 4.0d, 5.50d, false, false, false, true);
            tiles = new Tile[100, 100];
            tileFront = new PriorityQueue<int>();

            this.entrances = new Tile[entrances.Length];
            toConnect = new List<Tile>();
   
            for (int n = 0; n < entrances.Length; n++)
            {
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

                this.entrances[n] = entrances[n];
                toConnect.Add(entrances[n]);

                entrances[n].SetColor(ID);
                valuemap[mapX][mapY] = 256 + ID;
                ID++;
                tileCount++;
                tiles[mapX, mapY] = entrances[n];
                
            }

            for (int n = 0; n < entrances.Length; n++)
            {
                int mapX = entrances[n].GetX() - bottomLeftX;
                int mapY = entrances[n].GetY() - bottomLeftY;

                AddNeighbors(mapX * 100 + mapY, valuemap[mapX][mapY]);
            }

            // decide on exits, also add them to toConnect

            while (toConnect.Count > 1 || tileCount < 1000)
            {
                Expand("floor", "floor", true);
            }

            while (tileFront.Count() > 0)
            {
                Expand("wall", "wall", false);
            }

            // PerlinBitmaps.SaveBitmapFromNoisemap(valuemap, 100, 100, @"d:\temp\cave(" + seed +").bmp");

            Tile[] linkedTiles = LinkTiles();

            return linkedTiles;
        }

        private void AddNeighbors(int index, int color)
        {
            List<int> neighbors = GetNeighbors(index);

            foreach (int neighbor in neighbors)
            {
                AddToFront(neighbor, color);
            }
        }

        private void AddToFront(int index, int color)
        {
            int key = valuemap[index / 100][index % 100];

            if (key > 255)
            {
                // check if it's different from this color. If so, merge colors and decrease ToConnect
                return;
            }
            valuemap[index / 100][index % 100] = color;

            tileFront.Add(key, index);
        }

        private void Expand(String type, String representation, bool expand)
        {
            int pointToAdd = tileFront.RemoveMin();

            int mapX = pointToAdd / 100;
            int mapY = pointToAdd % 100;
            int realX = mapX + bottomLeftX;
            int realY = mapY + bottomLeftY;
            int realZ = bottomLeftZ;

            tiles[mapX, mapY] = (new Tile(type, representation, realX, realY, realZ, ID++, area, world));
            tileCount++;

            if (mapX < minX) minX = mapX;
            if (mapX > maxX) maxX = mapX;
            if (mapY < minY) minY = mapY;
            if (mapY > maxY) maxY = mapY;

            if (expand) AddNeighbors(pointToAdd, valuemap[pointToAdd / 100][pointToAdd % 100]);
        }

        private Tile[] LinkTiles()
        {
            Tile[] tileArray = new Tile[tileCount];

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    Tile tile = tiles[x,y];

                    if (tile != null)
                    {
                        if (x > 0 && tiles[x - 1, y] != null) tile.Link(Directions.WEST, tiles[x - 1, y]);
                        if (x < 99 && tiles[x + 1, y] != null) tile.Link(Directions.EAST, tiles[x + 1, y]);
                        if (y > 0 && tiles[x, y - 1] != null) tile.Link(Directions.SOUTH, tiles[x, y - 1]);
                        if (y < 99 && tiles[x, y + 1] != null) tile.Link(Directions.NORTH, tiles[x, y + 1]);

                        tileArray[tile.GetID()] = tile;

                        //Output.Print("cave at: " + tile.GetX() + ", " + tile.GetY() + ", " + tile.GetZ());
                    }
                }
            }

            return tileArray;
        }

        private List<int> GetNeighbors(int center)
        {
            int x = center / 100;
            int y = center % 100;

            List<int> neighbors = new List<int>();

            if (x > 0) neighbors.Add((x - 1) * 100 + y);
            if (x < 99) neighbors.Add((x + 1) * 100 + y);
            if (y > 0) neighbors.Add(x * 100 + y - 1);
            if (y < 99) neighbors.Add(x * 100 + y + 1);

            return neighbors;
        }
    }
}
