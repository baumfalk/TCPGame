using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using TCPGameSharedInfo;

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

        // list of players online and their locations
        private PlayerList playerList;

        // list of messages received
        private List<String> receivedMessages;

        // constructor allows the controller to give a grid size, which is
        // the area that will be "remembered"
        public LocalModel(int gridSizeX, int gridSizeY, int gridSizeZ)
        {
            this.gridSizeX = gridSizeX;
            this.gridSizeY = gridSizeY;
            this.gridSizeZ = gridSizeZ;

            map = new Field[gridSizeX, gridSizeY, gridSizeZ];

            receivedMessages = new List<string>();
            playerList = new PlayerList();
        }

        // add and retrieve messages received from the server
        public void AddReceivedMessage(string receivedMessage)
        {
            receivedMessages.Add(receivedMessage);
        }
        public List<String> GetReceivedMessages()
        {
            return receivedMessages;
        }

        public void updatePlayerList(string playerName, string area, string stateChange)
        {
            playerList.updatePlayerList(playerName, area, stateChange);
        }
        public List<String> GetPlayerList()
        {
            return playerList.GetList();
        }

        public void AddTile(int xPos, int yPos, int zPos, TileRepresentation representation, Creature creature)
        {
            // calculate position relative to the center of the map
            int mapPosX = gridSizeX / 2 + (xPos - currentX);
            int mapPosY = gridSizeY / 2 + (yPos - currentY);
            int mapPosZ = gridSizeZ / 2 + (zPos - currentZ);

            // add tile to the grid
            if (IsInBounds(mapPosX, mapPosY, mapPosZ)) map[mapPosX, mapPosY, mapPosZ] = new Field(representation, creature);
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
