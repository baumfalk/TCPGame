using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.ActionHandling
{
    class PlayerActionHandler
    {
        private Model model;

        public PlayerActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand, int tick)
        {
            String subcommand = splitCommand[1];

            switch (subcommand)
            {
                case "PLACE":
                    String area = splitCommand[2];
                    int ID = int.Parse(splitCommand[3]);
                    String name = player.GetName();

                    // get the position to place the player from the world
                    Tile position = model.GetTile(area, ID);

                    // don't accept commands until the player has been placed
                    player.SetCommandState(Player.COMMANDSTATE_PLACEMENT);

                    // add the player to the tile. For now, if someone else is there, just wait until he's gone
                    // and send a message to everyone. If it's an NPC, remove it.
                    if (position.HasOccupant())
                    {
                        Creature occupant = position.GetOccupant();

                        if (occupant.IsPlayer())
                        {
                            String occupantName = occupant.GetPlayer().GetName();

                            // add the player again at the same place.
                            player.AddBlockingCommand(new String[] { "PLAYER", "PLACE", area, ID.ToString() });

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

                    // if the player has just been placed and wasn't logged in yet, he is now
                    if (player.GetCommandState() == Player.COMMANDSTATE_PLACEMENT) player.SetCommandState(Player.COMMANDSTATE_NORMAL);

                    // make the player look around
                    player.AddImmediateCommand(new String[] { "LOOK", "TILES_INCLUDED", "PLAYER_INCLUDED" });
                    return;
            }
        }
    }
}
