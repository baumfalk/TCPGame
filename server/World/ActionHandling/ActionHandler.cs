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
        private MessageActionHandler messageActionHandler;

        public ActionHandler(Model model)
        {
            this.model = model;

            loginActionHandler = new LoginActionHandler(model);
            moveActionHandler = new MoveActionHandler(model);
            playerActionHandler = new PlayerActionHandler(model);
            lookActionHandler = new LookActionHandler(model);
            messageActionHandler = new MessageActionHandler(model);

        }

        public void Handle(Player player, String [] cmdAndParameters, int tick)
        {
            // hand off command handling to specialized classes
            switch (cmdAndParameters[0])
            {
                case "LOGIN":
                    loginActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "MOVE":
                    moveActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "PLAYER":
                    playerActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "LOOK":
                    lookActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "SAY":
                case "WHISPER":
                    messageActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "DELAY":
                    return;
            }
        }
    }
}
