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
        }

        private void HandleIdle(List<String> commands)
        {
            ServerOutputWindow.onlyWindow.addMessageToTextbox("user is idle");
        }

        private void HandleLogin(List<String> commands)
        {
            ServerOutputWindow.onlyWindow.addMessageToTextbox("user is logging in");

            if (commands[0].Equals("geerten")) player.addImmediateCommand("PLAYER,PLACE,4,4,0");
            else if (commands[0].Equals("jetze")) player.addImmediateCommand("PLAYER,PLACE,4,3,0");
            else player.addImmediateCommand("PLAYER,PLACE,2,2,0");

            player.addImmediateCommand("LOGIN,COMPLETE");
        }

        private void HandleNormal(List<String> commands) {
            foreach (String command in commands)
            {
                ServerOutputWindow.onlyWindow.addMessageToTextbox("user is in normal operation");

                bool isMovementCommand = (!(Directions.fromShortString(command).Equals("") || Directions.fromString(command).Equals("")));

                if (isMovementCommand)
                {
                    int direction = Directions.fromShortString(command);
                    if (direction == -1) direction = Directions.fromString(command);

                    player.addBlockingCommand("MOVE," + direction);
                }

                ServerOutputWindow.onlyWindow.addMessageToTextbox(command + " received from user");
            }
        }
    }
}