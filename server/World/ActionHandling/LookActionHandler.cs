using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.ActionHandling
{
    class LookActionHandler
    {
        private Model model;

        public LookActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand)
        {
            if (player.GetCommandState() != Player.COMMANDSTATE_NORMAL) return;

            bool includeTiles = false;
            bool includePlayer = false;

            if (splitCommand[1].Equals("TILES_INCLUDED")) includeTiles = true;
            if (splitCommand[2].Equals("PLAYER_INCLUDED")) includePlayer = true;

            Tile playerLocation = player.GetBody().GetPosition();

            List<Tile> tilesToSend = model.getSurroundingTiles(playerLocation, 10);

            if (includePlayer) player.AddMessage("PLAYER,POSITION," + playerLocation.GetX() + "," + playerLocation.GetY() + "," + playerLocation.GetZ());

            foreach (Tile toSend in tilesToSend)
            {
                if (includeTiles) player.AddMessage("TILE,DETECTED," + toSend.GetX() + "," + toSend.GetY() + "," + toSend.GetZ() + "," + toSend.GetRepresentation());

                if (toSend.HasOccupant() && (!(toSend == playerLocation) || includePlayer))
                {
                    Creature occupant = toSend.GetOccupant();

                    player.AddMessage("CREATURE,DETECTED," + toSend.GetX() + "," + toSend.GetY() + "," + toSend.GetZ() + "," + occupant.GetRepresentation());
                }
            }
        }
    }
}