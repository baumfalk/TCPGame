using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Players.Commands
{
    class SayPlayerCommand : PlayerCommand
    {
        private Player player;
        private Model model;
        private String message;

        public SayPlayerCommand(Player player, Model model, String message, bool isEmote)
        {
            this.model = model;
            this.message = message;

            // an emote is of the form "<name> has logged in", and shows as a server message
            if (isEmote)
            {
                message = player.GetName() + " " + message;
            }
            else this.player = player;
        }

        public void Handle(int tick)
        {
            // if no name is supplied, this is a server message
            String name = (player == null) ? "SERVER" : player.GetName();

            foreach (Player otherplayer in model.getCopyOfPlayerList())
            {
                otherplayer.AddMessage("MESSAGE," + name + "," + message, tick);
            }
        }
    }
}
