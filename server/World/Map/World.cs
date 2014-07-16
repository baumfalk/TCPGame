using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;
using TCPGameSharedInfo;
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

        // tells the map generator which type of map to make at which point of the world
        public String GetAreaType(Location worldGrid)
        {
            CrossPlatformRandom rnd = new CrossPlatformRandom(GetAreaSeed(worldGrid));

            double chance = rnd.NextDouble();

            if (chance < 0.8) return "Small Cave";
            else return "Tunnel Cave";
        }

        // for now, we will unload areas that have seen no activity for thirty
        // minutes or more when this method is called (every 10 minutes).
        public void UnloadInactiveAreas()
        {
            // a buffer, we don't want to alter the dictionary while we're
            // iterating through it's entries
            List<KeyValuePair<String, Area>> unloadList = new List<KeyValuePair<String,Area>>();

            // loop through all the loaded areas
            foreach (KeyValuePair<String, Area> entry in loadedAreas) {
                // take the area
                Area area = entry.Value;

                // check the period the area has been inactive
                TimeSpan inactivity = area.GetTimeInactive();

                // if it's more than 30 minutes, add it to the unload buffer
                if (inactivity.TotalMinutes > 30) unloadList.Add(entry);
            }

            // unload and remove each area that's in the buffer
            foreach (KeyValuePair<String, Area> entry in unloadList)
            {
                entry.Value.Unload();
                loadedAreas.Remove(entry.Key);
            }
        }

        // checks if an area is loaded
        public bool IsLoaded(String areaName)
        {
            return loadedAreas.ContainsKey(areaName);
        }

        // get a tile in the world
        public Tile GetTile(String areaName, int TileID)
        {
            Area area = getArea(areaName);

            return area.GetTile(TileID);
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

        // get the random number generator seed
        public int GetSeed()
        {
            return worldSeed;
        }

        // returns an area seed for a certain location on the map
        public int GetAreaSeed(Location worldGrid)
        {
            // multiplicands are all prime
            return worldSeed + worldGrid.x * 17 + worldGrid.y * 65537 + worldGrid.z * 478697;
        }
    }
}
