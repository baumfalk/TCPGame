using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.General;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections.Expansion
{
    class Front
    {
        Partition ID;

        private PriorityQueue<Location> tileFront;
        private bool[,] isInFront;

        delegate int WeightFunction(Partition partition, Location location);
        private WeightFunction GetWeight;

        public Front(Partition ID, WeightFunction GetWeight)
        {
            this.ID = ID;
            this.GetWeight = GetWeight;
        }

        public void AddToFront(Location location)
        {
            int x = location.x;
            int y = location.y;
            int z = location.z;

            if (!isInFront[x, y])
            {
                isInFront[x, y] = true;

                int weight = GetWeight(ID, location);

                tileFront.Add(weight, location);
            }
        }

        public void Merge(Front mergee)
        {
            tileFront.Merge(mergee.GetPriorityQueue());
        }

        public void RecalculateWeights()
        {
            PriorityQueue<Location> bufferQueue = new PriorityQueue<Location>();

            isInFront = new bool[100,100];

            while (tileFront.Count() > 0)
            {
                Location location = tileFront.RemoveMin();

                int x = location.x;
                int y = location.y;

                if (!isInFront[x,y])
                {
                    bufferQueue.Add(GetWeight(ID, location), location);
                    isInFront[x, y] = true;
                }
            }

            tileFront = bufferQueue;
        }

        private PriorityQueue<Location> GetPriorityQueue()
        {
            return tileFront;
        }

        public Location GetNext()
        {
            return tileFront.RemoveMin();
        }
    }
}
