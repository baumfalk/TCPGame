using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace TCPGameServer.Server
{
    class NetServer
    {
        private TcpListener server;

        private Controller control;

        int port;

        Boolean bRunning;

        public NetServer(Controller control, int port)
        {
            this.control = control;
            this.port = port;

            server = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            bRunning = true;

            startListening();
        }

        public void Stop()
        {
            bRunning = false;
        }

        private void startListening()
        {
            if (bRunning)
            {
                server.Start();

                server.BeginAcceptTcpClient(connectionMade, null);
            }
            else
            {
                server.Stop();
            }
        }

        private void connectionMade(IAsyncResult connection)
        {
            TcpClient newClient = server.EndAcceptTcpClient(connection);

            User newUser = new User(newClient);

            if (bRunning)
            {
                control.addUser(newUser);
                startListening();
            }
            else
            {
                server.Stop();
            }
        }
    }
}
