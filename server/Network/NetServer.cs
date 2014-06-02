using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace TCPGameServer.Network
{
    public class NetServer
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

            if (!Network.Controller.headless) ServerOutputWindow.Print("server created at port " + port);
        }

        public void Start()
        {
            bRunning = true;

            if (!Network.Controller.headless) ServerOutputWindow.Print("server started");

            server.Start();

            startListening();
        }

        public void Stop()
        {
            if (!Network.Controller.headless) ServerOutputWindow.Print("server stopping");

            bRunning = false;
        }

        private void startListening()
        {
            if (!Network.Controller.headless) ServerOutputWindow.Print("starting listening for connections");

            if (bRunning)
            {
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

            if (!Network.Controller.headless) ServerOutputWindow.Print("connection made with IP " + newClient.Client.RemoteEndPoint.ToString());

            User newUser = new User(control, newClient);

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
