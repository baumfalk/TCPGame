using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO;
using TCPGameServer.World.Map.Generation;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map
{
    public class Area
    {
        // the world this area is part of
        private World world;

        // the name of the area
        private String name;

        // list of tiles
        private Tile[] tiles;

        // a counter that checks how many ticks an area has been inactive
        private DateTime LastActivity;

        // on creation, an area should load itself from file
        public Area(World world, String name)
        {
            this.world = world;
            this.name = name;

            if (AreaReader.Exists(name))
            {
                // load the tiles from the area reader
                tiles = AreaReader.Load(name, this, world);
            }
            else
            {
                tiles = AreaGenerator.Generate(world.GetSeed(), name,0,0);
            }
            // the area is obviously active upon creation
            SetActive();
        }

        // get the area's name
        public String GetName()
        {
            return name;
        }

        // get a tile from the area by ID
        public Tile GetTile(int ID)
        {
            return tiles[ID];
        }

        // when something happens in the area, set the time it was last active to this
        // moment. At the moment, only creatures entering an area set its activity flag
        public void SetActive()
        {
            LastActivity = DateTime.Now;
        }

        // check how long an area has been inactive
        public TimeSpan GetLastActivity()
        {
            return DateTime.Now - LastActivity;
        }

        // on unload, we have to unlink area links
        public void Unload()
        {
            Output.Print("Unloading area " + name);

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
