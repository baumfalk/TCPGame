using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Tiles
{
    public class Tilemap
    {
        private Area area;
        private World world;

        private int tileCount;
        private List<TileAndLocation> tileList;
        private Tile[,] tiles;

        private Location bottomLeft;

        private TileBlockData[] fixedTiles;

        public Tilemap(int width, int height, Location bottomLeft, Area area, World world) {
            this.area = area;
            this.world = world;

            this.bottomLeft = bottomLeft;

            tileCount = 0;
            tileList = new List<TileAndLocation>();
            tiles = new Tile[width, height];
        }

        public int GetCount()
        {
            return tileCount;
        }

        public void AddEntrances(TileBlockData entrances)
        {
            AddTileBlock(entrances);
        }

        public void AddFixedTiles(TileBlockData[] fixedTiles)
        {
            this.fixedTiles = fixedTiles;
        }

        private void FinishFixedTiles()
        {
            for (int n = 0; n < fixedTiles.Length; n++)
            {
                AddTileBlock(fixedTiles[n]);
            }
        }

        public void AddExits(TileBlockData exits)
        {
            AddTileBlock(exits);

            FinishFixedTiles();
        }

        private void AddTileBlock(TileBlockData tilesToAdd)
        {
            int numberOfTiles = tilesToAdd.numberOfTiles;

            for (int n = 0; n < numberOfTiles; n++) 
            {
                Location mapLocation = MapGridHelper.TileLocationToCurrentMapLocation(tilesToAdd.tileData[n].location);
                String type = tilesToAdd.tileData[n].type;
                String representation = tilesToAdd.tileData[n].representation;

                Tile tile = AddTile(mapLocation, tilesToAdd.tileData[n].location, type, representation);

                TileLinker.SetAreaLinks(tile, tilesToAdd.tileData[n].links);
            }
        }

        public Tile AddTile(Location mapLocation, String type, String representation)
        {
            Location tileLocation = MapGridHelper.CurrentMapLocationToTileLocation(mapLocation, bottomLeft);

            return AddTile(mapLocation, tileLocation, type, representation);
        }

        public Tile AddTile(Location mapLocation, Location tileLocation, String type, String representation)
        {
            if (tiles[mapLocation.x, mapLocation.y] != null) return null;

            Tile tile = new Tile(type, representation, tileLocation, tileCount, area, world);

            TileAndLocation newTile = new TileAndLocation();
            newTile.tile = tile;
            newTile.location = mapLocation;

            tileList.Add(newTile);

            tiles[mapLocation.x, mapLocation.y] = tile;

            tileCount++;

            return tile;
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

        public int[][] GetLinks()
        {
            return TileLinker.GetLinks(tileCount, tiles, tileList);
        }
    }
}
