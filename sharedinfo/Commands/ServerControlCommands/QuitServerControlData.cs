using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerControlCommands
{
    class QuitServerControlData : ServerCommandData
    {
        public QuitServerControlData()
        {
            this.commandType = ServerCommandType.Quit;
        }
    }
}
