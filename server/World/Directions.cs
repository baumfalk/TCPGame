using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World
{
    // simple class with some functions to make working with directions easier and
    // reduce the chances developers will use the wrong values for directions
    class Directions
    {
        public const int NORTH = 0;
        public const int EAST = 1;
        public const int UP = 2;
        public const int SOUTH = 3;
        public const int WEST = 4;
        public const int DOWN = 5;

        // inverse direction
        public static int Inverse(int direction)
        {
            return (direction + 3) % 6;
        }

        // get the x, y and z coordinates for a move of one position in any direction
        public static int[] GetNeighboring(int direction, int x, int y, int z)
        {
            switch (direction)
            {
                case 0: return new int[] { x, y + 1, z };
                case 1: return new int[] { x + 1, y, z };
                case 2: return new int[] { x, y, z + 1 };
                case 3: return new int[] { x, y - 1, z };
                case 4: return new int[] { x - 1, y, z };
                case 5: return new int[] { x, y, z - 1 };
                default: return new int[] { -1, -1, -1 };
            }
        }

        // convert int to a long string version of the direction
        public static String ToString(int direction)
        {
            switch (direction)
            {
                case 0: return "north";
                case 1: return "east";
                case 2: return "up";
                case 3: return "south";
                case 4: return "west";
                case 5: return "down";
                default: return "";
            }
        }

        // convert int to a short string version of the direction
        public static String ToShortString(int direction)
        {
            switch (direction)
            {
                case 0: return "n";
                case 1: return "e";
                case 2: return "u";
                case 3: return "s";
                case 4: return "w";
                case 5: return "d";
                default: return "";
            }
        }

        // convert long string version of the direction to int
        public static int FromString(String direction)
        {
            switch (direction)
            {
                case "north": return 0;
                case "east": return 1;
                case "up": return 2;
                case "south": return 3;
                case "west": return 4;
                case "down": return 5;
                default: return -1;
            }
        }

        // convert short string version of the direction to int
        public static int FromShortString(String direction)
        {
            switch (direction)
            {
                case "n": return 0;
                case "e": return 1;
                case "u": return 2;
                case "s": return 3;
                case "w": return 4;
                case "d": return 5;
                default: return -1;
            }
        }
    }
}
