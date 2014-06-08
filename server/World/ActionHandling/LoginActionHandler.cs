using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            switch (splitCommand[1])
            {
                case "COMPLETE":
                    player.SetCommandState(Player.COMMANDSTATE_NORMAL);
                    player.AddMessage("MESSAGE,LOGIN,welcome " + player.GetName() + "!", tick);
                    model.AddModelCommand(new String[] { "SAY", player.GetName() + " has logged in" });
                    return;
            }
        }
    }
}
