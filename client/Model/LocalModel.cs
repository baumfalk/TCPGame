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

        // size of the display grid
        private int gridSizeX;
        private int gridSizeY;

        // display grid
        public Field[,] map;

        // constructor allows the controller to give a grid size, which is
        // the area that will be "remembered"
        public LocalModel(int gridSizeX, int gridSizeY)
        {
            this.gridSizeX = gridSizeX;
            this.gridSizeY = gridSizeY;

            map = new Field[gridSizeX, gridSizeY];
        }
        
        // updates based on strings sent to the controller by the server. Is not very
        // robust yet, the following two inputs are possible at the moment:

        // N,Player,Position,x,y 
        // N,Tile,Detected,x,y,representation

        // N indicates an order in which commands are sent from the server. Player position changes to x, y.
        public void update(List<String> updateData)
        {
            // sort our input so we handle them in order. Should already be sorted, but better to check?
            // possible chokepoint later, but I doubt sorting short lists will be an issue.
            updateData.Sort();

            // loop through all strings we received
            foreach (String input in updateData)
            {
                // split the inputs
                String[] inputPart = input.Split(',');

                // switch on the type of input, let other methods handle them.
                switch (inputPart[1])
                {
                    case "Player":
                        updatePlayer(inputPart);
                        break;
                    case "Tile":
                        updateTile(inputPart);
                        break;
                }
            }
        }

        // checks if a field exists
        public bool hasFieldAtPosition(int x, int y)
        {
            return (map[x, y] != null);
        }

        // returns field at a position
        public Field getFieldAtPosition(int x, int y)
        {
            return map[x, y];
        }

        // handles player-type updates. Only "position" update exists at the moment.
        private void updatePlayer(String[] inputPart)
        {
            
            if (inputPart[2].Equals("Position"))
            {
                int newX = int.Parse(inputPart[3]);
                int newY = int.Parse(inputPart[4]);

                // if we have moved, we should shift our map (which is player-centered)
                if (newX != currentX || newY != currentY) shiftMap(newX, newY);
            }
        }

        // shift the map. This is needed when the player moves, since the map is centered
        // on the player.
        private void shiftMap(int newX, int newY)
        {
            // the shift we need to make is the difference of the old (current) position 
            // with the new one
            int shiftX = currentX - newX;
            int shiftY = currentY - newY;

            // create a new grid
            Field[,] newMap = new Field[gridSizeX, gridSizeY];

            // loop over the old grid
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    // check if a field already exists on the old map
                    if (x - shiftX < 0 || x - shiftX > gridSizeX -1 ||
                        y - shiftY < 0 || y - shiftY > gridSizeY -1)
                    {
                        // this doesn't actually do anything, but we may want to do
                        // something different here at some point, and it's nice to see
                        // that a "new" field is null at the start.

                        newMap[x, y] = null;
                    }
                    else
                    {
                        // if the field did exist, copy it from the right position on
                        // the old map
                        newMap[x, y] = map[x - shiftX, y - shiftY];
                    }
                }
            }

            // update the grid and the player position
            map = newMap;
            currentX = newX;
            currentY = newY;
        }

        // handles "tile" type updates. Only "detection" updates exist at the moment.
        private void updateTile(String[] inputPart)
        {
            if (inputPart[2].Equals("Detected"))
            {
                // get position and representation from the input
                int xPos = int.Parse(inputPart[3]);
                int yPos = int.Parse(inputPart[4]);
                int zPos = int.Parse(inputPart[5]);
                String representation = inputPart[6];

                // calculate position relative to the player
                int mapPosX = gridSizeX / 2 + 1 + (xPos - currentX);
                int mapPosY = gridSizeX / 2 + 1 + (yPos - currentY);

                // add tile to the grid
                map[mapPosX, mapPosY] = new Field(representation);
            }
        }
    }
}
