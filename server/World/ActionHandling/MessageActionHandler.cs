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
            Player foundPlayer = model.getCopyOfPlayerList().Find(x => x.GetName().Equals(splitCommand[1]));
            if (foundPlayer == default(Player) || foundPlayer.Equals(player)) return;

            foundPlayer.AddMessage("MESSAGE," + player.GetName() + " (private)," + splitCommand[2], tick);
            player.AddMessage("MESSAGE," + player.GetName() + " (private to " + foundPlayer.GetName() + ")," + splitCommand[2], tick);
        }
    }
}
