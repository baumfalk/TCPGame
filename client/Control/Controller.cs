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
            worldModel = new LocalModel(101,101,101);

            // connect to the world-server
            connection = new NetConnector(this);
        }

        // connect to the server
        public void Connect()
        {
            connection.Connect(IP, port);
        }

        // disconnect from the server
        public void Disconnect()
        {
            connection.Disconnect();
        }

        // handle input from the form. Usually this will just be passed on to the server
        public void SendInput(String input)
        {
            
            if (input.StartsWith("connect"))
            {
                String[] connectInfo = input.Split(' ');

                if (connectInfo.Length > 1) IP = connectInfo[1];
                if (connectInfo.Length > 2) port = int.Parse(connectInfo[2]);

                Connect();
            }
            else
            {
                connection.SendData(input);
            }
        }

        public void DoUpdate(List<String> updateData)
        {
            if (updateData.Contains("QUIT"))
            {
                Disconnect();

                tdView.Stop();
            }

            // update local model using the data sent
            worldModel.Update(updateData);

            // update view
            tdView.DrawModel(worldModel);
        }
    }
}
