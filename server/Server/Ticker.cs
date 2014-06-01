using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

namespace TCPGameServer.Server
{
    class Ticker
    {
        private Timer tmTick;

        private int numCommand;

        public Ticker()
        {
            tmTick = new Timer(100);
            tmTick.Elapsed += tmTick_Elapsed;
        }

        void tmTick_Elapsed(object sender, ElapsedEventArgs e)
        {
            numCommand = 0;

            // vraag input;
            List<String> outputData = handleInput(user.getInput());

            // sorteer. Zou goed moeten zijn, maar better safe dan sorry.
            // TODO: beter sorteren
            outputData.Sort();

            // geef output;
            user.doUpdate(outputData);
        }
    }
}
