using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerControlCommands
{
    class ResetServerControlData : ServerCommandData
    {
        public ResetServerControlData()
        {
            this.commandType = ServerCommandType.Reset;
        }
    }
}
