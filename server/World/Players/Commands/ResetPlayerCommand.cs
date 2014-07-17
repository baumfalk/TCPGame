using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Players.Commands
{
    class ResetPlayerCommand : PlayerCommand
    {
        private Model model;

        public ResetPlayerCommand(Model model)
        {
            this.model = model;
        }

        public void Handle(int tick)
        {
            model.ResetMap();
        }
    }
}
