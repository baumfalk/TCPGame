using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World;

namespace TCPGameServer.Network.InputHandling
{
    class InputHandler
    {
        private Player player;

        public InputHandler(Player player)
        {
            this.player = player;
        }

        public void Handle(List<String> commands)
        {
            switch (player.getCommandState()) {
                case Player.COMMANDSTATE_IDLE:
                    HandleIdle(commands);
                    break;
                case Player.COMMANDSTATE_LOGIN:
                    HandleLogin(commands);
                    break;
                case Player.COMMANDSTATE_NORMAL:
                    HandleNormal(commands);
                    break;
            }

            if (commands[0].Equals("shutdown"))
            {
                Controller.running = false;
                if (!Controller.headless) ServerOutputWindow.Shutdown();
            }
            else if (commands[0].Equals("log"))
            {
                Network.Controller.Print("Sending log to user");

                foreach (string message in Controller.getLog())
                {
                    player.addMessage(message);
                }
            }
        }

        private void HandleIdle(List<String> commands)
        {
            Network.Controller.Print("user is idle but sending commands");
        }

        private void HandleLogin(List<String> commands)
        {
            if (commands.Count == 0) return;

            Network.Controller.Print("user is logging in");

            if (commands[0].Equals("geerten")) player.addImmediateCommand("PLAYER,PLACE,4,4,0");
            else if (commands[0].Equals("jetze")) player.addImmediateCommand("PLAYER,PLACE,4,3,0");
            else player.addImmediateCommand("PLAYER,PLACE,2,2,0");

            player.addImmediateCommand("LOGIN,COMPLETE");
        }

        private void HandleNormal(List<String> commands) {
            foreach (String command in commands)
            {
                Network.Controller.Print("user is in normal operation");

                bool isMovementCommand = (!(Directions.fromShortString(command).Equals("") || Directions.fromString(command).Equals("")));

                if (isMovementCommand)
                {
                    int direction = Directions.fromShortString(command);
                    if (direction == -1) direction = Directions.fromString(command);

                    player.addBlockingCommand("MOVE," + direction);
                }

                Network.Controller.Print(command + " received from user");
            }
        }
    }
}