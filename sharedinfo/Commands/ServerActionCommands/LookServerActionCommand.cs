using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerActionCommands
{
    class LookServerActionCommand : ServerCommandData
    {
        public LookServerActionCommand()
        {
            this.commandType = ServerCommandType.Look;
        }
    }
}
