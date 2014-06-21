using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.IO.MapFile;
using TCPGameServer.World.Map.Generation.LowLevel.Connections.Expansion;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections
{
    public class Connectionmap
    {
        private Queue<Partition> expansionQueue;
        private Partition[,] connectedBy;
        private List<Partition> entrances;

        public Connectionmap(int width, int height)
        {
            // create map data structures
            expansionQueue = new Queue<Partition>();
            connectedBy = new Partition[width, height];
            entrances = new List<Partition>(); 
        }

        // adds an entrance to the connection map. Entrances are partitions that have an associated
        // expansion front.
        public Partition[] AddEntrances(TileBlockData partitionData)
        {
            // the number of entrances to be added
            int numPartitions = partitionData.numberOfTiles;

            // the array of partitions to return
            Partition[] entrancePartitions = new Partition[numPartitions];

            for (int n = 0; n < numPartitions; n++)
            {
                // we need the location for the connection map
                Location partitionLocation = partitionData.tileData[n].location;

                // create a non-fixed partition with it's ID based on the current number of partitions
                // (note that partitions without an expansion front aren't counted for this statistic)
                Partition newPartition = new Partition(GetNumberOfPartitions(), false);

                // add the entrance to the connection map, the expansion queue and the list of entrances
                //connectedBy[partitionLocation.x, partitionLocation.y] = newPartition;
                expansionQueue.Enqueue(newPartition);
                entrances.Add(newPartition);

                // add the partition to the array
                entrancePartitions[n] = newPartition;
            }

            // return the partitions to the caller
            return entrancePartitions;
        }

        // fixed tiles need to be in a partition with an entrance as it's parent. Nothing can connect to
        // a fixed tile, so make sure you don't enclose entrances in fixed tiles.
        public void AddFixedTiles(TileBlockData[] fixedTileData)
        {
            // the array of fixed tiles is sorted in the same order as the entrances, so the associated entrance
            // of each block is the entrance already added
            for (int associatedEntrance = 0; associatedEntrance < fixedTileData.Length; associatedEntrance++)
            {
                // the parent partition is the partition associated with the entrance
                Partition parent = entrances[associatedEntrance];
                // create a fixed partition without an index
                Partition newPartition = new Partition(-1, true);
                // set the parent
                newPartition.SetParent(parent);

                // add each tile of this block to the connection map under the new partition
                for (int n = 0; n < fixedTileData[associatedEntrance].tileData.Length; n++)
                {
                    Location partitionLocation = fixedTileData[associatedEntrance].tileData[n].location;

                    connectedBy[partitionLocation.x, partitionLocation.y] = newPartition;
                }
            }
        }

        // returns the next partition to be expanded
        public Partition GetNext()
        {
            return expansionQueue.Dequeue();
        }

        // returns all partitions as an array
        public Partition[] GetPartitions()
        {
            return expansionQueue.ToArray();
        }

        // checks if a position is occupied, if so, returns the value of the occupying
        // partition. If not, returns null.
        public Partition CheckPlacement(Location location)
        {
            return connectedBy[location.x, location.y];
        }

        public void Place(Partition partition, Location location)
        {
            // if a position is unoccupied, add it to the connection mapping and put the
            // partition back in the queue.
            if (CheckPlacement(location) == null)
            {
                connectedBy[location.x, location.y] = partition;

                expansionQueue.Enqueue(partition);
            }
            else if (CheckPlacement(location).Equals(partition))
            {
                // when queues have just been joined, sometimes tiles in the front will
                // be in the same partition. In that case we don't want to remove this
                // partition from the queue.
                expansionQueue.Enqueue(partition);
            }
            else
            {
                // if it's not, this partition will be parsed as the partition it's been
                // joined to from now on.
                partition.SetParent(connectedBy[location.x,location.y]);
            }
        }

        public int GetNumberOfPartitions()
        {
            return expansionQueue.Count;
        }
    }
}
