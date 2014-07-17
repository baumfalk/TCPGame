using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Players.Commands
{
    class LoginCompletePlayerCommand : PlayerCommand
    {
        private Model model;
        private Player player;

        public LoginCompletePlayerCommand(Model model, Player player)
        {
            this.model = model;
            this.player = player;
        }

        public void Handle(int tick)
        {
            String name = player.GetName();

            
            Log.Print(name + " has logged in");
        }
    }
}
