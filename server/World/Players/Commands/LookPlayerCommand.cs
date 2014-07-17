using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.Players.Commands
{
    class LookPlayerCommand : PlayerCommand
    {
        // should we update the outer layer or everything
        public enum UpdateMode { Outer, All };
        // should we include the player location
        public enum IncludePlayerLocation { Yes, No };

        private Player player;
        private bool includePlayer;
        private UpdateMode updateMode;

        public LookPlayerCommand(Player player, IncludePlayerLocation includePlayer, UpdateMode updateMode)
        {
            this.player = player;
            this.includePlayer = includePlayer == IncludePlayerLocation.Yes;
            this.updateMode = updateMode;
        }

        public void Handle(int tick)
        {
            // the tile the player is standing on
            Tile playerPosition = player.GetBody().GetPosition();

            // the x/y/z coordinates of that tile
            Location playerLocation = playerPosition.GetLocation();

            // the vision range of the creature
            int visionRange = player.GetBody().GetVisionRange();

            // send tiles as indicated by the update mode
            List<Tile> tilesToSend;
            switch (updateMode)
            {
                case UpdateMode.Outer:
                    tilesToSend = playerPosition.GetTilesAtRange(visionRange);
                    break;
                case UpdateMode.All:
                default: // if somehow updatemode is null, just send everything.
                    tilesToSend = playerPosition.GetTilesInRange(visionRange);
                    break;
            }

            // if we need to include the player position, send X, Y and Z coordinates
            if (includePlayer) player.AddMessage("PLAYER,POSITION," + playerLocation.x + "," + playerLocation.y + "," + playerLocation.z, tick);

            // send a vision event for each tile we're looking at
            foreach (Tile toSend in tilesToSend)
            {
                player.GetBody().VisionEvent(toSend);
            }
        }
    }
}
