using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.ActionHandling
{
    class ActionHandler
    {
        private Model model;

        private LoginActionHandler loginActionHandler;
        private MoveActionHandler moveActionHandler;
        private PlayerActionHandler playerActionHandler;
        private LookActionHandler lookActionHandler;
        private SayActionHandler sayActionHandler;
        public ActionHandler(Model model)
        {
            this.model = model;

            loginActionHandler = new LoginActionHandler(model);
            moveActionHandler = new MoveActionHandler(model);
            playerActionHandler = new PlayerActionHandler(model);
            lookActionHandler = new LookActionHandler(model);
            sayActionHandler = new SayActionHandler(model);
        }

        public void Handle(Player player, String command, int tick)
        {
            String[] splitCommand = command.Split(',');

            // hand off command handling to specialized classes
            switch (splitCommand[0])
            {
                case "LOGIN":
                    loginActionHandler.Handle(player, splitCommand, tick);
                    return;
                case "MOVE":
                    moveActionHandler.Handle(player, splitCommand, tick);
                    return;
                case "PLAYER":
                    playerActionHandler.Handle(player, splitCommand, tick);
                    return;
                case "LOOK":
                    lookActionHandler.Handle(player, splitCommand, tick);
                    return;
                case "SAY":
                    sayActionHandler.Handle(player, splitCommand, tick);
                    return;
            }
        }
    }
}
