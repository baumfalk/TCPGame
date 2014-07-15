using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.Control.Output;
using TCPGameServer.World.Players;

namespace TCPGameServer.World.ActionHandling
{
    class ActionHandler
    {
        private Model model;

        private LoginActionHandler loginActionHandler;
        private MoveActionHandler moveActionHandler;
        private TeleportActionHandler teleportActionHandler;
        private LookActionHandler lookActionHandler;
        private MessageActionHandler messageActionHandler;
        private ResetActionHandler resetActionHandler;

        public ActionHandler(Model model)
        {
            this.model = model;

            loginActionHandler = new LoginActionHandler(model);
            moveActionHandler = new MoveActionHandler(model);
            teleportActionHandler = new TeleportActionHandler(model);
            lookActionHandler = new LookActionHandler(model);
            messageActionHandler = new MessageActionHandler(model);
            resetActionHandler = new ResetActionHandler(model);
        }

        public void Handle(Player player, String [] cmdAndParameters, int tick)
        {
            // hand off command handling to specialized classes
            switch (cmdAndParameters[0])
            {
                case "RESET":
                    resetActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "LOGIN":
                    loginActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "MOVE":
                    moveActionHandler.Handle(player, cmdAndParameters, tick);
                    return;
                case "TELEPORT":
                    teleportActionHandler.Handle(player, cmdAndParameters, tick);
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
