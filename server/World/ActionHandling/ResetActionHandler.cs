using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Players;

namespace TCPGameServer.World.ActionHandling
{
    class ResetActionHandler
    {
        Model model;

        public ResetActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand, int tick)
        {
            model.ResetMap();
        }
    }
}
