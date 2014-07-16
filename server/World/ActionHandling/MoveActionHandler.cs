﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Players;
using TCPGameServer.World.Map;
using TCPGameServer.Control.Output;



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

            // don't handle if the player has no position
            if (position == null) return;

            // position to move to
            Tile target = null;

            // get the direction the player wants to move to
            int direction = int.Parse(splitCommand[1]);

            // if there is no neighbor in that direction, abort
            if (position.HasNeighbor(direction))
            {
                // get the tile in the direction the player wants to move to
                Tile neighbor = position.GetNeighbor(direction);

                // if it's passable and empty, set the target tile to the neighboring tile
                if (neighbor.IsPassable() && !neighbor.HasOccupant())
                {
                    target = neighbor;
                }
            }

            // if we've got a valid target, move to it by vacating the current tile and setting us
            // as the occupant of the new tile.
            if (target != null)
            {
                player.GetBody().VisionDeregister(Creatures.Creature.RegisterMode.Outer);

                position.Vacate();
                target.SetOccupant(player.GetBody());

                // look on arrival
                player.AddImmediateCommand(new String[] { "LOOK", "TILES_INCLUDED", "PLAYER_INCLUDED", "UPDATE_OUTER" });
                
                player.GetBody().VisionRegister(Creatures.Creature.RegisterMode.Outer);
            }
        }
    }
}
