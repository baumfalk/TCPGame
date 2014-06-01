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
        public Field[,,] map;

        // constructor allows the controller to give a grid size, which is
        // the area that will be "remembered"
        public LocalModel(int gridSizeX, int gridSizeY, int gridSizeZ)
        {
            this.gridSizeX = gridSizeX;
            this.gridSizeY = gridSizeY;
            this.gridSizeZ = gridSizeZ;

            map = new Field[gridSizeX, gridSizeY, gridSizeZ];
        }

        // view needs to know how big the grid is to properly center it
        public int getGridSizeX()
        {
            return gridSizeX;
        }

        public int getGridSizeY()
        {
            return gridSizeY;
        }

        public int getGridSizeZ()
        {
            return gridSizeZ;
        }
        
        // updates based on strings sent to the controller by the server. Is not very
        // robust yet, the following two inputs are possible at the moment:

        // N,Player,Position,x,y 
        // N,Tile,Detected,x,y,representation

        // N indicates an order in which commands are sent from the server. Player position changes to x, y.
        public void update(List<String> updateData)
        {
            // loop through all strings we received
            foreach (String input in updateData)
            {
                // split the inputs
                String[] inputPart = input.Split(',');

                // switch on the type of input, let other methods handle them.
                switch (inputPart[0])
                {
                    case "LOGIN":
                        Debug.Print(inputPart[2]);
                        break;
                    case "Player":
                        updatePlayer(inputPart);
                        break;
                    case "Tile":
                        updateTile(inputPart);
                        break;
                    default:
                        Debug.Print(input);
                        break;
                }
            }
        }

        // checks if a field position is in bounds
        private bool isInBounds(int x, int y, int z)
        {
            return (x >= 0 && y >= 0 && z >= 0 && x < gridSizeX && y < gridSizeY && z < gridSizeZ);
        }

        // checks if a field exists
        public bool hasFieldAtPosition(int x, int y, int z)
        {
            return (isInBounds(x, y, z) && map[x, y, z] != null);
        }

        // returns field at a position
        public Field getFieldAtPosition(int x, int y, int z)
        {
            return map[x, y, z];
        }

        // handles player-type updates. Only "position" update exists at the moment.
        private void updatePlayer(String[] inputPart)
        {
            if (inputPart[1].Equals("Position"))
            {
                int newX = int.Parse(inputPart[2]);
                int newY = int.Parse(inputPart[3]);
                int newZ = int.Parse(inputPart[4]);

                // if we have moved, we should shift our map (which is player-centered)
                if (newX != currentX || newY != currentY || newZ != currentZ) shiftMap(newX, newY, newZ);
            }
        }

        // shift the map. This is needed when the player moves, since the map is centered
        // on the player.
        private void shiftMap(int newX, int newY, int newZ)
        {
            // the shift we need to make is the difference of the old (current) position 
            // with the new one
            int shiftX = currentX - newX;
            int shiftY = currentY - newY;
            int shiftZ = currentZ - newZ;

            // create a new grid
            Field[,,] newMap = new Field[gridSizeX, gridSizeY, gridSizeZ];

            // loop over the old grid
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    for (int z = 0; z < gridSizeZ; z++)
                    {
                        // check if a field already exists on the old map
                        if (isInBounds(x - shiftX, y - shiftY, z - shiftZ))
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

        // handles "tile" type updates. Only "detection" updates exist at the moment.
        private void updateTile(String[] inputPart)
        {
            if (inputPart[1].Equals("Detected"))
            {
                // get position and representation from the input
                int xPos = int.Parse(inputPart[2]);
                int yPos = int.Parse(inputPart[3]);
                int zPos = int.Parse(inputPart[4]);
                String representation = inputPart[5];

                // calculate position relative to the player
                int mapPosX = gridSizeX / 2 + 1 + (xPos - currentX);
                int mapPosY = gridSizeY / 2 + 1 + (yPos - currentY);
                int mapPosZ = gridSizeZ / 2 + 1 + (zPos - currentZ);

                // add tile to the grid
                map[mapPosX, mapPosY, mapPosZ] = new Field(representation);
            }
        }
    }
}
