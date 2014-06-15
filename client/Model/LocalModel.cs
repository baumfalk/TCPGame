using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TCPGameClient.Model
{
    public class LocalModel
    {
        // current location of the player
        private int currentX;
        private int currentY;
        private int currentZ;

        // size of the display grid
        private int gridSizeX;
        private int gridSizeY;
        private int gridSizeZ;

        // display grid
        private Field[, ,] map;
        private List<Creature> creatures;

        // constructor allows the controller to give a grid size, which is
        // the area that will be "remembered"
        public LocalModel(int gridSizeX, int gridSizeY, int gridSizeZ)
        {
            this.gridSizeX = gridSizeX;
            this.gridSizeY = gridSizeY;
            this.gridSizeZ = gridSizeZ;

            map = new Field[gridSizeX, gridSizeY, gridSizeZ];
            creatures = new List<Creature>();
        }

        // clear the creature list
        public void ResetCreatures()
        {
            creatures = new List<Creature>();
        }

        // add a new creature to the map
        public void AddCreature(int xPos, int yPos, int zPos, String representation)
        {
            // calculate position relative to the player
            int relPosX = xPos - currentX;
            int relPosY = yPos - currentY;
            int relPosZ = zPos - currentZ;

            // add creature to creature list
            creatures.Add(new Creature(relPosX, relPosY, relPosZ, representation));
        }

        // returns the list of nearby creatures
        public List<Creature> GetCreatures()
        {
            return creatures;
        }

        public void AddTile(int xPos, int yPos, int zPos, String representation)
        {
            // calculate position relative to the center of the map
            int mapPosX = gridSizeX / 2 + (xPos - currentX);
            int mapPosY = gridSizeY / 2 + (yPos - currentY);
            int mapPosZ = gridSizeZ / 2 + (zPos - currentZ);

            // add tile to the grid
            if (IsInBounds(mapPosX, mapPosY, mapPosZ)) map[mapPosX, mapPosY, mapPosZ] = new Field(representation);
        }

        // shift the map. This is needed when the player moves, since the map is centered
        // on the player.
        public void ShiftMap(int newX, int newY, int newZ)
        {
            // the shift we need to make is the difference of the old (current) position 
            // with the new one
            int shiftX = currentX - newX;
            int shiftY = currentY - newY;
            int shiftZ = currentZ - newZ;

            // a shift of 0 just returns the same grid, so don't do all the work
            if (shiftX == 0 && shiftY == 0 && shiftZ == 0) return;

            // create a new grid
            Field[, ,] newMap = new Field[gridSizeX, gridSizeY, gridSizeZ];

            // loop over the old grid
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    for (int z = 0; z < gridSizeZ; z++)
                    {
                        // check if a field already exists on the old map
                        if (IsInBounds(x - shiftX, y - shiftY, z - shiftZ))
                        {
                            newMap[x, y, z] = map[x - shiftX, y - shiftY, z - shiftZ];
                        }
                        else
                        {
                            newMap[x, y, z] = null;
                        }
                    }
                }
            }

            // update the grid and the player position
            map = newMap;
            currentX = newX;
            currentY = newY;
            currentZ = newZ;
        }

        // view needs to know how big the grid is to properly center it
        public int GetGridSizeX()
        {
            return gridSizeX;
        }

        public int GetGridSizeY()
        {
            return gridSizeY;
        }

        public int GetGridSizeZ()
        {
            return gridSizeZ;
        }

        // checks if a field position is in bounds
        private bool IsInBounds(int x, int y, int z)
        {
            return (x >= 0 && y >= 0 && z >= 0 && x < gridSizeX && y < gridSizeY && z < gridSizeZ);
        }

        // checks if a field exists
        public bool HasFieldAtPosition(int x, int y, int z)
        {
            return (IsInBounds(x, y, z) && map[x, y, z] != null);
        }

        // returns field at a position
        public Field GetFieldAtPosition(int x, int y, int z)
        {
            return map[x, y, z];
        }
    }
}
