﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Diagnostics;

using TCPGameClient.View;
using TCPGameClient.Model;

using TCPGameClient.Server;

namespace TCPGameClient.Control
{
    class Controller
    {
        private TileDisplayForm tdView;
        private LocalModel worldModel;

        public Controller(TileDisplayForm tdView)
        {
            // the view will be passed to the controller
            this.tdView = tdView;

            // we need to locally model the world to display it
            worldModel = new LocalModel(21,21);

            // connect to the world-server
            World theWorld = new World();

            // register with the server to start getting updates
            theWorld.registerUser(this);
        }

        // server will ask for our input every tick
        public List<String> getInput()
        {
            Debug.Print("getting input");

            return tdView.getInput();
        }

        // and server will send updates every tick
        public void doUpdate(List<String> updateData)
        {
            Debug.Print("doing update (size " + updateData.Count + ")");

            // update local model using the data sent
            worldModel.update(updateData);

            // update view
            tdView.drawModel(worldModel);
        }
    }
}
