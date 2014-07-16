using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.Creatures
{
    interface VisionSystem
    {
        void DoVisionEvent(Tile changedTile);
        void HandleVisionEvents(Creature owner, int tick);

        void SetVisionRange(int visionRange);
        int GetVisionRange();
    }
}
