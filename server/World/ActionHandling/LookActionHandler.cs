﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Map;
using TCPGameServer.World.Players;

namespace TCPGameServer.World.ActionHandling
{
    class LookActionHandler
    {
        private Model model;

        public LookActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand, int tick)
        {
            // check if tiles should be included 
            bool includeTiles = splitCommand[1].Equals("TILES_INCLUDED");
            // check if the player should be included
            bool includePlayer = splitCommand[2].Equals("PLAYER_INCLUDED");
            // position of the player
            Tile playerPosition = player.GetBody().GetPosition();
            // x/y/z location of the player
            Location playerLocation = playerPosition.GetLocation();

            // get the tiles surrounding the player
            List<Tile> tilesToSend = Geography.getSurroundingTiles(playerPosition, 5, true);

            // if we need to include player data, send X,Y and Z coordinates
            if (includePlayer) player.AddMessage("PLAYER,POSITION," + playerLocation.x + "," + playerLocation.y + "," + playerLocation.z, tick);

            // send data for each tile in sight (even when not including tile data, we still need creatures)
            foreach (Tile toSend in tilesToSend)
            {
                // x/y/z location of the tile
                Location tileLocation = toSend.GetLocation();

                // if we need to include tile data, send X, Y, Z and representation
                if (includeTiles) player.AddMessage("TILE,DETECTED," + tileLocation.x + "," + tileLocation.y + "," + tileLocation.z + "," + toSend.GetRepresentation(), tick);

                // Check if a tile has a creature. The player-body is a creature. If we're not sending player data, ignore him. 
                if (toSend.HasOccupant() && (!(toSend == playerPosition) || includePlayer))
                {
                    Creature occupant = toSend.GetOccupant();

                    // send X, Y, Z and representation of any creatures nearby
                    player.AddMessage("CREATURE,DETECTED," + tileLocation.x + "," + tileLocation.y + "," + tileLocation.z + "," + occupant.GetRepresentation(), tick);
                }
            }
        }
    }
}