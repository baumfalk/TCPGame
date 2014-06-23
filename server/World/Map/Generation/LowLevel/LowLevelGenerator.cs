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

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Map.Generation.LowLevel
{
    public abstract class LowLevelGenerator
    {
        protected int seed;
        protected bool isStub;

        protected EnvironmentManager environmentManager;

        protected Valuemap valuemap;
        protected Tilemap tilemap;
        protected Connectionmap connectionmap;
        protected Front[] expansionFront;

        protected Location mapGridLocation;
        protected Location bottomLeft;

        // initializes everything that needs the generatordata to do so
        public LowLevelGenerator(GeneratorData generatorData)
        {
            // set the seed
            seed = generatorData.fileData.header.seed;

            // check if we're generating a map for the first time
            isStub = generatorData.fileData.header.fileType.Equals("Stub");

            // set the bottom left point based on the grid location
            mapGridLocation = generatorData.fileData.header.mapGridLocation;
            bottomLeft = MapGridHelper.MapGridLocationToBottomLeft(mapGridLocation);

            // create environment manager
            environmentManager = new EnvironmentManager(
                GetWidth(),
                GetHeight(),
                mapGridLocation,
                generatorData.fileData.entrances,
                generatorData.fileData.fixedTiles,
                generatorData.area,
                generatorData.world);

            // create a new value map
            valuemap = GetValuemap();

            // create a new connection map
            connectionmap = new Connectionmap(
                GetWidth(),
                GetHeight());

            // add the entrances to the connection map
            Partition[] entrancePartitions = connectionmap.AddEntrances(generatorData.fileData.entrances);

            // create expansion fronts for these entrances
            CreateFront(generatorData.fileData.entrances, entrancePartitions);

            // add the fixed tiles to the connection map
            connectionmap.AddFixedTiles(generatorData.fileData.fixedTiles);

            // create a new tile map
            tilemap = new Tilemap(
                GetWidth(),
                GetHeight(),
                bottomLeft,
                generatorData.area,
                generatorData.world);

            // add the entrances to the tile map
            tilemap.AddEntrances(generatorData.fileData.entrances);

            // add the fixed tiles to the tile map
            tilemap.AddFixedTiles(generatorData.fileData.fixedTiles);
        }

        // generate the actual area
        public LowLevelData Generate()
        {
            // generate the exits
            TileBlockData exits = environmentManager.GenerateExits(isStub, seed, GetExitChance());

            // add the exits to the connection map
            Partition[] exitPartitions = connectionmap.AddEntrances(exits);
            // create expansion fronts for the exits
            CreateFront(exits, exitPartitions);

            // add the exits to the tile map
            tilemap.AddExits(exits);

            // do things that need to be done before the expansion loop
            DoBeforeExpansion();

            // expand the areas around the entrances until the completion criteria of
            // the map type are met
            ExpandUntilFinishedConditionMet();

            // do things that need to be done after the expansion loop
            DoAfterExpansion();

            // link up the tiles in the tile map
            LinkTiles();

            // if this is a stub, write the static file
            if (isStub) environmentManager.SaveStaticTiles();

            // generate the area data and return it
            return GenerateReturnData();
        }

        protected virtual void DoBeforeExpansion() { }
        protected virtual void DoAfterExpansion() { }

        // each entrance needs to have an associated expansion front
        protected void CreateFront(TileBlockData entrances, Partition[] partitions)
        {
            int numEntrances = entrances.numberOfTiles;
            int offset;

            if (expansionFront == null)
            {
                // create a new expansion front
                expansionFront = new Front[numEntrances];

                offset = 0;
            }
            else
            {
                Front[] frontBuffer = new Front[numEntrances + expansionFront.Length];

                offset = expansionFront.Length;

                Array.Copy(expansionFront, frontBuffer, offset);

                expansionFront = frontBuffer;
            }

            // create a front for each entrance and add the entrance location to it
            for (int n = 0; n < numEntrances; n++)
            {
                expansionFront[n + offset] = new Front(GetWidth(), GetHeight(), partitions[n], GetWeight);
                expansionFront[n + offset].AddToFront(entrances.tileData[n].location, 0);
            }
        }

        protected virtual void ExpandUntilFinishedConditionMet()
        {
            while (!GetFinishedCondition())
            {
                DoAtExpansionLoopStart();

                Partition partition = DeterminePartitionToExpand();

                TileType type = DetermineTypeOfNewPosition(partition);
                String representation = DetermineRepresentationOfNewPosition(partition, type);

                Location pointAdded = Expand(partition, type, representation, true);

                // update the connectionmap based on placement of this partition on this location
                connectionmap.Place(partition, pointAdded);

                DoAtExpansionLoopEnd(partition, pointAdded);
            }
        }

        protected virtual void DoAtExpansionLoopStart() { }
        protected virtual void DoAtExpansionLoopEnd(Partition partition, Location pointAdded) { }

        protected virtual Partition DeterminePartitionToExpand()
        {
            return connectionmap.GetNext();
        }

        protected virtual TileType DetermineTypeOfNewPosition(Partition partition)
        {
            return TileType.Floor;
        }

        protected virtual String DetermineRepresentationOfNewPosition(Partition partition, TileType type)
        {
            return "floor";
        }

        protected virtual Location Expand(Partition partition, TileType type, String representation, bool expand)
        {
            int index = partition.GetIndex();

            // get next location
            Location pointToAdd = expansionFront[index].GetNext();

            // check with connection map if this needs to be a merge or an add
            Partition occupyingPartition = connectionmap.CheckPlacement(pointToAdd);

            // if the tile is unoccupied, add a tile to it and add it's neighbors to the
            // expansion front
            if (occupyingPartition == null)
            {
                tilemap.AddTile(pointToAdd, type, representation);

                connectionmap.AddToQueue(partition);

                if (expand) AddNeighborsToFront(pointToAdd, partition);
            }
            else
            {
                // get the index of the queue associated with the occupying partition
                int occupyingIndex = occupyingPartition.GetIndex();

                // don't merge a priority queue with itself
                if (index != occupyingIndex)
                {
                    // if it is occupied, merge the fronts of the two partitions
                    expansionFront[occupyingIndex].Merge(expansionFront[index]);

                    // merge the partitions in the connection map
                    connectionmap.MergePartitions(partition, pointToAdd);

                    // if there are still multiple partitions, and the map generator requires
                    // a recalculation of values in the expansion front on a merge, recalculate
                    // the weights in the expansion front of the partition.
                    if (!GetFinishedCondition() && NeedsRecalculation())
                    {
                        expansionFront[occupyingIndex].RecalculateWeights();
                    }
                }
                else
                {
                    connectionmap.AddToQueue(partition);
                }
            }

            return pointToAdd;
        }

        // takes a location and a partition, and adds all neighboring locations that are not
        // already in that partition to it's expansion front
        protected void AddNeighborsToFront(Location location, Partition partition)
        {
            List<Location> neighbors = GetNeighbors(location);

            foreach (Location neighbor in neighbors)
            {
                Partition connector = connectionmap.CheckPlacement(neighbor);

                if (connector == null || connector.GetIndex() != partition.GetIndex())
                {
                    expansionFront[partition.GetIndex()].AddToFront(neighbor);
                }
            }
        }

        // links the tiles in the area together
        protected void LinkTiles()
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

        // return the three maps that the low level generator builds, plus the area type
        protected LowLevelData GenerateReturnData()
        {
            LowLevelData toReturn = new LowLevelData();

            toReturn.areaType = GetAreaType();

            toReturn.connectionmap = connectionmap;
            toReturn.tilemap = tilemap;
            toReturn.valuemap = valuemap;

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

        protected virtual Valuemap GetValuemap()
        {
            ValuemapData mapData = new ValuemapData();
            mapData.seed = seed;
            mapData.height = GetHeight();
            mapData.width = GetWidth();

            return new Valuemap(Valuemap.GENERATOR_TYPE_RANDOM, mapData);
        }

        protected virtual int GetWeight(Partition partition, Location location)
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

        protected List<Location> GetNeighbors(Location mapLocation)
        {
            List<Location> neighbors = new List<Location>();

            if (mapLocation.x > 0) neighbors.Add(new Location(mapLocation.x - 1, mapLocation.y, mapLocation.z));
            if (mapLocation.x < 99) neighbors.Add(new Location(mapLocation.x + 1, mapLocation.y, mapLocation.z));
            if (mapLocation.y > 0) neighbors.Add(new Location(mapLocation.x, mapLocation.y - 1, mapLocation.z));
            if (mapLocation.y < 99) neighbors.Add(new Location(mapLocation.x, mapLocation.y + 1, mapLocation.z));

            return neighbors;
        }
    }
}
