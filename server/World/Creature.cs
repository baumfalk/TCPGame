using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World
{
    // a creature is any mobile object, including players and such which may extend it
    public class Creature
    {
        private Player player;

        // image key
        private String representation;
        // position in the world
        private Tile position;

        // created with it's representation
        public Creature(String representation)
        {
            this.representation = representation;
        }

        public bool isPlayer()
        {
            return (player != null);
        }

        public void setPlayer(Player player)
        {
            this.player = player;
        }

        public Player getPlayer()
        {
            return player;
        }

        // position can be changed and requested freely
        public void setPosition(Tile position)
        {
            this.position = position;
        }
        public Tile getPosition()
        {
            return position;
        }

        // image key can be requested and changed freely
        public void setRepresentation(String representation)
        {
            this.representation = representation;
        }
        public String getRepresentation()
        {
            return representation;
        }
    }
}
