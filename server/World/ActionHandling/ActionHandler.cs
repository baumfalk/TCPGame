using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.World.ActionHandling
{
    class ActionHandler
    {
        private Model world;

        private LoginActionHandler loginActionHandler;
        private MoveActionHandler moveActionHandler;
        private PlayerActionHandler playerActionHandler;
        private LookActionHandler lookActionHandler;

        public ActionHandler(Model world)
        {
            this.world = world;

            loginActionHandler = new LoginActionHandler(world);
            moveActionHandler = new MoveActionHandler(world);
            playerActionHandler = new PlayerActionHandler(world);
            lookActionHandler = new LookActionHandler(world);
        }

        public void Handle(Player player, String command)
        {
            Network.Controller.Print("handling command: " + command);

            String[] splitCommand = command.Split(',');

            // hand off command handling to specialized classes
            switch (splitCommand[0])
            {
                case "LOGIN":
                    loginActionHandler.Handle(player, splitCommand);
                    return;
                case "MOVE":
                    moveActionHandler.Handle(player, splitCommand);
                    return;
                case "PLAYER":
                    playerActionHandler.Handle(player, splitCommand);
                    return;
                case "LOOK":
                    lookActionHandler.Handle(player, splitCommand);
                    return;
            }
        }
    }
}
