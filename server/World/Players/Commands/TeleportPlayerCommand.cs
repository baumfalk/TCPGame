using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;
using TCPGameServer.World.Creatures;

namespace TCPGameServer.World.Players.Commands
{
    class TeleportPlayerCommand : PlayerCommand
    {
        private Player player;
        private Model model;
        private Tile targetTile;

        public TeleportPlayerCommand(Player player, Model model, String areaName, int tileID)
        {
            this.player = player;
            this.model = model;

            // get the target tile from the model
            targetTile = model.GetTile(areaName, tileID);
        }

        public void Handle(int tick)
        {
            // the body of the player teleporting
            Creature playerBody = player.GetBody();
            Tile playerPosition = playerBody.GetPosition();

            // remove the player from his position, if he has one
            if (playerPosition != null)
            {
                playerBody.VisionDeregister(Creature.RegisterMode.All);
                playerPosition.Vacate();
            }

            // add the player to the tile. For now, if someone else is there, just wait until he's
            // moved, and send a message to everyone to let them know someone should move. If it's an
            // NPC, remove it.
            if (targetTile.HasOccupant())
            {
                Creature occupant = targetTile.GetOccupant();

                if (occupant.IsPlayer())
                {
                    String occupantName = occupant.GetPlayer().GetName();

                    // add the player again at the same place
                    player.AddBlockingCommand(this);

                    // wait 10 seconds
                    player.AddBlockingDelay(60);

                    // send a message to everyone telling another player to move
                    model.AddModelCommand(new SayPlayerCommand(player, model, "is trying to be placed at the position of " + occupantName, true));
                }
                else
                {
                    // if it's an NPC, remove it
                    targetTile.Vacate();
                }
            }
            else // the tile is unoccupied
            {
                // place the player at the location
                targetTile.SetOccupant(player.GetBody());

                // register with all tiles nearby
                playerBody.VisionRegister(Creature.RegisterMode.All);

                // make the player look at the nearby tiles
                player.AddImmediateCommand(new LookPlayerCommand(player, LookPlayerCommand.IncludePlayerLocation.Yes, LookPlayerCommand.UpdateMode.All));
            }
        }
    }
}
