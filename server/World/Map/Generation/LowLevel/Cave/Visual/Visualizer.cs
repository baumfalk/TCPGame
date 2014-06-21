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

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave.Visual
{
    public class Visualizer : CaveGenerator
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
            form = new frmVisualizer(this);

            Application.Run(form);
        }

        public Connectionmap getConnectionmap()
        {
            return connectionmap;
        }

        public Valuemap getValuemap()
        {
            return valuemap;
        }

        public Tilemap getTilemap()
        {
            return tilemap;
        }

        public void takeStep()
        {
            doStep = true;
        }

        public void indicatedLoaded()
        {
            loaded = true;
        }

        protected override void ExpandUntilFinishedConditionMet()
        {
            while (!loaded)
            {
                Thread.Sleep(100);
            }

            while (!GetFinishedCondition())
            {
                Partition partition = connectionmap.GetNext();
                Location pointAdded = Expand(partition, "floor", "floor", true);

                // update the connectionmap based on placement of this partition on this location
                connectionmap.Place(partition, pointAdded);

                doStep = false;

                form.DoUpdate(pointAdded);

                while (!doStep)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
