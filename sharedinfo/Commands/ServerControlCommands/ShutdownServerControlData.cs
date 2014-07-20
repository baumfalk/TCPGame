using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerControlCommands
{
    class ShutdownServerControlData : ServerCommandData
    {
        public ShutdownServerControlData()
        {
            this.commandType = ServerCommandType.Shutdown;
        }
    }
}
