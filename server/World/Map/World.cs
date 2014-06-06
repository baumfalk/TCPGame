using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map
{
    public class World
    {
        // seed for the random number generator
        private int worldSeed = 0;

        // the areas loaded at the moment
        private Dictionary<String, Area> loadedAreas;

        // world is the overarching maptype. Areas are parts of the world,
        // tiles are parts of areas. World maintains a dictionary of areas
        // which can be accessed by the model.
        public World()
        {
            loadedAreas = new Dictionary<String, Area>();
        }

        // get an area from the dictionary if it's in there. If not, load
        // it and add it.
        private Area getArea(String areaName)
        {
            Area area;

            if (!loadedAreas.TryGetValue(areaName, out area))
            {
                area = new Area(this, areaName);

                loadedAreas.Add(areaName, area);
            }

            return area;
        }

        // get a tile in the world
        public Tile GetTile(String areaName, int TileID)
        {
            Area area = getArea(areaName);

            return area.GetTile(TileID);
        }

        // get the random number generator seed
        public int GetSeed()
        {
            return worldSeed;
        }
    }
}
