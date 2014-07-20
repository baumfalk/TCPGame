using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerActionCommands
{
    class TeleportServerActionData : ServerCommandData
    {
        public String areaTarget;
        public int targetTileIndex;

        public TeleportServerActionData(String areaTarget, int targetTileIndex)
        {
            this.areaTarget = areaTarget;
            this.targetTileIndex = targetTileIndex;
            this.commandType = ServerCommandType.Teleport;
        }
    }
}
