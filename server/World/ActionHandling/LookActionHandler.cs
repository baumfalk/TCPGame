using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World.ActionHandling
{
    class LookActionHandler
    {
        private Model world;

        public LookActionHandler(Model world)
        {
            this.world = world;
        }

        public void Handle(Player player, String[] splitCommand)
        {
            if (player.getCommandState() != Player.COMMANDSTATE_NORMAL) return;

            bool includeTiles = false;
            bool includePlayer = false;

            if (splitCommand[1].Equals("TILES_INCLUDED")) includeTiles = true;
            if (splitCommand[2].Equals("PLAYER_INCLUDED")) includePlayer = true;

            Tile playerLocation = player.getBody().getPosition();

            List<Tile> tilesToSend = world.getSurroundingTiles(playerLocation, 5);

            if (includePlayer) player.addMessage("PLAYER,POSITION," + playerLocation.getX() + "," + playerLocation.getY() + "," + playerLocation.getZ());

            foreach (Tile toSend in tilesToSend)
            {
                if (includeTiles) player.addMessage("TILE,DETECTED," + toSend.getX() + "," + toSend.getY() + "," + toSend.getZ() + "," + toSend.getRepresentation());

                if (toSend.hasOccupant() && (!(toSend == playerLocation) || includePlayer))
                {
                    Creature occupant = toSend.getOccupant();

                    player.addMessage("CREATURE,DETECTED," + toSend.getX() + "," + toSend.getY() + "," + toSend.getZ() + "," + occupant.getRepresentation());
                }
            }
        }
    }
}