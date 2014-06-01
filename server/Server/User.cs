using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using TCPGameServer.World;

namespace TCPGameServer.Server
{
    class User
    {
        private TcpClient client;

        private Player player;

        private Queue<String> messageQueue;

        public User(TcpClient client)
        {
            this.client = client;

            messageQueue = new Queue<String>();

            Creature playerBody = new Creature("player");

            player = new Player(playerBody);

            player.addBlockingCommand("login");
            player.setCommandState(Player.COMMANDSTATE_LOGIN);
        }

        public void addMessage(String message)
        {
            messageQueue.Enqueue(message);
        }

        public bool hasMessages()
        {
            return (messageQueue.Count > 0);
        }

        public String getMessage()
        {
            if (hasMessages())
            {
                return messageQueue.Dequeue();
            }
            else
            {
                return "";
            }
        }
    }
}
