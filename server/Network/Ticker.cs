using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

using System.Diagnostics;

using TCPGameServer.World;

namespace TCPGameServer.Network
{
    class Ticker
    {
        private Controller control;

        private Timer tmTick;

        int tick;

        public Ticker(Controller control)
        {
            this.control = control;

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
            tick++;
            control.Tick(tick);
        }
    }
}
