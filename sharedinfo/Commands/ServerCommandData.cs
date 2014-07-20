using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands
{
    abstract class ServerCommandData
    {
        protected ServerCommandType commandType;

        ServerCommandType GetCommandType()
        {
            return commandType;
        }
    }
}
