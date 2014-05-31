using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

using TCPGameClient.Control;

namespace TCPGameClient.Server
{
    // a player is a pair of a creature and a controller registered to the server
    public class Player
    {
        private Creature body;
        private Controller user;

        public Player(Creature body, Controller user)
        {
            this.body = body;
            this.user = user;
        }

        // controller can be requested, but not changed
        public Controller getUser()
        {
            return user;
        }

        // body can be requested, but not changed
        public Creature getBody()
        {
            return body;
        }
    }
}
