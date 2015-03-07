using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerActionCommands
{
    class WhisperServerActionData : ServerCommandData
    {
        String target;
        String message;

        public WhisperServerActionData(String target, String message)
        {
            this.target = target;
            this.message = message;
            this.commandType = ServerCommandType.Whisper;
        }
    }
}
