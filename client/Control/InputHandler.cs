using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameClient.Model;

namespace TCPGameClient.Control
{
    class InputHandler
    {
        private Controller control;
        private LocalModel model;

        public InputHandler(Controller control, LocalModel model)
        {
            this.control = control;
            this.model = model;
        }

        // intercepts special input typed into the input textbox by the user,
        // returns a bool which indicates if the input should be sent to the
        // server.
        public bool HandleUserInput(String input)
        {
            if (input.StartsWith("connect"))
            {
                String[] connectInfo = input.Split(' ');

                control.Connect(connectInfo);

                // don't send data, input has been handled
                return false;
            } // intercept a zoom command
            else if (input.StartsWith("zoom"))
            {
                // default values
                int zoomLevelX = -1;
                int zoomLevelY = -1;

                String[] zoomInfo = input.Split(' ');

                // try to parse the rest of the info to integers
                if (zoomInfo.Length > 1) int.TryParse(zoomInfo[1], out zoomLevelX);
                if (zoomInfo.Length > 2) int.TryParse(zoomInfo[2], out zoomLevelY);

                // if a value hasn't been set, use 64. Don't accept values above 128 or below 16
                if (zoomLevelX == -1) zoomLevelX = 64;
                if (zoomLevelX < 16) zoomLevelX = 16;
                if (zoomLevelX > 128) zoomLevelX = 128;

                if (zoomLevelY == -1) zoomLevelY = zoomLevelX;
                if (zoomLevelY < 16) zoomLevelY = 16;
                if (zoomLevelY > 128) zoomLevelY = 128;

                // set the new zoom level
                control.SetZoom(zoomLevelX, zoomLevelY);

                // redraw the model
                control.Redraw();

                // don't send data, input has been handled
                return false;
            }

            // no special commands found, so send the data to the server
            return true;
        }

        // handles input sent by the server and tells the model or the controller
        // how to act on the input. Returns true if the view should be redrawn.
        public bool HandleServerInput(List<String> inputData)
        {
            bool DoRedraw = false;

            int tick = 0;

            // loop through all strings we received
            foreach (String input in inputData)
            {
                // split the inputs
                String[] inputPart = input.Split(',');

                // creature information is complete per tick. If we received
                // multiple ticks, reset creatures when handling later ones.
                if (tick != int.Parse(inputPart[0]))
                {
                    tick = int.Parse(inputPart[0]);
                    model.ResetCreatures();
                }

                // switch on the type of input, let other methods handle them.
                switch (inputPart[1])
                {
                    case "QUIT":
                        control.Stop();
                        break;
                    case "MESSAGE":
                        control.AddMessage(input);
                        break;
                    case "LOGIN":
                        break;
                    case "PLAYER":
                        ParsePlayerCommand(inputPart);
                        DoRedraw = true;
                        break;
                    case "TILE":
                        UpdateTile(inputPart);
                        DoRedraw = true;
                        break;
                    case "CREATURE":
                        UpdateCreature(inputPart);
                        DoRedraw = true;
                        break;
                    default:

                        break;
                }
            }

            return DoRedraw;
        }

        // handles player-type updates. Only "position" update exists at the moment.
        private void ParsePlayerCommand(String[] inputPart)
        {
            if (inputPart[2].Equals("POSITION"))
            {
                int newX = int.Parse(inputPart[3]);
                int newY = int.Parse(inputPart[4]);
                int newZ = int.Parse(inputPart[5]);

                // shift the map (which is player-centered)
                model.ShiftMap(newX, newY, newZ);
            }
        }

        // handles "creature" type updates. Only "detection" updates exist at the moment.
        private void UpdateCreature(String[] inputPart)
        {
            if (inputPart[2].Equals("DETECTED"))
            {
                // get position and representation from the input
                int xPos = int.Parse(inputPart[3]);
                int yPos = int.Parse(inputPart[4]);
                int zPos = int.Parse(inputPart[5]);
                String representation = inputPart[6];

                // add creature to the model
                model.AddCreature(xPos, yPos, zPos, representation);
            }
        }

        // handles "tile" type updates. Only "detection" updates exist at the moment.
        private void UpdateTile(String[] inputPart)
        {
            if (inputPart[2].Equals("DETECTED"))
            {
                // get position and representation from the input
                int xPos = int.Parse(inputPart[3]);
                int yPos = int.Parse(inputPart[4]);
                int zPos = int.Parse(inputPart[5]);
                String representation = inputPart[6];

                model.AddTile(xPos, yPos, zPos, representation);
            }
        }
    }
}
