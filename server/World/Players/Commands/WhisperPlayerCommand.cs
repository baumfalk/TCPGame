using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Players.Commands
{
    class WhisperPlayerCommand : PlayerCommand
    {
        private Player sender;
        private Player recipient;
        private String message;

        public WhisperPlayerCommand(Player sender, String recipient, Model model, String message)
        {
            this.sender = sender;
            this.recipient = model.getCopyOfPlayerList().Find(x => x.GetName().Equals(recipient));
            this.message = message;
        }

        public void Handle(int tick)
        {
            // if no target is found, tell the sender that the recipient is not online
            if (recipient == default(Player))
            {
                sender.AddMessage("MESSAGE,SERVER," + recipient + " is not online", tick);
            }
            else
            {
                // else send the private message
                recipient.AddMessage("MESSAGE," + sender.GetName() + " (private)," + message, tick);
                sender.AddMessage("MESSAGE," + sender.GetName() + " (private to " + recipient.GetName() + ")," + message, tick);
            }
        }
    }
}
