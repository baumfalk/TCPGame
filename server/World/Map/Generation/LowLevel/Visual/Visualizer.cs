using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.Generation.LowLevel.Cave;

using TCPGameServer.World.Map.Generation.LowLevel.Connections;
using TCPGameServer.World.Map.Generation.LowLevel.Values;
using TCPGameServer.World.Map.Generation.LowLevel.Tiles;


using System.Windows.Forms;
using System.Threading;

namespace TCPGameServer.World.Map.Generation.LowLevel.Visual
{
    public class Visualizer : Cave_TunnelGenerator
    {
        private bool doStep;
        private bool loaded;
        private frmVisualizer form;

        public Visualizer(GeneratorData generatorData) : 
            base(generatorData)
        {
            new Thread(OpenWindow).Start();
        }

        // create an output window
        private void OpenWindow()
        {
            form = new frmVisualizer(this, connectionmap, valuemap);

            Application.Run(form);
        }

        public void takeStep()
        {
            doStep = true;
        }

        public void indicateLoaded()
        {
            loaded = true;
        }

        protected override void DoBeforeExpansion()
        {
            while (!loaded)
            {
                Thread.Sleep(100);
            }

            base.DoBeforeExpansion();
        }

        protected override void DoAtExpansionLoopEnd(Partition partition, Location pointAdded)
        {
            base.DoAtExpansionLoopEnd(partition, pointAdded);

            doStep = false;

            form.DoUpdate(pointAdded);

            while (!doStep)
            {
                Thread.Sleep(100);
            }
        }
    }
}
