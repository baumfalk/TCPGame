using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.ActionHandling
{
    class MessageActionHandler
    {
        private Model model;
        public MessageActionHandler(Model model)
        {
            this.model = model;
        }
        public void Handle(Player player, String[] splitCommand, int tick)
        {
            if (splitCommand[0].Equals("SAY")) HandleSayCommand(player, splitCommand, tick);
            else if (splitCommand[0].Equals("WHISPER")) HandleWhisperCommand(player, splitCommand, tick);
        }

        private void HandleSayCommand(Player player, String[] splitCommand, int tick)
        {
            String name = (player == null) ? "" : player.GetName();

            foreach (Player otherPlayer in model.getCopyOfPlayerList())
            {
                otherPlayer.AddMessage("MESSAGE," + name + "," + splitCommand[1], tick);
            }
        }

        private void HandleWhisperCommand(Player player, String[] splitCommand, int tick)
        {
            // better readability
            String sender = player.GetName();
            String recipient = splitCommand[1];
            String message = splitCommand[2];

            // find a player with the correct name
            Player foundPlayer = model.getCopyOfPlayerList().Find(x => x.GetName().Equals(recipient));
            
            // if not found, tell the sender that the player is not online
            if (foundPlayer == default(Player))
            {
                player.AddMessage("MESSAGE,," + recipient + " is not online", tick);
                return;
            }

            // else send the private message
            foundPlayer.AddMessage("MESSAGE," + sender + " (private)," + message, tick);
            player.AddMessage("MESSAGE," + sender + " (private to " + recipient + ")," + message, tick);
        }
    }
}
