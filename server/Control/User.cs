using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Network;
using TCPGameServer.World;
using TCPGameServer.World.Players;
using TCPGameServer.World.Creatures;
using TCPGameServer.Control.Output;
using TCPGameServer.Control.Input;

using TCPGameSharedInfo;

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
        // data that needs to be saved during login
        private LoginInfo loginInfo;
        // a queue of messages to send to the client
        private Queue<String> messages;

        // login state which shows at which point of the login process we are
        private LoginState loginState = LoginState.NotStarted;
        public enum LoginState { NotStarted, Login, Password, Finished };

        // creates a creature for the player to control and starts the login process
        public User(Controller control, NetClient client)
        {
            // fill fields
            this.control = control;
            this.client = client;

            // start the login procedure
            loginInfo = new LoginInfo();

            // create the message queue
            messages = new Queue<String>();

            SetLoginState(LoginState.Login);
            AddMessage("MESSAGE,LOGIN,please input your character name", int.MinValue);
        }

        public void SetLoginState(LoginState loginState)
        {
            this.loginState = loginState;
        }
        public LoginState GetLoginState()
        {
            return loginState;
        }

        public LoginInfo GetLoginInfo()
        {
            return loginInfo;
        }

        public void CompleteLogin()
        {
            // get a new body, or the body of a player already logged in with the same name.
            Creature playerBody = GetBodyOf(loginInfo.name);

            // create the player with that body
            player = new Player(this, playerBody);

            // set the player's name
            player.SetName(loginInfo.name);

            // register the player with the world (via the controller)
            control.RegisterPlayer(player);

            // place the player in the world if he hasn't taken over an existing body
            if (player.GetBody().GetPosition() == null)
            {
                player.AddImmediateCommand(new String[] { "TELEPORT", loginInfo.areaName, loginInfo.tileIndex });
            }
            else
            {
                player.AddImmediateCommand(new String[] { "LOOK", "TILES_INCLUDED", "PLAYER_INCLUDED", "UPDATE_ALL" });
            }
            player.AddImmediateCommand(new String[] { "LOGIN", "COMPLETE" });
            
            // we're done with the login process
            loginState = LoginState.Finished;
        }

        private Creature GetBodyOf(String name)
        {
            List<Player> RegisteredPlayers = control.GetRegisteredPlayers();

            // check if someone by this name is already online, and if so, take over his body
            for (int n = 0; n < RegisteredPlayers.Count; n++)
            {
                if (RegisteredPlayers[n].GetName().Equals(name))
                {
                    RegisteredPlayers[n].Disconnect(true);
                    return RegisteredPlayers[n].GetBody();
                }
            }

            // create a body for the player
            return new Creature(CreatureRepresentation.Player);
        }

        // removing the user tells the player and the client to remove themselves
        public void Disconnect()
        {
            Queue<String> quitQueue = new Queue<String>();
            quitQueue.Enqueue("0,QUIT");

            client.SendMessages(quitQueue);

            if (player != null && !player.IsDisconnected()) player.Disconnect(false);
            client.Disconnect();
        }

        // a user is connected if it's netclient is connected
        public bool IsConnected()
        {
            return client.IsConnected();
        }

        // messages are sent across the connection to the player
        public bool HasMessages()
        {
            return (messages.Count > 0);
        }
        public void AddMessage(String message, int tick)
        {
            messages.Enqueue(tick + "," + message);
        }
        public Queue<String> GetMessages()
        {
            return messages;
        }

        // simply passes the message queue to the client
        public void SendMessages(int tick)
        {
            client.SendMessages(messages);
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
