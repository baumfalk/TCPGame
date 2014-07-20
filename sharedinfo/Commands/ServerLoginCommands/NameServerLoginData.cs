using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands.ServerLoginCommands
{
    class NameServerLoginData : ServerCommandData
    {
        public String loginName;

        public NameServerLoginData(String loginName)
        {
            this.loginName = loginName;
            this.commandType = ServerCommandType.Name;
        }
    }
}
