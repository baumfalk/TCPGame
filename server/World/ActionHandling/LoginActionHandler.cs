using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.ActionHandling
{
    class LoginActionHandler
    {
        private Model model;

        public LoginActionHandler(Model model)
        {
            this.model = model;
        }

        public void Handle(Player player, String[] splitCommand, int tick)
        {
            String name = player.GetName();
            String subCommand = splitCommand[1];

            switch (subCommand)
            {
                case "COMPLETE": // when done logging in, let people know
                    player.AddMessage("MESSAGE,LOGIN,welcome " + name + "!", tick);
                    model.AddModelCommand(new String[] { "SAY", name + " has logged in" });
                    Output.Print(name + " has logged in");
                    return;
            }
        }
    }
}
