using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace AngbandMud
{
    // miniservertje dat connections accepteert, player objects aanmaakt, en ze hun client passt.
    class TCPServer
    {
        // listener
        TcpListener tlListener;
        // 
        Boolean bRunning = false;
        public List<Player> pConnectedPlayers = new List<Player>();

        public void Start()
        {
            bRunning = true;

            Console.WriteLine("test");

            Thread t = new Thread(getClients);

            t.Start();
        }

        public void Stop()
        {
            foreach (Player pPlayer in pConnectedPlayers)
            {
                pPlayer.SendMessage("Server shutdown initiated...\r\n");

                pPlayer.Disconnect(false);
            }

            tlListener.Stop();
        }

        void getClients()
        {
            tlListener = new TcpListener(IPAddress.Any, 4502);

            tlListener.Start();

            while (bRunning)
            {
                try
                {
                    TcpClient tcClient = tlListener.AcceptTcpClient();

                    Player pNew = new Player(tcClient, this);

                    pConnectedPlayers.Add(pNew);
                }
                finally
                {
                    
                }
            }
        }
    }
}
