using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;
using TCPGameServer.World.Creatures;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Players.Commands
{
    class MovePlayerCommand : PlayerCommand
    {
        private Model model;
        private Player player;
        private Tile targetTile;

        public MovePlayerCommand(Model model, Player player, int direction)
        {
            this.model = model;
            this.player = player;

            // the player's position
            Tile position = player.GetBody().GetPosition();

            if (position.HasNeighbor(direction))
            {
                targetTile = position.GetNeighbor(direction);
            }
        }

        public void Handle(int tick)
        {
            if (targetTile != null && targetTile.IsPassable() && !targetTile.HasOccupant())
            {
                Creature playerBody = player.GetBody();
                Tile playerPosition = playerBody.GetPosition();

                // deregister from the outer layer of tiles the creature can currently see
                playerBody.VisionDeregister(Creature.RegisterMode.Outer);

                // remove the player from his old position and add him to the new one
                playerPosition.Vacate();
                targetTile.SetOccupant(playerBody);

                // register with the outer layers of tiles from the new location
                playerBody.VisionRegister(Creature.RegisterMode.Outer);

                // make the player look at his new location
                player.AddImmediateCommand(new LookPlayerCommand(player, LookPlayerCommand.IncludePlayerLocation.Yes, LookPlayerCommand.UpdateMode.Outer));

                // if the player changes areas, send a wholist update
                if (playerPosition.GetArea() != targetTile.GetArea())
                {
                    player.AddImmediateCommand(new WholistUpdateCommand(model, player, WholistUpdateCommand.StateChange.Changed_Area));
                }
            }
        }
    }
}
