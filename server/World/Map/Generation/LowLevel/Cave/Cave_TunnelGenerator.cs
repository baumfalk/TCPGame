using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.General;
using TCPGameServer.Control.IO;

using TCPGameServer.World.Map.Generation.LowLevel.Connections;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave
{
    class Cave_TunnelGenerator : CaveGenerator
    {
        private Location[] entrances;

        public Cave_TunnelGenerator(GeneratorData generatorData)
            : base(generatorData)
        {
            this.entrances = new Location[generatorData.fileData.entrances.numberOfTiles];

            for (int n = 0; n < entrances.Length; n++)
            {
                this.entrances[n] = MapGridHelper.TileLocationToCurrentMapLocation(generatorData.fileData.entrances.tileData[n].location);
            }
        }

        protected override bool GetFinishedCondition()
        {
            return (connectionmap.GetNumberOfPartitions() == 1);
        }

        protected override int GetWeight(Partition partition, Location location)
        {
            return valuemap.GetValue(location) + GetDistanceModifier(partition, location);
        }

        private int GetDistanceModifier(Partition partition, Location location)
        {
            int lowestDistance = GetDistanceToClosestOtherEntrance(partition, location);

            return (int) (Math.Sqrt(lowestDistance) * 25.00d);
        }

        private int DistMod(int lowestDistance)
        {
            return (int)(Math.Sqrt(lowestDistance) * 25.00d);
        }

        private int GetDistanceToClosestOtherEntrance(Partition partition, Location location)
        {
            int lowest = int.MaxValue;

            foreach (Location entrance in entrances)
            {
                if (!connectionmap.CheckPlacement(location).Equals(partition))
                {
                    int distance = Math.Abs(location.x - entrance.x) + Math.Abs(location.y - entrance.y) + Math.Abs(location.z - entrance.z);

                    if (distance < lowest) lowest = distance;
                }
            }

            return lowest;
        }

        protected override string GetAreaType()
        {
            return "Tunnel Cave";
        }
    }
}
