using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

using System.Diagnostics;

using TCPGameServer.World;

namespace TCPGameServer.Server
{
    class Ticker
    {
        private Controller control;
        private Model world;

        private Timer tmTick;

        public Ticker(Controller control, Model world)
        {
            this.control = control;
            this.world = world;

            tmTick = new Timer(100);
            tmTick.Elapsed += tmTick_Elapsed;
        }

        public void Start()
        {
            tmTick.Start();
        }

        public void Stop()
        {
            tmTick.Stop();
        }

        void tmTick_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (User user in control.getUsers())
            {
                
            }

            world.doUpdate();
        }
    }
}
