﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.General;
using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation
{
    class Cave_TunnelGenerator : CaveGenerator
    {
        private int[][] entrances;

        public Cave_TunnelGenerator(int seed, Tile[] entrances, bool generateExits, int bottomLeftX, int bottomLeftY, int bottomLeftZ, Area area, World world)
            : base(seed, entrances, generateExits, bottomLeftX, bottomLeftY, bottomLeftZ, area, world)
        {
            this.entrances = new int[entrances.Length][];

            for (int n = 0; n < entrances.Length; n++)
            {
                this.entrances[n] = new int[] { entrances[n].GetX() * 100, entrances[n].GetY() };
            }
        }

        protected override bool GetContinueCondition()
        {
            return toConnect.Count > 1;
        }

        protected override int GetWeight(int x, int y, int color)
        {
            return valuemap[x][y] + GetDistanceModifier(x, y, color);
        }

        private int GetDistanceModifier(int x, int y, int color)
        {
            int lowestDistance = GetDistanceToClosestOtherEntrance(x, y, color);

            return (int) (Math.Sqrt(lowestDistance) * 25.00d);
        }

        private int DistMod(int lowestDistance)
        {
            return (int)(Math.Sqrt(lowestDistance) * 25.00d);
        }

        private int GetDistanceToClosestOtherEntrance(int x, int y, int color)
        {
            int lowest = int.MaxValue;

            foreach (int[] entrance in entrances)
            {
                //if (!toConnect.Contains(entrance)) continue;

                int entranceX = entrance[0];
                int entranceY = entrance[1];

                int distance = Math.Abs(x - entranceX) + Math.Abs(y - entranceY);

                if (distance < lowest) lowest = distance;
            }

            return lowest;
        }

        protected override string GetAreaType()
        {
            return "Tunnel Cave";
        }
    }
}
