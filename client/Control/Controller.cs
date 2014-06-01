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

        public Controller(TileDisplayForm tdView)
        {
            // the view will be passed to the controller
            this.tdView = tdView;

            // we need to locally model the world to display it
            worldModel = new LocalModel(101,101,101);

            // connect to the world-server
            connection = new NetConnector(this, "127.0.0.1", 4502);
        }

        // connect to the server
        public void Connect()
        {
            connection.Connect();
        }

        // disconnect from the server
        public void Disconnect()
        {
            connection.Disconnect();
        }

        // server will ask for our input every tick
        public List<String> getInput()
        {
            return tdView.getInput();
        }

        // and server will send updates every tick
        public void doUpdate(List<String> updateData)
        {
            // update local model using the data sent
            worldModel.update(updateData);

            // update view
            tdView.drawModel(worldModel);
        }
    }
}
