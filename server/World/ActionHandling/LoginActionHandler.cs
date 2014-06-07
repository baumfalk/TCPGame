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

        public void Handle(Player player, String[] splitCommand)
        {
            switch (splitCommand[1])
            {
                case "COMPLETE":
                    player.SetCommandState(Player.COMMANDSTATE_NORMAL);
                    return;
            }
        }
    }
}
