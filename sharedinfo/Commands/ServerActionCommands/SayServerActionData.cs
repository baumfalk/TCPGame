using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerActionCommands
{
    class SayServerActionData : ServerCommandData
    {
        public String message;

        public SayServerActionData(String message)
        {
            this.message = message;
            this.commandType = ServerCommandType.Say;
        }
    }
}
