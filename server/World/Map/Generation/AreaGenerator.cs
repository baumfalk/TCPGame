﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

using TCPGameServer.World.Map.Generation.LowLevel;
using TCPGameServer.World.Map.Generation.LowLevel.Cave;

namespace TCPGameServer.World.Map.Generation
{
    class AreaGenerator
    {
        public static AreaData Generate(GeneratorData generatorData)
        {
            DateTime start = DateTime.Now;

            // low level generator (creates a walkable map)
            LowLevelGenerator generator;
            switch (generatorData.fileData.header.areaType)
            {
                case "Small Cave":
                    generator = new Cave_SmallCaveGenerator(generatorData);
                    break;
                case "Tunnel Cave":
                    generator = new Cave_TunnelGenerator(generatorData);
                    break;
                default:
                    Output.Print("nonexistent map type " + generatorData.fileData.header.areaType + ", returning cave");
                    generator = new CaveGenerator(generatorData);
                    break;
            }
            AreaData toReturn = generator.Generate();

            // mid level adjustments (think "fortress in the area", "river running through")

            // high level additions (creatures,items, etc)

            Output.Print("generation took " + (DateTime.Now - start).TotalMilliseconds + " milliseconds");

            return toReturn;
        }
    }
}
