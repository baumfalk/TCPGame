using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World
{
    class Directions
    {
        public static int NORTH = 0;
        public static int EAST = 1;
        public static int UP = 2;
        public static int SOUTH = 3;
        public static int WEST = 4;
        public static int DOWN = 5;

        public static int inverse(int direction)
        {
            return (direction + 3) % 6;
        }

        public static String toString(int direction)
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

        public static String toShortString(int direction)
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

        public static int fromString(String direction)
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

        public static int fromShortString(String direction)
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
