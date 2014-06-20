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
            // put the first items in the expansion front
            SetInitialFront();

            // expand the areas around the entrances until the completion criteria of
            // the map type are met
            ExpandUntilFinishedConditionMet();

            // add walls around all the open areas
            AddWalls();

            // link up the tiles in the tile map
            LinkTiles();

            // generate the area data and return it
            return GenerateAreaData();
        }

        protected override Valuemap GetValuemap(ValuemapData mapData)
        {
            PerlinGeneratedValuemapData perlinData = (PerlinGeneratedValuemapData)mapData;

            perlinData.octaves = 3;
            perlinData.frequencyIncrease = 2.9299d;
            perlinData.persistence = 1.5d;
            perlinData.smoothAfter = false;
            perlinData.smoothInbetween = false;
            perlinData.bowl = true;
            perlinData.normalize = false;

            return new Valuemap(Valuemap.GENERATOR_TYPE_PERLIN, perlinData);
        }

        private void AddWalls() {
            int remainingColor = toConnect[0];
            while (expansionFront[remainingColor].Count() > 0)
            {
                Expand(remainingColor, "wall", "wall", false);
            }
        }

        protected override bool GetFinishedCondition()
        {
            return (connectionmap.GetNumberOfPartitions() == 1 && tilemap.GetCount() > 300);
        }

        protected void AddTilesToConnect()
        {
            connected = new List<int>[entrances.Length + exits.Length];
            expansionFront = new PriorityQueue<Location>[entrances.Length + exits.Length];
            isInFront = new bool[entrances.Length + exits.Length][,];

            for (int n = 0; n < entrances.Length; n++)
            {
                AddTileToConnect(entrances[n], n);
            }

            int offset = entrances.Length;

            for (int n = 0; n < exits.Length; n++)
            {
                AddTileToConnect(exits[n], n + offset);
            }
        }

        private void AddTileToConnect(Tile tileToConnect, int entranceIndex)
        {
            connected[entranceIndex] = new List<int>();

            Location mapPosition = MapGridHelper.TileLocationToCurrentMapLocation(tileToConnect.GetLocation());

            toConnect.Add(entranceIndex);
            connected[entranceIndex].Add(entranceIndex);

            expansionFront[entranceIndex] = new PriorityQueue<Location>();
            isInFront[entranceIndex] = new bool[100, 100];

            AddTile(mapPosition, entranceIndex, tileToConnect);
        }

        private void AddFixedTiles()
        {
            for (int n = 0; n < fixedTiles.Length; n++)
            {
                for (int i = 0; i < fixedTiles[n].Length; i++)
                {
                    Location mapPosition = MapGridHelper.TileLocationToCurrentMapLocation(fixedTiles[n][i].GetLocation());

                    AddTile(mapPosition, n, fixedTiles[n][i]);
                }
            }
        }

        protected override String GetAreaType()
        {
            return "Cave";
        }
    }
}
