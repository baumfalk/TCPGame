using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerActionCommands
{
    class MoveServerActionCommand : ServerCommandData
    {
        public int direction;

        public MoveServerActionCommand(int direction)
        {
            this.commandType = ServerCommandType.Move;
            this.direction = direction;
        }
    }
}
