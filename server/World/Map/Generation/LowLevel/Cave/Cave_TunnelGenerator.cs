﻿using System;
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
        private int windupPeriod;

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

            return firstEntrance;
        }

        protected override bool GetFinishedCondition()
        {
            return (connectionmap.GetNumberOfPartitions() == 1);
        }

        protected override int GetWeight(Partition partition, Location location)
        {
            if (windupPeriod > 0) return base.GetWeight(partition, location);

            int value = valuemap.GetValue(location);
            int distMod = GetDistanceModifier(partition, location);

            Output.Print("value of tile = " + valuemap.GetValue(location) + ", distance mod = " + distMod);

            return value + distMod;
        }

        private int GetDistanceModifier(Partition partition, Location location)
        {
            int lowestDistance = GetDistanceToClosestOtherEntrance(partition, location);

            return 2 * lowestDistance;
            //return (int) (Math.Sqrt(lowestDistance) * 25.00d);
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
            return true;
        }

        protected override string GetAreaType()
        {
            return "Tunnel Cave";
        }
    }
}
