using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.ActionHandling
{
    class MoveActionHandler
    {
        private Model model;

        public MoveActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand, int tick)
        {
            // get the player position
            Tile position = player.GetBody().GetPosition();

            // get the direction the player wants to move to
            int direction = int.Parse(splitCommand[1]);

            // if there is no neighbor in that direction, abort
            if (position.HasNeighbor(direction))
            {
                // get the tile in the direction the player wants to move to
                Tile neighbor = position.GetNeighbor(direction);

                // if it's passable and empty, vacate the tile the player is on
                // and move the player to the new position
                if (neighbor.IsPassable() && !neighbor.HasOccupant()) {
                    position.Vacate();
                    neighbor.SetOccupant(player.GetBody());

                    player.AddImmediateCommand("LOOK,TILES_INCLUDED,PLAYER_INCLUDED");
                }
            }
        }
    }
}
