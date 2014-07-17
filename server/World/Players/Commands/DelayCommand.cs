using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Players.Commands
{
    class DelayCommand : PlayerCommand
    {
        public void Handle(int tick)
        {
            // delay does nothing
        }
    }
}
