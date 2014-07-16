using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO;
using TCPGameServer.World.Map.IO.MapFile;
using TCPGameServer.World.Map.Generation;

using TCPGameServer.Control.Output;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map
{
    public class Area
    {
        // the world this area is part of
        private World world;

        // the name of the area
        private String name;

        // the type of area this is (small cave / tunnel / etc)
        private String areaType;

        // list of tiles
        private Tile[] tiles;

        private Tile defaultTile; 

        // a counter that checks how many ticks an area has been inactive
        private DateTime LastActivity;

        // on creation, an area should load itself from file
        public Area(World world, String name)
        {
            this.world = world;
            this.name = name;

            defaultTile = Tile.CreateTileOfType(TileType.Floor, TileRepresentation.Floor, new Location(int.MaxValue, int.MaxValue, int.MaxValue), int.MaxValue, this, world);
            defaultTile.CreateAreaLink(Directions.DOWN, "x0y0z0", 0);

            if (AreaFile.Exists(name))
            {
                // load the tiles and area type from the area reader
                AreaData areaData = AreaReader.Load(name, this, world);

                tiles = areaData.tiles;
                areaType = areaData.areaType;
            }
            else
            {
                
                Log.Print("loading area (" + name + ") that does not exist");

                tiles = new Tile[0];
                areaType = "nonexistent";
            }
            // the area is obviously active upon creation
            SetActive();
        }

        // get the area's type
        public String GetAreaType()
        {
            return areaType;
        }

        // get the area's name
        public String GetName()
        {
            return name;
        }

        // get a tile from the area by ID
        public Tile GetTile(int ID)
        {
            if (ID >= tiles.Length) return defaultTile;

            return tiles[ID];
        }

        // when something happens in the area, set the time it was last active to this
        // moment. At the moment, area creation and creatures moving around in an area 
        // set its activity flag
        public void SetActive()
        {
            LastActivity = DateTime.Now;
        }

        // check how long an area has been inactive
        public TimeSpan GetTimeInactive()
        {
            return DateTime.Now - LastActivity;
        }

        // on unload, we have to unlink area links
        public void Unload()
        {
            Log.Print("Unloading area " + name);

            // check each tile for area links
            foreach (Tile tile in tiles)
            {
                // in each direction
                for (int direction = 0; direction < 6; direction++)
                {
                    // if the link text contains a semicolon, it's an area link
                    String linkText = tile.GetLinkText(direction);

                    if (linkText.Contains(';'))
                    {
                        String areaName = linkText.Split(';')[0];

                        if (!world.IsLoaded(areaName))
                        {
                            // remove the link from the other side
                            tile.GetNeighbor(direction).UnlinkAreaOnUnload(Directions.Inverse(direction));
                        }
                    }
                }
            }
        }
    }
}
