using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Players.Commands
{
    public interface PlayerCommand
    {
        // the command should have been initialized in the constructor, and only need
        // the tick to be handled.
        void Handle(int tick);
    }
}
