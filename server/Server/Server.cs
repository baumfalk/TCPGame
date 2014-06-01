using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.Server
{
    public class Server
    {
        private List<User> users;

        public List<User> getUsers()
        {
            return users;
        }
    }
}
