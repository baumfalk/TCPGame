using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

using TCPGameServer.General;

using TCPGameServer.World.Map.IO;
using TCPGameServer.World.Map.Generation.LowLevel.Values;
using TCPGameServer.World.Map.Generation.LowLevel.Values.Perlin;
using TCPGameServer.World.Map.Generation.LowLevel.Tiles;
using TCPGameServer.World.Map.Generation.LowLevel.Connections;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave
{
    class CaveGenerator : LowLevelGenerator
    {
        public CaveGenerator(GeneratorData generatorData)
            : base(generatorData)
        {

        }

        // generate the actual area
        public override AreaData Generate()
        {
            // generate the exits
            TileBlockData exits = environmentManager.GenerateExits(seed, GetExitChance());

            // add the exits to the connection map
            Partition[] exitPartitions = connectionmap.AddEntrances(exits);

            // create expansion fronts for the exits
            CreateFront(exits, exitPartitions);

            // add the exits to the tile map
            tilemap.AddExits(exits);

            // expand the areas around the entrances until the completion criteria of
            // the map type are met
            ExpandUntilFinishedConditionMet();

            // add walls
            AddWalls();

            // link up the tiles in the tile map
            LinkTiles();

            // generate the area data and return it
            return GenerateAreaData();
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
            Output.Print("adding walls");

            Partition finalPartition = connectionmap.GetNext();

            Output.Print("final partition has " + expansionFront[finalPartition.GetIndex()].Count() + " tiles in front");
            while (expansionFront[finalPartition.GetIndex()].HasNext())
            {
                Expand(finalPartition, "wall", "wall", false);
            }
        }

        protected override bool GetFinishedCondition()
        {
            return (connectionmap.GetNumberOfPartitions() == 1 && tilemap.GetCount() > 300);
        }

        protected override String GetAreaType()
        {
            return "Cave";
        }
    }
}
