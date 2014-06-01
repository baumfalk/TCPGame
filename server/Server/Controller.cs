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

        public Controller(ServerOutputWindow outputWindow)
        {
            this.outputWindow = outputWindow;

            world = new Model();

            ticker = new Ticker(this, world);

            server = new NetServer(this, 4502);
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
