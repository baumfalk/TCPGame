using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.General;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections.Expansion
{
    public class Front
    {
        Partition ID;

        private int height;
        private int width;

        private PriorityQueue<Location> tileFront;
        private bool[,] isInFront;

        public delegate int WeightFunction(Partition partition, Location location);
        private WeightFunction GetWeight;

        public Front(int width, int height, Partition ID, WeightFunction GetWeight)
        {
            this.width = width;
            this.height = height;

            this.ID = ID;
            this.GetWeight = GetWeight;

            tileFront = new PriorityQueue<Location>();
            isInFront = new bool[width, height];
        }

        public void AddToFront(Location location)
        {
            location = MapGridHelper.TileLocationToCurrentMapLocation(location);

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

            isInFront = new bool[width, height];

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

        public bool HasNext()
        {
            return (tileFront.Count() > 0);
        }

        public Location GetNext()
        {
            return tileFront.RemoveMin();
        }

        public int Count()
        {
            return tileFront.Count();
        }
    }
}
