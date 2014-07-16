using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.Creatures
{
    class NPCVisionSystem : VisionSystem
    {
        private int visionRange;

        public void DoVisionEvent(Tile changedTile)
        {

        }
        public void HandleVisionEvents(Creature owner, int tick)
        {

        }

        public void SetVisionRange(int visionRange)
        {
            this.visionRange = visionRange;
        }
        public int GetVisionRange()
        {
            return visionRange;
        }
    }
}
