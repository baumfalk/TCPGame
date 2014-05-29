using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace AngbandMud
{
    class Player
    {
        private TCPServer tsServer;

        public TcpClient tcClient;
        private NetworkStream nsClient;

        private String partialInput;

        public Player(TcpClient tcClient, TCPServer tsServer)
        {
            this.tsServer = tsServer;
            this.tcClient = tcClient;
            nsClient = tcClient.GetStream();
        }

        public String[] GetInput()
        {
            if (!nsClient.DataAvailable) return null;

            while (nsClient.DataAvailable)
            {
                byte[] byData = new byte[1024];

                nsClient.Read(byData, 0, 1024);

                partialInput += Encoding.ASCII.GetString(byData);
            }

            partialInput = partialInput.Replace("\r\n", "\n");

            String[] strInput = partialInput.Split('\n');

            if (partialInput.EndsWith("\n")) {
                partialInput = "";
                return strInput;
            }
            else {
                partialInput = strInput[strInput.Length - 1];

                String[] strToReturn = new String[strInput.Length - 1];

                for (int n = 0; n < strInput.Length - 1; n++)
                {
                    strToReturn[n] = strInput[n];
                }
                return strToReturn;
            }
        }

        public void SendMessage(String strMessage)
        {
            byte[] byMessage = Encoding.ASCII.GetBytes(strMessage);

            nsClient.Write(byMessage, 0, byMessage.Length);
        }

        public void Disconnect(bool selfInitiated)
        {
            if (selfInitiated) tsServer.pConnectedPlayers.Remove(this);

            nsClient.Flush();

            nsClient.Close();
        }
    }
}
