using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;

using TCPGameServer.General;

using TCPGameServer.World.Map.IO;
using TCPGameServer.World.Map.Generation.LowLevel.Values;
using TCPGameServer.World.Map.Generation.LowLevel.Values.Perlin;
using TCPGameServer.World.Map.Generation.LowLevel.Tiles;
using TCPGameServer.World.Map.Generation.LowLevel.Connections;

using TCPGameServer.World.Map.IO.MapFile;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave
{
    public class CaveGenerator : LowLevelGenerator
    {
        public CaveGenerator(GeneratorData generatorData)
            : base(generatorData)
        {

        }

        protected override void DoAfterExpansion()
        {
            AddWalls();
        }

        protected override Valuemap GetValuemap()
        {
            PerlinGeneratedValuemapData perlinData = new PerlinGeneratedValuemapData();

            perlinData.seed = seed;
            perlinData.width = GetWidth();
            perlinData.height = GetHeight();
            perlinData.octaves = 3;
            perlinData.frequencyIncrease = 2.9299d;
            perlinData.persistence = 1.5d;
            perlinData.smoothAfter = false;
            perlinData.smoothInbetween = false;
            perlinData.bowl = true;
            perlinData.normalize = false;

            return new Valuemap(Valuemap.GENERATOR_TYPE_PERLIN, perlinData);
        }

        private void AddWalls()
        {
            Partition finalPartition = connectionmap.GetNext();

            while (expansionFront[finalPartition.GetIndex()].HasNext())
            {
                Expand(finalPartition, TileType.Wall, TileRepresentation.Wall, false);
            }
        }

        protected override bool GetFinishedCondition()
        {
            int numberOfPartitions = connectionmap.GetNumberOfPartitions();
            int count = tilemap.GetCount();

            return (numberOfPartitions == 1 && count > 300);
        }

        protected override String GetAreaType()
        {
            return "Cave";
        }
    }
}
