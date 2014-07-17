using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using TCPGameServer.Control;
using TCPGameServer.Control.Output;

namespace TCPGameServer.Network
{
    public class NetServer
    {
        // a listener
        private TcpListener server;
        // the port
        private int port;
        // when false, the server will stop
        private bool server_running;
        // the controller
        Controller control;

        // fills fields and creates the TCP Listener, doesn't start yet
        public NetServer(Controller control, int port)
        {
            this.control = control;

            this.port = port;

            server = new TcpListener(IPAddress.Any, port);

            Log.Print("server created at port " + port);
        }

        // starts the listener
        public void Start()
        {
            // set the flag
            server_running = true;

            // give output that we've started
            Log.Print("server started");

            // start the TCP listener
            server.Start();

            // start asynchronous listening procedure
            StartListening();
        }

        public void Stop()
        {
            // give output that we're stopping
            Log.Print("server stopping");

            // set the flag, the server will stop on it's next operation
            server_running = false;
        }

        private void StartListening()
        {
            // give output that we're listening
            Log.Print("starting listening for connections");

            // if running, try to accept a client, otherwise stop the server
            if (server_running)
            {
                server.BeginAcceptTcpClient(ConnectionMade, null);
            }
            else
            {
                server.Stop();
            }
        }

        // called when a connection to the server has been made
        private void ConnectionMade(IAsyncResult connection)
        {
            // accept the client
            TcpClient newClient = server.EndAcceptTcpClient(connection);

            // give output that a new connection has been made
            Log.Print("connection made with IP " + newClient.Client.RemoteEndPoint.ToString());

            // create a netclient which will maintain the link
            NetClient newNetClient = new NetClient(newClient);

            // create a user which will facilitate communication between the
            // different parts of the program with the netclient
            control.CreateUser(newNetClient);

            // if running, start listening for the next client. Otherwise, stop the server.
            if (server_running)
            {
                StartListening();
            }
            else
            {
                server.Stop();
            }
        }
    }
}
