using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

using TCPGameServer.World;
using TCPGameServer.World.Players;
using TCPGameServer.Network;

using TCPGameServer.Control.Output;

namespace TCPGameServer.Control
{
    public class Controller
    {
        //set to true to not use a output window
        public static bool headless;
        //when running is set to false, the program quits on the next tick
        private static bool running = true;

        // list of all connected users
        private List<User> users;

        // list of users to put in on the next tick
        private List<User> newUsers;

        // the model
        private Model model;
        // connects new clients to user objects
        private NetServer server;

        // flags to sync threads and disallow updates while another update is ongoing
        private bool timer_block = false;
        private bool update_block = false;
        private bool newuser_block = false;

        // number of ticks that have passed since game start
        int tick;

        public Controller(bool headless_mode)
        {
            Thread.CurrentThread.Name = "Main Thread";
            Thread UIThread = null;

            // set the headless flag
            headless = headless_mode;

            // if the application is not headless, create a UI-thread and run the window on it
            if (!headless)
            {
                UIThread = new Thread(OpenWindow);
                UIThread.Start();
            }

            // list of active users
            users = new List<User>();

            // list of users connected between ticks.
            newUsers = new List<User>();

            // the model
            model = new Model();

            // create the server
            server = new NetServer(this, 8888);

            // start listening for clients
            server.Start();

            // put the version number in the log
            Log.Print("Server version: " + Server.version);

            // start the Ticker on it's own thread
            new Thread(Ticker).Start();

            // wait while the running flag is set. To be safe, also check if the UI
            // thread is alive when headless.
            while (running && (headless || UIThread.IsAlive))
            {
                Thread.Sleep(1000);
            }

            // if not headless, shut down the window.
            if (!headless) ServerOutputWindow.Shutdown();
        }

        // create an output window
        private void OpenWindow()
        {
            Thread.CurrentThread.Name = "UI Thread";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerOutputWindow());
        }

        // stop the server on the next cycle
        public static void Stop()
        {
            running = false;
        }

        // register a player to the model. Usually done when a user has finished login.
        public void RegisterPlayer(Player player)
        {
            model.addPlayer(player);
        }
        // the list of players currently registered to the server
        public List<Player> GetRegisteredPlayers()
        {
            return model.getCopyOfPlayerList();
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
            Thread.CurrentThread.Name = "Ticker Thread";

            new Thread(Update).Start();

            // while running, we want to run updates
            while (running)
            {
                // increase the tick counter
                tick++;

                // wait 100ms
                Thread.Sleep(100);

                // tell the update it doesn't have to wait for the timer anymore
                timer_block = false;

                // wait until the last update is finished, if needed
                while (update_block && running)
                {
                    Thread.Sleep(1);
                }
                update_block = true;
            }
        }

        // update will signal other threads to not access data it's using with
        // the update_block flag. It will then, in order, handle user input,
        // update the world (from the game state plus the new input), send
        // output to the players, checking which ones of them have disconnected
        // since the last tick, and adding new users / removing disconnected
        // users. After that the flag is unset
        private void Update() {
            Thread.CurrentThread.Name = "Update Thread";

            while (running)
            {
                // handle input from the clients since the last tick
                HandleUserInput();

                // update the world from game state and input
                UpdateWorld();

                // send output, return disconnects
                List<User> disconnectedUsers = DoUserOutputAndReturnDisconnects();

                // add new users, disconnect users no longer in the game
                UpdateUsers(disconnectedUsers);

                // tell the timer it doesn't have to wait for the update to finish anymore
                update_block = false;
                
                // wait until the timer is finished, then set the flag again.
                while (timer_block && running)
                {
                    Thread.Sleep(1);
                }
                timer_block = true;
            }
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
            model.doUpdate(tick);
        }

        private string GetPlayerList()
        {
            string playerlist = "";
            foreach(User user in users)
            {
                playerlist += user.GetPlayerName() + ":" + user.GetPlayerLocation() +",";
            }
            if (playerlist == "") playerlist = " ";
            return playerlist.Substring(0,playerlist.Length-1);
        }

        // send output, and return which players are disconnected
        private List<User> DoUserOutputAndReturnDisconnects()
        {
            // a list of disconnected users. 
            List<User> disconnectedUsers = new List<User>();
            String playerListMessage = null;
            if ((tick % 10) == 0)
            {
                playerListMessage = GetPlayerList();
            }
            foreach (User user in users)
            {
                if (user.IsConnected())
                {
                    // we can only check if people are online if we send them data
                    // now and then.
                    if ((tick % 100) == 0)
                    {
                        user.AddMessage("PING", tick);
                        
                    }
                    if ((tick % 10) == 0)
                    {
                        user.AddMessage("WHOLIST," + playerListMessage, tick);
                    }
                    user.SendMessages(tick);
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
                user.Disconnect();
                users.Remove(user);
            }
            // clear the disconnected user list
            disconnectedUsers.Clear();
        }
    }
}
