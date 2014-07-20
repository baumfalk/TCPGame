using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerControlCommands
{
    class LogServerControlData : ServerCommandData
    {
        public LogServerControlData()
        {
            this.commandType = ServerCommandType.Log;
        }
    }
}
