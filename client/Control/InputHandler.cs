﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using TCPGameClient.Model;

using TCPGameSharedInfo;

namespace TCPGameClient.Control
{
    class InputHandler
    {
        private Controller control;
        private LocalModel model;

        private bool sendingPassword;

        public InputHandler(Controller control, LocalModel model)
        {
            this.control = control;
            this.model = model;
        }

        // intercepts special input typed into the input textbox by the user,
        // returns a bool which indicates if the input should be sent to the
        // server.
        public String HandleUserInput(String input)
        {
            if (input.StartsWith("connect"))
            {
                String[] connectInfo = input.Split(' ');

                control.Connect(connectInfo);

                // don't send data, input has been handled
                return null;
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
                return null;
            }

            if (sendingPassword)
            {
                input = Hasher.ComputeHash(input);

                sendingPassword = false;
            }

            // no special commands found, so send the data to the server
            return input;
        }

        // handles input sent by the server and tells the model or the controller
        // how to act on the input. Returns true if the view should be redrawn.
        public bool HandleServerInput(List<String> inputData)
        {
            bool doRedraw = false;

            // loop through all strings we received
            foreach (String input in inputData)
            {
                if (input == "")
                    continue;
                // split the inputs
                String[] inputPart = input.Split(',');

                // switch on the type of input, let other methods handle them.
                switch (inputPart[1])
                {
                    case "QUIT":
                        control.Stop();
                        break;
                    case "MESSAGE":
                        model.AddReceivedMessage(input);
                        doRedraw = true;
                        break;
                    case "LOGIN":
                        ParseLoginCommand(inputPart);
                        break;
                    case "PLAYER":
                        ParsePlayerCommand(inputPart);
                        doRedraw = true;
                        break;
                    case "TILE":
                        UpdateTile(inputPart);
                        doRedraw = true;
                        break;
                    case "WHOLIST":
                        UpdateWholist(inputPart);
                        doRedraw = true;
                        break;
                    default:
                        System.Diagnostics.Debug.Print("NOT HANDLED: " + input);
                        break;
                }
            }

            return doRedraw;
        }

        // handles wholist updates
        private void UpdateWholist(String[] inputPart)
        {
            String playerName = inputPart[2];
            String areaName = inputPart[3];
            String stateChange = inputPart[4];

            model.updatePlayerList(playerName, areaName, stateChange);
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

        private void ParseLoginCommand(String[] inputPart)
        {
            if (inputPart[2].Equals("SALT"))
            {
                Hasher.saltBytes = Convert.FromBase64String(inputPart[3]);

                sendingPassword = true;
            }
        }

        // handles "tile" type updates. Only "detection" updates exist at the moment.
        private void UpdateTile(String[] inputPart)
        {
            if (inputPart[2].Equals("DETECTED"))
            {
                Debug.Print("detected got called");
            }

            if (inputPart[2].Equals("CHANGED"))
            {
                // get position and representation from the input
                int xPos = int.Parse(inputPart[3]);
                int yPos = int.Parse(inputPart[4]);
                int zPos = int.Parse(inputPart[5]);
                String tileRepresentationString = inputPart[6];
                String creatureString = inputPart[7];

                TileRepresentation tileRepresentation =
                    (TileRepresentation)Enum.Parse(typeof(TileRepresentation), tileRepresentationString);

                Creature creature = Creature.parse(creatureString);

                model.AddTile(xPos, yPos, zPos, tileRepresentation, creature);
            }
        }
    }
}
