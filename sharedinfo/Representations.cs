using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameSharedInfo
{
    public enum TileRepresentation { Floor, Floor2, Wall, Campfire, Stairs };
    public enum CreatureRepresentation { Player };

    public static class Representation
    {
        public static String toString(TileRepresentation representation)
        {
            switch (representation)
            {
                case TileRepresentation.Floor:
                    return "floor";
                case TileRepresentation.Floor2:
                    return "2floor";
                case TileRepresentation.Wall:
                    return "wall";
                case TileRepresentation.Campfire:
                    return "campfire";
                case TileRepresentation.Stairs:
                    return "stairs";
                default:
                    return "default";
            }
        }

        public static String toString(CreatureRepresentation representation)
        {
            switch (representation)
            {
                case CreatureRepresentation.Player:
                    return "player";
                default:
                    return "default";
            }
        }
    }
}
