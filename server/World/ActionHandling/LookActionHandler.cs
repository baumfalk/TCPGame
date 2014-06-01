using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World.ActionHandling
{
    class LookActionHandler
    {
        static public void Look(Player player, Model world)
        {
            Tile playerLocation = player.getBody().getPosition();

            List<Tile> tilesToSend = world.getSurroundingTiles(playerLocation, 5);

            player.addMessage("PLAYER,POSITION," + playerLocation.getX() + "," + playerLocation.getY() + "," + playerLocation.getZ());

            foreach (Tile toSend in tilesToSend)
            {
                player.addMessage("TILE,DETECTED," + toSend.getX() + "," + toSend.getY() + "," + toSend.getZ() + "," + toSend.getRepresentation());

                if (toSend.hasOccupant())
                {
                    Creature occupant = toSend.getOccupant();

                    player.addMessage("CREATURE,DETECTED," + occupant.getRepresentation());
                }
            }
        }
    }
}