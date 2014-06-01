using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World;

namespace TCPGameServer.Server
{
    public class Controller
    {
        private List<User> users;

        private ServerOutputWindow outputWindow;

        private Model world;
        private Ticker ticker;
        private NetServer server;

        bool block;

        public Controller(ServerOutputWindow outputWindow)
        {
            this.outputWindow = outputWindow;

            users = new List<User>();

            world = new Model();

            ticker = new Ticker(this);

            server = new NetServer(this, 4502);

            server.Start();

            ticker.Start();

            block = false;
        }

        public void Tick()
        {
            // last tick hasn't finished, so skip the next one
            if (block)
            {
                outputMessage("block happened");
                return;
            }
            block = true;

            List<User> disconnectedUsers = new List<User>();

            world.doUpdate();

            foreach (User user in users)
            {
                if (user.isConnected())
                {
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

        public void outputMessage(String message)
        {
            outputWindow.addMessageToTextbox(message);
        }
    }
}
