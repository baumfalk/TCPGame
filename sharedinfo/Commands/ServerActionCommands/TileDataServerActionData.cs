using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerActionCommands
{
    class TileDataServerActionData : ServerCommandData
    {
        public TileDataServerActionData()
        {
            this.commandType = ServerCommandType.TileData;
        }
    }
}
