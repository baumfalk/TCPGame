using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;
using TCPGameServer.World.Players;

namespace TCPGameServer.World.Creatures
{
    class VisionSystem
    {
        // vision range
        private int visionRange = 5;

        public void DoEvent(Tile changedTile, Creature owner, int tick)
        {
            if (owner.IsPlayer())
            {
                Player player = owner.GetPlayer();

                Location tileLocation = changedTile.GetLocation();

                player.AddMessage("TILE,DETECTED," +
                    tileLocation.x + "," +
                    tileLocation.y + "," +
                    tileLocation.z + "," +
                    changedTile.GetRepresentation(),
                    tick);
                if (changedTile.GetOccupant() != null)
                {
                    player.AddMessage("CREATURE,DETECTED," +
                        tileLocation.x + "," +
                        tileLocation.y + "," +
                        tileLocation.z + "," +
                        changedTile.GetOccupant().GetRepresentation(),
                        tick);
                }
            }
        }

        public int GetVisionRange()
        {
            return visionRange;
        }

        public void SetVisionRange(int visionRange)
        {
            this.visionRange = visionRange;
        }
    }
}
