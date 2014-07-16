using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;
using TCPGameServer.World.Players;
using TCPGameSharedInfo;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Creatures
{
    class PlayerVisionSystem : VisionSystem
    {
        // vision range
        private int visionRange = 5;

        private HashSet<Tile> changedTiles;

        public PlayerVisionSystem()
        {
            changedTiles = new HashSet<Tile>();
        }

        public void DoVisionEvent(Tile changedTile)
        {
            changedTiles.Add(changedTile);
        }

        public void HandleVisionEvents(Creature owner, int tick)
        {
            Player player = owner.GetPlayer();

            foreach (Tile changedTile in changedTiles)
            {
                Location tileLocation = changedTile.GetLocation();

                CreatureRepresentation occupantRepresentation = CreatureRepresentation.None;

                if (changedTile.HasOccupant()) occupantRepresentation = changedTile.GetOccupant().GetRepresentation();

                player.AddMessage("TILE,CHANGED," +
                    tileLocation.x + "," +
                    tileLocation.y + "," +
                    tileLocation.z + "," +
                    changedTile.GetRepresentation() + "," +
                    occupantRepresentation,
                    tick);
            }

            changedTiles.Clear();
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
