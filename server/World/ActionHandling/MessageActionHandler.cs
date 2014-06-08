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
            if (splitCommand[0].Equals("SAY"))
                HandleSayCommand(player, splitCommand, tick);
            else if (splitCommand[0].Equals("WHISPER"))
                HandleWhisperCommand(player,splitCommand,tick);
            else 
                return;
        }

        private void HandleSayCommand(Player player, String[] splitCommand, int tick)
        {
            foreach (Player otherPlayer in model.getCopyOfPlayerList())
            {
                otherPlayer.AddMessage("MESSAGE_FROM," + player.GetName() + "," + splitCommand[1], tick);
            }
        }

        private void HandleWhisperCommand(Player player, String[] splitCommand, int tick)
        {
            Player foundPlayer = model.getCopyOfPlayerList().Find(x => x.GetName().Equals(splitCommand[1]));
            if (foundPlayer == null || foundPlayer.Equals(player))
                return;

            foundPlayer.AddMessage("MESSAGE_FROM," + player.GetName() + " (private)," + splitCommand[2], tick);
            player.AddMessage("MESSAGE_FROM," + player.GetName() + " (private to " + foundPlayer.GetName() + ")," + splitCommand[2], tick);
        }
    }
}
