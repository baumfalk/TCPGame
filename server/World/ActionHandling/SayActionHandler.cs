using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.ActionHandling
{
    class SayActionHandler
    {
        private Model model;
        public SayActionHandler(Model model)
        {
            this.model = model;
        }
        public void Handle(Player player, String[] splitCommand)
        {
            // we need a recipient and a message (first index just contains "say")
            if (!splitCommand[0].Equals("SAY")) return;
            if (splitCommand.Length < 3)  return;
            switch (splitCommand[1])
            {
                // send to everybody except the sender
                case "ALL":
                    foreach(Player otherPlayer in model.getPlayers()) {                     
                        otherPlayer.AddMessage("MESSAGE_FROM,"+player.GetName()+","+splitCommand[2]);
                    }
                    return;
                // try to send to specific player (cannot be self)
                default:
                    Player foundPlayer = model.getPlayers().Find(x => x.GetName().Equals(splitCommand[1]));
                    if (foundPlayer == null || foundPlayer.Equals(player)) 
                        return;

                    foundPlayer.AddMessage("MESSAGE_FROM," + player.GetName() + " (private)," + splitCommand[2]);
                    player.AddMessage("MESSAGE_FROM," + player.GetName() + " (private to "+foundPlayer.GetName()+")," + splitCommand[2]);
                    return;
            }
        }
    }
}
