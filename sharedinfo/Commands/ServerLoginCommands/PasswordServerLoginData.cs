using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerLoginCommands
{
    class PasswordServerLoginData : ServerCommandData
    {
        public String password;

        public PasswordServerLoginData(String password)
        {
            this.password = password;
            this.commandType = ServerCommandType.Password;
        }
    }
}