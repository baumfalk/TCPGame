using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Network;
using TCPGameServer.World;
using TCPGameServer.Control.IO;

namespace TCPGameServer.Control
{
    public class User
    {
        // the player in the model this user represents
        private Player player;
        // the client on the network this user is using to connect
        private NetClient client;
        // the controller
        private Controller control;

        // creates a creature for the player to control and starts the login process
        public User(Controller control, NetClient client)
        {
            // fill fields
            this.control = control;
            this.client = client;

            // create a body for the player
            Creature playerBody = new Creature("player");

            // create the player with that body
            player = new Player(playerBody);

            // register the player with the world (via the controller)
            control.RegisterPlayer(player);

            // start the login procedure
            player.SetCommandState(Player.COMMANDSTATE_LOGIN);
            AddMessage("LOGIN,MESSAGE,please input your character name");
        }

        // removing the user tells the player and the client to remove themselves
        public void Remove()
        {
            player.Remove();
            client.Remove();
        }

        // a user is connected if it's netclient is connected
        public bool IsConnected()
        {
            return client.IsConnected();
        }

        // disconnecting a user disconnects the client, and tells the player it's
        // been disconnected.
        public void Disconnect()
        {
            player.SetDisconnected(true);

            client.Disconnect();
        }

        // messages from the server could be kept in a separate list, but for now we
        // just add them to the messages the player object maintains.
        public void AddMessage(String message)
        {
            player.AddMessage(message);
        }

        // get the message queue the player object maintains, and pass it to the client
        public void SendMessages(int tick)
        {
            Queue<String> messages = player.GetMessages();

            client.SendMessages(messages, tick);
        }

        // data should be sent with separate lines separated by semicolons. It will be passed
        // to the controller as a list.
        public void HandleInput()
        {
            List<String> inputList = client.GetInput();

            // if there is no full command, don't try to parse it
            if (inputList.Count == 0) return;

            // send the updates to the input handler
            InputHandler.Handle(inputList, player, this);
        }
    }
}
