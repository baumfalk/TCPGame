using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World;

namespace TCPGameServer.Network
{
    public class Controller
    {
        // list of all connected users
        private List<User> users;

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

            world = new Model();

            ticker = new Ticker(this);

            server = new NetServer(this, 4502);

            server.Start();

            ticker.Start();

            block = false;
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
                ServerOutputWindow.Print("block happened");
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

                    user.sendMessages();
                }
                else
                {
                    disconnectedUsers.Add(user);
                }
            }

            foreach(User user in disconnectedUsers) {
                users.Remove(user);
            }

            disconnectedUsers.Clear();

            block = false;
        }

        public void addUser(User newUser)
        {
            users.Add(newUser);
        }

        public List<User> getUsers()
        {
            return users;
        }
    }
}
