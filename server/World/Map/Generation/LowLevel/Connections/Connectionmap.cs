using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections
{
    class Connectionmap
    {
        private Queue<Partition> toConnect;
        private Partition[,] connectedBy;

        private Partition fixedPartition;

        public Connectionmap(int width, int height, TileBlockData entrances, TileBlockData[] fixedTiles)
        {
            // create map
            toConnect = new Queue<Partition>();
            connectedBy = new Partition[width, height];

            // add entrances to map, and to connection stuff
            for (int n = 0; n < entrances.numberOfTiles; n++)
            {
                Location entranceLocation = entrances.tileData[n].location;

                Partition entrancePartition = new Partition();
                entrancePartition.index = n;
                entrancePartition.isFixed = false;

                connectedBy[entranceLocation.x, entranceLocation.y] = entrancePartition;

                toConnect.Enqueue(entrancePartition);
            }

            // set the partition for fixed tiles
            fixedPartition = new Partition();
            fixedPartition.index = -1;
            fixedPartition.isFixed = true;

            // add the fixed tiles to the fixed partition
            for (int n = 0; n < fixedTiles.Length; n++)
            {
                for (int i = 0; i < fixedTiles[n].numberOfTiles; i++)
                {
                    Location fixedTileLocation = fixedTiles[n].tileData[i].location;

                    connectedBy[fixedTileLocation.x, fixedTileLocation.y] = fixedPartition;
                }
            }
        }

        // returns the index of the next partition to be expanded
        public Partition GetNext()
        {
            return toConnect.Dequeue();
        }

        // returns all partitions as an array
        public Partition[] GetPartitions()
        {
            return toConnect.ToArray();
        }

        // checks if a position is occupied, if so, returns the value of the occupying
        // partition. If not, returns null.
        public Partition CheckPlacement(Location location)
        {
            return connectedBy[location.x, location.y].Get();
        }

        public void Place(Partition partition, Location location)
        {
            // if a position is unoccupied, add it to the connection mapping and put the
            // partition back in the queue.
            if (CheckPlacement(location) == null)
            {
                connectedBy[location.x, location.y] = partition.Get();

                toConnect.Enqueue(partition);
            }
            else
            {
                // if it's not, this partition will be parsed as the partition it's been
                // joined to from now on.
                partition.Set(connectedBy[location.x,location.y].Get());
            }
        }

        public TileBlockData GetTiles()
        {
            // return entrances, fixedtiles and entrances all in one block
            return null;
        }

        public int GetNumberOfPartitions()
        {
            return toConnect.Count;
        }
    }
}
