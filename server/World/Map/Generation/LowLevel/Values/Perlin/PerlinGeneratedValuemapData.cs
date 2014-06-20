using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.Generation.LowLevel.Values.Perlin
{
    class PerlinGeneratedValuemapData : ValuemapData
    {
        public int octaves;
        public double frequencyIncrease;
        public double persistence;
        public  bool smoothInbetween;
        public bool smoothAfter;
        public bool bowl;
        public bool normalize;
    }
}
