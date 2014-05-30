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
        private int currentX;
        private int currentY;

        private int gridSizeX;
        private int gridSizeY;

        // display grid
        public Field[,] map;

        public LocalModel(int gridSizeX, int gridSizeY)
        {
            this.gridSizeX = gridSizeX;
            this.gridSizeY = gridSizeY;

            map = new Field[gridSizeX, gridSizeY];
        }

        public void update(List<String> updateData)
        {
            foreach (String input in updateData)
            {
                Debug.Print(input);

                String[] inputPart = input.Split(',');

                switch (inputPart[1])
                {
                    case "Player":
                        updatePlayer(inputPart);
                        break;
                    case "Tile":
                        updateMap(inputPart);
                        break;
                }
            }
        }

        public bool hasFieldAtPosition(int x, int y)
        {
            return (map[x, y] != null);
        }

        public Field getFieldAtPosition(int x, int y)
        {
            return map[x, y];
        }

        private void updatePlayer(String[] inputPart)
        {
            if (inputPart[2].Equals("Position"))
            {
                int newX = int.Parse(inputPart[3]);
                int newY = int.Parse(inputPart[4]);

                Debug.Print("position (" + newX + ", " + newY + ")");

                if (newX != currentX || newY != currentY) shiftMap(newX, newY);
            }
        }

        private void shiftMap(int newX, int newY)
        {
            int shiftX = newX - currentX;
            int shiftY = newY - currentY;

            Field[,] newMap = new Field[gridSizeX, gridSizeY];

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (x - shiftX < 0 || x - shiftX > gridSizeX ||
                        y - shiftY < 0 || y - shiftY > gridSizeY)
                    {
                        newMap[x, y] = null;
                    }
                    else
                    {
                        newMap[x, y] = map[x - shiftX, y - shiftY];
                    }
                }
            }
            map = newMap;
            currentX = newX;
            currentY = newY;
        }

        private void updateMap(String[] inputPart)
        {
            int xPos = int.Parse(inputPart[3]);
            int yPos = int.Parse(inputPart[4]);
            String representation = inputPart[5];

            int mapPosX = gridSizeX / 2 + 1 + (xPos - currentX);
            int mapPosY = gridSizeX / 2 + 1 + (yPos - currentY);

            Debug.Print(xPos + ", " + yPos + " -> " + mapPosX + ", " + mapPosY);

            map[mapPosX, mapPosY] = new Field(representation);
        }
    }
}
