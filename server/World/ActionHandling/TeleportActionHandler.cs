using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Players;
using TCPGameServer.World.Creatures;
using TCPGameServer.World.Map;

namespace TCPGameServer.World.ActionHandling
{
    class TeleportActionHandler
    {
        private Model model;

        public TeleportActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand, int tick)
        {
            String area = splitCommand[2];
            int ID = int.Parse(splitCommand[3]);
            String name = player.GetName();

            // get the position to place the player from the world
            Tile position = model.GetTile(area, ID);

            // add the player to the tile. For now, if someone else is there, just wait until he's gone
            // and send a message to everyone. If it's an NPC, remove it.
            if (position.HasOccupant())
            {
                Creature occupant = position.GetOccupant();

                if (occupant.IsPlayer())
                {
                    String occupantName = occupant.GetPlayer().GetName();

                    // add the player again at the same place.
                    player.AddBlockingCommand(new String[] { "TELEPORT", area, ID.ToString() });

                    // wait 10 seconds
                    player.AddBlockingDelay(60);

                    // send a message to everyone telling another player to move.
                    model.AddModelCommand(new String[] { "SAY", name + " is trying to be placed at the position of " + occupantName });

                    return;
                }
                else
                {
                    // if it's an NPC, remove it.
                    position.Vacate();
                }
            }

            // place the player at the location
            position.SetOccupant(player.GetBody());

            // make the player look around
            player.AddImmediateCommand(new String[] { "LOOK", "TILES_INCLUDED", "PLAYER_INCLUDED", "REGISTER_ALL" });
        }
    }
}
