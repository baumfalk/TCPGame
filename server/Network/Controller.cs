using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using TCPGameServer.World;

namespace TCPGameServer.Network
{
    public class Controller
    {
        //set to true to not use a output window
        public static bool headless = false;
        //when running is set to false, the headless version quits
        public static bool running = true;

        // list of all connected users
        private List<User> users;

        // list of users to put in on the next tick
        private List<User> newUsers;

        // the model
        private Model world;
        // connects new clients to user objects
        private NetServer server;

        // flags to disallow updates while another update is ongoing
        private bool update_block = false;
        private bool newuser_block = false;

        // number of ticks that have passed since game start
        int tick;

        public Controller()
        {
            // list of active users
            users = new List<User>();

            // list of users connected between ticks.
            newUsers = new List<User>();

            // the model
            world = new Model();

            // create the server
            server = new NetServer(this, 8888);

            // start listening for clients
            server.Start();

            // start the Ticker on it's own thread
            new Thread(Ticker).Start();

            // if the program is running headless, this is the startup thread. The program
            // will end when it does, so we sleep the thread until the running flag is set
            // to false.
            while (headless && running)
            {
                Thread.Sleep(1000);
            }
        }

        // output to the window, or to the debug stream if headless
        public static void Print(string message)
        {
            if (!Network.Controller.headless) ServerOutputWindow.Print(message);
            else Console.WriteLine(message);
        }

        // register a player to the model. Usually done when a user has finished login.
        // a player is an object in the model, connecting it to the users on the server.
        // a user is a client on the server, connected to a player in the model.
        public void registerPlayer(Player player)
        {
            world.addPlayer(player);
        }

        // add a new user. Won't alter the new user list while it's being worked on.
        public void AddUser(User newUser)
        {
            while (newuser_block)
            {
                Thread.Sleep(1);
            }
            newUsers.Add(newUser);
        }

        // ticker handling when to update everything
        private void Ticker()
        {
            // while running, we want to run updates
            while (running)
            {
                // increase the tick counter
                tick++;

                // wait 100ms
                Thread.Sleep(100);

                // wait until the last update is finished, if needed
                while (update_block)
                {
                    Thread.Sleep(1);
                }

                // run the update on a different thread, so the ticker doesn't
                // get blocked
                new Thread(update).Start();
            }
        }

        // update will signal other threads to not access data it's using with
        // the update_block flag. It will then, in order, handle user input,
        // update the world (from the game state plus the new input), send
        // output to the players, checking which ones of them have disconnected
        // since the last tick, and adding new users / removing disconnected
        // users. After that the flag is unset
        private void update() {
            // set the blocking flag
            update_block = true;

            // handle input from the clients since the last tick
            HandleUserInput();

            // update the world from game state and input
            UpdateWorld();

            // send output, return disconnects
            List<User> disconnectedUsers = DoUserOutputAndReturnDisconnects();

            // add new users, disconnect users no longer in the game
            UpdateUsers(disconnectedUsers);

            // unset the blocking flag
            update_block = false;
        }

        // get each user to handle it's input and put it in the correct command
        // queues
        private void HandleUserInput()
        {
            foreach (User user in users)
            {
                user.HandleInput();
            }
        }

        // ask the model to update
        private void UpdateWorld() {
            world.doUpdate();
        }

        // send output, and return which players are disconnected
        private List<User> DoUserOutputAndReturnDisconnects()
        {
            // a list of disconnected users. 
            List<User> disconnectedUsers = new List<User>();

            foreach (User user in users)
            {
                if (user.isConnected())
                {
                    // we can only check if people are online if we send them data
                    // now and then.
                    if ((tick % 100) == 0) user.addMessage("PING (" + tick + ")");

                    user.sendMessages(tick);
                }
                else
                {
                    disconnectedUsers.Add(user);
                }
            }

            return disconnectedUsers;
        }

        // add new users, disconnect disconnected users passed to the method
        private void UpdateUsers(List<User> disconnectedUsers) {
            // set a flag blocking the new user list, signaling AddUser to
            // wait updating this list
            newuser_block = true;

            // add each new user to the user list
            foreach (User newUser in newUsers)
            {
                users.Add(newUser);
            }
            // clear the new user list
            newUsers.Clear();

            // unset the blocking flag
            newuser_block = false;

            // remove each user that has disconnected from the list, also
            // send the user object notification for cleanup.
            foreach(User user in disconnectedUsers) {
                user.Remove();
                users.Remove(user);
            }
            // clear the disconnected user list
            disconnectedUsers.Clear();
        }
    }
}
