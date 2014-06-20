using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.General;
using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave
{
    class Cave_TunnelGenerator : CaveGenerator
    {
        private Location[] entrances;

        public Cave_TunnelGenerator(GeneratorData generatorData)
            : base(generatorData)
        {
            this.entrances = new Location[entrances.Length];

            for (int n = 0; n < entrances.Length; n++)
            {
                this.entrances[n] = MapGridHelper.TileLocationToCurrentMapLocation(generatorData.entrances[n].GetLocation());
            }
        }

        protected override bool GetContinueCondition()
        {
            return toConnect.Count > 1;
        }

        protected override int GetWeight(Location location, int color)
        {
            return valuemap[location.x][location.y] + GetDistanceModifier(location, color);
        }

        private int GetDistanceModifier(Location location, int color)
        {
            int lowestDistance = GetDistanceToClosestOtherEntrance(location, color);

            return (int) (Math.Sqrt(lowestDistance) * 25.00d);
        }

        private int DistMod(int lowestDistance)
        {
            return (int)(Math.Sqrt(lowestDistance) * 25.00d);
        }

        private int GetDistanceToClosestOtherEntrance(Location location, int color)
        {
            int lowest = int.MaxValue;

            foreach (Location entrance in entrances)
            {
                if (connectedBy[entrance.x, entrance.y] != color)
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
