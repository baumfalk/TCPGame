using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Diagnostics;

using TCPGameClient.View;
using TCPGameClient.Model;

namespace TCPGameClient.Control
{
    public class Controller
    {
        // view is the display form
        private TileDisplayForm tdView;
        // the local model
        private LocalModel worldModel;
        // the inputhandler
        private InputHandler inputHandler;
        // the network connector which communicates with the server
        private NetConnector connection;

        // default server is Jetze's server
        private String IP = "145.97.240.63";
        private int port = 8888;

        public Controller(TileDisplayForm tdView)
        {
            // the view will be passed to the controller
            this.tdView = tdView;

            // we need to locally model the world to display it
            worldModel = new LocalModel(51, 51, 3);

            inputHandler = new InputHandler(this, worldModel);

            // connect to the world-server
            connection = new NetConnector(this);
        }

        // connect to the server
        public void Connect(String[] connectInfo)
        {
            if (connectInfo.Length > 1) IP = connectInfo[1];
            if (connectInfo.Length > 2) port = int.Parse(connectInfo[2]);

            connection.Connect(IP, port);
        }

        // disconnect from the server
        public void Disconnect()
        {
            connection.Disconnect();
        }

        // ends the program
        public void Stop()
        {
            Disconnect();

            tdView.Stop();
        }

        // handle input from the form. Usually this will just be passed on to the server
        public void SendInput(String input)
        {
            // let the inputhandler check for special commands that can be handled
            // client-side (such as "connect"). Returns true if nothing has been intercepted
            // and the data should be sent to the server.
            String parsedInput = inputHandler.HandleUserInput(input);

            // if we have to send, send.
            if (parsedInput != null) connection.SendData(parsedInput);
        }

        public void DoUpdate(List<String> updateData)
        {
            // let the inputhandler handle the data sent by the server. Returns true if
            // a redraw is warranted.
            bool doRedraw = inputHandler.HandleServerInput(updateData);
                
            // update view
            if (doRedraw) Redraw();
        }

        public void SetZoom(int sizeX, int sizeY)
        {
            tdView.SetZoom(sizeX, sizeY);
        }

        // redraw the view
        public void Redraw()
        {
            tdView.DrawModel(worldModel);
        }

        public void AddMessage(String message)
        {
            worldModel.AddReceivedMessage(message);
            tdView.DrawMessages(worldModel);
            System.Threading.Thread.Sleep(10);
        }
    }
}
