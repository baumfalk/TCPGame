using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.General;
using TCPGameServer.Control.IO;

using TCPGameServer.World.Map.Generation.LowLevel.Connections;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave
{
    public class Cave_TunnelGenerator : CaveGenerator
    {
        private Location[] entrances;
        private Partition firstEntrance;

        private int leftToConnect;

        private int windupPeriod;
        private bool windupDone;

        public Cave_TunnelGenerator(GeneratorData generatorData)
            : base(generatorData)
        {
            
        }

        protected override void DoBeforeExpansion()
        {
            TileBlockData allEntrances = environmentManager.GetAllEntrances();

            entrances = new Location[allEntrances.numberOfTiles];

            for (int n = 0; n < entrances.Length; n++)
            {
                entrances[n] = MapGridHelper.TileLocationToCurrentMapLocation(allEntrances.tileData[n].location);
            }

            windupPeriod = 30 * entrances.Length;

            leftToConnect = entrances.Length;

            firstEntrance = connectionmap.GetEntrancePartitions()[0];
        }

        protected override void DoAtExpansionLoopStart()
        {
            base.DoAtExpansionLoopStart();

            windupPeriod--;
        }

        protected override Partition DeterminePartitionToExpand()
        {
            if (windupPeriod > 0) return base.DeterminePartitionToExpand();

            if (!windupDone)
            {
                expansionFront[firstEntrance.GetIndex()].RecalculateWeights();
                windupDone = true;
            }

            return firstEntrance;
        }

        protected override bool GetFinishedCondition()
        {
            return (leftToConnect == 1);
        }

        protected override int GetWeight(Partition partition, Location location)
        {
            if (windupPeriod > 0) return base.GetWeight(partition, location);

            int value = valuemap.GetValue(location);
            int distMod = GetDistanceModifier(partition, location);

            return value + distMod;
        }

        private int GetDistanceModifier(Partition partition, Location location)
        {
            int lowestDistance = GetDistanceToClosestOtherEntrance(partition, location);

            return 4 * lowestDistance;
        }

        private int GetDistanceToClosestOtherEntrance(Partition partition, Location location)
        {
            int lowest = int.MaxValue;

            foreach (Location entrance in entrances)
            {
                Partition entrancePartition = connectionmap.CheckPlacement(entrance);

                if (entrancePartition != null && !partition.Equals(entrancePartition))
                {
                    int distance = Math.Abs(location.x - entrance.x) + Math.Abs(location.y - entrance.y) + Math.Abs(location.z - entrance.z);

                    if (distance < lowest) lowest = distance;
                }
            }

            return lowest;
        }

        protected override bool NeedsRecalculation()
        {
            leftToConnect -= 1;

            return true;
        }

        protected override string GetAreaType()
        {
            return "Tunnel Cave";
        }
    }
}
