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
        public static bool headless = true;
        public static bool running = true;

        // list of all connected users
        private List<User> users;

        // list of users to put in on the next tick
        private List<User> newUsers;

        // the model
        private Model world;
        // a ticker which tells the server to update
        private Ticker ticker;
        // connects new clients to user objects
        private NetServer server;

        // flag to disallow updates while another update is ongoing
        bool block;

        public Controller()
        {
            users = new List<User>();
            newUsers = new List<User>();

            world = new Model();

            ticker = new Ticker(this);

            server = new NetServer(this, 8888);

            server.Start();

            ticker.Start();

            block = false;

            if (headless)
            {
                while (running)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public static void Print(string message)
        {
            if (!Network.Controller.headless) ServerOutputWindow.Print(message);
            else Console.WriteLine(message);
        }

        public void registerPlayer(Player player)
        {
            world.addPlayer(player);
        }

        public void Tick(int tick)
        {
            // last tick hasn't finished, so skip the next one
            if (block)
            {
                Controller.Print("Block happened");

                return;
            }
            block = true;

            List<User> disconnectedUsers = new List<User>();

            world.doUpdate();

            foreach (User user in users)
            {
                if (user.isConnected())
                {
                    if ((tick % 100) == 0) user.addMessage("PING (" + tick + ")");

                    user.sendMessages(tick);
                }
                else
                {
                    disconnectedUsers.Add(user);
                }
            }

            foreach(User user in disconnectedUsers) {
                users.Remove(user);
            }

            foreach (User newUser in newUsers)
            {
                users.Add(newUser);
            }
            newUsers.Clear();

            disconnectedUsers.Clear();

            block = false;
        }

        public void addUser(User newUser)
        {
            newUsers.Add(newUser);
        }

        public List<User> getUsers()
        {
            return users;
        }
    }
}
