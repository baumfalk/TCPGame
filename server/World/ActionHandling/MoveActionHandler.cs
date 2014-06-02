using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World.ActionHandling
{
    class MoveActionHandler
    {
        private Model world;

        public MoveActionHandler(Model world)
        {
            this.world = world;
        }

        public void Handle(Player player, String[] splitCommand)
        {
            Tile position = player.getBody().getPosition();

            int direction = int.Parse(splitCommand[1]);

            if (position.hasNeighbor(direction))
            {
                Tile neighbor = position.getNeighbor(direction);

                if (neighbor.isPassable() && !neighbor.hasOccupant()) {
                    position.vacate();
                    neighbor.setOccupant(player.getBody());

                    player.setMoved(true);
                }
            }
        }
    }
}
