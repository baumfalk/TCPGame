using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.World.Map.Generation.LowLevel.Values;
using TCPGameServer.World.Map.Generation.LowLevel.Tiles;
using TCPGameServer.World.Map.Generation.LowLevel.Connections;
using TCPGameServer.World.Map.Generation.LowLevel.Connections.Expansion;

using TCPGameServer.General;

using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.Generation.LowLevel
{
    class LowLevelGenerator
    {
        protected int seed;
        protected bool isStub;

        protected Valuemap valuemap;
        protected Tilemap tilemap;
        protected Connectionmap connectionmap;
        protected Front[] expansionFront;

        protected Location bottomLeft;

        // initializes everything that needs the generatordata to do so
        public LowLevelGenerator(GeneratorData generatorData)
        {
            // set the seed
            seed = generatorData.fileData.header.seed;

            // check if we're generating a map for the first time
            isStub = generatorData.fileData.header.fileType.Equals("Stub");

            // set data to create a value map
            ValuemapData mapData = new ValuemapData();
            mapData.seed = seed;
            mapData.height = GetHeight();
            mapData.width = GetWidth();

            // create a new value map
            valuemap = GetValuemap(mapData);

            // generate exits
            TileBlockData exits =
                ExitGenerator.GenerateExits(
                    GetWidth(),
                    GetHeight(),
                    seed,
                    generatorData.fileData.header.mapGridLocation,
                    generatorData.fileData.entrances);

            // create a new connection map
            connectionmap = new Connectionmap(
                GetWidth(),
                GetHeight(),
                generatorData.fileData.entrances,
                generatorData.fileData.fixedTiles);

            // set the bottom left point based on the grid location
            bottomLeft = MapGridHelper.MapGridLocationToBottomLeft(generatorData.fileData.header.mapGridLocation);

            // the entrances already in the file are not the only ones,
            // more may have been added to link maps that did not exist
            // until this generator ran
            TileBlockData tilesToAdd = connectionmap.GetTiles();

            // create a new tile map
            tilemap = new Tilemap(
                GetWidth(),
                GetHeight(),
                tilesToAdd,
                bottomLeft,
                generatorData.area,
                generatorData.world);
        }

        // generate the actual area
        public virtual AreaData Generate()
        {
            // put the first items in the expansion front
            SetInitialFront();

            // expand the areas around the entrances until the completion criteria of
            // the map type are met
            ExpandUntilFinishedConditionMet();

            // link up the tiles in the tile map
            LinkTiles();

            // generate the area data and return it
            return GenerateAreaData();
        }

        protected void SetInitialFront()
        {
            int numEntrances = connectionmap.GetNumberOfPartitions();

            expansionFront = new Front[numEntrances];

            for (int n = 0; n < numEntrances; n++)
            {
                expansionFront[n] = new Front(entrancePartition, GetWeight);
            }
        }

        protected void ExpandUntilFinishedConditionMet()
        {
            while (!GetFinishedCondition())
            {
                Expand(connectionmap.GetNext(), "floor", "floor", true);
            }
        }

        protected void Expand(Partition partition, String type, String representation, bool expand)
        {
            // get next location
            Location pointToAdd = expansionFront[partition.index].GetNext();

            // check with connection map if this needs to be a merge or an add
            Partition occupyingPartition = connectionmap.CheckPlacement(pointToAdd);

            // update the connectionmap based on placement of this partition on this location
            connectionmap.Place(partition, pointToAdd);

            // if the tile is unoccupied, add a tile to it and add it's neighbors to the
            // expansion front
            if (occupyingPartition == null)
            {
                tilemap.AddTile(pointToAdd, type, representation);

                if (expand) AddNeighborsToFront(pointToAdd, partition);
            }
            else
            {
                // if it is occupied, merge the fronts of the two partitions
                expansionFront[occupyingPartition.index].Merge(expansionFront[partition.index]);

                // if there are still multiple partitions, and the map generator requires
                // a recalculation of values in the expansion front on a merge, recalculate
                // the weights in the expansion front of the partition.
                if (connectionmap.GetNumberOfPartitions() > 1 && NeedsRecalculation())
                {
                    expansionFront[partition.index].RecalculateWeights();
                }
            }
        }

        // takes a location and a partition, and adds all neighboring locations that are not
        // already in that partition to it's expansion front
        private void AddNeighborsToFront(Location location, Partition partition)
        {
            List<Location> neighbors = GetNeighbors(location);

            foreach (Location neighbor in neighbors)
            {
                Partition connector = connectionmap.CheckPlacement(neighbor);

                if (connector != partition) expansionFront[partition.index].AddToFront(neighbor);
            }
        }

        // links the tiles in the area together
        private void LinkTiles()
        {
            Tile[] tiles = tilemap.GetTiles();

            int[][] links = tilemap.GetLinks();

            int numTiles = tiles.Length;

            // loop through all the tiles
            for (int n = 0; n < numTiles; n++)
            {
                // split into the six different directions. -1 indicates no link,
                // a link into another area is denoted with the area name, a semicolon
                // and the tile ID in the other area.

                // we check for each direction
                for (int direction = 0; direction < 6; direction++)
                {
                    // get the link in this direction
                    int linkTo = links[n][direction];

                    // if there is a link, tell the Tile to hook it up
                    if (linkTo > -1) tiles[n].Link(direction, tiles[linkTo]);
                }
            }
        }

        // area data for a low level generator is just the tiles it generated
        protected AreaData GenerateAreaData()
        {
            AreaData toReturn = new AreaData();

            toReturn.tiles = tilemap.GetTiles();

            return toReturn;
        }

        protected virtual bool GetFinishedCondition()
        {
            return (connectionmap.GetNumberOfPartitions() == 1);
        }

        protected virtual int GetWidth()
        {
            return 100;
        }

        protected virtual int GetHeight()
        {
            return 100;
        }

        protected virtual Valuemap GetValuemap(ValuemapData mapData)
        {
            return new Valuemap(Valuemap.GENERATOR_TYPE_RANDOM, mapData);
        }

        protected virtual int GetWeight(Location location, int partition)
        {
            return valuemap.GetValue(location);
        }

        protected virtual bool NeedsRecalculation()
        {
            return false;
        }

        protected virtual double GetExitChance()
        {
            // niet een value gebruiken waar je niet van kan uitleggen waarom hij
            // die value heeft. Nadenken over goede waarde.
            return 0.67d;
        }

        protected virtual String GetAreaType()
        {
            return "Generic Map";
        }

        private List<Location> GetNeighbors(Location mapLocation)
        {
            List<Location> neighbors = new List<Location>();

            if (mapLocation.x > 0) neighbors.Add(new Location(mapLocation.x - 1, mapLocation.y, mapLocation.z));
            if (mapLocation.x < 99) neighbors.Add(new Location(mapLocation.x + 1, mapLocation.y, mapLocation.z));
            if (mapLocation.y > 0) neighbors.Add(new Location(mapLocation.x, mapLocation.y - 1, mapLocation.z));
            if (mapLocation.y < 99) neighbors.Add(new Location(mapLocation.x, mapLocation.y + 1, mapLocation.z));

            return neighbors;
        }

        private void SaveStaticTiles()
        {
            AreaWriter.SaveStatic(area.GetName(), entrances, exits, fixedTiles);
        }
    }
}
