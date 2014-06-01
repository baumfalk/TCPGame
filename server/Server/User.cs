using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.IO;

using TCPGameServer.World;

namespace TCPGameServer.Server
{
    public class User
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

            player.setCommandState(Player.COMMANDSTATE_LOGIN);

            addMessage("PING");
            addMessage("LOGIN,MESSAGE,please input your character name");
        }

        public bool isConnected()
        {
            return client.Connected;
        }

        public void addMessage(String message)
        {
            messageQueue.Enqueue(message);
        }

        public void sendMessages()
        {
            NetworkStream stream = client.GetStream();

            String message = MessageFormatting.formatCollection(messageQueue);

            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            ServerOutputWindow.onlyWindow.addMessageToTextbox("sending " + message + " to client at " + client.Client.RemoteEndPoint.ToString());

            {
                try
                {
                    stream.BeginWrite(messageInBytes, 0, messageInBytes.Length, messageSent, null);
                    
                }
                catch (IOException e)
                {
                    ServerOutputWindow.onlyWindow.addMessageToTextbox("exception trying to begin write to " + client.Client.RemoteEndPoint.ToString());
                    ServerOutputWindow.onlyWindow.addMessageToTextbox(e.Message);
                }
            }

            addMessage("PING");
        }

        private void messageSent(IAsyncResult sent)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                stream.EndWrite(sent);
            }
            catch (IOException e)
            {
                ServerOutputWindow.onlyWindow.addMessageToTextbox("exception trying to end write to " + client.Client.RemoteEndPoint.ToString());
                ServerOutputWindow.onlyWindow.addMessageToTextbox(e.Message);
            }
        }
    }
}
