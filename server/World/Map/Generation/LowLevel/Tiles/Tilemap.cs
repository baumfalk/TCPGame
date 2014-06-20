using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Tiles
{
    class Tilemap
    {
        private Area area;
        private World world;

        private int tileCount;
        private List<TileAndLocation> tileList;
        private Tile[,] tiles;

        private Location bottomLeft;

        private class TileAndLocation
        {
            public Tile tile;
            public Location location;
        }

        public Tilemap(int width, int height, Location bottomLeft, Area area, World world) {
            this.area = area;
            this.world = world;

            tileCount = 0;
            tileList = new List<TileAndLocation>();
            tiles = new Tile[width, height];
        }

        public int GetCount()
        {
            return tileCount;
        }

        public void AddTileBlock(TileBlockData tilesToAdd)
        {
            foreach (TileData tileData in tilesToAdd.tileData)
            {
                Location mapLocation = MapGridHelper.TileLocationToCurrentMapLocation(tileData.location);
                String type = tileData.type;
                String representation = tileData.representation;

                Tile tile = AddTile(mapLocation, tileData.location, type, representation);

                TileParser.SetAreaLinks(tile, tileData.links);
            }
        }

        public Tile AddTile(Location mapLocation, String type, String representation)
        {
            Location tileLocation = MapGridHelper.CurrentMapLocationToTileLocation(mapLocation, bottomLeft);

            return AddTile(mapLocation, tileLocation, type, representation);
        }

        public Tile AddTile(Location mapLocation, Location tileLocation, String type, String representation)
        {
            Tile tile = new Tile(type, representation, tileLocation, tileCount, area, world);

            TileAndLocation newTile = new TileAndLocation();
            newTile.tile = tile;
            newTile.location = mapLocation;

            tileList.Add(newTile);

            tiles[mapLocation.x, mapLocation.y] = tile;

            tileCount++;

            return tile;
        }

        public int[][] GetLinks()
        {
            int[][] linkData = new int[tileCount][];

            for (int ID = 0; ID < tileList.Count; ID++)
            {
                linkData[ID] = new int[6];

                for (int direction = 0; direction < 6; direction++)
                {
                    linkData[ID][direction] = GetLink(direction, tileList[ID].tile, tileList[ID].location);
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

        public Tile[] GetTiles()
        {
            Tile[] toReturn = new Tile[tileList.Count];

            for (int n = 0; n < tileList.Count; n++)
            {
                toReturn[n] = tileList[n].tile;
            }

            return toReturn;
        }
    }
}
